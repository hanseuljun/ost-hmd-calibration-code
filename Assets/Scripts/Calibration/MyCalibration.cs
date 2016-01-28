using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class MyCalibration : MonoBehaviour
{
	public MyIisuInputProvider IisuInput;
	public MyColorImage colorImage;
	public MyDepthImage depthImage;
	
	private Texture2D depthMap;
	private float timer;
	
	void Awake()
	{
		timer = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//we update the depthmap 30fps
		if(timer >= 0.0333f)
		{
			timer = 0;
			
			int colorWidth = IisuInput.ColorMapWidth;
			int colorHeight = IisuInput.ColorMapHeight;
			
			Mat colorMat = new Mat(colorHeight, colorWidth, MatType.CV_8UC3);
			MyImageConvertor.generateColorImage(IisuInput.ColorMap, ref colorMat);

			int depthWidth = IisuInput.DepthMapWidth;
			int depthHeight = IisuInput.DepthMapHeight;
			
			Mat depthMat = new Mat(depthHeight, depthWidth, MatType.CV_16U);
			MyImageConvertor.generateDepthImage(IisuInput.DepthMap, ref depthMat);

//			Mat depthMat255 = UShortMatToByteMat(depthMat);
//			depthMat255 = depthMat255.BilateralFilter(5, 50.0, 50.0, BorderTypes.Reflect101);

			Mat depthMatFloat = UShortMatToFloatMat(depthMat);
			depthMatFloat = depthMatFloat.BilateralFilter(5, 50.0, 50.0, BorderTypes.Default);
			FilterFloatMat(depthMatFloat);

			colorImage.SetImage(colorMat);
			depthImage.SetFloatImage(depthMatFloat);
		}
		else
		{
			timer += Time.deltaTime;
		}
	}

	private Mat UShortMatToByteMat(Mat ushortMat)
	{
		int width = ushortMat.Width;
		int height = ushortMat.Height;

		MatOfUShort matUS = new MatOfUShort (ushortMat);
		var indexer = matUS.GetIndexer ();

		Mat byteMat = new Mat (height, width, MatType.CV_8U);
		MatOfByte matB = new MatOfByte (byteMat);
		var byteIndexer = matB.GetIndexer ();

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				ushort us = indexer[j, i];
				byteIndexer[j, i] = us > 2550 ? (byte) 255 : (byte) (us / 10);
			}
		}

		return byteMat;
	}
	
	private Mat UShortMatToFloatMat(Mat ushortMat)
	{
		int width = ushortMat.Width;
		int height = ushortMat.Height;
		
		MatOfUShort matUS = new MatOfUShort (ushortMat);
		var indexer = matUS.GetIndexer ();
		
		Mat floatMat = new Mat (height, width, MatType.CV_32F);
		MatOfFloat matFloat = new MatOfFloat (floatMat);
		var floatIndexer = matFloat.GetIndexer ();
		
		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				ushort us = indexer[j, i];
				floatIndexer[j, i] = (float) us;
			}
		}
		
		return floatMat;
	}

	private void FilterFloatMat(Mat mat)
	{
		int width = mat.Width;
		int height = mat.Height;
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				float value = indexer[j, i];
				if(value < 300.0f || value > 500.0f)
				{
					indexer[j, i] = 0.0f;
				}
			}
		}
	}
}
