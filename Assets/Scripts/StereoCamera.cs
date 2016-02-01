using UnityEngine;
using System.Collections;

public class StereoCamera : MonoBehaviour {

	public Camera leftCamera;
	public Camera rightCamera;
	public float ipd;
	public float aspectRatio;

	void Start () {
		
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
}
