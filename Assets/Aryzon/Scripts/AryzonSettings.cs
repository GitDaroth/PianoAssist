using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//Do not edit this class

namespace Aryzon {
    public delegate void SettingsRetrievedEventHandler (string status, bool success);
    public delegate void UpdateLayoutEventHandler ();

    public class AryzonSettings : MonoBehaviour {
        public event SettingsRetrievedEventHandler SettingsRetrieved;
        public event UpdateLayoutEventHandler UpdateLayout;

        public bool AryzonMode = false;
        public bool LandscapeMode = false;
        public bool PortraitMode = false;
        public bool ShowReticle = false;

        public Camera reticleCamera;

        public AryzonTracking aryzonTracking;

        protected AryzonSettings () {}
        private static AryzonSettings _instance;
        private static object _lock = new object();
        public static AryzonSettings Instance
        {
            get
            {
                if (applicationIsQuitting) {
                    Debug.LogWarning("[Aryzon] Instance '"+ typeof(AryzonSettings) + "' already destroyed on application quit." + " Won't create again - returning null.");
                    return null;
                }

                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (AryzonSettings) FindObjectOfType(typeof(AryzonSettings));

                        if ( FindObjectsOfType(typeof(AryzonSettings)).Length > 1 )
                        {
                            Debug.LogError("[Aryzon] there should never be more than 1 AryzonManager");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject aryzonManager = new GameObject();
                            _instance = aryzonManager.AddComponent<AryzonSettings>();
                            aryzonManager.name = "AryzonManager";

                            DontDestroyOnLoad(aryzonManager);
                        } else {
                            Debug.Log("[Aryzon] Using instance already created: " +
                                      _instance.gameObject.name);
                        }
                    }

                    return _instance;
                }
            }
        }



        private static bool applicationIsQuitting = false;

        public void OnDestroy () {
            applicationIsQuitting = true;
        }

        public static class Calibration {
            public static bool didCalibrate = false;
            public static bool skipCalibrate = false;
            public static float xShift = 0.03f;
            public static float yShift = -0.105f;
            public static float IPD = 0.064f;
            public static bool rotatedSensor = false;

            static Calibration() {
                //Debug.Log ("Loading calibration settings");
                bool ok = SerializeStatic.Load (typeof(Calibration), Application.persistentDataPath + "/AryzonCalibrationSettings.bfd");
                if (!ok) {
                    //Debug.Log ("Could not load calibration settings");
                } else {
                    AryzonSettings.Instance.Apply ();
                }
            }
        }

        public void Initialize () {
            //Debug.Log ("Loaded singleton");
            if (!Calibration.didCalibrate && !Phone.aryzonCalibrated) {
                //Debug.Log ("RetrieveSettingsForPhone ();");
                RetrieveSettingsForPhone ();
            }
        }

        public static class Phone {
            public static float xShift = 0.03f;
            public static float yShift = -0.105f;
            public static float screenWidth = 0.1f;
            public static bool rotatedSensor = false;
            public static bool aryzonCalibrated = false;

            static Phone() {
                //Debug.Log ("Loading phone settings");

                bool ok = SerializeStatic.Load (typeof(Phone), Application.persistentDataPath + "/AryzonPhoneSettings.bfd");
                if (!ok) {
                    //Debug.Log ("Could not load phone settings");
                } else {
                    AryzonSettings.Instance.Apply ();
                }
            }
        }

        public static class Headset {
            public static string name = "Aryzon";
            public static float xShift = 0f;
            public static float yShift = 0.03f;
            public static float distortion = 0f;
            public static float redShift = 1.01f;
            public static float greenShift = 1.02f;
            public static float blueShift = 1.04f;
            public static float lensToScreen = 0.063f;
            public static float eyeToLens = 0.11f;
            public static float focalLength = 0.082f;
            public static float lensCenterDistance = 0.06f;
            public static float bottomToCenter = 0.105f;
            public static float fovFactor = 1.0f;
            public static bool didAcceptDisclaimer = false;

            static Headset() {
                //Debug.Log ("Loading headset settings");
                bool ok = SerializeStatic.Load (typeof(Headset), Application.persistentDataPath + "/AryzonHeadsetSettings.bfd");
                if (!ok) {
                    //Debug.Log ("Could not load headset settings");
                } else {
                    AryzonSettings.Instance.Apply ();
                }
            }
        }

        public bool Save () {
            string savePathHeadset = Application.persistentDataPath + "/AryzonHeadsetSettings.bfd";
            string savePathPhone = Application.persistentDataPath + "/AryzonPhoneSettings.bfd";
            string savePathCalibration = Application.persistentDataPath + "/AryzonCalibrationSettings.bfd";
            if (File.Exists (savePathHeadset)) {
                File.Delete (savePathHeadset);
            }
            if (File.Exists (savePathPhone)) {
                File.Delete (savePathPhone);
            }
            if (File.Exists (savePathCalibration)) {
                File.Delete (savePathCalibration);
            }

            bool ok = SerializeStatic.Save(typeof(Headset), savePathHeadset);
            ok = SerializeStatic.Save(typeof(Phone), savePathPhone);
            ok = SerializeStatic.Save(typeof(Calibration), savePathCalibration);

            if (!ok) {
                //Debug.Log ("Could not save settings");
            } else {
                //Debug.Log ("Saved settings to: " + savePathPhone);
            }

            return ok;
        }

        public void Apply () {
            if (UpdateLayout != null) {
                UpdateLayout ();
            }
        }

        private void RetrieveSettingsForPhone () {
            string JsonArraystring = "{\"phoneID\": \"" + SystemInfo.deviceModel + "\"}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            byte[] body = Encoding.UTF8.GetBytes(JsonArraystring);
            WWW www = new WWW("https://sdk.aryzon.com/GetPhoneValues.php", body, headers);
            StartCoroutine("RetrievedataForPhoneEnumerator", www);
        }

        public void RetrieveSettingsForCode(string id)
        {
            string JsonArraystring = "{\"userID\": \"" + id + "\"}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            byte[] body = Encoding.UTF8.GetBytes(JsonArraystring);
            WWW www = new WWW("https://sdk.aryzon.com/GetValues.php", body, headers);
            StartCoroutine("RetrievedataForCodeEnumerator", www);
        }

        IEnumerator RetrievedataForCodeEnumerator(WWW www)
        {
            string returnString = "Retrieving data..";
            bool success = false;
            yield return www;
            if (www.error != null)
            {
                returnString = "Could not retrieve your settings. Please check your internet connection.";
                success = false;
            }
            else
            {
                string result = www.text;
                if (result.StartsWith ("result:")) {
                    result = result.Replace ("result:", "");

                    CalibrationData data = JsonUtility.FromJson<CalibrationData> (result);
                    //data.IPD = 0.06f;
                    if (SystemInfo.deviceModel != data.MakeModel) {
                        returnString = "It looks like you have a new phone, please recalibrate.";
                        success = false;
                    } else {
                        AryzonSettings.Headset.xShift = data.xShiftLens;
                        AryzonSettings.Headset.yShift = data.yShiftLens;
                        AryzonSettings.Headset.distortion = data.distortion;
                        AryzonSettings.Headset.redShift = data.redShift;
                        AryzonSettings.Headset.greenShift = data.greenShift;
                        AryzonSettings.Headset.blueShift = data.blueShift;
                        AryzonSettings.Headset.lensCenterDistance = data.lensCenterDistance;
                        AryzonSettings.Headset.eyeToLens = data.eyeToLens;
                        AryzonSettings.Headset.lensToScreen = data.lensToScreen;
                        AryzonSettings.Headset.focalLength = data.focalLength;
                        AryzonSettings.Headset.name = data.headsetName;
                        AryzonSettings.Headset.bottomToCenter = data.bottomToCenter;
                        AryzonSettings.Headset.fovFactor = data.fovFactor;
                        AryzonSettings.Calibration.rotatedSensor = data.rotatedSensor;
                        AryzonSettings.Calibration.xShift = data.xShift;
                        AryzonSettings.Calibration.yShift = data.yShift;
                        AryzonSettings.Calibration.IPD = data.IPD;
                        AryzonSettings.Calibration.didCalibrate = true;

                        AryzonSettings.Instance.Apply ();
                        AryzonSettings.Instance.Save ();
                        success = true;
                        returnString = "Successfully retrieved your settings, enjoy!";
                    }
                } else {
                    returnString = "Invalid code";
                    success = false;
                }
            }
            if (SettingsRetrieved != null) {
                SettingsRetrieved (returnString, success);
            }
        }
        IEnumerator RetrievedataForPhoneEnumerator(WWW www)
        {
            string returnString = "Retrieving data..";
            bool success = false;
            yield return www;
            if (www.error != null)
            {
                returnString = "Could not retrieve your settings. Please check your internet connection.";
                success = false;
            }
            else
            {
                string result = www.text;
                if (result.StartsWith ("result:")) {
                    result = result.Replace ("result:", "");

                    CalibrationData data = JsonUtility.FromJson<CalibrationData> (result);

                    AryzonSettings.Phone.xShift = data.xShiftLens;
                    AryzonSettings.Phone.yShift = data.yShiftLens;
                    AryzonSettings.Phone.rotatedSensor = data.rotatedSensor;
                    AryzonSettings.Phone.screenWidth = data.screenWidth;
                    AryzonSettings.Phone.aryzonCalibrated = data.aryzonCalibrated;

                    AryzonSettings.Instance.Apply ();
                    AryzonSettings.Instance.Save ();
                    success = true;
                    returnString = "Successfully retrieved settings for your phone!";
                } else {
                    returnString = "Could not find settings for your phone";
                    success = false;
                }
            }
            if (SettingsRetrieved != null) {
                SettingsRetrieved (returnString, success);
            }
        }
    }

    public class SerializeStatic
    {
        public static bool Save(Type static_class, string filename)
        {
            try
            {
                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);
                object[,] a = new object[fields.Length,2];
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    a[i, 0] = field.Name;
                    a[i, 1] = field.GetValue(null);
                    i++;
                };
                Stream f = File.Open(filename, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();                
                formatter.Serialize(f, a);
                f.Close();
                return true;
            }
            catch (SerializationException e)
            {
                Debug.LogWarning ("[Aryzon] " + e);
                return false;
            }
        }

        public static bool Load(Type static_class, string filename)
        {
            try
            {
                if (!File.Exists(filename)) {
                    //Debug.Log("Could not load settings from file because file does not exist");
                    return false;
                }

                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);                
                object[,] a;
                Stream f = File.Open(filename, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                a = formatter.Deserialize(f) as object[,];
                f.Close();
                if (a.GetLength(0) != fields.Length) return false;
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    if (field.Name == (a[i, 0] as string))
                    {
                        field.SetValue(null, a[i,1]);
                    }
                    i++;
                };                
                return true;
            }
            catch (SerializationException e)
            {
                Debug.LogWarning ("[Aryzon] " + e);
                return false;
            }
        }
    }
    [Serializable]
    class CalibrationData {
        public float xShift = 0f;
        public float yShift = -0.105f;
        public float IPD = 0.064f;
        public string headsetName = "Aryzon";
        public float xShiftLens = 0f;
        public float yShiftLens = 0f;
        public float distortion = 0f;
        public float redShift = 0f;
        public float greenShift = 0f;
        public float blueShift = 0f;
        public float lensToScreen = 0.063f;
        public float eyeToLens = 0.11f;
        public float focalLength = 0.082f;
        public float lensCenterDistance = 0.082f;
        public float bottomToCenter = 0.105f;
        public float fovFactor = 1.0f;
        public string MakeModel = "";
        public bool rotatedSensor = false;
        public bool aryzonCalibrated = false;
        public float screenWidth = 0.1f;
    }
}
