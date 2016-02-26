using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class SkinImage : MonoBehaviour {
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	private Texture2D skinMap;
	
	void OnGUI() {
		if (skinMap != null) {
			GUI.DrawTexture (new UnityEngine.Rect (Screen.width * NormalizedXCoordinate,
			                                       Screen.height * NormalizedYCoordinate,
			                                       Screen.width * NormalizedWidth,
			                                       Screen.width * NormalizedWidth * (float)skinMap.height / (float)skinMap.width),
			                 					   skinMap);
		}
	}
	
	public static Mat ConvertColorMat(Mat mat, byte threshold) {
		int width = mat.Width;
		int height = mat.Height;
		
		MatOfByte3 matB3 = new MatOfByte3 (mat);
		var indexer = matB3.GetIndexer ();
		
		Mat skinMat = new Mat (height, width, MatType.CV_8U);
		MatOfByte matByte = new MatOfByte (skinMat);
		var byteIndexer = matByte.GetIndexer ();
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				byte r = indexer[j, i].Item0;
				byte g = indexer[j, i].Item1;
				byte b = indexer[j, i].Item2;
				byteIndexer[j, i] = ((r - g) > threshold) && ((r - b) > threshold) ? (byte) 255 : (byte) 0;
			}
		}
		
		return skinMat;
	}
	
	public void SetSkinMat(Mat mat) {
		if (!enabled) {
			return;
		}

		int width = mat.Width;
		int height = mat.Height;
		
		Color32[] pixels = new Color32[width * height];
		
		MatOfByte matByte = new MatOfByte (mat);
		var indexer = matByte.GetIndexer ();

		if (skinMap == null) {
			skinMap = new Texture2D (width, height, TextureFormat.ARGB32, false);
		}
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				byte color = indexer [j, i];
				pixels [i + (height - 1 - j) * width] = new Color32(color,
				                                                    color,
				                                                    color,
				                                                    (byte) 255);
			}
		}
		
		skinMap.SetPixels32 (pixels);
		skinMap.Apply ();
	}
}
