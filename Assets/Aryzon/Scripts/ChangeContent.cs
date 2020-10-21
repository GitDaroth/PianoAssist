using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangeContent : MonoBehaviour {

	private int level;
	private RectTransform previousMenu;
	private RectTransform currentMenu;
	private ScrollRect sRect;
	public Button previousButton;

	// Use this for initialization
	void Start () {
		level = 0;
		sRect = GetComponent<ScrollRect> ();

		currentMenu = sRect.content;
		previousButton.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GoTo(GameObject submenu) {
		previousMenu = currentMenu;
		currentMenu = submenu.GetComponent<RectTransform> ();

		sRect.content.gameObject.SetActive (false);
		sRect.content = currentMenu;
		sRect.content.gameObject.SetActive (true);

		level += 1;
		if (level == 1) {
			previousButton.gameObject.SetActive (true);
		}
	}

	public void Previous () {
		currentMenu = previousMenu;
		sRect.content.gameObject.SetActive (false);
		sRect.content = previousMenu;
		sRect.content.gameObject.SetActive (true);
		level -= 1;
		if (level == 0) {
			previousButton.gameObject.SetActive (false);
		}
	}
}
