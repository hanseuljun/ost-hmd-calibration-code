/***************************************************************************************/
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

public class MyDepthImage : MonoBehaviour
{
	public MyIisuInputProvider IisuInput;

	public Texture2D DepthMap;
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	
	private float _heightWidthRatio;
	
	private float _timer;
	
	void Awake()
	{
		_timer = 0;
		_heightWidthRatio = 120f/160f;
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
			
			if (DepthMap == null)
			{
				DepthMap = new Texture2D(160, 120, TextureFormat.ARGB32, false);
			}

			ushort[] values = new ushort[0];
			MyImageConvertor.generateDepthImage(IisuInput.DepthMap, 160, 120, ref values);

			Color[] pixels = new Color[values.Length];
			
			for(int i = 0; i < values.Length; ++i)
			{
				//normalize depth value in millimeter so that 5m <-> color 255
				ushort depthValue = (ushort)(values[i] * 255 / (5000));
				if (depthValue > 255) depthValue = 255;
				pixels[i] = new Color(depthValue / 255f, depthValue / 255f, depthValue / 255f, 1);
			}
			
			DepthMap.SetPixels(pixels);
			DepthMap.Apply();
			
		}
		else
		{
			_timer += Time.deltaTime;
		}
	}
	
	void OnGUI()
	{
		if (DepthMap != null)
		{
			GUI.DrawTexture(new Rect(Screen.width * NormalizedXCoordinate + Screen.width * NormalizedWidth,
			                         Screen.height * NormalizedYCoordinate + Screen.width * NormalizedWidth * _heightWidthRatio,
			                         -Screen.width * NormalizedWidth,
			                         -Screen.width * NormalizedWidth * _heightWidthRatio), DepthMap);
		}
	}
}
