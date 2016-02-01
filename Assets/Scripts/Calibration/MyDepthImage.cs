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

public class MyDepthImage : MonoBehaviour
{
	public MyIisuInputProvider IisuInput;

	public Texture2D DepthMap;

	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;

	public void SetUShortImage(Mat mat)
	{	
		int width = mat.Width;
		int height = mat.Height;

		if(DepthMap == null)
		{
			DepthMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
		}

		Color[] pixels = new Color[width * height];
		
		MatOfUShort matUS = new MatOfUShort (mat);
		var indexer = matUS.GetIndexer ();
		
		float multiplier = 1.0f / 5000.0f;
		
		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				float depthValue = 1.0f - (indexer[j, i] * multiplier);
				pixels[i + (height - 1 - j) * width] = new Color(depthValue, depthValue, depthValue, 1);
			}
		}
		
		DepthMap.SetPixels(pixels);
		DepthMap.Apply();
	}
	
	public void SetFloatImage(Mat mat, int fingerI, int fingerJ)
	{	
		int width = mat.Width;
		int height = mat.Height;
		
		if(DepthMap == null)
		{
			DepthMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
		}
		
		Color[] pixels = new Color[width * height];
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		float multiplier = 1.0f / 1.0f; // up to 1m

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				float depthValue = indexer[j, i] * multiplier;
				pixels[i + (height - 1 - j) * width] = new Color(depthValue, depthValue, depthValue, 1);
			}
		}

		if (fingerJ != int.MaxValue) {
			for(int i = -1; i < 2; ++i)
			{
				for(int j = -1; j < 2; ++j)
				{
					int ii = fingerI + i;
					int jj = fingerJ + j;
					if(ii >= 0 && ii < width && jj >= 0 && jj < height)
					{
						pixels [ii + (height - 1 - jj) * width] = Color.red;
					}
				}
			}
		}
		DepthMap.SetPixels(pixels);
		DepthMap.Apply();
	}
	
	void OnGUI()
	{
		if (DepthMap != null)
		{
			float heightWidthRatio = (float) DepthMap.height / (float) DepthMap.width;

			GUI.DrawTexture(new UnityEngine.Rect(Screen.width * NormalizedXCoordinate,
			                         Screen.height * NormalizedYCoordinate,
			                         Screen.width * NormalizedWidth,
			                         Screen.width * NormalizedWidth * heightWidthRatio), DepthMap);
		}
	}
}
