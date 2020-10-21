using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARSongProgressBar : MonoBehaviour
{
    public Image m_songBar;

    private float m_songDuration = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetSongDuration(float songDuration)
    {
        m_songDuration = songDuration;
    }

    public void SetSongElapsedTime(float elapsedTime)
    {
        elapsedTime = Mathf.Max(elapsedTime, 0.0f);
        m_songBar.fillAmount = Mathf.Min(elapsedTime / m_songDuration, 1.0f);
    }
}
