using UnityEngine;
using System.Collections;
using Iisu;
using IisuUnity;

/// <summary>
/// Handles the iisu data from one specific hand
/// </summary>
public class HandIisuInput : MonoBehaviour {
	
	public IisuInputProvider InputProvider;
	public int HandNumber;
	
	private IDataHandle<Iisu.Data.Vector3> _tipPosition;
	private IDataHandle<Iisu.Data.Vector3> _thumbDirection;
	private IDataHandle<Iisu.Data.Vector3> _palmDirection;
	
	private IDataHandle<Iisu.Data.Vector3> _palmPosition;
	private IDataHandle<float> _palmRadius;
	
	private IDataHandle<Iisu.Data.Vector3[]> _fingerTipPositions;
	private IDataHandle<int[]> _fingersStatus;
	
	private IDataHandle<int> _handStatus;
	
	void Start()
	{
		//Registering data for the specific hand
		_tipPosition = InputProvider.Device.RegisterDataHandle<Iisu.Data.Vector3>("CI.HAND" + HandNumber + ".TipPosition3D");
		_palmDirection = InputProvider.Device.RegisterDataHandle<Iisu.Data.Vector3>("CI.HAND" + HandNumber + ".PalmNormal3D");
		
		_palmPosition = InputProvider.Device.RegisterDataHandle<Iisu.Data.Vector3>("CI.HAND" + HandNumber + ".PalmPosition3D");
		
		_fingerTipPositions = InputProvider.Device.RegisterDataHandle<Iisu.Data.Vector3[]>("CI.HAND" + HandNumber + ".FingerTipPositions3D");
		
		_fingersStatus = InputProvider.Device.RegisterDataHandle<int[]>("CI.HAND" + HandNumber + ".FingerStatus");
		_handStatus = InputProvider.Device.RegisterDataHandle<int>("CI.HAND" + HandNumber + ".Status");
	}
	
	//Meaning of the status values, taken from the developer guide:
	//0	Inactive
	//1	Just detected at current frame. No temporal coherence with the same data at previous frame.
	//2	Tracked. Temporal coherence with the same data at previous frame.
	//3	Extrapolated. Temporal coherence with the same data at previous frame however the object (hand or finger) has not been detected at current frame.
	public bool Detected
	{
		get
		{
			return _handStatus.Value >= 1;
		}
	}
	
	//Get all hand related data, and encapsulate it in a HandData object.
	public HandData HandPositions
	{
		get
		{
			HandData data = new HandData();
			
			data.PalmPosition = _palmPosition.Value.ToUnityVector3();
			data.PalmDirection = _palmDirection.Value.ToUnityVector3();
			data.PointingDirection = (_tipPosition.Value.ToUnityVector3() - _palmPosition.Value.ToUnityVector3());
			
			data.FingerTipPositions = _fingerTipPositions.Value.ToUnityVector3Array();
			
			int[] fingerStatus = _fingersStatus.Value;
			
			for(int i=0; i< data.FingerStatus.Length; ++i)
			{
				data.FingerStatus[i] = (fingerStatus[i] >= 1);
			}
			
			return data;
		}
	}
}

