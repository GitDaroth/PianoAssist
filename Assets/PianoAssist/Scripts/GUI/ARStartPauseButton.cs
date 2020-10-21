using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARStartPauseButton : ARButton
{
    public Material m_startDefaultMaterial;
    public Material m_startHoveredMaterial;
    public Material m_continueDefaultMaterial;
    public Material m_continueHoveredMaterial;

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }

    protected override void OnEnter()
    {
        
    }

    protected override void OnExit()
    {
        
    }

    protected override void OnClick(int note)
    {
        m_defaultMaterial = m_continueDefaultMaterial;
        m_hoveredMaterial = m_continueHoveredMaterial;
        UpdateMaterial();

        m_midiLogicController.GetComponent<MIDILogicController>().OnStartPause();
    }
}
