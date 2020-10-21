using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Aryzon
{
    public class AryzonTracking : MonoBehaviour 
    {
		#if UNITY_IPHONE
		[DllImport ("__Internal")]
		private static extern void setBrightnessToValue (float value);
		[DllImport ("__Internal")]
		private static extern void setBrightnessToHighest ();
		[DllImport ("__Internal")]
		private static extern float getBrightness ();
		private float resetBrightness = 1f;
		#endif

        public bool useARCore;
        public bool useARKit;
        public bool useVuforia;
        public bool useOther;

        private Reticle reticle;
        private FadeOnEnableAndDisable reticleFader;
        private bool reticleEnabled;

        public Transform arCoreTransform;
        public Transform arKitTransform;
        public Transform vuforiaTransform;
        public Transform otherTransform;

        public AryzonUIController aryzonUIController;

		public bool setAryzonModeOnStart;
		public bool aryzonMode;
		public bool stereoscopicMode;

		public Units sceneUnits;
		public float customUnits = 1f;

		public TrackingEngine trackingEngine;

		[Serializable]
		public class ButtonClickedEvent : UnityEvent {}

		public ButtonClickedEvent onClick = new ButtonClickedEvent();

		public UnityEvent onARCoreStart;
		public UnityEvent onARCoreStop;
        public UnityEvent onARCoreSet;
		public UnityEvent onARKitStart;
		public UnityEvent onARKitStop;
        public UnityEvent onARKitSet;
		public UnityEvent onVuforiaStart;
		public UnityEvent onVuforiaStop;
        public UnityEvent onVuforiaSet;
		public UnityEvent onOtherStart;
		public UnityEvent onOtherStop;
        public UnityEvent onOtherSet;
		public UnityEvent onStart;
		public UnityEvent onStop;
		public UnityEvent onRotationToLandscape;
		public UnityEvent onRotationToPortrait;

		public bool showARCoreEvents;
		public bool showARKitEvents;
		public bool showVuforiaEvents;
		public bool showOtherEvents;
		public bool showAryzonModeEvents;
		public bool showRotationEvents;

		public bool startTrackingBool;
		public bool stopTrackingBool;

        public bool showReticle;

		private TrackingEngine currentTrackingEngine;
        private Transform currentTrackingTransform;
        
		private bool objectsConnected = false;
        
		private Camera cameraLeft;
		private Camera cameraRight;

		private GameObject cameras;
		private GameObject uiOverlay;
		private GameObject portraitUI;
	    private GameObject landscapeUI;
		private Transform stereoscopic;

		public ARCameraShifter cameraShifter;

		private UnityAction arkitStart;

		private bool checkingOrientation;
		private bool landscape;
		private bool landscapeMode;

		private bool autorotateToLandscapeRight;
		private bool autorotateToLandscapeLeft;
		private bool autorotateToPortrait;
		private bool autorotateToPortraitUpsideDown;
		private ScreenOrientation orientation;

        private bool doCheckOnAspect;

        private float timer;

		private void OnEnable() {
            AryzonSettings.Instance.UpdateLayout += UpdateLayoutFromEvent;
		}

        private void OnDisable() {
            if (AryzonSettings.Instance != null) {
                AryzonSettings.Instance.UpdateLayout -= UpdateLayoutFromEvent;
            }
        }

        private void UpdateLayoutFromEvent () {
            Debug.Log ("[Aryzon] Updating view layout");
            cameraShifter.UpdateLayout ();
        }

		private void ConnectObjects () {

			if (!objectsConnected) {
				Transform uiOverlayTransform = transform.Find ("UI Overlay");
				Transform portraitUITransform = transform.Find ("UI Overlay/Portrait");
				Transform landscapeUITransform = transform.Find ("UI Overlay/Landscape");

				stereoscopic = transform.Find ("Stereoscopic");
				Transform camerasTransform = transform.Find ("Stereoscopic/Cameras");
				Transform cameraLeftTransform = transform.Find ("Stereoscopic/Cameras/Left");
				Transform cameraRightTransform = transform.Find ("Stereoscopic/Cameras/Right");

                Transform reticleTransform = transform.Find ("Stereoscopic/Cameras/Right/Reticle/Dot");

				if (uiOverlayTransform) {
					uiOverlay = uiOverlayTransform.gameObject;
				}
				if (portraitUITransform) {
					portraitUI = portraitUITransform.gameObject;
				}
				if (landscapeUITransform) {
					landscapeUI = landscapeUITransform.gameObject;
				}
				if (camerasTransform) {
					cameras = camerasTransform.gameObject;
				}
				if (cameraLeftTransform) {
					cameraLeft = cameraLeftTransform.gameObject.GetComponent<Camera> ();
				}
				if (cameraRightTransform) {
					cameraRight = cameraRightTransform.gameObject.GetComponent<Camera> ();
				}

                reticle = cameraRightTransform.gameObject.GetComponentInChildren<Reticle> ();
                reticleFader = reticleTransform.gameObject.GetComponent<FadeOnEnableAndDisable>();
                aryzonUIController = portraitUITransform.gameObject.GetComponentInChildren<AryzonUIController>();

                reticleEnabled = false;

                objectsConnected = true;
			}
		}

        private void Awake () {
			ConnectObjects ();

			aryzonMode = false;
			landscapeMode = false;
			stereoscopicMode = false;

            showReticle = false;

			cameraShifter = new ARCameraShifter ();
			cameraShifter.singleMode = false;
			cameraShifter.cameras = cameras;
			cameraShifter.left = cameraLeft;
			cameraShifter.right = cameraRight;

			if (sceneUnits == Units.Meters) {
				cameraShifter.sceneUnitsPerMeter = 1;
			} else if (sceneUnits == Units.Centimeters) {
				cameraShifter.sceneUnitsPerMeter = 100f;
			} else if (sceneUnits == Units.Millimeters) {
				cameraShifter.sceneUnitsPerMeter = 1000f;
			} else if (sceneUnits == Units.Inches) {
				cameraShifter.sceneUnitsPerMeter = 39.37f;
			} else if (sceneUnits == Units.Custom) {
				cameraShifter.sceneUnitsPerMeter = customUnits;
			}

			cameraShifter.Setup ();
        }

		void Start () {
			if (setAryzonModeOnStart) {
				StartAryzonMode ();
			}
		}

		private void AddCameraEvent () {
			Debug.Log ("AddCameraEvent");
			MethodInfo method = typeof(UnityEngine.Camera).GetMethod("set_enabled");
			if (method != null) {
				Action<bool> action = System.Delegate.CreateDelegate (typeof(Action<bool>), arKitTransform.gameObject.GetComponent<Camera>(), method) as Action<bool>;

				if (action == null) {
					Debug.Log ("Action == null");
				}
				arkitStart = delegate {
					arKitTransform.gameObject.GetComponent<Camera>().enabled = false;
				};
				onARKitStart.AddListener (arkitStart);

			} else {
				Debug.Log ("Method == null");
			}
		}

		void Update () {
            if (aryzonMode && landscapeMode && Application.platform == RuntimePlatform.IPhonePlayer) {
                if (timer > 5f) {
                    SetScreenBrightness(1f);
                    timer = 0f;
                }
                timer += Time.deltaTime;
            }

			#if UNITY_EDITOR
			if (startTrackingBool) {
				startTrackingBool = false;
				StartAryzonMode ();
			}
			if (stopTrackingBool) {
				stopTrackingBool = false;
				StopAryzonMode ();
			}
			#endif

            if (doCheckOnAspect) {
                doCheckOnAspect = false;
                CheckAspectRatio ();
                StartCoroutine(CheckAspectAfter(0.5f));
            }
		}

        IEnumerator CheckAspectAfter(float seconds) {
            yield return new WaitForSeconds(seconds);
            CheckAspectRatio ();
        }

        public void DoCheckOnAspect () {
            doCheckOnAspect = true;
        }

        private void LateUpdate () {
			if (trackingEngine != currentTrackingEngine) {
				SetTrackingEngine (trackingEngine);
			}
			if (aryzonMode && currentTrackingTransform) {
				stereoscopic.transform.position = currentTrackingTransform.position;
				stereoscopic.transform.rotation = currentTrackingTransform.rotation;
            }
            //Debug.Log("showReticleCount: " + showReticleCount);
            if (!reticleEnabled && showReticle) {
                reticle.ReticleTransform.gameObject.SetActive(true);
                reticleFader.Enable();
                reticleEnabled = true;
                Debug.Log("Enabling reticle");
            } else if (reticleEnabled && !showReticle) {
                reticleFader.Disable();
                reticleEnabled = false;
                Debug.Log("Disabling reticle");
            }
            showReticle = false;
        }

        public void SetTrackingEngine(TrackingEngine engine) {
            trackingEngine = engine;
			if (currentTrackingEngine != trackingEngine) {
                if (currentTrackingEngine == TrackingEngine.ARCore) {
                    onARCoreStop.Invoke();
                } else if (currentTrackingEngine == TrackingEngine.ARKit) {
                    onARKitStop.Invoke();
                } else if (currentTrackingEngine == TrackingEngine.Vuforia) {
                    onVuforiaStop.Invoke();
                } else if (currentTrackingEngine == TrackingEngine.Other) {
                    onOtherStop.Invoke();
                }

                if (engine == TrackingEngine.ARCore) {
					SetARCoreTracking ();
                    onARCoreSet.Invoke();
				} else if (engine == TrackingEngine.ARKit) {
					SetARKitTracking ();
                    onARKitSet.Invoke();
				} else if (engine == TrackingEngine.Vuforia) {
					SetVuforiaTracking ();
                    onVuforiaSet.Invoke();
				} else if (engine == TrackingEngine.Other) {
					SetOtherTracking ();
                    onOtherSet.Invoke();
				} else {
					currentTrackingTransform = null;
					currentTrackingEngine = TrackingEngine.None;
				}
			}
        }

        public void StartAryzonMode () {
            if (!aryzonMode) {
                AryzonSettings.Instance.AryzonMode = true;
                AryzonSettings.Instance.reticleCamera = cameraRight;
                AryzonSettings.Instance.aryzonTracking = this;

                if (!Application.isEditor) {
                    SaveRotationParameters();
                    SetRotationLandscapeLeftAndPortrait();
                }
				aryzonMode = true;
				ConnectObjects ();
				uiOverlay.SetActive (true);
				if (trackingEngine == TrackingEngine.ARCore) {
					onARCoreStart.Invoke ();
				} else if (trackingEngine == TrackingEngine.ARKit) {
					onARKitStart.Invoke ();
				} else if (trackingEngine == TrackingEngine.Vuforia) {
					onVuforiaStart.Invoke ();
				} else if (trackingEngine == TrackingEngine.Other) {
					onOtherStart.Invoke ();
				}
				/*if (Screen.width > Screen.height) {
					landscapeMode = true;
				}*/

				CheckAspectRatio ();

				onStart.Invoke ();
			}
        }

        public void StopAryzonMode () {
			if (aryzonMode) {
                AryzonSettings.Instance.AryzonMode = false;
				uiOverlay.SetActive (false);
				if (trackingEngine == TrackingEngine.ARCore) {
					onARCoreStop.Invoke ();
				} else if (trackingEngine == TrackingEngine.ARKit) {
					onARKitStop.Invoke ();
				} else if (trackingEngine == TrackingEngine.Vuforia) {
					onVuforiaStop.Invoke ();
				} else if (trackingEngine == TrackingEngine.Other) {
					onOtherStop.Invoke ();
				}
				if (landscape) {
					ResetScreenBrightness ();
				}

				stereoscopic.gameObject.SetActive (false);
                if (!Application.isEditor) {
                    ResetRotationParameters ();
                }
				onStop.Invoke ();

				aryzonMode = false;
			}
        }

		public void EnterStereoscopicMode () {
			portraitUI.SetActive (false);
			landscapeUI.SetActive (true);
		}

		public void ExitStereoscopicMode () {
			portraitUI.SetActive (true);
			landscapeUI.SetActive (false);
		}

        private void SetARCoreTracking () {
			if (useARCore) {
				currentTrackingTransform = arCoreTransform;
				trackingEngine = TrackingEngine.ARCore;
				currentTrackingEngine = trackingEngine;
                if (aryzonMode)
                {
                    onARCoreStart.Invoke();
                }
			} else {
				trackingEngine = currentTrackingEngine;
			}
        }

        private void SetARKitTracking () {
			if (useARKit) {
				currentTrackingTransform = arKitTransform;
				trackingEngine = TrackingEngine.ARKit;
				currentTrackingEngine = trackingEngine;
                if (aryzonMode)
                {
                    onARKitStart.Invoke();
                }

			} else {
				trackingEngine = currentTrackingEngine;
			}
        }

        private void SetVuforiaTracking () {
			if (useVuforia) {
				currentTrackingTransform = vuforiaTransform;
				trackingEngine = TrackingEngine.Vuforia;
				currentTrackingEngine = trackingEngine;
                if (aryzonMode)
                {
                    onVuforiaStart.Invoke();
                }
			} else {
				trackingEngine = currentTrackingEngine;
			}
        }

        private void SetOtherTracking () {
			if (useOther) {
				currentTrackingTransform = otherTransform;
				trackingEngine = TrackingEngine.Other;
				currentTrackingEngine = trackingEngine;
                if (aryzonMode)
                {
                    onOtherStart.Invoke();
                }
			} else {
				trackingEngine = currentTrackingEngine;
			}
        }

		private void SaveRotationParameters () {
			autorotateToLandscapeLeft = Screen.autorotateToLandscapeLeft;
			autorotateToLandscapeRight = Screen.autorotateToLandscapeRight;
			autorotateToPortrait = Screen.autorotateToPortrait;
			autorotateToPortraitUpsideDown = Screen.autorotateToPortraitUpsideDown;
			orientation = Screen.orientation;
		}

		private void ResetRotationParameters () {
			Screen.autorotateToLandscapeLeft = autorotateToLandscapeLeft;
			Screen.autorotateToLandscapeRight = autorotateToLandscapeRight;
			Screen.autorotateToPortrait = autorotateToPortrait;
			Screen.autorotateToPortraitUpsideDown = autorotateToPortraitUpsideDown;
			Screen.orientation = orientation;
		}

		private void SetRotationLandscapeLeftAndPortrait () {
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToPortrait = true;
			Screen.orientation = ScreenOrientation.AutoRotation;
		}

		private void SetScreenBrightness (float value) {
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
				AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
				ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
					using (
					AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
					windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
					layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
					) {
						layoutParams.Set<float>("screenBrightness",value);
						windowInstance.Call("setAttributes", new AndroidJavaObject[1] {layoutParams});
					}
				}));
				#endif
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
#if UNITY_IPHONE
                resetBrightness = getBrightness ();
				setBrightnessToValue (value);
#endif
			}
		}

		private void ResetScreenBrightness () {
			if (Application.platform == RuntimePlatform.Android) {
#if UNITY_ANDROID
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
#endif
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
#if UNITY_IPHONE
                setBrightnessToValue (resetBrightness);
#endif
			}
		}

		public void CheckAspectRatio () {
			if (!checkingOrientation && this.gameObject.activeInHierarchy && aryzonMode) {
				checkingOrientation = true;

				landscape = false;
				if (Screen.width > Screen.height) {
					landscape = true;
				}
				if (landscape && !landscapeMode) {
					Debug.Log ("[Aryzon] Setting landscape mode");
                    SetScreenBrightness (1f);
                    timer = 0f;
					stereoscopic.gameObject.SetActive (true);
                    foreach (FadeOutAndDisable fader in portraitUI.GetComponentsInChildren<FadeOutAndDisable>()) {
                        fader.Disable();
                    }
                    aryzonUIController.Inactivate();
					landscapeUI.SetActive (true);
					StartCoroutine (InvokeARModeAfter(1.1f));
					StartCoroutine (InvokeARModeAfter(1.2f));
                    AryzonSettings.Instance.LandscapeMode = true;
					onRotationToLandscape.Invoke ();
					landscapeMode = true;
				} else if (!landscape && landscapeMode) {
					Debug.Log ("[Aryzon] Setting portrait mode");
					ResetScreenBrightness ();
					stereoscopic.gameObject.SetActive (false);
					//portraitUI.SetActive (true);
                    foreach (FadeOutAndDisable fader in portraitUI.GetComponentsInChildren<FadeOutAndDisable>(true)) {
                        fader.fading = false;
                        fader.gameObject.SetActive(false);
                        fader.gameObject.SetActive(true);
                    }
                    aryzonUIController.Activate();
					landscapeUI.SetActive (false);
                    AryzonSettings.Instance.PortraitMode = true;
					onRotationToPortrait.Invoke ();
					landscapeMode = false;
				}
				StartCoroutine (CheckOrientationAfter(0.5f));
			}
		}


		IEnumerator InvokeARModeAfter (float seconds) {
			yield return new WaitForSeconds (seconds);
			cameraShifter.UpdateLayout ();
		}
		IEnumerator CheckOrientationAfter (float seconds) {
			yield return new WaitForSeconds (seconds);
			checkingOrientation = false;
		}

        private void OnApplicationQuit() {
            
        }
    }

    public enum TrackingEngine {
		None,
        ARCore,
        ARKit,
        Vuforia,
        Other
    }

	public enum Units {
		Meters,
		Centimeters,
		Millimeters,
		Inches,
		Custom
	}
}
