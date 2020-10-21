using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MIDIFileReader
{
    static private byte[] m_midiData;
    static private uint m_index;
    static private MIDISong m_song;
    static private int m_status;
    static private int m_prevStatus;
    static private float m_defaultMicrosecondsPerQuarterNote = 500000.0f; // 120 beats per minute

    static public MIDISong Read(TextAsset midiFile)
    {
        m_song = new MIDISong();
        m_midiData = midiFile.bytes;
        m_index = 0;

        ReadHeader();
        ReadTracks();
        CalcEventTimestamps();
        CalcMeasureTimestamps();

        return m_song;
    }

    static private void ReadHeader()
    {
        string headerChunkType = ReadString(4);
        if (!headerChunkType.Equals("MThd"))
            throw new System.FormatException("No header chunk type has been found.");

        int headerChunkLength = ReadInt32();
        if (headerChunkLength != 6)
            throw new System.FormatException("Incorrect header chunk length. It has to be always 6.");

        m_song.standardMIDIFileFormat = ReadInt16();
        m_song.numberOfTracks = ReadInt16();

        // check if first bit in time division is set
        // first bit = 0 -> metric (ticks per quarter note)
        // first bit = 1 -> time-code-based (number of frames per second SMPTE time and number of beats per frame)
        int timeDivision = ReadInt16();
        if ((timeDivision & 0b1000000000000000) != 0)
            throw new System.FormatException("SMPTE time division is not supported");

        m_song.ticksPerQuarterNote = timeDivision;
    }

    static private void ReadTracks()
    {
        m_song.tracks = new List<MIDITrack>();
        for (uint i = 0; i < m_song.numberOfTracks; i++)
            ReadTrack();
    }

    static private void ReadTrack()
    {
        MIDITrack track = new MIDITrack();
        track.events = new List<MIDIEvent>();

        string trackChunkType = ReadString(4);
        if (!trackChunkType.Equals("MTrk"))
            throw new System.FormatException("No track chunk type has been found.");

        int trackChunkLength = ReadInt32();

        uint trackChunkEnd = m_index + (uint)trackChunkLength;
        int accumulatedDeltaTicks = 0;
        m_prevStatus = 0;
        while (m_index < trackChunkEnd)
        {
            int deltaTicks = ReadVariableLengthInt();
            accumulatedDeltaTicks += deltaTicks;
            m_status = ReadByte();

            MIDIEvent midiEvent = ReadEvent(accumulatedDeltaTicks);
            if (midiEvent != null)
            {
                track.events.Add(midiEvent);
                accumulatedDeltaTicks = 0;
            }
        }

        m_song.tracks.Add(track);
    }

    static private MIDIEvent ReadEvent(int deltaTicks)
    {
        MIDIEvent midiEvent = null;

        if (IsSysExEvent())
            DiscardSysExEvent();
        else if (IsMetaEvent())
            midiEvent = ReadMetaEvent(deltaTicks);
        else
        {
            // Handle running status 
            // (the status byte is omitted if the current event is supposed to have the same status as the last event)
            if ((m_status & 0b10000000) == 0)
            {
                m_status = m_prevStatus;
                m_index--;
            }

            midiEvent = ReadChannelEvent(deltaTicks);

            m_prevStatus = m_status;
        }

        return midiEvent;
    }

    static private bool IsSysExEvent()
    {
        return (m_status == (int)MIDISysExEventType.Normal) || (m_status == (int)MIDISysExEventType.Divided);
    }

    static private bool IsMetaEvent()
    {
        return m_status == 0xFF;
    }

    static private MIDIMetaEvent ReadMetaEvent(int deltaTicks)
    {
        int metaEventType = ReadByte();
        int dataLength = ReadVariableLengthInt();

        switch(metaEventType)
        {
            case (int)MIDIMetaEventType.Tempo:
                MIDITempoEvent tempoEvent = new MIDITempoEvent();
                tempoEvent.deltaTicks = deltaTicks;
                tempoEvent.tempo = ReadMultiByteInt((uint)dataLength);
                tempoEvent.dataLength = dataLength;
                return tempoEvent;

            case (int)MIDIMetaEventType.Time_Signature:
                MIDITimeSignatureEvent timeSignatureEvent = new MIDITimeSignatureEvent();
                timeSignatureEvent.deltaTicks = deltaTicks;
                timeSignatureEvent.numerator = ReadByte();
                timeSignatureEvent.denominator = (int)Mathf.Pow(2, ReadByte());
                timeSignatureEvent.ticksPerMetronomeClick = ReadByte();
                timeSignatureEvent.numberOf32ndNotesPerBeat = ReadByte();
                timeSignatureEvent.dataLength = dataLength;
                return timeSignatureEvent;

            case (int)MIDIMetaEventType.Key_Signature:
                MIDIKeySignatureEvent keySignatureEvent = new MIDIKeySignatureEvent();
                keySignatureEvent.deltaTicks = deltaTicks;
                keySignatureEvent.key = ReadByte();
                if (ReadByte() == 0)
                    keySignatureEvent.isMajor = true;
                else
                    keySignatureEvent.isMajor = false;
                keySignatureEvent.dataLength = dataLength;
                return keySignatureEvent;

            default:
                ReadBytes((uint)dataLength);
                return null;
        }
    }

    static private MIDIChannelEvent ReadChannelEvent(int deltaTicks)
    {
        int channel = (m_status & 0b00001111); // last 4 bits of status byte
        int channelEventType = (m_status & 0b11110000) >> 4; // first 4 bits of status byte

        byte dataByte1 = ReadByte();
        byte dataByte2 = 0;
        if ((channelEventType != (int)MIDIChannelEventType.Program_Change) &&
            (channelEventType != (int)MIDIChannelEventType.Channel_Pressure_Or_Aftertouch))
        {
            dataByte2 = ReadByte();
        }

        MIDINoteEvent noteEvent = null;
        switch (channelEventType)
        {
            case (int)MIDIChannelEventType.Note_On:
                noteEvent = new MIDINoteEvent();
                noteEvent.deltaTicks = deltaTicks;
                int velocity = dataByte2;
                if (velocity == 0)
                    noteEvent.isNoteOn = false; // Note On with zero velocity means Note Off
                else
                    noteEvent.isNoteOn = true;
                noteEvent.channel = channel;
                noteEvent.note = dataByte1;
                noteEvent.velocity = velocity;
                break;

            case (int)MIDIChannelEventType.Note_Off:
                noteEvent = new MIDINoteEvent();
                noteEvent.deltaTicks = deltaTicks;
                noteEvent.isNoteOn = false;
                noteEvent.channel = channel;
                noteEvent.note = dataByte1;
                noteEvent.velocity = dataByte2;
                break;
        }
        return noteEvent;
    }

    static private void DiscardSysExEvent()
    {
        int dataLength = ReadVariableLengthInt();
        byte[] data = ReadBytes((uint)dataLength);
    }

    static private void CalcEventTimestamps()
    {
        float elapsedTime = 0.0f;
        float secondsPerTick = CalcSecondsPerTick(m_defaultMicrosecondsPerQuarterNote);

        List<MIDIEvent> nextEventList = new List<MIDIEvent>();
        foreach (MIDITrack track in m_song.tracks)
            nextEventList.Add(track.events[0]);

        MIDIEvent minNextEvent;
        do
        {
            minNextEvent = FindMinNextEvent(nextEventList);

            if(minNextEvent != null)
            {
                elapsedTime += secondsPerTick * minNextEvent.deltaTicks;
                minNextEvent.timestamp = elapsedTime;

                CorrectDeltaTicksOfNextEvents(minNextEvent, nextEventList);
                ForwardMinNextEvent(minNextEvent, nextEventList);

                // adjust tempo if respective event occurs
                if (minNextEvent.GetType() == typeof(MIDITempoEvent))
                {
                    MIDITempoEvent tempoEvent = minNextEvent as MIDITempoEvent;
                    secondsPerTick = CalcSecondsPerTick(tempoEvent.tempo);
                }
            }
        }
        while (minNextEvent != null);

        m_song.duration = elapsedTime;
    }

    static private float CalcSecondsPerTick(float microsecondsPerQuarterNote)
    {
        return (microsecondsPerQuarterNote / (float)m_song.ticksPerQuarterNote) / 1000000.0f;
    }

    static private MIDIEvent FindMinNextEvent(List<MIDIEvent> nextEventList)
    {
        int minDeltaTicks = 99999999;
        MIDIEvent minNextEvent = null;
        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            MIDIEvent nextEvent = nextEventList[i];
            if (nextEvent == null)
                continue;

            if (nextEvent.deltaTicks < minDeltaTicks)
            {
                minDeltaTicks = nextEvent.deltaTicks;
                minNextEvent = nextEvent;
            }
        }
        return minNextEvent;
    }

    static private void ForwardMinNextEvent(MIDIEvent minNextEvent, List<MIDIEvent> nextEventList)
    {
        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            MIDITrack track = m_song.tracks[i];
            int nextEventIndex = track.events.IndexOf(minNextEvent);
            if (nextEventIndex == -1)
                continue;

            nextEventList[i] = null;
            nextEventIndex++;
            if (nextEventIndex < track.events.Count)
                nextEventList[i] = track.events[nextEventIndex];
        }
    }

    static private void CorrectDeltaTicksOfNextEvents(MIDIEvent minNextEvent, List<MIDIEvent> nextEventList)
    {
        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            MIDIEvent nextEvent = nextEventList[i];
            if (nextEvent == null || nextEvent == minNextEvent)
                continue;

            nextEvent.deltaTicks -= minNextEvent.deltaTicks;
        }
    }            

    static private void CalcMeasureTimestamps()
    {
        m_song.measureTimestamps = new List<float>();

        // GetMetaEvents
        List<MIDIEvent> metaEvents = new List<MIDIEvent>();
        foreach (MIDITrack track in m_song.tracks)
            foreach (MIDIEvent midiEvent in track.events)
                if (midiEvent.GetType().IsSubclassOf(typeof(MIDIMetaEvent)))
                    metaEvents.Add(midiEvent);

        // Sort MetaEvents by Timestamp 
        metaEvents.Sort(new MIDIEventComparer());

        // calc timing metrics
        float microsecondsPerQuarterNote = m_defaultMicrosecondsPerQuarterNote;
        float beatsPerMeasure = 4.0f;
        float beatNoteLength = 1.0f / 4.0f;
        float quarterNotesPerBeat = 4.0f * beatNoteLength;
        float secondsPerMeasure = CalcSecondsPerMeasure(microsecondsPerQuarterNote, quarterNotesPerBeat, beatsPerMeasure);

        // iterate through measures
        for (float elapsedTime = 0.0f; elapsedTime < m_song.duration + secondsPerMeasure; elapsedTime += secondsPerMeasure)
        {
            List<MIDIEvent> metaEventsToRemove = new List<MIDIEvent>();
            foreach (MIDIEvent metaEvent in metaEvents)
            {
                if (metaEvent.timestamp <= elapsedTime)
                {
                    if (metaEvent.GetType() == typeof(MIDITempoEvent))
                    {
                        MIDITempoEvent tempoEvent = metaEvent as MIDITempoEvent;
                        microsecondsPerQuarterNote = tempoEvent.tempo;
                        secondsPerMeasure = CalcSecondsPerMeasure(microsecondsPerQuarterNote, quarterNotesPerBeat, beatsPerMeasure);
                    }
                    else if (metaEvent.GetType() == typeof(MIDITimeSignatureEvent))
                    {
                        MIDITimeSignatureEvent timeSignatureEvent = metaEvent as MIDITimeSignatureEvent;
                        beatsPerMeasure = timeSignatureEvent.numerator;
                        beatNoteLength = 1.0f / timeSignatureEvent.denominator;
                        quarterNotesPerBeat = 4.0f * beatNoteLength;
                        secondsPerMeasure = CalcSecondsPerMeasure(microsecondsPerQuarterNote, quarterNotesPerBeat, beatsPerMeasure);
                    }
                    metaEventsToRemove.Add(metaEvent);
                }
            }
            foreach (MIDIEvent metaEventToRemove in metaEventsToRemove)
                metaEvents.Remove(metaEventToRemove);

            m_song.measureTimestamps.Add(elapsedTime);
        }

        m_song.firstMeasureDuration = m_song.measureTimestamps[1] - m_song.measureTimestamps[0];
    }

    static private float CalcSecondsPerMeasure(float microsecondsPerQuarterNote, float quarterNotesPerBeat, float beatsPerMeasure)
    {
        float secondsPerBeat = (quarterNotesPerBeat * microsecondsPerQuarterNote) / 1000000.0f;
        return beatsPerMeasure * secondsPerBeat;
    }

    static private byte ReadByte(bool advanceForward = true)
    {
        byte byteReadIn = m_midiData[m_index];
        if (advanceForward)
            m_index++;
        return byteReadIn;
    }

    static private byte[] ReadBytes(uint byteCount, bool advanceForward = true)
    {
        byte[] bytesReadIn = new byte[byteCount];
        for (uint i = 0; i < byteCount; i++)
            bytesReadIn[i] = ReadByte();

        if (!advanceForward)
            m_index -= byteCount;

        return bytesReadIn;
    }

    static private string ReadString(uint charCount, bool advanceForward = true)
    {
        string stringReadIn = "";
        for (uint i = 0; i < charCount; i++)
            stringReadIn += ((char)ReadByte()).ToString();

        if (!advanceForward)
            m_index -= charCount;

        return stringReadIn;
    }

    static private int ReadInt16(bool advanceForward = true)
    {
        return ReadMultiByteInt(2, advanceForward);
    }

    static private int ReadInt32(bool advanceForward = true)
    {
        return ReadMultiByteInt(4, advanceForward);
    }

    static private int ReadMultiByteInt(uint byteCount, bool advanceForward = true)
    {
        int intReadIn = 0;

        for(uint i = 0; i < byteCount; i++)
            intReadIn += (ReadByte() << ((int)(byteCount - 1 - i) * 8));

        if (!advanceForward)
            m_index -= byteCount;

        return intReadIn;
    }

    static private int ReadVariableLengthInt()
    {
        int value = 0;
        bool foundLastByteOfValue = false;
        while(!foundLastByteOfValue)
        {
            int byteReadIn = ReadByte();
            value = value << 7; // left-shift by 7 bits since the 7 bits of the next byte will be placed on the right
            value += byteReadIn & 0b01111111; // Remove the first bit for the value since it determines whether to read another byte or not.
            // Check if the first bit is 0 which means that no more byte needs to be read.
            if ((byteReadIn & 0b10000000) == 0)
                foundLastByteOfValue = true;                
        }
        return value;
    }

    static private string ConvertByteToHex(int byteNumber)
    {
        int lsb = byteNumber & 0b00001111;
        int msb = (byteNumber & 0b11110000) >> 4;

        return Convert4BitsToHex(msb).ToString() + Convert4BitsToHex(lsb).ToString();
    }

    static private char Convert4BitsToHex(int bits)
    {
        switch (bits)
        {
            case 0:
                return '0';
            case 1:
                return '1';
            case 2:
                return '2';
            case 3:
                return '3';
            case 4:
                return '4';
            case 5:
                return '5';
            case 6:
                return '6';
            case 7:
                return '7';
            case 8:
                return '8';
            case 9:
                return '9';
            case 10:
                return 'A';
            case 11:
                return 'B';
            case 12:
                return 'C';
            case 13:
                return 'D';
            case 14:
                return 'E';
            case 15:
                return 'F';
            default:
                return ' ';
        }
    }
}

enum MIDIMetaEventType : int
{
    Sequence_Number = 0x00,
    Text = 0x01,
    Copyright_Notice = 0x02,
    Track_Name = 0x03,
    Instrument_Name = 0x04,
    Lyric = 0x05,
    Marker = 0x06,
    Cue_Point = 0x07,
    Channel_Number = 0x20,
    Port_Number = 0x21,
    End_Of_Track = 0x2F,
    Tempo = 0x51,
    SMPTE_Offset = 0x54,
    Time_Signature = 0x58,
    Key_Signature = 0x59,
    Sequencer_Specific = 0x7F
}

enum MIDISysExEventType : int
{
    Normal = 0xF0,
    Divided = 0xF7
}

enum MIDIChannelEventType : int
{
    Note_Off = 0x8,
    Note_On = 0x9,
    Key_Pressure_Or_Aftertouch = 0xA,
    Extended = 0xB,
    Program_Change = 0xC,
    Channel_Pressure_Or_Aftertouch = 0xD,
    Pitch_Bend = 0xE
}

enum MIDIExtendedChannelEventType : int
{
    Bank_Select = 0x00,
    Modulation_Wheel = 0x01,
    Breath_Controller = 0x02,
    Foot_Controller = 0x04,
    Portamento_Time = 0x05,
    Data_Entry_Slider_MSB = 0x06,
    Main_Volume = 0x07,
    Balance = 0x08,
    Pan = 0x0A,
    Expression_Controller = 0x0B,
    Effect_Control_1 = 0x0C,
    Effect_Control_2 = 0x0D,
    General_Purpose_Controllers_1 = 0x10,
    General_Purpose_Controllers_2 = 0x11,
    General_Purpose_Controllers_3 = 0x12,
    General_Purpose_Controllers_4 = 0x13,
    Controller_0_LSB = 0x20,
    Controller_1_LSB = 0x21,
    Controller_2_LSB = 0x22,
    Controller_3_LSB = 0x23,
    Controller_4_LSB = 0x24,
    Controller_5_LSB = 0x25,
    Controller_6_LSB = 0x26,
    Controller_7_LSB = 0x27,
    Controller_8_LSB = 0x28,
    Controller_9_LSB = 0x29,
    Controller_10_LSB = 0x2A,
    Controller_11_LSB = 0x2B,
    Controller_12_LSB = 0x2C,
    Controller_13_LSB = 0x2D,
    Controller_14_LSB = 0x2E,
    Controller_15_LSB = 0x2F,
    Controller_16_LSB = 0x30,
    Controller_17_LSB = 0x31,
    Controller_18_LSB = 0x32,
    Controller_19_LSB = 0x33,
    Controller_20_LSB = 0x34,
    Controller_21_LSB = 0x35,
    Controller_22_LSB = 0x36,
    Controller_23_LSB = 0x37,
    Controller_24_LSB = 0x38,
    Controller_25_LSB = 0x39,
    Controller_26_LSB = 0x3A,
    Controller_27_LSB = 0x3B,
    Controller_28_LSB = 0x3C,
    Controller_29_LSB = 0x3D,
    Controller_30_LSB = 0x3E,
    Controller_31_LSB = 0x3F,
    Sustain_Pedal = 0x40,
    Portamento = 0x41,
    Sostenato_Pedal = 0x42,
    Soft_Pedal = 0x43,
    Legato_Footswitch = 0x44,
    Hold_2 = 0x45,
    Sound_Controller_1 = 0x46,
    Sound_Controller_2 = 0x47,
    Sound_Controller_3 = 0x48,
    Sound_Controller_4 = 0x49,
    Sound_Controller_5 = 0x4A,
    Sound_Controller_6 = 0x4B,
    Sound_Controller_7 = 0x4C,
    Sound_Controller_8 = 0x4D,
    Sound_Controller_9 = 0x4E,
    Sound_Controller_10 = 0x4F,
    General_Purpose_Controllers_5 = 0x50,
    General_Purpose_Controllers_6 = 0x51,
    General_Purpose_Controllers_7 = 0x52,
    General_Purpose_Controllers_8 = 0x53,
    Portamento_Control = 0x54,
    Effect_1_Depth = 0x5B,
    Effect_2_Depth = 0x5C,
    Effect_3_Depth = 0x5D,
    Effect_4_Depth = 0x5E,
    Effect_5_Depth = 0x5F,
    Data_Increment_1 = 0x60,
    Data_Increment_2 = 0x61,
    Non_Registered_Parameter_Number_LSB = 0x62,
    Non_Registered_Parameter_Number_MSB = 0x63,
    Registered_Parameter_Number_LSB = 0x64,
    Registered_Parameter_Number_MSB = 0x65,
    Mode_Messages_1 = 0x79,
    Mode_Messages_2 = 0x7A,
    Mode_Messages_3 = 0x7B,
    Mode_Messages_4 = 0x7C,
    Mode_Messages_5 = 0x7D,
    Mode_Messages_6 = 0x7E,
    Mode_Messages_7 = 0x7F,
}

