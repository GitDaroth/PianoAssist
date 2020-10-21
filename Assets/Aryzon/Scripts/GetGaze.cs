using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetGaze : MonoBehaviour {

    public EventSystem eventSystem;

    public GazeBasicInputModule gazeBasicInputModule;
    public GameObject currentGameObject;
    public GameObject prevGameObject;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (eventSystem.IsPointerOverGameObject()) {
            Debug.Log("Over");
            if (gazeBasicInputModule != null) {
                currentGameObject = gazeBasicInputModule.GameObjectUnderPointer();
            } else {
                gazeBasicInputModule = (GazeBasicInputModule)eventSystem.currentInputModule;
            }

            if ((currentGameObject != null) && (currentGameObject != prevGameObject)) {
                Debug.LogFormat("Selected: {0}", currentGameObject.name);
            }
        } else {
            currentGameObject = null;
        }
        prevGameObject = currentGameObject;
    }
}
