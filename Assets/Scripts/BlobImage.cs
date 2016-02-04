using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;

public class BlobImage : MonoBehaviour {	
	public IisuInputProvider IisuInput;
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	private Texture2D blobMap;
	
	void OnGUI() {
		if (blobMap != null) {
			GUI.DrawTexture (new UnityEngine.Rect (Screen.width * NormalizedXCoordinate,
			                                     Screen.height * NormalizedYCoordinate,
			                                     Screen.width * NormalizedWidth,
			                                     Screen.width * NormalizedWidth * (float)blobMap.height / (float)blobMap.width),
			                					 blobMap);
		}
	}

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

		Mat blobMat = new Mat (height, width, MatType.CV_8UC3);
		MatOfByte3 blobMatByte = new MatOfByte3 (blobMat);
		var blobIndexer = blobMatByte.GetIndexer ();

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				blobIndexer[j, i] = new Vec3b(0, 0, 0);
			}
		}
		
		if (blobs.Count == 0) {
			return blobMat;
		}

		blobs.FilterByArea (blobs.LargestBlob ().Area / 2, int.MaxValue);
		
		//Colors the blob by (127, 127, 255)
		blobs.RenderBlobs (blobMat, blobMat, RenderBlobsMode.Color);
		
		return blobMat;
	}
	
	public void SetBlobMat(Mat mat) {
		int width = mat.Width;
		int height = mat.Height;

		MatOfByte3 blobMatByte = new MatOfByte3 (mat);
		var blobIndexer = blobMatByte.GetIndexer ();

		if (enabled) {
			if (blobMap == null) {
				blobMap = new Texture2D (width, height, TextureFormat.ARGB32, false);
			}

			Color[] pixels = new Color[width * height];
		
			for (int i = 0; i < width; ++i) {
				for (int j = 0; j < height; ++j) {
					pixels [i + (height - 1 - j) * width] = blobIndexer [j, i].Item0 != 0 ? Color.white : Color.black;
				}
			}

			blobMap.SetPixels (pixels);
			blobMap.Apply ();
		}
	}
}
