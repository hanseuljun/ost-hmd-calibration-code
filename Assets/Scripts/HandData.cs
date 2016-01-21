using UnityEngine;
using System.Collections;

//Encapsulate all hand related data
public class HandData
{
	public Vector3 PalmPosition;
	
	public Vector3 PalmDirection;
	public Vector3 PointingDirection;
	
	public Vector3[] FingerTipPositions;

	public bool[] FingerStatus;
	public bool HandStatus;

	public HandData ()
	{
		FingerTipPositions = new Vector3[5];
		FingerStatus = new bool[5];
	}
	
}
