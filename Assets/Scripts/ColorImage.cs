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

public class ColorImage : MonoBehaviour
{
	public IisuInputProvider IisuInput;
	
	private Texture2D colorMap;
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;

	public static void ConvertColorMat(Mat mat, ref Texture2D colorMap) {
		int width = mat.Width;
		int height = mat.Height;
		
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
		
		colorMap.SetPixels32(pixels);
		colorMap.Apply();
	}

	public void SetImage(Texture2D colorMap)
	{
		this.colorMap = colorMap;
	}
	
	void OnGUI()
	{
		if (colorMap != null)
		{
			float heightWidthRatio = (float) colorMap.height / (float) colorMap.width;
			
			GUI.DrawTexture(new UnityEngine.Rect(Screen.width * NormalizedXCoordinate,
			                         Screen.height * NormalizedYCoordinate,
			                         Screen.width * NormalizedWidth,
			                                     Screen.width * NormalizedWidth * heightWidthRatio), colorMap);
		}
	}
}
