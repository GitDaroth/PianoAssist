using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwapSpriteOnToggle : MonoBehaviour {

	public Sprite onSprite;
	public Sprite offSprite;
	bool state = false;
	// Use this for initialization
	void Start () {
		SwapSprite ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwapSprite () {
		state = GetComponent<Toggle> ().isOn;
		Image image = GetComponentInChildren<Image> ();
		if (image) {
			if (state) {
				image.sprite = onSprite;
			} else {
				image.sprite = offSprite;
			}
		}
	}
}
