using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDISongVisualizer : MonoBehaviour, MIDISongListener
{
    public Material m_glowMaterial;
    public Material m_depthMaskMaterial;
    public GameObject m_particleEffect;

    private MIDISong m_song;
    private RingBuffer<GameObject> m_noteObjectsRingBuffer;
    private RingBuffer<LineRenderer> m_measureLinesRingBuffer;
    private Dictionary<int, GameObject> m_activeParticleEffects;

    private float m_elapsedTime = 0.0f;
    private float m_defaultSpeed = 0.1f;

    private float m_noteWidth = 0.825f / 36.0f;
    private float m_noteHeight = 0.002f;

    private float m_keyboardOffset = -32.0f;
    private int m_octavePitch = -1;

    private float m_fadeOutStart = 0.25f;
    private float m_fadeOutEnd = 1.0f;

    private int m_mostLeftNote = 14;
    private int m_mostRightNote = 50;

    private bool m_isRightHandEnabled = true;
    private bool m_isLeftHandEnabled = true;

    void Awake()
    {
        m_activeParticleEffects = new Dictionary<int, GameObject>();

        m_noteObjectsRingBuffer = new RingBuffer<GameObject>();
        for (uint i = 0; i < 500; i++)
        {
            GameObject noteObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noteObject.transform.SetParent(transform, false);
            noteObject.GetComponent<MeshRenderer>().material = m_glowMaterial;
            noteObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("PianoAssist/NoteShader");
            noteObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutStart", m_fadeOutStart);
            noteObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutEnd", m_fadeOutEnd);
            noteObject.GetComponent<MeshRenderer>().material.renderQueue = 2500;
            m_noteObjectsRingBuffer.AddElement(noteObject);
        }

        m_measureLinesRingBuffer = new RingBuffer<LineRenderer>();
        for (uint i = 0; i < 10; i++)
        {
            GameObject measureLineObject = new GameObject();
            LineRenderer measureLine = measureLineObject.AddComponent<LineRenderer>();
            measureLine.transform.SetParent(transform, false);
            measureLine.material = new Material(Shader.Find("PianoAssist/LineShader"));
            measureLine.material.SetFloat("_FadeOutStart", m_fadeOutStart);
            measureLine.material.SetFloat("_FadeOutEnd", m_fadeOutEnd);
            measureLine.material.renderQueue = 2501;
            measureLine.material.color = Color.yellow;
            measureLine.startWidth = 0.001f;
            measureLine.endWidth = 0.001f;
            measureLine.positionCount = 2;
            measureLine.useWorldSpace = false;
            measureLine.SetPosition(0, new Vector3(0.0f, 0.0f, -1.0f));
            measureLine.SetPosition(1, new Vector3(1.0f, 0.0f, -1.0f));
            m_measureLinesRingBuffer.AddElement(measureLine);
        }

        for (int i = 24; i <= 84; i++) // C1 to C6
        {
            if((i % 12 == 0) || ((i - 5) % 12 == 0))
            {
                GameObject noteLineObject = new GameObject();
                LineRenderer noteLine = noteLineObject.AddComponent<LineRenderer>();
                noteLine.transform.SetParent(transform, false);
                noteLine.material = new Material(Shader.Find("PianoAssist/LineShader"));
                noteLine.material.SetFloat("_FadeOutStart", m_fadeOutStart);
                noteLine.material.SetFloat("_FadeOutEnd", m_fadeOutEnd);
                noteLine.material.color = Color.yellow;
                noteLine.material.renderQueue = 2501;
                noteLine.startWidth = 0.001f;
                noteLine.endWidth = 0.001f;
                noteLine.positionCount = 2;
                noteLine.useWorldSpace = false;
                noteLine.SetPosition(0, new Vector3(GetNoteOffset(i) * m_noteWidth, 0.0f - 0.059f, 0.0f));
                noteLine.SetPosition(1, new Vector3(GetNoteOffset(i) * m_noteWidth, 0.0f - 0.059f, m_fadeOutEnd));
            }        
        }

        GameObject whiteKeysObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        whiteKeysObject.transform.SetParent(transform, false);
        whiteKeysObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("PianoAssist/LineShader");
        whiteKeysObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutStart", 0.0f);
        whiteKeysObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutEnd", 0.17f);
        whiteKeysObject.GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
        whiteKeysObject.GetComponent<MeshRenderer>().material.renderQueue = 2503;

        float startX, startY, startZ, endX, endY, endZ;
        startZ = 0.0f;
        endZ = 0.17f;
        startY = -0.005f - 0.059f;
        endY = 0.0f - 0.059f;
        startX = (m_mostLeftNote + m_keyboardOffset) * m_noteWidth;
        endX = (m_mostRightNote + m_keyboardOffset) * m_noteWidth;

        whiteKeysObject.transform.localPosition = new Vector3((startX + endX) * 0.5f, (startY + endY) * 0.5f, (startZ + endZ) * 0.5f);
        whiteKeysObject.transform.localScale = new Vector3(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY), Mathf.Abs(endZ - startZ));

        for (int note = 24; note <= 84; note++) // C1 to C6
        {
            if (IsBlackNote(note))
            {
                GameObject blackKeyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blackKeyObject.transform.SetParent(transform, false);
                blackKeyObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("PianoAssist/LineShader");
                blackKeyObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutStart", 0.0f);
                blackKeyObject.GetComponent<MeshRenderer>().material.SetFloat("_FadeOutEnd", 0.17f);
                blackKeyObject.GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
                blackKeyObject.GetComponent<MeshRenderer>().material.renderQueue = 2502;

                float offset = GetNoteOffset(note);
                startZ = 0.0f;
                endZ = 0.17f;
                startY = 0.0f - 0.059f;
                endY = 0.012f - 0.059f;
                startX = offset * m_noteWidth + 0.001f;
                endX = (offset + 0.55f) * m_noteWidth - 0.001f;

                blackKeyObject.transform.localPosition = new Vector3((startX + endX) * 0.5f, (startY + endY) * 0.5f, (startZ + endZ) * 0.5f);
                blackKeyObject.transform.localScale = new Vector3(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY), Mathf.Abs(endZ - startZ));


                GameObject blackKeyDepthMaskObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blackKeyDepthMaskObject.transform.SetParent(transform, false);
                blackKeyDepthMaskObject.GetComponent<MeshRenderer>().material = m_depthMaskMaterial;

                startZ = 0.0f;
                endZ = -0.1f;
                startY = 0.0f - 0.059f;
                endY = 0.012f - 0.059f;
                startX = offset * m_noteWidth + 0.001f;
                endX = (offset + 0.55f) * m_noteWidth - 0.001f;

                blackKeyDepthMaskObject.transform.localPosition = new Vector3((startX + endX) * 0.5f, (startY + endY) * 0.5f, (startZ + endZ) * 0.5f);
                blackKeyDepthMaskObject.transform.localScale = new Vector3(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY), Mathf.Abs(endZ - startZ));
            }
        }
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void Restart()
    {
        if (m_song == null)
            return;

        m_elapsedTime = -m_song.firstMeasureDuration;
        ShowObjectsAtTimestamp(m_elapsedTime);
    }

    void MIDISongListener.OnSongChanged(MIDISong song)
    {
        if (song == null)
            return;

        m_song = song;

        Restart();
    }

    void MIDISongListener.OnSongEvent(MIDIEvent midiEvent, int trackNumber)
    {
    }

    void MIDISongListener.OnUpdate(float deltaTime)
    {
        if (m_song == null)
            return;

        m_elapsedTime += deltaTime;
        ShowObjectsAtTimestamp(m_elapsedTime);
    }

    void MIDISongListener.OnRestart()
    {
        Restart();
    }

    void MIDISongListener.OnSeek(float timestamp)
    {
        if (m_song == null)
            return;

        m_elapsedTime = timestamp;
        ShowObjectsAtTimestamp(timestamp);
    }

    void MIDISongListener.OnRightHandChanged(bool isEnabled)
    {
        m_isRightHandEnabled = isEnabled;

        if(m_song != null)
            ShowObjectsAtTimestamp(m_elapsedTime);
    }

    void MIDISongListener.OnLeftHandChanged(bool isEnabled)
    {
        m_isLeftHandEnabled = isEnabled;

        if (m_song != null)
            ShowObjectsAtTimestamp(m_elapsedTime);
    }

    public void StartNoteEffect(int note)
    {
        float offset = GetNoteOffset(note, m_octavePitch);

        Vector3 position;
        position.x = 0.0f;
        position.y = 0.0f;
        position.z = 0.0f;
        if (IsBlackNote(note))
        {
            position.x = offset * m_noteWidth + m_noteWidth * 0.3f;
            position.y = 0.017f - 0.059f;
        }
        else
        {
            position.x = offset * m_noteWidth + m_noteWidth * 0.5f;
            position.y = 0.005f - 0.059f;
        }

        StopNoteEffect(note);

        GameObject particleEffect = Instantiate(m_particleEffect, transform);
        particleEffect.transform.localPosition = position;
        particleEffect.GetComponent<ParticleSystem>().Play();
        m_activeParticleEffects.Add(note, particleEffect);
    }

    public void StopNoteEffect(int note)
    {
        if(m_activeParticleEffects.ContainsKey(note))
        {
            m_activeParticleEffects[note].GetComponent<ParticleSystem>().Stop();
            m_activeParticleEffects.Remove(note);
        }
    }

    public void ChangeNoteEffectColor(int note, Color color)
    {
        if (m_activeParticleEffects.ContainsKey(note))
        {
            ParticleSystem.MainModule particleSystemMainModule = m_activeParticleEffects[note].GetComponent<ParticleSystem>().main;
            particleSystemMainModule.startColor = color;
        }
    }

    private void ShowObjectsAtTimestamp(float timestamp)
    {
        float startTimeOfVisibleRegion = timestamp;
        float endTimeOfVisibleRegion = startTimeOfVisibleRegion + m_fadeOutEnd / m_defaultSpeed;

        ShowNotesInVisibleRegion(startTimeOfVisibleRegion, endTimeOfVisibleRegion);
        ShowMeasureLinesInVisibleRegion(startTimeOfVisibleRegion, endTimeOfVisibleRegion);
    }

    private void ShowNotesInVisibleRegion(float startTimeOfVisibleRegion, float endTimeOfVisibleRegion)
    {
        for (int i = 0; i < m_noteObjectsRingBuffer.GetSize(); i++)
        {
            GameObject noteObject = m_noteObjectsRingBuffer.GetCurrentElement();
            m_noteObjectsRingBuffer.Advance();
            noteObject.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);
            noteObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        List<MIDINoteEvent> pressedNoteEvents = new List<MIDINoteEvent>();
        for (int i = 0; i < m_song.tracks.Count; i++)
        {
            if (!m_isRightHandEnabled && i == 0)
                continue;

            if (!m_isLeftHandEnabled && i == 1)
                continue;

            MIDITrack track = m_song.tracks[i];
            foreach (MIDIEvent midiEvent in track.events)
            {
                if (midiEvent.GetType() == typeof(MIDINoteEvent))
                {
                    MIDINoteEvent noteEvent = midiEvent as MIDINoteEvent;

                    if (noteEvent.isNoteOn &&
                        noteEvent.timestamp >= startTimeOfVisibleRegion - 5.0f &&
                        noteEvent.timestamp <= endTimeOfVisibleRegion + 5.0f)
                    {
                        pressedNoteEvents.Add(noteEvent);
                    }
                    else if (!noteEvent.isNoteOn &&
                             noteEvent.timestamp >= startTimeOfVisibleRegion - 5.0f &&
                             noteEvent.timestamp <= endTimeOfVisibleRegion + 5.0f)
                    {
                        MIDINoteEvent releasedNoteEvent = noteEvent;
                        foreach (MIDINoteEvent pressedNoteEvent in pressedNoteEvents)
                        {
                            if (pressedNoteEvent.note == releasedNoteEvent.note)
                            {
                                if (releasedNoteEvent.timestamp < pressedNoteEvent.timestamp)
                                    break;

                                pressedNoteEvents.Remove(pressedNoteEvent);

                                GameObject noteObject = m_noteObjectsRingBuffer.GetCurrentElement();
                                m_noteObjectsRingBuffer.Advance();

                                float startX, startY, startZ, endX, endY, endZ;

                                startZ = (pressedNoteEvent.timestamp - startTimeOfVisibleRegion) * m_defaultSpeed;
                                endZ = (releasedNoteEvent.timestamp - startTimeOfVisibleRegion) * m_defaultSpeed;

                                float offset = GetNoteOffset(noteEvent.note, m_octavePitch);
                                if (IsBlackNote(noteEvent.note))
                                {
                                    startX = offset * m_noteWidth + 0.0015f;
                                    endX = (offset + 0.5f) * m_noteWidth - 0.0015f;

                                    startY = 0.0f - 0.059f;
                                    endY = 0.0121f - 0.059f;
                                }
                                else
                                {
                                    startX = offset * m_noteWidth;
                                    endX = (offset + 1.0f) * m_noteWidth;

                                    startY = 0.0f - 0.059f;
                                    endY = m_noteHeight - 0.059f;
                                }

                                noteObject.transform.localPosition = new Vector3((startX + endX) * 0.5f, (startY + endY) * 0.5f, (startZ + endZ) * 0.5f);
                                noteObject.transform.localScale = new Vector3(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY), Mathf.Abs(endZ - startZ));

                                // determine vertex colors
                                Color color = Color.HSVToRGB((float)i / (float)m_song.tracks.Count, 1.0f, 1.0f);
                                if (IsBlackNote(noteEvent.note))
                                    color *= 0.5f;

                                Mesh mesh = noteObject.GetComponent<MeshFilter>().mesh;
                                Vector3[] vertPositions = mesh.vertices;
                                Color[] vertColors = new Color[vertPositions.Length];
                                for (int j = 0; j < vertPositions.Length; j++)
                                {
                                    float t = vertPositions[j].z + 0.5f;
                                    vertColors[j] = Color.Lerp(color, color * 0.5f, t);
                                }
                                mesh.colors = vertColors;

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void ShowMeasureLinesInVisibleRegion(float startTimeOfVisibleRegion, float endTimeOfVisibleRegion)
    {
        for (int i = 0; i < m_measureLinesRingBuffer.GetSize(); i++)
        {
            LineRenderer measureLine = m_measureLinesRingBuffer.GetCurrentElement();
            m_measureLinesRingBuffer.Advance();
            measureLine.SetPosition(0, new Vector3(0.0f, 0.0f, -1.0f));
            measureLine.SetPosition(1, new Vector3(1.0f, 0.0f, -1.0f));   
        }

        foreach (float measureTimestamp in m_song.measureTimestamps)
        {
            if (measureTimestamp >= startTimeOfVisibleRegion &&
                measureTimestamp <= endTimeOfVisibleRegion)
            {
                LineRenderer measureLine = m_measureLinesRingBuffer.GetCurrentElement();
                m_measureLinesRingBuffer.Advance();
                Vector3 startPosition = new Vector3((m_mostLeftNote + m_keyboardOffset) * m_noteWidth, 0.0f - 0.059f, (measureTimestamp - startTimeOfVisibleRegion) * m_defaultSpeed);
                Vector3 endPosition = new Vector3((m_mostRightNote + m_keyboardOffset) * m_noteWidth, 0.0f - 0.059f, (measureTimestamp - startTimeOfVisibleRegion) * m_defaultSpeed);
                measureLine.SetPosition(0, startPosition);
                measureLine.SetPosition(1, endPosition);
            }
        }
    }

    private bool IsBlackNote(int note)
    {
        int key = note % 12;
        if (key == 1 || key == 3 || key == 6 || key == 8 || key == 10)
            return true;
        return false;
    }

    private float GetNoteOffset(int note, int octavePitch = 0)
    {
        int octave = note / 12 + octavePitch;
        int key = note % 12;

        float offset = octave * 7.0f + m_keyboardOffset;
        switch (key)
        {
            case 0: //C
                offset += 0.0f;
                break;
            case 2: //D
                offset += 1.0f;
                break;
            case 4: //E
                offset += 2.0f;
                break;
            case 5: //F
                offset += 3.0f;
                break;
            case 7: //G
                offset += 4.0f;
                break;
            case 9: //A
                offset += 5.0f;
                break;
            case 11: //B
                offset += 6.0f;
                break;

            case 1: //C#
                offset += 0.6f;
                break;
            case 3: //D#
                offset += 1.85f;
                break;
            case 6: //F#
                offset += 3.6f;
                break;
            case 8: //G#
                offset += 4.7f;
                break;
            case 10: //A#
                offset += 5.85f;
                break;
        }

        return offset;
    }
}
