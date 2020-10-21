using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDISongPerformanceFeedback : MIDIDeviceListener, MIDISongListener
{
    private MIDISongVisualizer m_songVisualizer;
    private MIDISong m_song;
    private Dictionary<int, float> m_deviceNoteTimestamps;
    private List<MIDINoteEvent> m_leftHandSongNotes;
    private List<MIDINoteEvent> m_rightHandSongNotes;
    private int m_leftHandSongNoteIndex = 0;
    private int m_rightHandSongNoteIndex = 0;
    private float m_noteHitTimingThreshold = 0.4f;
    private float m_elapsedTime = 0.0f;
    private int m_score = 0;
    private int m_hitStreak = 0;
    private List<int> m_hitStreakLevels;
    private int m_totalNoteCount = 0;
    private int m_correctNoteCount = 0;
    private float m_multiplier = 1.0f;
    private bool m_isEnabled = true;
    private bool m_isRightHandEnabled = true;
    private bool m_isLeftHandEnabled = true;
    private Color m_correctHitColor;
    private Color m_wrongHitColor;

    public MIDISongPerformanceFeedback(MIDISongVisualizer songVisualizer)
    {
        m_songVisualizer = songVisualizer;
        m_leftHandSongNotes = new List<MIDINoteEvent>();
        m_rightHandSongNotes = new List<MIDINoteEvent>();
        m_deviceNoteTimestamps = new Dictionary<int, float>();
        m_hitStreakLevels = new List<int>();
        m_hitStreakLevels.Add(25);
        m_hitStreakLevels.Add(50);
        m_hitStreakLevels.Add(100);
        m_correctHitColor = Color.yellow;
        m_wrongHitColor = Color.red;
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
        if (m_song == null)
            return;

        m_elapsedTime = -m_song.firstMeasureDuration;
        m_score = 0;
        m_totalNoteCount = 0;
        m_correctNoteCount = 0;
        m_hitStreak = 0;
        m_multiplier = 1.0f;
        m_leftHandSongNoteIndex = 0;
        m_rightHandSongNoteIndex = 0;
        m_leftHandSongNotes.Clear();
        m_rightHandSongNotes.Clear();

        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            MIDITrack track = m_song.tracks[i];
            foreach (MIDIEvent midiEvent in track.events)
            {
                if (midiEvent.GetType() == typeof(MIDINoteEvent))
                {
                    MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;
                    if (noteEvent.isNoteOn)
                    {
                        if (i == 1)
                            m_leftHandSongNotes.Add(noteEvent);
                        else if (i == 0)
                            m_rightHandSongNotes.Add(noteEvent);
                    }
                }
            }
        }

        m_deviceNoteTimestamps.Clear();
    }

    void MIDIDeviceListener.OnDeviceEvent(MIDIEvent midiEvent)
    {
        if (midiEvent.GetType() == typeof(MIDINoteEvent))
        {
            MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;

            if (noteEvent.isNoteOn)
            {
                if (m_isEnabled)
                    m_deviceNoteTimestamps.Add(noteEvent.note, m_elapsedTime);

                m_songVisualizer.StartNoteEffect(noteEvent.note);

                bool isCorrectHit = false;
                if (m_isRightHandEnabled)
                {
                    foreach(MIDINoteEvent rightHandSongNote in m_rightHandSongNotes)
                    {
                        if(rightHandSongNote.note == noteEvent.note)
                        {
                            if (m_elapsedTime >= (rightHandSongNote.timestamp - m_noteHitTimingThreshold / 2.0f) &&
                                m_elapsedTime <= (rightHandSongNote.timestamp + m_noteHitTimingThreshold / 2.0f))
                            {
                                isCorrectHit = true;
                                break;
                            }
                        }
                    }
                }
                if (m_isLeftHandEnabled)
                {
                    foreach (MIDINoteEvent leftHandSongNote in m_leftHandSongNotes)
                    {
                        if (leftHandSongNote.note == noteEvent.note)
                        {
                            if (m_elapsedTime >= (leftHandSongNote.timestamp - m_noteHitTimingThreshold / 2.0f) &&
                                m_elapsedTime <= (leftHandSongNote.timestamp + m_noteHitTimingThreshold / 2.0f))
                            {
                                isCorrectHit = true;
                                break;
                            }
                        }
                    }
                }

                if(isCorrectHit)
                    m_songVisualizer.ChangeNoteEffectColor(noteEvent.note, m_wrongHitColor);
                else
                    m_songVisualizer.ChangeNoteEffectColor(noteEvent.note, m_correctHitColor);

            }
            else
            {
                m_songVisualizer.StopNoteEffect(noteEvent.note);
            }
        }
    }

    void MIDISongListener.OnSongEvent(MIDIEvent midiEvent, int trackNumber)
    {
    }

    void MIDISongListener.OnUpdate(float deltaTime)
    {
        m_elapsedTime += deltaTime;

        if (!m_isEnabled)
            return;

        // Remove device notes that are older than the m_noteHitTimingThreshold
        List<int> deviceNotesToRemove = new List<int>();
        foreach (KeyValuePair<int, float> devicePressedNote in m_deviceNoteTimestamps)
            if ((m_elapsedTime - devicePressedNote.Value) > m_noteHitTimingThreshold)
                deviceNotesToRemove.Add(devicePressedNote.Key);

        foreach (int deviceNoteToRemove in deviceNotesToRemove)
            m_deviceNoteTimestamps.Remove(deviceNoteToRemove);

        // Find song events that are within the m_noteHitTimingThreshold
        Dictionary<int, MIDINoteEvent> currentSongNotes = new Dictionary<int, MIDINoteEvent>();
        if(m_isRightHandEnabled)
        {
            while (m_rightHandSongNotes[m_rightHandSongNoteIndex].timestamp + m_noteHitTimingThreshold / 2.0f <= m_elapsedTime)
            {
                currentSongNotes.Add(m_rightHandSongNotes[m_rightHandSongNoteIndex].note, m_rightHandSongNotes[m_rightHandSongNoteIndex]);
                m_rightHandSongNoteIndex++;
            }
        }
        if(m_isLeftHandEnabled)
        {
            while (m_leftHandSongNotes[m_leftHandSongNoteIndex].timestamp + m_noteHitTimingThreshold / 2.0f <= m_elapsedTime)
            {
                currentSongNotes.Add(m_leftHandSongNotes[m_leftHandSongNoteIndex].note, m_leftHandSongNotes[m_leftHandSongNoteIndex]);
                m_leftHandSongNoteIndex++;
            }
        }

        if (currentSongNotes.Count == 0)
            return;

        int correctHits = 0;
        int wrongHits = 0;
        int missedHits = 0;

        // Calc correct and missed hits
        foreach (KeyValuePair<int, MIDINoteEvent> currentSongNote in currentSongNotes)
        {
            if (m_deviceNoteTimestamps.ContainsKey(currentSongNote.Key))
            {
                m_deviceNoteTimestamps.Remove(currentSongNote.Key);
                correctHits++;
            }
            else
                missedHits++;
        }

        // save wrong device notes for next notes, if there are song events that match their key within the m_noteHitTimingThreshold
        Dictionary<int, float> savedDeviceNoteTimestamps = new Dictionary<int, float>();
        foreach (KeyValuePair<int, float> devicePressedNote in m_deviceNoteTimestamps)
        {
            if(m_isRightHandEnabled)
            {
                foreach (MIDINoteEvent songNote in m_rightHandSongNotes)
                {
                    if (devicePressedNote.Key == songNote.note &&
                        (songNote.timestamp - devicePressedNote.Value) <= m_noteHitTimingThreshold / 2.0f &&
                        (songNote.timestamp - devicePressedNote.Value) > 0.0f)
                    {
                        savedDeviceNoteTimestamps.Add(devicePressedNote.Key, devicePressedNote.Value);
                        break;
                    }
                }
            }
            if (m_isLeftHandEnabled)
            {
                foreach (MIDINoteEvent songNote in m_leftHandSongNotes)
                {
                    if (devicePressedNote.Key == songNote.note &&
                        (songNote.timestamp - devicePressedNote.Value) <= m_noteHitTimingThreshold / 2.0f &&
                        (songNote.timestamp - devicePressedNote.Value) > 0.0f)
                    {
                        savedDeviceNoteTimestamps.Add(devicePressedNote.Key, devicePressedNote.Value);
                        break;
                    }
                }
            }
        }

        // calc wrong hits
        foreach (KeyValuePair<int, float> savedDeviceNoteTimestamp in savedDeviceNoteTimestamps)
            m_deviceNoteTimestamps.Remove(savedDeviceNoteTimestamp.Key);

        wrongHits = m_deviceNoteTimestamps.Count;

        m_deviceNoteTimestamps.Clear();
        foreach (KeyValuePair<int, float> savedDeviceNoteTimestamp in savedDeviceNoteTimestamps)
            m_deviceNoteTimestamps.Add(savedDeviceNoteTimestamp.Key, savedDeviceNoteTimestamp.Value);

        // calc hit streak
        if (missedHits == 0 && wrongHits == 0)
            m_hitStreak += correctHits;
        else
            m_hitStreak = 0;

        // calc multiplier
        for(int i = 0; i < m_hitStreakLevels.Count; i++)
        {
            if(m_hitStreak < m_hitStreakLevels[i])
            {
                m_multiplier = i + 1;
                break;
            }
            else
            {
                m_multiplier = m_hitStreakLevels.Count + 1;
            }
        }

        // calc score
        int validHits = Math.Max(correctHits - Math.Max(wrongHits - missedHits, 0), 0);
        m_correctNoteCount += validHits;
        m_totalNoteCount += currentSongNotes.Count;
        m_score += (int)(validHits * 100 * m_multiplier);
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
        m_song = song;
        Reset();
    }

    void MIDISongListener.OnRightHandChanged(bool isEnabled)
    {
        m_isRightHandEnabled = isEnabled;
    }

    void MIDISongListener.OnLeftHandChanged(bool isEnabled)
    {
        m_isLeftHandEnabled = isEnabled;
    }

    public int GetHitStreak()
    {
        return m_hitStreak;
    }

    public List<int> GetHitStreakLevels()
    {
        return m_hitStreakLevels;
    }

    public int GetScore()
    {
        return m_score;
    }

    public int GetCorrectNoteCount()
    {
        return m_correctNoteCount;
    }

    public int GetTotalNoteCount()
    {
        return m_totalNoteCount;
    }
}
