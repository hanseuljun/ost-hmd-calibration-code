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
public class MyIisuInputProvider : MonoBehaviour
{
	
	//the IisuUnityBehaviour object handles the iisu device, including its update thread, and disposing.
	private IisuUnityBehaviour _iisuUnity;
	
	private IDataHandle<Iisu.Data.IImageData> _colorImage;
	private IDataHandle<Iisu.Data.IImageData> _depthImage;
	
	private List<uint> _poseIDsDetected;
	
	void Awake ()
	{
		//this has to be done first. Inside the IisuUnityBehaviour object, iisu is initialized, and the update thread for the current device (camera, movie) is started
		_iisuUnity = GetComponent<IisuUnityBehaviour> ();
		_iisuUnity.Initialize ();
		
		_colorImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.COLOR.Image");

		//register iisu data needed to display the depthimage
		_depthImage = _iisuUnity.Device.RegisterDataHandle<Iisu.Data.IImageData> ("SOURCE.CAMERA.DEPTH.Image");
		
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
	
	public IDevice Device 
	{
		get 
		{ 
			return _iisuUnity.Device; 
		}
	}
	
	public Iisu.Data.IImageData ColorMap 
	{
		get 
		{ 
			return _colorImage.Value; 
		}
	}
	
	public Iisu.Data.IImageData DepthMap 
	{
		get 
		{ 
			return _depthImage.Value; 
		}
	}
	
}
