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
using OpenCvSharp;

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

			int width = IisuInput.ColorMapWidth;
			int height = IisuInput.ColorMapHeight;

			if (ColorMap == null)
			{
				ColorMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
			}
			
			Mat mat = new Mat(height, width, MatType.CV_8UC3);
			MyImageConvertor.generateColorImage(IisuInput.ColorMap, ref mat);
			
			Color32[] pixels = new Color32[width * height];

			MatOfByte3 matB3 = new MatOfByte3 (mat);
			var indexer = matB3.GetIndexer ();

			for(int i = 0; i < width; ++i)
			{
				for(int j = 0; j < height; ++j)
				{
					Vec3b color = indexer[j, i];
					pixels[i + j * width] = new Color32(color.Item0, color.Item1, color.Item2, (byte) 255);
				}
			}

			ColorMap.SetPixels32(pixels);
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
			
			GUI.DrawTexture(new UnityEngine.Rect(Screen.width * NormalizedXCoordinate + Screen.width * NormalizedWidth,
			                         Screen.height * NormalizedYCoordinate + Screen.width * NormalizedWidth * _heightWidthRatio,
			                         -Screen.width * NormalizedWidth,
			                         -Screen.width * NormalizedWidth * _heightWidthRatio), ColorMap);
		}
	}
}
