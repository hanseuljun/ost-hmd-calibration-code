using UnityEngine;
using System.Collections;

public class StereoCamera : MonoBehaviour {

	public Camera leftCamera;
	public Camera rightCamera;
	public float ipd;
	public float aspectRatio;

	void Awake () {
		ApplyParameters ();
	}

	void Update () {
		ApplyParameters ();
	}

	public void ApplyParameters() {
		leftCamera.transform.localPosition = new Vector3(-0.5f * ipd, 0.0f, 0.0f);
		rightCamera.transform.localPosition = new Vector3(0.5f * ipd, 0.0f, 0.0f);
		leftCamera.aspect = aspectRatio;
		rightCamera.aspect = aspectRatio;
	}

	public bool IsInside(Vector3 v) {
		if (v.z <= 0.0f) {
			return false;
		}

		Vector3 leftPoint = leftCamera.WorldToScreenPoint (v);
		Vector3 rightPoint = rightCamera.WorldToScreenPoint (v);

		return leftCamera.pixelRect.Contains (leftPoint) && rightCamera.pixelRect.Contains (rightPoint);
	}
}
