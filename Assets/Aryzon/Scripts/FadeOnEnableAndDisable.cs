using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOnEnableAndDisable : MonoBehaviour {

	private CanvasGroup cGroup;
	public float fadeTime = 0.5f;
    private bool disableWhenDone;
    private bool fading;
    private float newAlpha;
    private float timer;
    private float startAlpha;

	
	void Start () {
		cGroup = GetComponent<CanvasGroup> ();
		cGroup.alpha = 0f;
	}

	private void OnEnable()
	{
        Enable();
	}

    public void OnDisable() {
        cGroup.alpha = 0f;
    }

    public void Enable () {
        //Debug.Log("Enabling");
        if (!cGroup) {
            cGroup = GetComponent<CanvasGroup> ();
        }
        startAlpha = cGroup.alpha;
        disableWhenDone = false;
        newAlpha = 1f;
        timer = 0f;
        if (!fading) {
            fading = true;
            StartCoroutine (fadeToAlpha());
        }
    }

    public void Disable()
    {
        //Debug.Log("Disabling");
        if (gameObject.activeInHierarchy && this.enabled) {
            if (!cGroup) {
                cGroup = GetComponent<CanvasGroup> ();
            }
            startAlpha = cGroup.alpha;
            disableWhenDone = true;
            newAlpha = 0f;
            timer = 0f;
            if (!fading) {
                fading = true;
                StartCoroutine (fadeToAlpha());
            }
        }
    }

	IEnumerator fadeToAlpha () {
		while (timer <= fadeTime) {
			cGroup.alpha = Mathf.Lerp (startAlpha, newAlpha, timer / fadeTime);
			timer += Time.deltaTime;
			yield return null;
		}
		cGroup.alpha = newAlpha;
        fading = false;
        if (disableWhenDone) {
            cGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
	}
}