using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MIDILogicController : MonoBehaviour
{
    public TextAsset m_midiFile;
    public GameObject m_songVisualizer;
    public GameObject m_midiDevice;
    public GameObject m_measureLabel;
    public GameObject m_scoreLabel;
    public GameObject m_accuracyLabel;
    public Canvas m_hitStreakProgressBar;
    public Canvas m_songProgressBar;

    private MIDISongPlayer m_songPlayer;
    private MIDISongPracticeMode m_songPracticeMode;
    private MIDISongPerformanceFeedback m_songPerformanceFeedback;

    void Start()
    {
        MIDISong song = MIDIFileReader.Read(m_midiFile);

        m_songPlayer = new MIDISongPlayer();
        m_songPlayer.SetSong(song);
        m_songPlayer.SetSpeedFactor(1.0f);

        m_songPlayer.RegisterSongListener(m_songVisualizer.GetComponent<MIDISongVisualizer>());

        m_songPracticeMode = new MIDISongPracticeMode(m_songPlayer);
        m_songPlayer.RegisterSongListener(m_songPracticeMode);
        m_midiDevice.GetComponent<MIDIDevice>().RegisterDeviceListener(m_songPracticeMode);
        m_songPracticeMode.Disable();

        m_songPerformanceFeedback = new MIDISongPerformanceFeedback(m_songVisualizer.GetComponent<MIDISongVisualizer>());
        m_songPlayer.RegisterSongListener(m_songPerformanceFeedback);
        m_midiDevice.GetComponent<MIDIDevice>().RegisterDeviceListener(m_songPerformanceFeedback);
    }

    void Update()
    {
        m_songPlayer.Update(Time.deltaTime);
        m_measureLabel.GetComponent<TextMesh>().text = m_songPlayer.GetCurrentMeasure().ToString();
       
        int correctNoteCount = m_songPerformanceFeedback.GetCorrectNoteCount();
        int totalNoteCount = m_songPerformanceFeedback.GetTotalNoteCount();
        int accuracy = 100;
        if (totalNoteCount != 0)
            accuracy = (100 * correctNoteCount) / totalNoteCount;

        m_accuracyLabel.GetComponent<TextMesh>().text = "Accuracy: " + correctNoteCount + "/" + totalNoteCount + " (" + accuracy + "%)";
        m_scoreLabel.GetComponent<TextMesh>().text = "Score: " + m_songPerformanceFeedback.GetScore().ToString();
        m_hitStreakProgressBar.GetComponent<ARHitStreakProgressBar>().SetHitStreakLevels(m_songPerformanceFeedback.GetHitStreakLevels());
        m_hitStreakProgressBar.GetComponent<ARHitStreakProgressBar>().SetHitStreak(m_songPerformanceFeedback.GetHitStreak());

        m_songProgressBar.GetComponent<ARSongProgressBar>().SetSongDuration(m_songPlayer.GetSong().duration);
        m_songProgressBar.GetComponent<ARSongProgressBar>().SetSongElapsedTime(m_songPlayer.getElapsedTime());
    }

    public void OnSpeedFactorChanged(float speedFactor)
    {
        m_songPlayer.SetSpeedFactor(speedFactor);
    }

    public void OnStartPause()
    {
        if (m_songPlayer.IsPlaying())
            m_songPlayer.Pause();
        else
            m_songPlayer.Start();
    }

    public void OnStop()
    {
        m_songPlayer.Stop();
    }

    public void OnRestart()
    {
        m_songPlayer.Restart();
    }

    public void OnSeek(float timestamp)
    {
        m_songPlayer.Seek(timestamp);
    }

    public void OnForward()
    {
        m_songPlayer.Seek(m_songPlayer.GetCurrentMeasure() + 1);
    }

    public void OnBackward()
    {
        m_songPlayer.Seek(m_songPlayer.GetCurrentMeasure() - 1);
    }

    public void OnFastForward()
    {
        m_songPlayer.Seek(m_songPlayer.GetCurrentMeasure() + 10);
    }

    public void OnFastBackward()
    {
        m_songPlayer.Seek(m_songPlayer.GetCurrentMeasure() - 10);
    }

    public void OnEnableLeftHand(bool isEnabled)
    {
        if(isEnabled)
            m_songPlayer.EnableLeftHand();
        else
            m_songPlayer.DisableLeftHand();
    }

    public void OnEnableRightHand(bool isEnabled)
    {
        if (isEnabled)
            m_songPlayer.EnableRightHand();
        else
            m_songPlayer.DisableRightHand();
    }

    public void OnEnableLearningMode(bool isEnabled)
    {
        if (isEnabled)
        {
            m_songPracticeMode.Enable();
            //m_songPerformanceFeedback.Disable();
            m_accuracyLabel.SetActive(false);
            m_scoreLabel.SetActive(false);
            m_hitStreakProgressBar.GetComponent<ARHitStreakProgressBar>().Hide();
        }
        else
        {
            m_songPracticeMode.Disable();
            //m_songPerformanceFeedback.Enable();
            m_accuracyLabel.SetActive(true);
            m_scoreLabel.SetActive(true);
            m_hitStreakProgressBar.GetComponent<ARHitStreakProgressBar>().Show();
        }
    }
}
