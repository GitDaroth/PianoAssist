package com.master.midinative;

import android.content.Context;
import android.media.midi.MidiDevice;
import android.media.midi.MidiDeviceInfo;
import android.media.midi.MidiManager;
import android.media.midi.MidiOutputPort;
import android.media.midi.MidiReceiver;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.IOException;

public class MIDICommunication
{
    private MidiManager m_midiManager;

    public void initialize(Context context)
    {
        m_midiManager = (MidiManager)context.getSystemService(Context.MIDI_SERVICE);

        m_midiManager.registerDeviceCallback(new MidiManager.DeviceCallback()
        {
            public void onDeviceAdded(MidiDeviceInfo deviceInfo)
            {
                String deviceName = deviceInfo.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME);
                if(deviceName != null)
                    UnityPlayer.UnitySendMessage("MIDIDevice", "OnDeviceConnected", deviceName);
                openDeviceConnection(deviceInfo);
            }
            public void onDeviceRemoved(MidiDeviceInfo deviceInfo)
            {
                String deviceName = deviceInfo.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME);
                if(deviceName != null)
                    UnityPlayer.UnitySendMessage("MIDIDevice", "OnDeviceDisconnected", deviceName);
            }
        }, new Handler(Looper.getMainLooper()));
    }

    public void searchForDevice()
    {
        MidiDeviceInfo[] deviceInfos = m_midiManager.getDevices();
        for(int i = 0; i < deviceInfos.length; i++)
        {
            MidiDeviceInfo deviceInfo = deviceInfos[i];
            String deviceName = deviceInfo.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME);
            if(deviceName != null)
                UnityPlayer.UnitySendMessage("MIDIDevice", "OnDeviceConnected", deviceName);
            openDeviceConnection(deviceInfo);
        }
    }

    public void openDeviceConnection(MidiDeviceInfo deviceInfo)
    {
        m_midiManager.openDevice(deviceInfo, new MidiManager.OnDeviceOpenedListener()
        {
            @Override
            public void onDeviceOpened(MidiDevice device)
            {
                if(device == null)
                {
                    Log.d("UnityMIDI", "Cannot open device");
                }
                else
                {
                    MidiOutputPort outputPort = device.openOutputPort(0);
                    outputPort.connect(new MidiReceiver()
                    {
                        @Override
                        public void onSend(byte[] msg, int offset, int count, long timestamp) throws IOException
                        {
                            byte[] midiEvent = new byte[count];
                            for(int i = 0; i < count; i++)
                                midiEvent[i] = msg[offset + i];

                            byte status = midiEvent[0];
                            if((status & 0xF0) == 0x80 || (status & 0xF0) == 0x90)
                            {
                                String isNoteOn = "";
                                int channel = status & 0x0F;
                                int key = midiEvent[1];
                                int velocity = midiEvent[2];
                                if((status & 0xF0) == 0x80) // note off
                                {
                                    isNoteOn = "f";
                                }
                                else if((status & 0xF0) == 0x90) // note on
                                {
                                    if(velocity == 0)
                                        isNoteOn = "f";
                                    else
                                        isNoteOn = "t";
                                }

                                String message = isNoteOn + "," + key + "," + velocity;
                                UnityPlayer.UnitySendMessage("MIDIDevice", "OnNoteEvent", message);
                            }
                        }
                    });
                }
            }
        }, new Handler(Looper.getMainLooper()));
    }
}
