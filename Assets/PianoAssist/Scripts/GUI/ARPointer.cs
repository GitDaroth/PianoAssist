using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPointer : MonoBehaviour, MIDIDeviceListener
{
    public GameObject m_midiDevice;
    public Material m_defaultMaterial;
    public Material m_hoveredMaterial;

    private GameObject m_hoveredGameObject;

    void Start()
    {
        m_hoveredGameObject = null;
        m_midiDevice.GetComponent<MIDIDevice>().RegisterDeviceListener(this);
        gameObject.GetComponent<MeshRenderer>().material = m_defaultMaterial;
    }

    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, LayerMask.GetMask("AugmentedUI")))
        {
            ARButton arButton;
            if(hit.collider.gameObject != m_hoveredGameObject && m_hoveredGameObject != null)
            {
                arButton = m_hoveredGameObject.GetComponent(typeof(ARButton)) as ARButton;
                if(arButton != null)
                {
                    arButton.Exit();
                    m_hoveredGameObject = null;
                }            
            }

            m_hoveredGameObject = hit.collider.gameObject;
            arButton = m_hoveredGameObject.GetComponent(typeof(ARButton)) as ARButton;
            if (arButton != null)
            {
                gameObject.GetComponent<MeshRenderer>().material = m_hoveredMaterial;
                arButton.Enter();
            }
        }
        else
        {
            if (m_hoveredGameObject != null)
            {
                ARButton arButton = m_hoveredGameObject.GetComponent(typeof(ARButton)) as ARButton;
                if (arButton != null)
                {
                    gameObject.GetComponent<MeshRenderer>().material = m_defaultMaterial;
                    arButton.Exit();
                    m_hoveredGameObject = null;
                }
               
            }
        }
    }

    void MIDIDeviceListener.OnDeviceEvent(MIDIEvent midiEvent)
    {
        if (midiEvent.GetType() == typeof(MIDINoteEvent))
        {
            MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;
            if (noteEvent.isNoteOn && m_hoveredGameObject != null)
            {
                ARButton arButton = m_hoveredGameObject.GetComponent(typeof(ARButton)) as ARButton;
                if (arButton != null)
                    arButton.Click(noteEvent.note);
            }
        }
    }
}
