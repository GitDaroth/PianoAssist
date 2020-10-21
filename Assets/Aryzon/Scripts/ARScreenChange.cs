using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//[RequireComponent(typeof(HorizontalLayoutGroup))]
public class ARScreenChange : MonoBehaviour {

	//RectTransform rTransform;
	public UnityEvent transformed;

	// Use this for initialization
	void Start () {
		//rTransform = (RectTransform)transform;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnRectTransformDimensionsChange() {
		transformed.Invoke ();
	}
}
