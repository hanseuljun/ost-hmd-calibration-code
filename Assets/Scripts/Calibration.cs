using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class Calibration : MonoBehaviour {
	public IisuInputProvider IisuInput;
	public ColorImage colorImage;
	public DepthImage depthImage;
	public SkinImage skinImage;
	public BlobImage blobImage;
	public DepthMesh depthMesh;
	public Transform depthCameraRig;
	public StereoCamera stereoCamera;
	public Transform target;
	public Text text;
	public byte skinThreshold;
	
	private Texture2D colorMap;
	private float timer;
	private bool measureFingerTip;
	private List<Vector3> targetPositions;
	private List<Vector3> fingerTipPositions;

	private List<MeshFilter> savedMeshFilters;
	private List<PointCloud> savedPointClouds;

	void Start() {
		Cursor.visible = false;
		timer = 0;
		measureFingerTip = false;
		//file = new CalibrationFile ();
		targetPositions = new List<Vector3> ();
		fingerTipPositions = new List<Vector3> ();
		savedMeshFilters = new List<MeshFilter> ();
		savedPointClouds = new List<PointCloud> ();
		RandomizeTarget ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.A)) {
			print ("measureFingerTip");
			measureFingerTip = true;
		}
		else if (Input.GetKeyDown (KeyCode.S)) {
			float s;
//			Quaternion q;
			Quaternion q = Quaternion.identity;
			Vector3 t;
//			Optimizer.Optimize (file.fingerTips, file.targets, out s, out q, out t);
			Optimizer.Optimize (fingerTipPositions, targetPositions, q, out s, out t);
			
//			stereoCamera.ipd = stereoCamera.ipd * s;
//			depthCameraRig.localPosition = t;
//			depthCameraRig.localRotation = q;

			depthCameraRig.localPosition = t * s;
			depthCameraRig.localRotation = q;
			depthCameraRig.localScale = new Vector3 (s, s, s);
		}
		else if (Input.GetKeyDown (KeyCode.D)) {
			depthCameraRig.localPosition = Vector3.zero;
			depthCameraRig.localRotation = Quaternion.identity;
			depthCameraRig.localScale = Vector3.one;
		}
		else if (Input.GetKeyDown (KeyCode.F)) {
			bool active = !depthMesh.gameObject.activeSelf;
			depthMesh.gameObject.SetActive (active);
			Color background;
			if(active) {
				background = new Color(0.0f, 0.0f, 0.0f);
			}
			else {
				background = new Color(0.5f, 0.5f, 1.0f);
			}
			stereoCamera.background = background;
		}
		//we update the depthmap 30fps
		if (timer >= 0.0333f) {
			timer = 0;
			
			int colorWidth = IisuInput.ColorMapWidth;
			int colorHeight = IisuInput.ColorMapHeight;

			if (colorMap == null) {
				colorMap = new Texture2D (colorWidth, colorHeight);
				colorImage.SetImage (colorMap);
			}
			
			Mat colorMat = new Mat (colorHeight, colorWidth, MatType.CV_8UC3);
			ImageConverter.GenerateColorMat (IisuInput.ColorMap, ref colorMat);
			ImageConverter.GenerateColorMap (colorMat, ref colorMap);

			int depthWidth = IisuInput.DepthMapWidth;
			int depthHeight = IisuInput.DepthMapHeight;

			Mat depthMat = new Mat (depthHeight, depthWidth, MatType.CV_32F);
			ImageConverter.GenerateDepthMat (IisuInput.DepthMap, ref depthMat);
			
			Mat skinMat = SkinImage.ConvertColorMat(colorMat, skinThreshold);

			int fingerI;
			int fingerJ;
			FilterFloatMat (depthMat, out fingerI, out fingerJ);

			Mat blobMat = BlobImage.ConvertDepthMat (depthMat);

			if (depthImage.enabled) {
				depthImage.UpdateFloatMat (depthMat, fingerI, fingerJ);
			}

			if (skinImage.enabled) {
				skinImage.SetSkinMat (skinMat);
			}

			if (blobImage.enabled) {
				blobImage.SetBlobMat (blobMat);
			}

			depthMesh.SetFloatMat (depthMat, skinMat, blobMat);
			depthMesh.SetTexture (colorMap);

			text.text = depthMesh.fingerTip.localPosition.z.ToString ();

			if (measureFingerTip) {
				measureFingerTip = false;
				targetPositions.Add(target.position);
				fingerTipPositions.Add(depthMesh.fingerTip.position);
				RandomizeTarget ();
			}
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

	private void RandomizeTarget() {
		System.Random random = new System.Random ();
		while (true) {
			float x = ((float)random.NextDouble () - 0.5f) * 0.4f;
			float y = ((float)random.NextDouble () - 0.5f) * 0.4f;
			float z = ((float)random.NextDouble ()) * 0.25f + 0.35f;
			Vector3 v = new Vector3(x, y, z);
			if(stereoCamera.IsInside(v)) {
				target.position = v;
				break;
			}
		}
	}
}
