using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSpeedButton : ARButton
{
    public float speedFactor = 1.0f;

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
        for(int i = 0; i < transform.parent.childCount; i++)
        {
            if(transform.parent.GetChild(i).name != name)
                transform.parent.GetChild(i).gameObject.GetComponent<ARSpeedButton>().SetIsChecked(false);
        }

        if (!m_isChecked)
            SetIsChecked(true);
        m_midiLogicController.GetComponent<MIDILogicController>().OnSpeedFactorChanged(speedFactor);
    }
}
