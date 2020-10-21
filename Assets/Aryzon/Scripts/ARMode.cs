using Aryzon;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ARMode : MonoBehaviour {
    public GameObject scanningForMarker;

    #if UNITY_IPHONE
    [DllImport ("__Internal")]
    private static extern void setBrightnessToValue (float value);
    [DllImport ("__Internal")]
    private static extern void setBrightnessToHighest ();
    [DllImport ("__Internal")]
    private static extern float getBrightness ();
    #endif

    public bool autoStartARMode = false;

    public bool autoRotate = false;
    public bool autoRotateInEditorMode = true;

    public bool rotateOnAspectRatio = false;
    private bool landscapeMode = true;

    public Camera cameraUI;
    public GameObject cameraLeft;
    public GameObject cameraRight;
    public GameObject nonARMenu;
    public GameObject ARMenu;
    public Transform cameras;

    public ARCameraShifter shifter;

    public string OnExitLoadScene = "";

    public List<GameObject> activateWithARMode = new List<GameObject> ();
    public List<GameObject> deactivateWithARMode = new List<GameObject> ();

    public UnityEvent started3DMode;
    public UnityEvent exited3DMode;

    private bool arMode;
    private bool rotated;
    private CameraClearFlags clearFlags;

    public bool exitOnRotate = false;

    #if UNITY_IPHONE
    private float timer;
    private float resetBrightness = 1f;
    #endif

    void Start () {
        landscapeMode = true;
        if (autoRotateInEditorMode && Application.isEditor) {
            autoRotate = true;
        }

        restart ();
    }

    public void restart () {
        rotated = false;

        #if UNITY_IPHONE
        timer = 0f;
        #endif

        arMode = false;
        SetActiveCamsAndMenus (false);

        if (cameraUI) {
            clearFlags = cameraUI.clearFlags;
        } else {
            Debug.Log ("[Aryzon] UI camera not connected on ARCameraMode script, please fix.");
        }

        foreach (GameObject obj in deactivateWithARMode) {
            if (obj) {
                obj.SetActive (false);
            }
        }

        if (autoStartARMode) {
            StartARMode ();
        }

    }

    IEnumerator InvokeAfterSecond () {
        yield return new WaitForSeconds (3f);
        started3DMode.Invoke ();
    }

    void Update () {

        if (Application.platform == RuntimePlatform.Android) {
            if (Input.GetKey (KeyCode.Escape)) {
                Exit ();
                return;
            }
        }

        #if UNITY_IPHONE
        if (rotated && arMode && Application.platform == RuntimePlatform.IPhonePlayer && Screen.orientation == ScreenOrientation.LandscapeLeft) {
            if (timer > 5f) {
                setBrightnessToHighest();
                timer = 0f;
            }
            timer += Time.deltaTime;
        }
        #endif

        if (!rotated && arMode && !autoRotate) {
            if (Screen.orientation == ScreenOrientation.LandscapeLeft && AryzonSettings.Headset.didAcceptDisclaimer) {
                //Did rotate phone to start 3D mode
                if (cameraUI) {
                    cameraUI.clearFlags = CameraClearFlags.Depth;
                }

                SetActiveCamsAndMenus (true);

                started3DMode.Invoke ();
                StartCoroutine(InvokeAfterSecond());


                #if UNITY_ANDROID
                                if (Application.platform == RuntimePlatform.Android) {
                    AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
                    AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");

                    ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
                        using (
                            AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
                            windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
                            layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
                        ) {
                            layoutParams.Set<float>("screenBrightness",1f);
                            windowInstance.Call("setAttributes", new AndroidJavaObject[1] {layoutParams});
                        }
                    }));
                }
                #elif UNITY_IPHONE
                if (Application.platform == RuntimePlatform.IPhonePlayer) {
                    resetBrightness = getBrightness ();
                    setBrightnessToHighest();
                }
                #endif

                rotated = true;

                shifter.UpdateLayout ();
            }

        } else if (rotated && arMode && !autoRotate) {

            if (Screen.orientation == ScreenOrientation.Portrait) {
                if (exitOnRotate) {
                    exitOnRotate = false;
                    ExitARMode ();
                } else {
                    if (cameraUI) {
                        cameraUI.clearFlags = clearFlags;
                    }

                    SetActiveCamsAndMenus (false);

                    foreach (GameObject obj in activateWithARMode) {
                        if (obj) {
                            obj.SetActive (false);
                        }
                    }

                    #if UNITY_ANDROID
                                        if (Application.platform == RuntimePlatform.Android) {
                    AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
                    AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");

                    ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
                    using (
                    AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
                    windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
                    layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
                    ) {
                    layoutParams.Set<float>("screenBrightness",-1f);
                    windowInstance.Call("setAttributes", new AndroidJavaObject[1] {layoutParams});
                    }
                    }));
                    }
                    #elif UNITY_IPHONE
                    setBrightnessToValue(resetBrightness);
                    #endif
                    rotated = false;
                    exited3DMode.Invoke ();
                }
            }
        }
    }

    public void StartARMode () {
        if (!arMode) {

            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToPortrait = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

            Application.targetFrameRate = 60;

            if (autoRotate) {
                started3DMode.Invoke ();
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToPortrait = false;
                Screen.orientation = ScreenOrientation.LandscapeLeft;

                if (cameraUI) {
                    cameraUI.clearFlags = CameraClearFlags.Depth;
                }

                SetActiveCamsAndMenus (true);

                #if UNITY_ANDROID
                                if (Application.platform == RuntimePlatform.Android) {
                    AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
                    AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");

                    ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
                        using (
                            AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
                            windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
                            layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
                        ) {
                            layoutParams.Set<float>("screenBrightness",1f);
                            windowInstance.Call("setAttributes", new AndroidJavaObject[1] {layoutParams});
                        }
                    }));
                }
                #elif UNITY_IPHONE
                if (Application.platform == RuntimePlatform.IPhonePlayer) {
                    setBrightnessToHighest();
                }
                #endif

                rotated = true;
                shifter.UpdateLayout ();
            }

            arMode = true;


        } else {
            Debug.Log ("[Aryzon] AR mode already active, will do nothing");
        }
    }

    public void StartARModeWithScene (string sceneName) {
        StartARMode ();
    }

    public void ExitARMode () {
        if (arMode) {
            Application.targetFrameRate = 60;

            if (cameraUI) {
                cameraUI.clearFlags = clearFlags;
            }

            SetActiveCamsAndMenus (false);

            foreach (GameObject obj in activateWithARMode) {
                if (obj) {
                    obj.SetActive (false);
                }
            }

            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.orientation = ScreenOrientation.Portrait;

            arMode = false;

            #if UNITY_ANDROID
                        if (Application.platform == RuntimePlatform.Android) {
                AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
                AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");

                ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
                    using (
                        AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
                        windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
                        layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
                    ) {
                        layoutParams.Set<float>("screenBrightness",-1f);
                        windowInstance.Call("setAttributes", new AndroidJavaObject[1] {layoutParams});
                    }
                }));
            }

            #endif

        } else {
            Debug.Log ("AR mode already inactive, will do nothing");
        }
    }

    private void SetActiveCamsAndMenus (bool active) {
        Debug.Log ("Setting Cams to " + active.ToString());
        if (cameraRight && cameraLeft) {
            cameraRight.SetActive (active);
            cameraLeft.SetActive (active);
        } else {
            Debug.Log ("Left and or Right camera not connected on ARCameraMode script, please fix.");
        }

        if (nonARMenu && ARMenu) {
            nonARMenu.SetActive (!active);
            ARMenu.SetActive (active);
        } else {
            Debug.Log ("NonARMenu and or ARMenu not connected on ARCameraMode script, please fix.");
        }
    }

    public string GetAndroidExternalStoragePath ()
    {
        #if UNITY_ANDROID
                if (Application.platform == RuntimePlatform.Android) {
            string path = "";
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment") ;
                path = jc.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getAbsolutePath");
                return path;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        #endif
        return "";
    }

    public void Exit ()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if (OnExitLoadScene == "" || OnExitLoadScene == null) {
            Debug.Log ("Exit scene not set loading scene at index 0. You can set the name of the scene to load on exit on the ARMode component of the Aryzon GameObject.");
            SceneManager.LoadScene (0);
        } else {
            SceneManager.LoadSceneAsync(OnExitLoadScene);
        }
    }

    public void CheckAspectRatio () {
        if (rotateOnAspectRatio) {
            bool landscape = false;
            if (Screen.width > Screen.height) {
                landscape = true;
            }
            if (!landscapeMode && landscape) {
                Debug.Log ("[Aryzon] Setting landscape mode");
                landscapeMode = true;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                autoRotate = false;
                rotated = false;
                if (cameraUI) {
                    cameraUI.clearFlags = CameraClearFlags.Depth;
                }

                SetActiveCamsAndMenus (true);

                StartCoroutine(InvokeAfterSecond());

                shifter.UpdateLayout ();
                //StartARMode ();
            } else if (landscapeMode && !landscape) {
                Debug.Log ("[Aryzon] Setting portrait mode");
                Screen.orientation = ScreenOrientation.Portrait;
                landscapeMode = false;

                if (exitOnRotate) {
                    exitOnRotate = false;
                    ExitARMode ();
                    exited3DMode.Invoke ();
                } else {
                    if (cameraUI) {
                        cameraUI.clearFlags = clearFlags;
                    }

                    SetActiveCamsAndMenus (false);

                    foreach (GameObject obj in activateWithARMode) {
                        if (obj) {
                            obj.SetActive (false);
                        }
                    }

                    rotated = false;
                }
            }
        }
    }
}
