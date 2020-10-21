using Aryzon;

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Events;

using UnityEditor;
using UnityEditor.Events;


[CanEditMultipleObjects]
[CustomEditor(typeof(AryzonTracking))]
public class AryzonEditor : Editor 
{
	AryzonTracking aryzonTracking;

	SerializedProperty trackingEngineProp;

    SerializedProperty useARCoreProp;
    SerializedProperty useARKitProp;
    SerializedProperty useVuforiaProp;
    SerializedProperty useOtherProp;

    SerializedProperty arCoreTransformProp;
    SerializedProperty arKitTransformProp;
    SerializedProperty vuforiaTransformProp;
    SerializedProperty otherTransformProp;

	SerializedProperty setAryzonModeOnStartProp;

	SerializedProperty sceneUnitsProp;
	SerializedProperty customUnitsProp;

	SerializedProperty onARCoreStartProp;
	SerializedProperty onARCoreStopProp;
    SerializedProperty onARCoreSetProp;
	SerializedProperty onARKitStartProp;
	SerializedProperty onARKitStopProp;
    SerializedProperty onARKitSetProp;
	SerializedProperty onVuforiaStartProp;
	SerializedProperty onVuforiaStopProp;
    SerializedProperty onVuforiaSetProp;
	SerializedProperty onOtherStartProp;
	SerializedProperty onOtherStopProp;
    SerializedProperty onOtherSetProp;
	SerializedProperty onStartProp;
	SerializedProperty onStopProp;
	SerializedProperty onRotationToPortraitProp;
	SerializedProperty onRotationToLandscapeProp;

	SerializedProperty m_OnClickProperty;

	float logoHeight = 25f;
	float logoWidth = 155f;
	float fieldHeight = 16f;
	float buttonHeight = 18f;

    private int resetState = -1;

	Texture logo;

	Transform arCoreTransform;
	Transform arKitTransform;
	Transform vuforiaTransform;
	Transform otherTransform;

    static object gameViewSizesInstance;
    static MethodInfo getGroup;

	private void AddCameraEventsForTrackingEngine (TrackingEngine engine) {
		Camera camera = null;
		UnityEvent startEvent = null;
		UnityEvent stopEvent = null;

		if (engine == TrackingEngine.ARCore) {
			if (aryzonTracking.arCoreTransform) {
				camera = aryzonTracking.arCoreTransform.gameObject.GetComponent<Camera> ();
				startEvent = aryzonTracking.onARCoreStart;
				stopEvent = aryzonTracking.onARCoreStop;
			} else {
				return;
			}
		} else if (engine == TrackingEngine.ARKit) {
			if (aryzonTracking.arKitTransform) {
				camera = aryzonTracking.arKitTransform.gameObject.GetComponent<Camera> ();
				startEvent = aryzonTracking.onARKitStart;
				stopEvent = aryzonTracking.onARKitStop;
			} else {
				return;
			}
		} else if (engine == TrackingEngine.Vuforia) {
			if (aryzonTracking.vuforiaTransform) {
				camera = aryzonTracking.vuforiaTransform.gameObject.GetComponent<Camera> ();
				startEvent = aryzonTracking.onVuforiaStart;
				stopEvent = aryzonTracking.onVuforiaStop;
			} else {
				return;
			}
		} else if (engine == TrackingEngine.Other) {
			if (aryzonTracking.otherTransform) {
				camera = aryzonTracking.otherTransform.gameObject.GetComponent<Camera> ();
				startEvent = aryzonTracking.onOtherStart;
				stopEvent = aryzonTracking.onOtherStop;
			} else {
				return;
			}
		} else if (engine == TrackingEngine.None) {
			return;
		}

		if (camera == null) {
			return;
		}

		int startCount = startEvent.GetPersistentEventCount ();
		int stopCount = stopEvent.GetPersistentEventCount ();

		bool startEventFound = false;
		bool stopEventFound = false;

		for (int i = 0; i < startCount; i++) {
			if (startEvent.GetPersistentTarget(i) == camera && startEvent.GetPersistentMethodName(i) == "set_enabled") {
				startEventFound = true;
				break;
			}
		}

		for (int i = 0; i < stopCount; i++) {
			if (stopEvent.GetPersistentTarget(i) == camera && stopEvent.GetPersistentMethodName(i) == "set_enabled") {
				stopEventFound = true;
				break;
			}
		}

		MethodInfo method = null;

		if (!startEventFound || !stopEventFound) {
			method = typeof(UnityEngine.Camera).GetMethod ("set_enabled");
			if (engine == TrackingEngine.ARCore) {
				//aryzonTracking.showARCoreEvents = true;
                EditorPrefs.SetBool("showARCoreEvents", true);
			} else if (engine == TrackingEngine.ARKit) {
				//aryzonTracking.showARKitEvents = true;
                EditorPrefs.SetBool("showARKitEvents", true);
			} else if (engine == TrackingEngine.Vuforia) {
				//aryzonTracking.showVuforiaEvents = true;
                EditorPrefs.SetBool("showVuforiaEvents", true);
			} else if (engine == TrackingEngine.Other) {
				//aryzonTracking.showOtherEvents = true;
                EditorPrefs.SetBool("showOtherEvents", true);
			}
		}

		if (!startEventFound) {
			UnityAction<bool> startAction = System.Delegate.CreateDelegate (typeof(UnityAction<bool>), camera, method) as UnityAction<bool>;
			UnityEventTools.AddBoolPersistentListener (startEvent, startAction, false);
		}

		if (!stopEventFound) {
			UnityAction<bool> stopAction = System.Delegate.CreateDelegate (typeof(UnityAction<bool>), camera, method) as UnityAction<bool>;
			UnityEventTools.AddBoolPersistentListener (stopEvent, stopAction, true);
		}
	}

	void OnEnable () {
		aryzonTracking = (AryzonTracking)target;
		logo = Resources.Load("LogoAryzonSidebar") as Texture;

		trackingEngineProp = serializedObject.FindProperty ("trackingEngine");

		useARCoreProp = serializedObject.FindProperty("useARCore");
		useARKitProp = serializedObject.FindProperty("useARKit");
		useVuforiaProp = serializedObject.FindProperty("useVuforia");
		useOtherProp = serializedObject.FindProperty("useOther");

		arCoreTransformProp = serializedObject.FindProperty("arCoreTransform");
		arKitTransformProp = serializedObject.FindProperty("arKitTransform");
		vuforiaTransformProp = serializedObject.FindProperty("vuforiaTransform");
		otherTransformProp = serializedObject.FindProperty("otherTransform");

		setAryzonModeOnStartProp = serializedObject.FindProperty("setAryzonModeOnStart");

		sceneUnitsProp = serializedObject.FindProperty("sceneUnits");
		customUnitsProp = serializedObject.FindProperty("customUnits");

		m_OnClickProperty = serializedObject.FindProperty("onClick");

		arCoreTransform = aryzonTracking.arCoreTransform;
		arKitTransform = aryzonTracking.arKitTransform;
		vuforiaTransform = aryzonTracking.vuforiaTransform;
		otherTransform = aryzonTracking.otherTransform;

        onARCoreStartProp = serializedObject.FindProperty("onARCoreStart");
        onARCoreStopProp = serializedObject.FindProperty("onARCoreStop");
        onARCoreSetProp = serializedObject.FindProperty("onARCoreSet");
        onARKitStartProp = serializedObject.FindProperty("onARKitStart");
		onARKitStopProp = serializedObject.FindProperty("onARKitStop");
        onARKitSetProp = serializedObject.FindProperty("onARKitSet");
		onVuforiaStartProp = serializedObject.FindProperty("onVuforiaStart");
		onVuforiaStopProp = serializedObject.FindProperty("onVuforiaStop");
        onVuforiaSetProp = serializedObject.FindProperty("onVuforiaSet");
		onOtherStartProp = serializedObject.FindProperty("onOtherStart");
		onOtherStopProp = serializedObject.FindProperty("onOtherStop");
        onOtherSetProp = serializedObject.FindProperty("onOtherSet");
		onStartProp = serializedObject.FindProperty("onStart");
		onStopProp = serializedObject.FindProperty("onStop");
		onRotationToPortraitProp = serializedObject.FindProperty("onRotationToPortrait");
		onRotationToLandscapeProp = serializedObject.FindProperty("onRotationToLandscape");

        EditorApplication.playModeStateChanged += ResetView;

        initializeGameView();
	}

    BuildTargetGroup CurrentGroup () {
        
        #if UNITY_IOS
        return BuildTargetGroup.iOS;
        #elif UNITY_ANDROID
        return BuildTargetGroup.Android;
        #endif
        return BuildTargetGroup.Standalone;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space(); 
        
        Rect controlRect = EditorGUILayout.GetControlRect(false, logoHeight);

        GUI.DrawTexture(new Rect(controlRect.x, controlRect.y, logoWidth, controlRect.height), logo, ScaleMode.ScaleToFit, true);

		EditorGUILayout.Space();
		GUIStyle boldWrap = EditorStyles.boldLabel;
		boldWrap.wordWrap = true;
		EditorGUILayout.LabelField("Select the tracking engines you use in your scene", boldWrap);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(useARCoreProp, new GUIContent("ARCore"), GUILayout.Height(fieldHeight));
        EditorGUILayout.PropertyField(useARKitProp, new GUIContent("ARKit"), GUILayout.Height(fieldHeight));
        EditorGUILayout.PropertyField(useVuforiaProp, new GUIContent("Vuforia"), GUILayout.Height(fieldHeight));
        if (aryzonTracking.useVuforia) {
            if (!PlayerSettings.GetPlatformVuforiaEnabled(CurrentGroup())) {
                EditorGUILayout.LabelField("Please enable Vuforia in the player settings for your build target", EditorStyles.miniLabel);
            }
        }
        EditorGUILayout.PropertyField(useOtherProp, new GUIContent("Other"), GUILayout.Height(fieldHeight));
        
        EditorGUILayout.Space();
        EditorGUILayout.Space(); 

		int fieldCount = 0;

        if (aryzonTracking.useARCore) {
            EditorGUILayout.PropertyField(arCoreTransformProp, new GUIContent("ARCore Transform"), GUILayout.Height(fieldHeight));
			if (arCoreTransform != aryzonTracking.arCoreTransform) {
				arCoreTransform = aryzonTracking.arCoreTransform;
				AddCameraEventsForTrackingEngine (TrackingEngine.ARCore);
			}
			fieldCount++;
        }
        
        if (aryzonTracking.useARKit) {
            EditorGUILayout.PropertyField(arKitTransformProp, new GUIContent("ARKit Transform"), GUILayout.Height(fieldHeight));
			if (arKitTransform != aryzonTracking.arKitTransform) {
				arKitTransform = aryzonTracking.arKitTransform;
				AddCameraEventsForTrackingEngine (TrackingEngine.ARKit);
			}
			fieldCount++;
        }
        
        if (aryzonTracking.useVuforia) {
            EditorGUILayout.PropertyField(vuforiaTransformProp, new GUIContent("Vuforia Transform"), GUILayout.Height(fieldHeight));
			if (vuforiaTransform != aryzonTracking.vuforiaTransform) {
				vuforiaTransform = aryzonTracking.vuforiaTransform;
				AddCameraEventsForTrackingEngine (TrackingEngine.Vuforia);
			}
            fieldCount++;
        }

        if (aryzonTracking.useOther) {
            EditorGUILayout.PropertyField(otherTransformProp, new GUIContent("Other Transform"), GUILayout.Height(fieldHeight));
			if (otherTransform != aryzonTracking.otherTransform) {
				otherTransform = aryzonTracking.otherTransform;
				AddCameraEventsForTrackingEngine (TrackingEngine.Other);
			}
			fieldCount++;
        }

        EditorGUI.indentLevel--;

		EditorGUILayout.Space();
		EditorGUILayout.Space();        

		EditorGUILayout.LabelField("Select transform to follow", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;

		string[] fields = new string[fieldCount+1];
		int i = 0;
		int selected = -1;


		if (aryzonTracking.trackingEngine == TrackingEngine.None) {
			selected = i;
		}
		fields[i] = "None";
		i++;

		if (aryzonTracking.useARCore) {
			if (aryzonTracking.trackingEngine == TrackingEngine.ARCore) {
				selected = i;
			}
			fields[i] = "ARCore";
			i++;
		}
		if (aryzonTracking.useARKit) {
			if (aryzonTracking.trackingEngine == TrackingEngine.ARKit) {
				selected = i;
			}
			fields[i] = "ARKit";
			i++;
		}
		if (aryzonTracking.useVuforia) {
			if (aryzonTracking.trackingEngine == TrackingEngine.Vuforia) {
				selected = i;
			}
			fields[i] = "Vuforia";
			i++;
		}
		if (aryzonTracking.useOther) {
			if (aryzonTracking.trackingEngine == TrackingEngine.Other) {
				selected = i;
			}
			fields[i] = "Other";
		}
			
		selected = EditorGUILayout.Popup ("Follow", selected, fields);

		if (selected > -1 && selected < fieldCount+1) {
			string selectedName = fields [selected];
			if (selectedName == "None") {
				trackingEngineProp.enumValueIndex = (int)TrackingEngine.None;
				aryzonTracking.SetTrackingEngine (TrackingEngine.None);
			} else if (selectedName == "ARCore") {
				trackingEngineProp.enumValueIndex = (int)TrackingEngine.ARCore;
				aryzonTracking.SetTrackingEngine (TrackingEngine.ARCore);
			} else if (selectedName == "ARKit") {
				trackingEngineProp.enumValueIndex = (int)TrackingEngine.ARKit;
				aryzonTracking.SetTrackingEngine (TrackingEngine.ARKit);
			} else if (selectedName == "Vuforia") {
				trackingEngineProp.enumValueIndex = (int)TrackingEngine.Vuforia;
				aryzonTracking.SetTrackingEngine (TrackingEngine.Vuforia);
			} else if (selectedName == "Other") {
				trackingEngineProp.enumValueIndex = (int)TrackingEngine.Other;
				aryzonTracking.SetTrackingEngine (TrackingEngine.Other);
			}
		} else {
			trackingEngineProp.enumValueIndex = (int)TrackingEngine.None;
			aryzonTracking.SetTrackingEngine (TrackingEngine.None);
		}
			
		EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.Space();        
        
		EditorGUILayout.LabelField("Select your scene units", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(sceneUnitsProp, new GUIContent("Units"), GUILayout.Height(fieldHeight));

		if (aryzonTracking.sceneUnits == Units.Custom) {
			EditorGUILayout.PropertyField(customUnitsProp, new GUIContent("(units / meter)"), GUILayout.Height(fieldHeight));
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(setAryzonModeOnStartProp, new GUIContent("Set Aryzon mode on Start"), GUILayout.Height(fieldHeight));

		EditorGUILayout.Space();
		EditorGUILayout.Space();

        aryzonTracking.showAryzonModeEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showAryzonModeEvents", false), "Aryzon mode events");
        EditorPrefs.SetBool("showAryzonModeEvents", aryzonTracking.showAryzonModeEvents);
        if (aryzonTracking.showAryzonModeEvents) {
			EditorGUILayout.PropertyField (onStartProp, new GUIContent ("On start Aryzon mode"));
			EditorGUILayout.PropertyField (onStopProp, new GUIContent ("On stop Aryzon mode"));
		}
		if (aryzonTracking.useARCore) {
            aryzonTracking.showARCoreEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showARCoreEvents", false), "ARCore events");
            EditorPrefs.SetBool("showARCoreEvents", aryzonTracking.showARCoreEvents);
		}
		if (aryzonTracking.useARCore && aryzonTracking.showARCoreEvents) {
			EditorGUILayout.PropertyField (onARCoreStartProp, new GUIContent ("On start Aryzon mode when following ARCore"));
			EditorGUILayout.PropertyField (onARCoreStopProp, new GUIContent ("On stop Aryzon mode when following ARCore"));
            EditorGUILayout.PropertyField (onARCoreSetProp, new GUIContent ("On set to follow ARCore"));
		}
		if (aryzonTracking.useARKit) {
            aryzonTracking.showARKitEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showARKitEvents", false), "ARKit events");
            EditorPrefs.SetBool("showARKitEvents", aryzonTracking.showARKitEvents);
		}
		if (aryzonTracking.useARKit && aryzonTracking.showARKitEvents) {
			EditorGUILayout.PropertyField (onARKitStartProp, new GUIContent ("On start Aryzon mode when following ARKit"));
			EditorGUILayout.PropertyField (onARKitStopProp, new GUIContent ("On stop Aryzon mode when following ARKit"));
            EditorGUILayout.PropertyField (onARKitSetProp, new GUIContent ("On set to follow ARKit"));
		}
		if (aryzonTracking.useVuforia) {
            aryzonTracking.showVuforiaEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showVuforiaEvents", false), "Vuforia events");
            EditorPrefs.SetBool("showVuforiaEvents", aryzonTracking.showVuforiaEvents);
		}
		if (aryzonTracking.useVuforia && aryzonTracking.showVuforiaEvents) {
			EditorGUILayout.PropertyField (onVuforiaStartProp, new GUIContent ("On start Aryzon mode when following Vuforia"));
			EditorGUILayout.PropertyField (onVuforiaStopProp, new GUIContent ("On stop Aryzon mode when following Vuforia"));
            EditorGUILayout.PropertyField (onVuforiaSetProp, new GUIContent ("On set to follow Vuforia"));
		}
		if (aryzonTracking.useOther) {
            aryzonTracking.showOtherEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showOtherEvents", false), "Other events");
            EditorPrefs.SetBool("showOtherEvents", aryzonTracking.showOtherEvents);
		}
		if (aryzonTracking.useOther && aryzonTracking.showOtherEvents) {
			EditorGUILayout.PropertyField (onOtherStartProp, new GUIContent ("On start Aryzon mode when following other"));
			EditorGUILayout.PropertyField (onOtherStopProp, new GUIContent ("On stop Aryzon mode when following other"));
            EditorGUILayout.PropertyField (onOtherSetProp, new GUIContent ("On set to follow other"));
		}
        aryzonTracking.showRotationEvents = EditorGUILayout.Foldout (EditorPrefs.GetBool("showRotationEvents", false), "Rotation events");
        EditorPrefs.SetBool("showRotationEvents", aryzonTracking.showRotationEvents);
		if (aryzonTracking.showRotationEvents) {
			EditorGUILayout.PropertyField (onRotationToPortraitProp, new GUIContent ("On rotation to portrait"));
			EditorGUILayout.PropertyField (onRotationToLandscapeProp, new GUIContent ("On rotation to landscape"));
		}

		GUIStyle wrap = EditorStyles.miniLabel;
		wrap.wordWrap = true;
		wrap.fontStyle = FontStyle.Normal;
		EditorGUILayout.LabelField ("Tip: simulate phone rotation by setting the aspect ratio of the Game screen to Tall or Wide", wrap);

		EditorGUI.indentLevel--;
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		Rect flexRect = GUILayoutUtility.GetLastRect ();

		float inspectorWidth = flexRect.width;
		float spaceBetween = 4f;
		float buttonWidth = (inspectorWidth - spaceBetween) / 2f;

		EditorGUILayout.BeginHorizontal ();

		if (!aryzonTracking.aryzonMode && Application.isPlaying) {
			GUI.enabled = true;
		} else {
			GUI.enabled = false;
		}

		if (GUI.Button(new Rect(flexRect.x, flexRect.y, buttonWidth, buttonHeight), "Start")) {
			aryzonTracking.startTrackingBool = true;
		}

		if (aryzonTracking.aryzonMode && Application.isPlaying) {
			GUI.enabled = true;
		} else {
			GUI.enabled = false;
		}

		if (GUI.Button(new Rect(flexRect.x + buttonWidth + spaceBetween, flexRect.y, buttonWidth, buttonHeight), "Stop")) {
			aryzonTracking.stopTrackingBool = true;
		}

		GUI.enabled = true;
		EditorGUILayout.EndHorizontal ();

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) {
            EditorGUILayout.BeginHorizontal ();

            GameViewSizeGroupType sizeGroupType = GameViewSizeGroupType.iOS;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
                sizeGroupType = GameViewSizeGroupType.Android;
            }

            int mode = -1;
            int sizeIndex = GetSizeIndex();
            string inverse = "Free Aspect";
            if (sizeIndex != -1) {
                inverse = GetInverseWindowSizeString(sizeGroupType, sizeIndex, out mode);
            }
            if (mode == 0 || mode == -1) {
                GUI.enabled = true;
            } else {
                GUI.enabled = false;
            }

            if (GUI.Button(new Rect(flexRect.x, flexRect.y + 24, buttonWidth, buttonHeight), "Portrait")) {
                if (inverse == "Free Aspect") {
                    SetSize(FindSize(sizeGroupType, ":16)"));
                } else {
                    SetSize(FindSize(sizeGroupType, inverse));
                }
                if (EditorApplication.isPlaying && resetState  == -1) {
                    resetState = 0;
                }
            }
            if (mode == 1 || mode == -1) {
                GUI.enabled = true;
            } else {
                GUI.enabled = false;
            }

            if (GUI.Button(new Rect(flexRect.x + buttonWidth + spaceBetween, flexRect.y + 24, buttonWidth, buttonHeight), "Landscape")) {
                if (inverse == "Free Aspect") {
                    SetSize(FindSize(sizeGroupType, "(16:"));
                } else {
                    SetSize(FindSize(sizeGroupType, inverse));
                }
                if (EditorApplication.isPlaying && resetState  == -1) {
                    resetState = 1;
                }
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal ();
        }

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
        
    }

    void OnApplicationQuit () {
        Debug.Log("Quit");
    }

    void ResetView (PlayModeStateChange state) {
        
        if (EditorApplication.isPlaying) {
            return;
        }

        if (resetState != -1) {
            
            GameViewSizeGroupType sizeGroupType = GameViewSizeGroupType.iOS;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
                sizeGroupType = GameViewSizeGroupType.Android;
            }

            int mode = -1;
            int sizeIndex = GetSizeIndex();
            string inverse = "Free Aspect";
            if (sizeIndex != -1) {
                inverse = GetInverseWindowSizeString(sizeGroupType, sizeIndex, out mode);
            }
            if (((mode == 0 && resetState == 1) ||(mode == 1 && resetState == 0)) && inverse != "Free Aspect") {
                SetSize(FindSize(sizeGroupType, inverse));
            }

            resetState = -1;
        }
    }

    public string GetInverseWindowSizeString (GameViewSizeGroupType sizeGroupType, int index, out int mode) {
        mode = -1;
        var group = GetGroup(sizeGroupType);
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

        string display = displayTexts[index];
        int pren = display.IndexOf("(");
        if (pren != -1) {
            display = display.Substring(pren+1, display.Length-(pren+2));
        } else {
            return display;
        }

        string divider = "x";
        int div = display.IndexOf('x');
        if (div == -1) {
            div = display.IndexOf(':');
            divider = ":";
        }

        if (div == -1) {
            return display;
        }

        string first = "";
        string second = "";
        if (div != -1) {
            first = display.Substring(0, div);
            second = display.Substring(div+1, display.Length-(div+1));
        }
        if (int.Parse(first) > int.Parse(second)) {
            mode = 0;
        } else {
            mode = 1;
        }
        return ("(" + second + divider + first + ")");
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
    {
        var group = GetGroup(sizeGroupType);
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

        for(int i = 0; i < displayTexts.Length; i++)
        {
            string display = displayTexts[i];
            if (display.Contains(text)) {
                return i;
            }
        }
        return -1;
    }

    static object GetGroup(GameViewSizeGroupType type)
    {
        return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
    }

    void initializeGameView ()
    {
        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        getGroup = sizesType.GetMethod("GetGroup");
        gameViewSizesInstance = instanceProp.GetValue(null, null);
    }

    public enum GameViewSizeType
    {
        AspectRatio, FixedResolution
    }

    public static void SetSize(int index)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                                                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }

    public int GetSizeIndex()
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        bool isOpen = false;
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
            if (window.titleContent.text == "Game") {
                isOpen = true;
                break;
            }
        }

        if (isOpen) {
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType, false, "Game", false);
            return (int)selectedSizeIndexProp.GetValue(gvWnd, null);
        }
        return -1;
    }
}