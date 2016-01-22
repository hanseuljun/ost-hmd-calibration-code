﻿/***************************************************************************************/
//
//  SoftKinetic iisu SDK code samples 
//
//  Project Name      : skeletonBubbleManSample
//  Revision          : 1.0
//  Description       : Tutorial on how to use the Skeleton and Bubbleman. 
//						It is recommended to use this sample to get started, 
//						as it covers the most common uses of iisu: skeleton, 
//						bubbleman and displaying the depthmap + usermask.
//
// DISCLAIMER
// All rights reserved to SOFTKINETIC INTERNATIONAL SA/NV (a company incorporated
// and existing under the laws of Belgium, with its principal place of business at
// Boulevard de la Plainelaan 15, 1050 Brussels (Belgium), registered with the Crossroads
// bank for enterprises under company number 0811 784 189 - “Softkinetic”)
//
// For any question about terms and conditions, please contact: info@softkinetic.com
// Copyright (c) 2007-2011 SoftKinetic SA/NV
//
/****************************************************************************************/

using UnityEngine;
using System.Collections;

public class MyColorImage : MonoBehaviour
{
	public MyIisuInputProvider IisuInput;
	
	public Texture2D ColorMap;
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	
	private float _heightWidthRatio;
	
	private float _timer;
	
	void Awake()
	{
		_timer = 0;
	}
	
	/// <summary>
	/// We get the depth image from iisu, which is in a 16bit grey image format 
	/// The image is converted by the ImageConvertor class to a Unity image, and then applied to the 2D GUI texture
	/// </summary>	
	void Update()
	{
		//we update the depthmap 30fps
		if(_timer >= 0.0333f)
		{
			_timer = 0;
			
			if (ColorMap == null)
			{
				ColorMap = new Texture2D(IisuInput.ColorMapWidth, IisuInput.ColorMapHeight, TextureFormat.ARGB32, false);
			}
			
			Color32[] values = new Color32[0];
			MyImageConvertor.generateColorImage(IisuInput.ColorMap, IisuInput.ColorMapWidth, IisuInput.ColorMapHeight, ref values);
			
			ColorMap.SetPixels32(values);
			ColorMap.Apply();
			
		}
		else
		{
			_timer += Time.deltaTime;
		}
	}
	
	void OnGUI()
	{
		if (ColorMap != null)
		{
			_heightWidthRatio = (float) IisuInput.ColorMapHeight / (float) IisuInput.ColorMapWidth;
			
			GUI.DrawTexture(new Rect(Screen.width * NormalizedXCoordinate + Screen.width * NormalizedWidth,
			                         Screen.height * NormalizedYCoordinate + Screen.width * NormalizedWidth * _heightWidthRatio,
			                         -Screen.width * NormalizedWidth,
			                         -Screen.width * NormalizedWidth * _heightWidthRatio), ColorMap);
		}
	}
}