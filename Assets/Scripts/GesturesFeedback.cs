using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GesturesFeedback : MonoBehaviour {

	public IisuInputProvider InputProvider;
	
	private List<uint> _poses;
	
	void Awake()
	{
		_poses = new List<uint>();	
	}
	
	void Update () {
	
		//fetch new events registered since the last update
		List<uint> poses = InputProvider.DetectedPoses;		
		
		//add them to the events list, and if we have more than
		//10 events, remove the oldest.
		foreach(uint pose in poses)
		{
			_poses.Add(pose);
			if(_poses.Count == 10)
			{
				_poses.RemoveAt(0);	
			}
		}
	}
	
	void OnGUI()
	{
		GUI.Box(new Rect(10, 10, 150, 250),"");
	
		GUILayout.BeginArea(new Rect(10, 10, 150, 250));
		GUILayout.Label("Detected poses (id):");
		for (int i= _poses.Count - 1; i >= 0; --i)
		{
			GUILayout.Label(_poses[i].ToString());
		}
		GUILayout.EndArea();
	}
}
