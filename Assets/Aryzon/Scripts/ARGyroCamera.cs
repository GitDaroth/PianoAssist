using UnityEngine;

public class ARGyroCamera : MonoBehaviour
{
	private float initialYAngle = 0f;
	private float appliedGyroYAngle = 0f;
	private float calibrationYAngle = 0f;

	public bool headTracking = true;

	void Start()
	{
		if (headTracking && SystemInfo.supportsGyroscope) {
			Input.gyro.enabled = true;
			initialYAngle = transform.eulerAngles.y;
			CalibrateYAngle();
		} else {
			Debug.Log ("[Aryzon] This device does not have a gyroscope");
			headTracking = false;
		}
	}

	void Update()
	{
		if (headTracking) {
			ApplyGyroRotation();
			ApplyCalibration();
		}
	}


	//Offsets the y angle in case it wasn't 0 at edit time.
	public void CalibrateYAngle()
	{
		calibrationYAngle = appliedGyroYAngle - initialYAngle;
	}

	void ApplyGyroRotation()
	{
		transform.rotation = Input.gyro.attitude;

		transform.Rotate( 0f, 0f, 180f, Space.Self );
		transform.Rotate( 90f, 180f, 0f, Space.World );

		//Save the angle around y axis for use in calibration.
		appliedGyroYAngle = transform.eulerAngles.y;
	}

	//Rotates y angle back however much it deviated when calibrationYAngle was saved.
	void ApplyCalibration()
	{
		transform.Rotate( 0f, -calibrationYAngle, 0f, Space.World );
	}
}