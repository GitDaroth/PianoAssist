using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARStopButton : ARButton
{
    public GameObject m_startPauseButton;

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
        ARStartPauseButton arStartPauseButton = m_startPauseButton.GetComponent<ARStartPauseButton>();
        arStartPauseButton.m_defaultMaterial = arStartPauseButton.m_startDefaultMaterial;
        arStartPauseButton.m_hoveredMaterial = arStartPauseButton.m_startHoveredMaterial;
        arStartPauseButton.SetIsChecked(false);

        m_midiLogicController.GetComponent<MIDILogicController>().OnStop();
    }
}