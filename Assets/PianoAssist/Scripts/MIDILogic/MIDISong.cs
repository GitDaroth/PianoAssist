using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDISong
{
    public int numberOfTracks;
    public int standardMIDIFileFormat;
    public int ticksPerQuarterNote;
    public float duration;
    public float firstMeasureDuration;
    public List<MIDITrack> tracks;
    public List<float> measureTimestamps;
}

public class MIDITrack
{
    public List<MIDIEvent> events;
}

public abstract class MIDIEvent
{
    public int deltaTicks;
    public float timestamp;
}

public abstract class MIDIChannelEvent : MIDIEvent
{
    public int channel;
}

public class MIDINoteEvent : MIDIChannelEvent
{
    public bool isNoteOn;
    public int note;
    public int velocity;
}

public abstract class MIDIMetaEvent : MIDIEvent
{
    public int dataLength;
}

public class MIDIStringEvent : MIDIMetaEvent
{
    public string text;
}

public class MIDITimeSignatureEvent : MIDIMetaEvent
{
    public int numerator;
    public int denominator;
    public int ticksPerMetronomeClick;
    public int numberOf32ndNotesPerBeat;
}

public class MIDIKeySignatureEvent : MIDIMetaEvent
{
    public int key;
    public bool isMajor;
}

public class MIDITempoEvent : MIDIMetaEvent
{
    public int tempo;
}

class MIDIEventComparer : IComparer<MIDIEvent>
{
    public int Compare(MIDIEvent x, MIDIEvent y)
    {

        if (x == null || y == null)
            return 0;

        return x.timestamp.CompareTo(y.timestamp);
    }
}