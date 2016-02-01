using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class Calibration : MonoBehaviour
{
	public IisuInputProvider IisuInput;
	public ColorImage colorImage;
	public DepthImage depthImage;
	public BlobImage blobImage;
	public DepthMesh depthMesh;

	private Texture2D colorMap;
	private float timer;
	
	void Awake()
	{
		Cursor.visible = false;
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

			if(colorMap == null) {
				colorMap = new Texture2D(colorWidth, colorHeight);
				colorImage.SetImage(colorMap);
			}
			
			Mat colorMat = new Mat(colorHeight, colorWidth, MatType.CV_8UC3);
			ImageConvertor.generateColorImage(IisuInput.ColorMap, ref colorMat);
			ColorImage.ConvertColorMat(colorMat, ref colorMap);

			int depthWidth = IisuInput.DepthMapWidth;
			int depthHeight = IisuInput.DepthMapHeight;

			Mat depthMat = new Mat(depthHeight, depthWidth, MatType.CV_32F);
			ImageConvertor.generateDepthImage(IisuInput.DepthMap, ref depthMat);

			Mat bilateralMat = depthMat.BilateralFilter(5, 5.0, 5.0, BorderTypes.Constant);
			depthMat = bilateralMat;

			int fingerI;
			int fingerJ;
			FilterFloatMat(depthMat, out fingerI, out fingerJ);
			
			Mat blobMat = BlobImage.ConvertDepthMat(depthMat);

			if(depthImage.enabled) {
				depthImage.UpdateFloatMat(depthMat, fingerI, fingerJ);
			}

			if(blobImage.enabled) {
				blobImage.SetBlobMat(blobMat);
			}

			depthMesh.SetFloatMat(depthMat, blobMat);
			depthMesh.SetTexture(colorMap);
		}
		else
		{
			timer += Time.deltaTime;
		}
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
