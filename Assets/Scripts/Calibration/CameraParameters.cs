using UnityEngine;
using System.Collections;

public class CameraParameters {

	public float ScaleX { get; private set; }
	public float ScaleY { get; private set; }
	public float OffsetX { get; private set; }
	public float OffsetY { get; private set; }

	private CameraParameters() {
	}

	public static CameraParameters CreateMetaDepth() {
		CameraParameters cp = new CameraParameters();
		cp.ScaleX = 1.0f;
		cp.ScaleY = 1.0f;
		cp.OffsetX = 0.0f;
		cp.OffsetY = 0.0f;

		return cp;
	}
}
