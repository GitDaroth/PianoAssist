using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ARButton : MonoBehaviour
{
    public GameObject m_midiLogicController;
    public bool m_isCheckable;
    public bool m_isChecked = false;
    public Material m_defaultMaterial;
    public Material m_hoveredMaterial;
    public Material m_checkedMaterial;
    public Material m_checkedHoveredMaterial;
    public int m_triggerNote = 96;

    protected bool m_isHovered = false;

    void Start()
    {
        UpdateMaterial();
        OnStart();
    }

    void Update()
    {
        OnUpdate();
    }

    public void Enter()
    {
        m_isHovered = true;
        UpdateMaterial();

        OnEnter();
    }

    public void Exit()
    {
        m_isHovered = false;
        UpdateMaterial();

        OnExit();
    }

    public void Click(int note)
    {
        if(note == m_triggerNote)
        {
            if (m_isCheckable)
                SetIsChecked(!m_isChecked);

            OnClick(note);
        }     
    }

    public void SetIsChecked(bool isChecked)
    {
        m_isChecked = isChecked;
        UpdateMaterial();
    }

    protected void UpdateMaterial()
    {
        if (m_isHovered)
        {
            if (m_isChecked)
                gameObject.GetComponent<MeshRenderer>().material = m_checkedHoveredMaterial;
            else
                gameObject.GetComponent<MeshRenderer>().material = m_hoveredMaterial;
        }
        else
        {
            if (m_isChecked)
                gameObject.GetComponent<MeshRenderer>().material = m_checkedMaterial;
            else
                gameObject.GetComponent<MeshRenderer>().material = m_defaultMaterial;
        }
    }

    protected abstract void OnStart();

    protected abstract void OnUpdate();

    protected abstract void OnEnter();

    protected abstract void OnExit();

    protected abstract void OnClick(int note);
}