using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Aryzon {
	public class PasteAndApplySettings : MonoBehaviour {
		
		public Text calibrationCodeText;
		public Text statusText;
		public UnityEvent OnSuccess;
		private SettingsRetrievedEventHandler settingsRetrieved;

		public void Paste () {
			statusText.text = "";
			string id = UniClipboard.GetText ();
			id = id.Replace (System.Environment.NewLine, "");
			calibrationCodeText.text = id;
			AryzonSettings.Instance.RetrieveSettingsForCode (id);
			statusText.text = "Loading your personal settings..";
		}

		void OnEnable () {
			settingsRetrieved = new SettingsRetrievedEventHandler(SettingsRetrieved);
			AryzonSettings.Instance.SettingsRetrieved += settingsRetrieved;
		}

		void OnDisable () {
			AryzonSettings.Instance.SettingsRetrieved -= settingsRetrieved;
		}

		private void SettingsRetrieved (string status, bool success) {
			Debug.Log ("Received settings: " + status);
			statusText.text = status;
			if (!success) {
				calibrationCodeText.text = "Tap to paste code";
			} else {
				OnSuccess.Invoke ();
			}
		}
	}
}