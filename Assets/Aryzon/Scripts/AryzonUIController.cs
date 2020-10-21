using Aryzon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AryzonUIController : MonoBehaviour {

	public GameObject main;
	public GameObject calibration;
	public GameObject firstTime;

	public string OnBackLoadScene;

    private List<int> ints;

    void Awake () {
        Init ();
    }

	void Init () {
        ints = new List<int> ();
		ints.Add (-1);
		if (AryzonSettings.Calibration.didCalibrate || AryzonSettings.Calibration.skipCalibrate) {
			SetMain ();
		} else {
			SetFirstTime ();
		}
	}

	public void SetMain () {
		main.SetActive (true);
		calibration.SetActive (false);
		firstTime.SetActive (false);
		ints.Add (0);
	}

	public void SetCalibration () {
		main.SetActive (false);
		calibration.SetActive (true);
		firstTime.SetActive (false);
		ints.Add (1);
	}

	public void SetFirstTime () {
		firstTime.SetActive (true);
		main.SetActive (false);
		calibration.SetActive (false);
		ints.Add (2);
	}

    public void Inactivate () {
        firstTime.SetActive (false);
        main.SetActive (false);
        calibration.SetActive (false);
    }

    public void Activate () {
        main.SetActive (true);
        calibration.SetActive (false);
        firstTime.SetActive (false);
    }

	public void SetMainAfter (float seconds) {
		StartCoroutine (SetMainAfterEnumerator (seconds));
	}

	IEnumerator SetMainAfterEnumerator (float seconds) {
		yield return new WaitForSeconds (seconds);
		SetMain ();
	}

	public void SetSkipCalibration () {
		AryzonSettings.Calibration.skipCalibrate = true;
	}

	public void BackButtonPress () {
		int currentScreen = ints [ints.Count - 1];
		int previousScreen = ints[ints.Count-2];
		ints.RemoveAt (ints.Count-1);
		if (ints.Count >= 1) {
			ints.RemoveAt (ints.Count -1);
		}
		if (previousScreen == -1 || currentScreen == 0) {
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = false;
			Screen.orientation = ScreenOrientation.Portrait;

			if (OnBackLoadScene == "" || OnBackLoadScene == null) {
                Init();
                AryzonSettings.Instance.aryzonTracking.StopAryzonMode();
			} else {
				SceneManager.LoadSceneAsync(OnBackLoadScene);
			}
		} else if (previousScreen == 0) {
			SetMain ();
		} else if (previousScreen == 1) {
			SetCalibration ();
		} else if (previousScreen == 2) {
			SetFirstTime ();
		}
	}

	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
			BackButtonPress ();
			return;
		}
	}
}
