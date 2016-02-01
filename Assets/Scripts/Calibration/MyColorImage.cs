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
	
	public Texture2D ColorMap { get; private set; }
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;

	public void SetImage(Mat mat)
	{
		int width = mat.Width;
		int height = mat.Height;

		if (ColorMap == null) {
			ColorMap = new Texture2D(width, height);
		}
		
		Color32[] pixels = new Color32[width * height];
		
		MatOfByte3 matB3 = new MatOfByte3 (mat);
		var indexer = matB3.GetIndexer ();

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				Vec3b color = indexer[j, i];
				pixels[i + (height - 1 - j) * width] = new Color32(color.Item0, color.Item1, color.Item2, (byte) 255);
			}
		}
		
		ColorMap.SetPixels32(pixels);
		ColorMap.Apply();
	}
	
	void OnGUI()
	{
		if (ColorMap != null)
		{
			float heightWidthRatio = (float) ColorMap.height / (float) ColorMap.width;
			
			GUI.DrawTexture(new UnityEngine.Rect(Screen.width * NormalizedXCoordinate,
			                         Screen.height * NormalizedYCoordinate,
			                         Screen.width * NormalizedWidth,
			                         Screen.width * NormalizedWidth * heightWidthRatio), ColorMap);
		}
	}
}
