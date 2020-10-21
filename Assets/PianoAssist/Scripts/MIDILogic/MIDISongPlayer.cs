using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDISongPlayer
{
    private MIDISong m_song = null;
    private List<MIDISongListener> m_songListeners = new List<MIDISongListener>();
    private List<int> m_nextEventIndexList;
    private float m_elapsedTime;
    private bool m_isPlaying = false;
    private bool m_isSleeping = false;
    private float m_speedFactor = 1.0f;
    private int m_currentMeasure = 0; 

    public void SetSong(MIDISong song)
    {
        m_song = song;

        m_nextEventIndexList = new List<int>();
        foreach (MIDITrack track in m_song.tracks)
            m_nextEventIndexList.Add(0);

        NotifySongChanged();

        Stop();
    }

    public void RegisterSongListener(MIDISongListener songListener)
    {
        m_songListeners.Add(songListener);
        songListener.OnSongChanged(m_song);
    }

    public void UnregisterSongListener(MIDISongListener songListener)
    {
        m_songListeners.Remove(songListener);
    }

    public void Start()
    {
        m_isPlaying = true;
    }

    public void Restart()
    {
        m_elapsedTime = -m_song.firstMeasureDuration;
        CalcCurrentMeasure();
        PrepareNextEventIndexList();

        NotifyRestart();
    }

    public void Pause()
    {
        m_isPlaying = false;
    }

    public void Stop()
    {
        Restart();
        Pause();
    }

    public void Seek(float timestamp)
    {
        m_elapsedTime = timestamp;
        CalcCurrentMeasure();
        PrepareNextEventIndexList();

        NotifySeek(timestamp);
    }

    public void Seek(int measure)
    {
        measure = Math.Max(Math.Min(measure, m_song.measureTimestamps.Count), 0);

        if (measure == 0)
            Seek(-m_song.firstMeasureDuration);
        else
            Seek(m_song.measureTimestamps[measure - 1]);
    }

    public void Sleep()
    {
        m_isSleeping = true;
    }

    public void WakeUp()
    {
        m_isSleeping = false;
    }

    public void Update(float deltaTime)
    {
        if(m_isPlaying && !m_isSleeping)
        {
            deltaTime = deltaTime * m_speedFactor;
            NotifyUpdate(deltaTime);

            m_elapsedTime += deltaTime;
            CalcCurrentMeasure();

            for (int i = 0; i < m_song.tracks.Count; i++)
            {
                MIDITrack track = m_song.tracks[i];

                bool processedAllEventsWithCurrentTimestamp = false;
                while (!processedAllEventsWithCurrentTimestamp)
                {
                    int nextEventIndex = m_nextEventIndexList[i];
                    if (nextEventIndex >= track.events.Count)
                        break;

                    MIDIEvent nextEvent = track.events[nextEventIndex];
                    if (nextEvent.timestamp <= m_elapsedTime)
                    {
                        NotifyEvent(nextEvent, i);
                        m_nextEventIndexList[i]++;
                    }
                    else
                    {
                        processedAllEventsWithCurrentTimestamp = true;
                    }
                }
            }
        }
    }

    private void CalcCurrentMeasure()
    {
        if(m_elapsedTime < 0.0f)
            m_currentMeasure = 0;
        else
        {
            for (int i = 0; i < m_song.measureTimestamps.Count; i++)
            {
                if (m_song.measureTimestamps[i] <= m_elapsedTime)
                {
                    m_currentMeasure = i + 1;
                }
            }
        }    
    }

    private void PrepareNextEventIndexList()
    {
        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            MIDITrack track = m_song.tracks[i];

            for (int j = 0; j < track.events.Count; j++)
            {
                if(track.events[j].timestamp >= m_elapsedTime - 0.001f)
                {
                    m_nextEventIndexList[i] = j;
                    break;
                }
                else
                {
                    m_nextEventIndexList[i] = track.events.Count;
                }
            }
        }
    }

    private void NotifyUpdate(float deltaTime)
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnUpdate(deltaTime);
    }

    private void NotifyRestart()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnRestart();
    }

    private void NotifySeek(float timestamp)
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnSeek(timestamp);
    }

    private void NotifyEvent(MIDIEvent midiEvent, int trackNumber)
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnSongEvent(midiEvent, trackNumber);
    }

    private void NotifySongChanged()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnSongChanged(m_song);
    }

    public void EnableRightHand()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnRightHandChanged(true);
    }

    public void DisableRightHand()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnRightHandChanged(false);
    }

    public void EnableLeftHand()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnLeftHandChanged(true);
    }

    public void DisableLeftHand()
    {
        foreach (MIDISongListener m_songListener in m_songListeners)
            m_songListener.OnLeftHandChanged(false);
    }

    public void SetSpeedFactor(float speedFactor)
    {
        m_speedFactor = speedFactor;
    }

    public bool IsPlaying()
    {
        return m_isPlaying;
    }

    public int GetCurrentMeasure()
    {
        return m_currentMeasure;
    }
    
    public MIDISong GetSong()
    {
        return m_song;
    }

    public float getElapsedTime()
    {
        return m_elapsedTime;
    }
}
