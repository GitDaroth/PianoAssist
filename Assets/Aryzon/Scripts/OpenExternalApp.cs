using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class OpenExternalApp : MonoBehaviour {

	[DllImport ("__Internal")]
	private static extern void callApp ();

	public void OpenApp () {
		if (Application.platform == RuntimePlatform.Android) {
			Application.OpenURL("http://play.google.com/store/apps/details?id=com.Aryzon.AryzonDemo");
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			callApp ();
		}
	}
}
