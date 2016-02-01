using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StereoCamera))]
public class StereoCameraEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if(GUILayout.Button("Apply Camera Parameters")) {
			StereoCamera stereoCamera = target as StereoCamera;
			stereoCamera.ApplyParameters();
		}
	}
}
