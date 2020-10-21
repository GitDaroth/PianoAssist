public interface MIDISongListener
{
    void OnSongChanged(MIDISong song);
    void OnSongEvent(MIDIEvent midiEvent, int trackNumber);
    void OnUpdate(float deltaTime);
    void OnRestart();
    void OnSeek(float timestamp);
    void OnRightHandChanged(bool isEnabled);
    void OnLeftHandChanged(bool isEnabled);
}