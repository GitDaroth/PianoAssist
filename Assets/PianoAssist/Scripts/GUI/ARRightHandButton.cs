﻿public class ARRightHandButton : ARButton
{
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
        m_midiLogicController.GetComponent<MIDILogicController>().OnEnableRightHand(m_isChecked);
    }
}
