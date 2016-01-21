using UnityEngine;
using System.Collections;
using Iisu;
using IisuUnity;
using System;
using System.Collections.Generic;

/// <summary>
/// Takes care of the communication between iisu and the Unity application by providing
/// the necessary data from iisu
/// </summary>
public class IisuInputProvider : MonoBehaviour
{

	//the IisuUnityBehaviour object handles the iisu device, including its update thread, and disposing.
	private IisuUnityBehaviour _iisuUnity;

	private IDataHandle<Iisu.Data.IImageData> _depthImage;
	private IDataHandle<Iisu.Data.IImageData> _labelImage;
	private IDataHandle<int> _hand1ID;
	private IDataHandle<int> _hand2ID;
	
	private delegate void OnPoseDelegate(string gestureName, int handId1, int handId2, uint gestureId);
	
	private List<uint> _poseIDsDetected;
	
	void Awake ()
	{
		//this has to be done first. Inside the IisuUnityBehaviour object, iisu is initialized, and the update thread for the current device (camera, movie) is started
		_iisuUnity = GetComponent<IisuUnityBehaviour> ();
		_iisuUnity.Initialize ();
		
		//register iisu data needed to display the depthimage
		_depthImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.DEPTH.Image");
		
		_hand1ID = _iisuUnity.Device.RegisterDataHandle<int> ("CI.HAND1.Label");
		_hand2ID = _iisuUnity.Device.RegisterDataHandle<int> ("CI.HAND2.Label");
		_labelImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("CI.SceneLabelImage");
		
		_iisuUnity.Device.EventManager.RegisterEventListener("CI.HandPosingGesture", new OnPoseDelegate(OnPoseEvent));
		
		_poseIDsDetected = new List<uint>();
	}
	
	public List<uint> DetectedPoses
	{
		get
		{
			List<uint> poses = new List<uint>(_poseIDsDetected);
			_poseIDsDetected.Clear();
			return poses;	
		}
	}
	
	private void OnPoseEvent(string gestureName, int handId1, int handId2, uint gestureId)
	{
		_poseIDsDetected.Add(gestureId);
	}
	
	public IDevice Device 
	{
		get 
		{ 
			return _iisuUnity.Device; 
		}
	}

	public Iisu.Data.IImageData DepthMap 
	{
		get 
		{ 
			return _depthImage.Value; 
		}
	}

	/// <summary>
	/// The IDs of the label image indicate which pixels of the depthmap belong to a certain object, in this case the hand.
	/// </summary>
	public int Hand1Label 
	{
		get 
		{ 
			return _hand1ID.Value; 
		}
	}

	/// <summary>
	/// The IDs of the label image indicate which pixels of the depthmap belong to a certain object, in this case the hand.
	/// </summary>
	public int Hand2Label 
	{
		get 
		{ 
			return _hand2ID.Value; 
		}
	}

	/// <summary>
	/// Provides the label image that contains the IDs for each depth pixel to define pixels that belong to the same object in the scene.
	/// </summary>
	public Iisu.Data.IImageData LabelImage 
	{
		get 
		{ 
			return _labelImage.Value; 
		}
	}
	
}
