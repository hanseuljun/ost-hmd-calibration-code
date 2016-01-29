using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class Calibration : MonoBehaviour
{
	public MyIisuInputProvider IisuInput;
	public MyColorImage colorImage;
	public MyDepthImage depthImage;
	public DepthMesh depthMesh;
	
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

			Mat depthMatFloat = UShortMatToFloatMat(depthMat);
			depthMatFloat = depthMatFloat.BilateralFilter(5, 5.0, 5.0, BorderTypes.Constant);
//			depthMatFloat = depthMatFloat.BilateralFilter(5, 10000.0, 10000.0, BorderTypes.Reflect101);

			int fingerI;
			int fingerJ;
			FilterFloatMat(depthMatFloat, out fingerI, out fingerJ);

			colorImage.SetImage(colorMat);
			depthImage.SetFloatImage(depthMatFloat, fingerI, fingerJ);

			depthMesh.SetFloatMat(depthMatFloat);
			depthMesh.SetTexture(colorImage.ColorMap);
		}
		else
		{
			timer += Time.deltaTime;
		}
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
				floatIndexer[j, i] = (float) us * 0.001f; // millis to meters
			}
		}
		
		return floatMat;
	}

	private void FilterFloatMat(Mat mat, out int fingerI, out int fingerJ)
	{
		int width = mat.Width;
		int height = mat.Height;
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		fingerI = 0;
		fingerJ = int.MaxValue;

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				float value = indexer[j, i];
				if(value < 0.25f || value > 0.6f)
				{
					indexer[j, i] = 0.0f;
				}
				else if(j < fingerJ)
				{
					fingerI = i;
					fingerJ = j;
				}
			}
		}
	}
}
