using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOutAndDisable : MonoBehaviour {

	private CanvasGroup cGroup;
	public float fadeTime = 0.3f;
    public float delay = 0.9f;
    public bool disableImmediately;
    public bool fading = false;

	// Use this for initialization
	void Awake () {
        fading = false;
		cGroup = GetComponent<CanvasGroup> ();
	}

	public void Disable() {
        if (disableImmediately) {
            gameObject.SetActive(false);
            return;
        }
		StartCoroutine (fadeToAlpha(0f));
	}


	IEnumerator fadeToAlpha (float newAlpha) {
        fading = true;
        yield return new WaitForSeconds(delay);
		float startAlpha = cGroup.alpha;
		float timer = 0f;

        while (timer <= fadeTime && fading) {
			cGroup.alpha = Mathf.Lerp (startAlpha, newAlpha, timer / fadeTime);
			timer += Time.deltaTime;
			yield return null;
		}
        if (fading) {
    		cGroup.alpha = newAlpha;
    		gameObject.SetActive (false);
        }
	}
}
