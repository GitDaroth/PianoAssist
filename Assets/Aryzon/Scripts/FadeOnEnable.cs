using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOnEnable : MonoBehaviour {

	private CanvasGroup cGroup;
    public float delay = 0f;
	public float fadeTime = 0.5f;

	// Use this for initialization
	void Awake () {
		cGroup = GetComponent<CanvasGroup> ();
		cGroup.alpha = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnEnable()
	{
		StartCoroutine (fadeToAlpha(1f));
	}

	private void OnDisable()
	{
		cGroup.alpha = 0f;
	}

	IEnumerator fadeToAlpha (float newAlpha) {
        yield return new WaitForSeconds(delay);
		float startAlpha = cGroup.alpha;
		float timer = 0f;

		while (timer <= fadeTime) {
			cGroup.alpha = Mathf.Lerp (startAlpha, newAlpha, timer / fadeTime);
			timer += Time.deltaTime;
			yield return null;
		}
		cGroup.alpha = newAlpha;
	}
}
