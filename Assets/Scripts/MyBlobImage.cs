﻿using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;

public class MyBlobImage : MonoBehaviour
{	
	public MyIisuInputProvider IisuInput;
	
	public Texture2D BlobMap { get; private set; }
	
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;

	public static Mat ConvertDepthMat(Mat mat) {
		int width = mat.Width;
		int height = mat.Height;
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();
		
		Mat byteMat = new Mat (height, width, MatType.CV_8U);
		MatOfByte matByte = new MatOfByte (byteMat);
		var byteIndexer = matByte.GetIndexer ();
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				byteIndexer[j, i] = indexer[j, i] > 0.0f ? (byte) 255 : (byte) 0;
			}
		}
		
		CvBlobs blobs = new CvBlobs (byteMat);
		blobs.FilterByArea (blobs.LargestBlob ().Area / 2, int.MaxValue);

		Mat blobMat = new Mat (height, width, MatType.CV_8UC3);
		MatOfByte3 blobMatByte = new MatOfByte3 (blobMat);
		var blobIndexer = blobMatByte.GetIndexer ();
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				blobIndexer[j, i] = new Vec3b(0, 0, 0);
			}
		}
		
		//Colors the blob by (127, 127, 255)
		blobs.RenderBlobs (blobMat, blobMat, RenderBlobsMode.Color);
		
		return blobMat;
	}
	
	public void SetBlobMat(Mat mat)
	{
		int width = mat.Width;
		int height = mat.Height;

		MatOfByte3 blobMatByte = new MatOfByte3 (mat);
		var blobIndexer = blobMatByte.GetIndexer ();

		if (enabled) {
			if (BlobMap == null) {
				BlobMap = new Texture2D (width, height, TextureFormat.ARGB32, false);
			}

			Color[] pixels = new Color[width * height];
		
			for (int i = 0; i < width; ++i) {
				for (int j = 0; j < height; ++j) {
					pixels [i + (height - 1 - j) * width] = blobIndexer [j, i].Item0 != 0 ? Color.white : Color.black;
				}
			}

			BlobMap.SetPixels (pixels);
			BlobMap.Apply ();
		}
	}
	
	void OnGUI()
	{
		if (BlobMap != null)
		{
			float heightWidthRatio = (float) BlobMap.height / (float) BlobMap.width;
			
			GUI.DrawTexture(new UnityEngine.Rect(Screen.width * NormalizedXCoordinate,
			                                     Screen.height * NormalizedYCoordinate,
			                                     Screen.width * NormalizedWidth,
			                                     Screen.width * NormalizedWidth * heightWidthRatio), BlobMap);
		}
	}
}