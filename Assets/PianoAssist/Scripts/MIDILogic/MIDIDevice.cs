using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MIDIDevice : MonoBehaviour
{
    public GameObject m_keyboardConnectionIcon;
    private List<MIDIDeviceListener> m_deviceListeners = new List<MIDIDeviceListener>();
    private bool m_isDeviceConnected = false;
    private bool m_foundDeviceTheFirstTime = false;
    private AndroidJavaObject m_midiCommunication;

    void Start()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        m_midiCommunication = new AndroidJavaObject("com.master.midinative.MIDICommunication");
        m_midiCommunication.Call("initialize", context);
    }

    void Update()
    {
        if(!m_foundDeviceTheFirstTime)
            m_midiCommunication.Call("searchForDevice");
    }

    public void OnNoteEvent(string message) // message i.e. "t,65,84" --> isNoteOn, key, velocity
    {
        MIDINoteEvent noteEvent = new MIDINoteEvent();

        string[] messages = message.Split(',');
        if (messages[0] == "t")
            noteEvent.isNoteOn = true;
        else
            noteEvent.isNoteOn = false;

        noteEvent.note = int.Parse(messages[1]);
        noteEvent.velocity = int.Parse(messages[2]);

        NotifyEvent(noteEvent);
    }

    public void OnDeviceConnected(string deviceName)
    {
        m_foundDeviceTheFirstTime = true;
        m_isDeviceConnected = true;
        m_keyboardConnectionIcon.GetComponent<ARKeyboardConnectionIcon>().SetIsConnected(m_isDeviceConnected);
    }

    public void OnDeviceDisconnected(string deviceName)
    {
        m_isDeviceConnected = false;
        m_keyboardConnectionIcon.GetComponent<ARKeyboardConnectionIcon>().SetIsConnected(m_isDeviceConnected);
    }

    public void RegisterDeviceListener(MIDIDeviceListener deviceListener)
    {
        m_deviceListeners.Add(deviceListener);
    }

    public void UnregisterDeviceListener(MIDIDeviceListener deviceListener)
    {
        m_deviceListeners.Remove(deviceListener);
    }

    public bool IsDeviceConntected()
    {
        return m_isDeviceConnected;
    }

    private void NotifyEvent(MIDIEvent midiEvent)
    {
        foreach (MIDIDeviceListener deviceListener in m_deviceListeners)
            deviceListener.OnDeviceEvent(midiEvent);
    }
}
