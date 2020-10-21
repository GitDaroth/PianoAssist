using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDISongPracticeMode : MIDIDeviceListener, MIDISongListener
{
    private MIDISongPlayer m_songPlayer;
    private Dictionary<MIDINoteEvent, int> m_pendingNotes;
    private List<MIDINoteEvent> m_earlyPressedNotes;
    private bool m_isEnabled = true;
    private bool m_isRightHandEnabled = true;
    private bool m_isLeftHandEnabled = true;
    private float m_noteHitTimingThreshold = 0.1f;

    public MIDISongPracticeMode(MIDISongPlayer songPlayer)
    {
        m_songPlayer = songPlayer;
        m_pendingNotes = new Dictionary<MIDINoteEvent, int>();
        m_earlyPressedNotes = new List<MIDINoteEvent>();
    }

    public void Enable()
    {
        m_isEnabled = true;
    }

    public void Disable()
    {
        m_isEnabled = false;
        Reset();
    }

    private void Reset()
    {
        m_songPlayer.WakeUp();
        m_pendingNotes.Clear();
        m_earlyPressedNotes.Clear();
    }

    void MIDIDeviceListener.OnDeviceEvent(MIDIEvent midiEvent)
    {
        if (!m_isEnabled)
            return;

        if (midiEvent.GetType() == typeof(MIDINoteEvent))
        {
            MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;

            if (noteEvent.isNoteOn)
            {
                bool pressedPendingNote = false;

                foreach (var pendingNote in m_pendingNotes)
                {
                    if (pendingNote.Key.note == noteEvent.note)
                    {
                        m_pendingNotes.Remove(pendingNote.Key);
                        pressedPendingNote = true;
                        if (m_pendingNotes.Count == 0)
                            m_songPlayer.WakeUp();
                        break;
                    }
                }

                if (!pressedPendingNote)
                {
                    noteEvent.timestamp = 0.0f;
                    m_earlyPressedNotes.Add(noteEvent);
                }
            }
        }
    }

    void MIDISongListener.OnSongEvent(MIDIEvent midiEvent, int trackNumber)
    {
        if (!m_isEnabled)
            return;

        if (!m_isRightHandEnabled && trackNumber == 0)
            return;

        if (!m_isLeftHandEnabled && trackNumber == 1)
            return;

        if (midiEvent.GetType() == typeof(MIDINoteEvent))
        {
            MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;

            if (noteEvent.isNoteOn)
            {
                bool pressedEarlyNote = false;
                foreach (MIDINoteEvent earlyPressedNote in m_earlyPressedNotes)
                {
                    if (earlyPressedNote.note == noteEvent.note)
                    {
                        m_earlyPressedNotes.Remove(earlyPressedNote);
                        pressedEarlyNote = true;
                        break;
                    }
                }

                if (!pressedEarlyNote)
                {
                    m_pendingNotes.Add(noteEvent, trackNumber);
                    if (m_pendingNotes.Count == 1)
                        m_songPlayer.Sleep();
                }
            }
        }
    }

    void MIDISongListener.OnUpdate(float deltaTime)
    {
        List<MIDINoteEvent> notesToRemove = new List<MIDINoteEvent>();
        foreach (MIDINoteEvent earlyPressedNote in m_earlyPressedNotes)
        {
            earlyPressedNote.timestamp += deltaTime;
            if (earlyPressedNote.timestamp >= m_noteHitTimingThreshold)
                notesToRemove.Add(earlyPressedNote);
        }

        foreach (MIDINoteEvent noteToRemove in notesToRemove)
        {
            m_earlyPressedNotes.Remove(noteToRemove);
        }
    }

    void MIDISongListener.OnRestart()
    {
        Reset();
    }

    void MIDISongListener.OnSeek(float timestamp)
    {
        Reset();
    }

    void MIDISongListener.OnSongChanged(MIDISong song)
    {
        Reset();
    }
    
    void MIDISongListener.OnRightHandChanged(bool isEnabled)
    {
        m_isRightHandEnabled = isEnabled;
        if(!m_isRightHandEnabled)
            RemovePendingNotesByTrackNumber(0);
    }

    void MIDISongListener.OnLeftHandChanged(bool isEnabled)
    {
        m_isLeftHandEnabled = isEnabled;
        if(!m_isLeftHandEnabled)
            RemovePendingNotesByTrackNumber(1);
    }

    private void RemovePendingNotesByTrackNumber(int trackNumber)
    {
        if (m_pendingNotes.Count == 0)
            return;

        List<MIDINoteEvent> notesToRemove = new List<MIDINoteEvent>();
        foreach (var pendingNote in m_pendingNotes)
        {
            if (pendingNote.Value == trackNumber)
                notesToRemove.Add(pendingNote.Key);
        }

        foreach (MIDINoteEvent noteToRemove in notesToRemove)
            m_pendingNotes.Remove(noteToRemove);

        if (m_pendingNotes.Count == 0)
            m_songPlayer.WakeUp();
    }
}
