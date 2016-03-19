using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Blob;

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
	public Text lowerText;
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
		ResetDepthCameraRig();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F1)) {
			bool textEnabled = !text.enabled;
			text.enabled = textEnabled;
			lowerText.enabled = textEnabled;
		}
		else if (Input.GetKeyDown (KeyCode.A)) {
			print ("measureFingerTip");
			measureFingerTip = true;
		}
		else if (Input.GetKeyDown (KeyCode.S)) {
			if(targetPositions.Count >= 2)
			{
				float s;
				Quaternion q;
				Vector3 t;
				Optimizer.Optimize (fingerTipPositions, targetPositions, out s, out q, out t);
				
				t = t * s;
				
				float s0 = depthCameraRig.localScale.x;
				Quaternion q0 = depthCameraRig.localRotation;
				Vector3 t0 = depthCameraRig.localPosition;

				float newS = s * s0;
				Quaternion newQ = q * q0;
				Vector3 newT = s * (q * t0) + t;

//				depthCameraRig.localPosition = newT;
//				depthCameraRig.localRotation = newQ;
//				depthCameraRig.localScale = Vector3.one * newS;
				depthCameraRig.localPosition = t;
				depthCameraRig.localRotation = q;
				depthCameraRig.localScale = Vector3.one * s;
				targetPositions.Clear();
				fingerTipPositions.Clear();
			}
		}
		else if (Input.GetKeyDown (KeyCode.D)) {
			if(targetPositions.Count >= 3){
				float s;
				Vector3 t;
				Optimizer.Optimize (fingerTipPositions, targetPositions, depthCameraRig.localRotation, out s, out t);
				
				t = t * s;
				
				float s0 = depthCameraRig.localScale.x;
				Quaternion q0 = depthCameraRig.localRotation;
				Vector3 t0 = depthCameraRig.localPosition;
				
				float newS = s * s0;
				Vector3 newT = s * t0 + t;
				
//				depthCameraRig.localPosition = newT;
//				depthCameraRig.localScale = Vector3.one * newS;
				depthCameraRig.localPosition = t;
				depthCameraRig.localScale = Vector3.one * s;
				targetPositions.Clear();
				fingerTipPositions.Clear();
			}
		}
		else if (Input.GetKeyDown (KeyCode.F)) {
			ResetDepthCameraRig();
			targetPositions.Clear();
			fingerTipPositions.Clear();
		}
		else if (Input.GetKeyDown (KeyCode.G)) {
			bool active = !depthMesh.gameObject.activeSelf;
			depthMesh.gameObject.SetActive (active);
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

			MatOfFloat depthMat = new MatOfFloat (depthHeight, depthWidth);
			ImageConverter.GenerateDepthMat (IisuInput.DepthMap, ref depthMat);

			MatOfByte validMat = HandExtractor.GenerateValidMat (depthMat, 0.02f, 1.0f);
			CvBlobs validMatBlobs = new CvBlobs (validMat);
			CvBlob handBlob = HandExtractor.ExtractHandBlob (depthMat, validMatBlobs);
			
			if (handBlob.Rect.Width > 200 && handBlob.Rect.Height > 100) {
				return; // it is viewing the whole scene
			}
			
			MatOfByte handBlobMat = HandExtractor.GenerateBlobMat (validMatBlobs, handBlob);
			
			MatOfByte closeMat = new MatOfByte(handBlobMat.Height, handBlobMat.Width);
			Cv2.MorphologyEx (handBlobMat, closeMat, MorphTypes.Close, null, null, 1, BorderTypes.Constant, 0);
			handBlobMat = closeMat;
			
			MatOfFloat handMat = HandExtractor.GenerateHandMat (depthMat, handBlobMat, handBlob);
			
//			Mat skinMat = SkinImage.ConvertColorMat(colorMat, skinThreshold);

			int fingerI;
			int fingerJ;
			FilterFloatMat (depthMat, out fingerI, out fingerJ);

//			Mat blobMat = BlobImage.ConvertDepthMat (depthMat);

			if (depthImage.enabled) {
				depthImage.UpdateFloatMat (depthMat, fingerI, fingerJ);
			}

//			if (skinImage.enabled) {
//				skinImage.SetSkinMat (skinMat);
//			}

//			if (blobImage.enabled) {
//				blobImage.SetBlobMat (blobMat);
//			}

			depthMesh.SetFloatMat (handMat);
			depthMesh.SetTexture (colorMap);

			Vector3 translation = depthCameraRig.localPosition;
			Vector3 rotation = depthCameraRig.localRotation.eulerAngles;
			Vector3 scale = depthCameraRig.localScale;

			text.text = "Measured " + targetPositions.Count + " times"
				+ string.Format("\nTranslation: {0}, {1}, {2}", translation.x, translation.y, translation.z)
				+ string.Format("\nRotation: {0}, {1}, {2}", rotation.x, rotation.y, rotation.z)
				+ string.Format("\nScale: {0}", scale.x);

			if (measureFingerTip) {
				measureFingerTip = false;
				targetPositions.Add(target.position);
				fingerTipPositions.Add(depthMesh.fingerTip.localPosition);
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

	private void ResetDepthCameraRig() {
		depthCameraRig.localPosition = new Vector3(0.0f, 0.02f, 0.05f);
		depthCameraRig.localRotation = Quaternion.Euler (0.0f, 0.0f, 10.0f);
		depthCameraRig.localScale = Vector3.one * 1.1f;
	}
}
