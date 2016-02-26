using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Gesture {
	Click, None
}

public class GestureDetector {

	private Gesture gesture;
	private List<Vector3> positions;

	public GestureDetector() {
		gesture = Gesture.None;
		positions = new List<Vector3> ();
	}

	public void Update(Vector3 position) {
		positions.Add (position);
		Debug.Log ("z :" + position.z);
	}

	public Gesture GetGesture() {
		return gesture;
	}
}
