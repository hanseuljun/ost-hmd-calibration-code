using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class ButtonSimulation : MonoBehaviour {
	public IisuInputProvider IisuInput;
	public DepthMesh depthMesh;
	public Transform depthCameraRig;
	public StereoCamera stereoCamera;

	private GestureDetector detector;
	private Texture2D colorMap;
	private float timer;

	void Start() {
		detector = new GestureDetector ();
		Cursor.visible = false;
		timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
		//we update the depthmap 30fps
		if (timer >= 0.0333f) {
			timer = 0;
			
			int colorWidth = IisuInput.ColorMapWidth;
			int colorHeight = IisuInput.ColorMapHeight;
			
			if (colorMap == null) {
				colorMap = new Texture2D (colorWidth, colorHeight);
			}
			
			Mat colorMat = new Mat (colorHeight, colorWidth, MatType.CV_8UC3);
			ImageConverter.GenerateColorMat (IisuInput.ColorMap, ref colorMat);
			ImageConverter.GenerateColorMap (colorMat, ref colorMap);
			
			int depthWidth = IisuInput.DepthMapWidth;
			int depthHeight = IisuInput.DepthMapHeight;
			
			Mat depthMat = new Mat (depthHeight, depthWidth, MatType.CV_32F);
			ImageConverter.GenerateDepthMat (IisuInput.DepthMap, ref depthMat);
			
			Mat bilateralMat = depthMat.BilateralFilter (5, 5.0, 5.0, BorderTypes.Constant);
			depthMat = bilateralMat;
			
			int fingerI;
			int fingerJ;
			FilterFloatMat (depthMat, out fingerI, out fingerJ);
			
			Mat blobMat = BlobImage.ConvertDepthMat (depthMat);
			
			depthMesh.SetFloatMat (depthMat, blobMat);
			depthMesh.SetTexture (colorMap);

			Vector3 fingerTipPosition = depthMesh.fingerTip.position;
			detector.Update(fingerTipPosition);
			detector.GetGesture();
		}
		else {
			timer += Time.deltaTime;
		}
	}
	
	private void FilterFloatMat(Mat mat, out int fingerI, out int fingerJ) {
		int width = mat.Width;
		int height = mat.Height;
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();
		
		fingerI = 0;
		fingerJ = int.MaxValue;
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				float value = indexer [j, i];
				if (value < 0.25f || value > 0.65f) {
					indexer [j, i] = 0.0f;
				}
				else if (j < fingerJ) {
					fingerI = i;
					fingerJ = j;
				}
			}
		}
	}
}
