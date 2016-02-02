using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Optimization : MonoBehaviour {

	public MeshRenderer prefab;
	private CalibrationFile file;
	List<Vector3> targets;
	List<Vector3> fingerTips;
	private GameObject targetRoot;
	private GameObject fingerTipRoot;
	private List<Transform> targetTransforms;
	private List<Transform> fingerTipTransforms;
	private bool optimized;

	void Start () {
		file = new CalibrationFile ();
		targets = file.targets;
		fingerTips = file.fingerTips;

		targetRoot = new GameObject ();
		targetRoot.name = "target root";
		targetRoot.transform.position = Vector3.zero;

		fingerTipRoot = new GameObject ();
		fingerTipRoot.name = "fingertip root";
		fingerTipRoot.transform.position = Vector3.zero;

		targetTransforms = new List<Transform> ();
		fingerTipTransforms = new List<Transform> ();

		foreach (var v in targets) {
			MeshRenderer mr = Instantiate (prefab);
			mr.name = "target";
			mr.transform.localPosition = v;
			mr.transform.parent = targetRoot.transform;
			mr.material.color = new Color(1.0f, 0.5f, 0.5f);
			targetTransforms.Add(mr.transform);
		}
		
		foreach (var v in fingerTips) {
			MeshRenderer mr = Instantiate (prefab);
			mr.name = "finger tip";
			mr.transform.localPosition = v;
			mr.transform.parent = fingerTipRoot.transform;
			mr.material.color = new Color(0.5f, 0.5f, 1.0f);
			fingerTipTransforms.Add(mr.transform);
		}

		optimized = false;

		print ("Raw root MSE: " + Mathf.Sqrt (CalculateMSE ()));
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.A) && !optimized) {
			optimized = true;
			float s;
			Quaternion q;
			Vector3 t;
			Optimizer.Optimize(fingerTips, targets, out s, out q, out t);

			fingerTipRoot.transform.localPosition = t;
			fingerTipRoot.transform.localRotation = q;
			fingerTipRoot.transform.localScale = new Vector3(s, s, s);
			
			print ("Optimized root MSE: " + Mathf.Sqrt(CalculateMSE ()));
		}
	}

	private float CalculateMSE() {
		float mse = 0.0f;
		for (int i = 0; i < targetTransforms.Count; ++i) {
			float error = (targetTransforms[i].position - fingerTipTransforms[i].position).sqrMagnitude;
			mse += error;
		}
		return mse / targetTransforms.Count;
	}

	void ApplicationQuit() {
		file.Close ();
	}
}
