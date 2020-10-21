using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARHitStreakProgressBar : MonoBehaviour
{
    private List<int> m_hitStreakLevels;
    public GameObject m_multiplierLabel;
    public Image m_hitStreakBar;
    public Image m_hitStreakBackground;

    void Start()
    {
    }

    void Update()
    {   
    }

    public void Hide()
    {
        this.enabled = false;
        m_hitStreakBar.enabled = false;
        m_hitStreakBackground.enabled = false;
        m_multiplierLabel.SetActive(false);
    }

    public void Show()
    {
        this.enabled = true;
        m_hitStreakBar.enabled = true;
        m_hitStreakBackground.enabled = true;
        m_multiplierLabel.SetActive(true);
    }

    public void SetHitStreakLevels(List<int> hitStreakLevels)
    {
        m_hitStreakLevels = hitStreakLevels;
    }

    public void SetHitStreak(int hitStreak)
    {
        Color startColor = new Color(1.0f, 1.0f, 0.0f);
        Color endColor = new Color(0.0f, 1.0f, 1.0f);
        Color defaultBackgroundColor = new Color(0.443f, 0.4745f, 0.502f);
        int multiplier = 0;
        for(int i = 0; i < m_hitStreakLevels.Count; i++)
        {
            if(hitStreak < m_hitStreakLevels[i])
            {
                multiplier = i + 1;
                if(i > 0)
                {
                    m_hitStreakBar.fillAmount = (hitStreak - m_hitStreakLevels[i -1]) / (float)(m_hitStreakLevels[i] - m_hitStreakLevels[i - 1]);
                    float s = m_hitStreakLevels[i - 1] / (float)m_hitStreakLevels[m_hitStreakLevels.Count - 1];
                    m_hitStreakBackground.color = ((1.0f - s) * startColor + s * endColor) * 0.5f;
                }
                else
                {
                    m_hitStreakBar.fillAmount = hitStreak / (float)m_hitStreakLevels[i];
                    m_hitStreakBackground.color = defaultBackgroundColor;
                }
                break;
            }
            else if(i == m_hitStreakLevels.Count - 1)
            {
                multiplier = m_hitStreakLevels.Count + 1;
                m_hitStreakBar.fillAmount = 1.0f;
            }
        }

        float t = hitStreak / (float)m_hitStreakLevels[m_hitStreakLevels.Count - 1];
        m_hitStreakBar.color = (1.0f - t) * startColor + t * endColor;
        m_multiplierLabel.GetComponent<TextMesh>().text = multiplier.ToString() + "x";
    }
}
