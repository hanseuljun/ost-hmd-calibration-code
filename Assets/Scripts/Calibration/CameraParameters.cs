using UnityEngine;
using System.Collections;

public class CameraParameters {

	public int Width { get; private set; }
	public int Height { get; private set; }
	public float FX { get; private set; }
	public float FY { get; private set; }
	public float CX { get; private set; }
	public float CY { get; private set; }
	public float K1 { get; private set; }
	public float K2 { get; private set; }
	public float K3 { get; private set; }
	public float P1 { get; private set; }
	public float P2 { get; private set; }

	private CameraParameters() {
	}

	public static CameraParameters CreateMetaDepth() {
		CameraParameters cp = new CameraParameters();
		cp.Width = 320;
		cp.Height = 240;
		cp.FX = 224.50200f;
		cp.FY = 230.49400f;
		cp.CX = 160.0f;
		cp.CY = 120.0f;
		cp.K1 = -0.17010300f;
		cp.K2 = 0.14406399f;
		cp.K3 = -0.047699399f;
		cp.P1 = 0.0f;
		cp.P2 = 0.0f;

		return cp;
	}

	public static CameraParameters CreateMetaColor() {
		CameraParameters cp = new CameraParameters();
		cp.Width = 640;
		cp.Height = 480;
		cp.FX = 583.07898f;
		cp.FY = 596.20300f;
		cp.CX = 320.0f;
		cp.CY = 240.0f;
		cp.K1 = 0.022575200f;
		cp.K2 = -0.16266800f;
		cp.K3 = 0.18613800f;
		cp.P1 = 0.0f;
		cp.P2 = 0.0f;
		
		return cp;
	}
}
