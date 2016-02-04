using UnityEngine;
using System.Collections;
using OpenCvSharp;

public class ColorImage : MonoBehaviour {
	public IisuInputProvider IisuInput;
	public float NormalizedXCoordinate;
	public float NormalizedYCoordinate;
	public float NormalizedWidth;
	private Texture2D colorMap;
	
	void OnGUI() {
		if (colorMap != null) {
			GUI.DrawTexture (new UnityEngine.Rect (Screen.width * NormalizedXCoordinate,
			                                     Screen.height * NormalizedYCoordinate,
			                                     Screen.width * NormalizedWidth,
			                                     Screen.width * (float)colorMap.height / (float)colorMap.width),
			                					 colorMap);
		}
	}

	public void SetImage(Texture2D colorMap) {
		this.colorMap = colorMap;
	}
}
