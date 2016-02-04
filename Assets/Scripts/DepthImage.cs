using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class DepthImage : MonoBehaviour {
	public IisuInputProvider IisuInput;
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	private Texture2D depthMap;

	void OnGUI() {
		if (depthMap != null) {
			GUI.DrawTexture (new UnityEngine.Rect (Screen.width * NormalizedXCoordinate,
			                                     Screen.height * NormalizedYCoordinate,
			                                     Screen.width * NormalizedWidth,
			                                     Screen.width * NormalizedWidth * (float)depthMap.height / (float)depthMap.width),
			                					 depthMap);
		}
	}

	public void UpdateFloatMat(Mat mat, int fingerI, int fingerJ) {
		int width = mat.Width;
		int height = mat.Height;
		
		if (depthMap == null) {
			depthMap = new Texture2D (width, height, TextureFormat.ARGB32, false);
		}
		
		Color[] pixels = new Color[width * height];
		
		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		float multiplier = 1.0f / 1.0f; // up to 1m

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				float depthValue = indexer [j, i] * multiplier;
				pixels [i + (height - 1 - j) * width] = new Color (depthValue, depthValue, depthValue, 1);
			}
		}

		if (fingerJ != int.MaxValue) {
			for (int i = -1; i < 2; ++i) {
				for (int j = -1; j < 2; ++j) {
					int ii = fingerI + i;
					int jj = fingerJ + j;
					if (ii >= 0 && ii < width && jj >= 0 && jj < height) {
						pixels [ii + (height - 1 - jj) * width] = Color.red;
					}
				}
			}
		}
		depthMap.SetPixels (pixels);
		depthMap.Apply ();
	}
}
