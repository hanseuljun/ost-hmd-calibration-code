using UnityEngine;
using System.Collections;

public class HandVisualizer : MonoBehaviour {

	public HandIisuInput HandInput;
	
	public GameObject FingerTipPrefab;
	public GameObject PalmPrefab;
	
	public Color HandColor;
	
	public float PalmScale;
	
	private Transform[] _fingers;
	private Transform _palm;
	
	void Awake()
	{
		//initialize all finger/palm primitives
		_fingers = new Transform[5];
		for(int i=0; i<_fingers.Length; ++i)
		{
			_fingers[i] = ((GameObject)Instantiate(FingerTipPrefab)).transform;
			_fingers[i].GetComponent<Renderer>().material.color = HandColor;
			_fingers[i].parent = transform.parent;
			_fingers[i].GetComponent<Renderer>().enabled = false;
		}
		
		_palm = ((GameObject)Instantiate(PalmPrefab)).transform;
		_palm.GetComponent<Renderer>().material.color = HandColor;
		_palm.parent = transform.parent;
		_palm.GetComponent<Renderer>().enabled = false;
	}
	
	private UnityEngine.Vector3 getMiddle(Vector3 j1, Vector3 j2)
	{
		return new UnityEngine.Vector3((j1.x + j2.x)/2,
		                               (j1.y + j2.y)/2,
		                               (j1.z + j2.z)/2 );
	}
	
	private UnityEngine.Vector3 getDirection(Vector3 from, Vector3 to)
	{
		return new UnityEngine.Vector3(to.x - from.x,
		                               to.y - from.y,
		                               to.z - from.z );
	}
	
	private float getScale(Vector3 j1, Vector3 j2)
	{
		return getDirection(j1, j2).magnitude;
	}
	
	private void setFinger(int index, HandData data)
	{
		//if the finger is not detected, we do not render it
		if(!data.FingerStatus[index] && _fingers[index].GetComponent<Renderer>().enabled)
		{
			_fingers[index].GetComponent<Renderer>().enabled = false;	
		}
		//otherwise, we do render it
		else if(data.FingerStatus[index] && !_fingers[index].GetComponent<Renderer>().enabled)
		{
			_fingers[index].GetComponent<Renderer>().enabled = true;
		}
		
		_fingers[index].localPosition = data.FingerTipPositions[index];
	}
	
	private void setPalm(HandData data)
	{
		//if the palm is detected, we render it
		if(!_palm.GetComponent<Renderer>().enabled)
			_palm.GetComponent<Renderer>().enabled = true;
		
		//set the position of the palm
		_palm.localPosition = new Vector3(data.PalmPosition.x, data.PalmPosition.y, data.PalmPosition.z);
		//set the orientation of the palm., using the PalmDirection as the lookat vector, 
		//and the PointingDirection as the up vector
		_palm.LookAt(_palm.position + data.PalmDirection, data.PointingDirection);
	}
	
	private void disableHandRenderers()
	{
		for(int i=0; i<5; ++i)
		{
			_fingers[i].GetComponent<Renderer>().enabled = false;
		}
		
		_palm.GetComponent<Renderer>().enabled = false;
	}
		
	void Update () {
	
		if(HandInput.Detected)
		{
			HandData data = HandInput.HandPositions;
			
			setPalm(data);
			
			for(int i=0; i<5; ++i)
			{
				setFinger(i, data);	
			}
		}
		else
		{
			if(_palm.GetComponent<Renderer>().enabled)
				disableHandRenderers();
		}
	}
	
}