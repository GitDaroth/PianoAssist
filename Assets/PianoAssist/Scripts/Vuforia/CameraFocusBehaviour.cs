using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CameraFocusBehaviour : MonoBehaviour
{
    void Start()
    {
        var vuforiaController = VuforiaARController.Instance;
        vuforiaController.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        vuforiaController.RegisterOnPauseCallback(OnPaused);
    }
    private void OnVuforiaStarted()
    {
        Application.targetFrameRate = 60;
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }
}