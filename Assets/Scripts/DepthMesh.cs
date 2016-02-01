using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class DepthMesh : MonoBehaviour {
	
	public Transform fingerTip;
	private MeshFilter filter;
	private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
		filter = GetComponent<MeshFilter> ();
		meshRenderer = GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetFloatMat(Mat depthMat, Mat blobMat) {
		int width = depthMat.Width;
		int height = depthMat.Height;

		MatOfFloat matFloat = new MatOfFloat (depthMat);
		var indexer = matFloat.GetIndexer ();
		
		MatOfByte3 matBlob = new MatOfByte3 (blobMat);
		var blobIndexer = matBlob.GetIndexer ();

		CameraParameters depthCamera = CameraParameters.CreateMetaDepth ();
		CameraParameters colorCamera = CameraParameters.CreateMetaColor ();

		if (width != depthCamera.Width || height != depthCamera.Height) {
			Debug.LogError("Wrong Parameters!");
		}
		
		if (width != blobMat.Width || height != blobMat.Height) {
			Debug.LogError("Wrong Parameters!");
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2> ();
		int[] vertexMap = new int[width * height];
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				vertexMap[i + j * width] = -1;

				float depth = indexer[j, i];
				if(depth > 0.0f && blobIndexer[j, i].Item0 != 0 && vertices.Count < 65000) {
					//Pixels ([0, depth width] x [0, depth height]) to Meters
					Vector3 depthVertex = DepthPixelToVertex(i, height - 1 - j, depth, depthCamera);
					
					//Meters to Meters
					Vector3 colorVertex = DepthVertexToColorVertex(depthVertex);

					float u;
					float v;

					//Meters to Pixels ([0, color width] x [0, color height])
					ColorVertexToPixel(colorVertex, colorCamera, out u, out v);

					//Changing scale from pixels to [-1, 1]
					u /= colorCamera.Width;
					v /= colorCamera.Height;

					if(u >= 0.0f && u <= 1.0f && v >= 0.0f && v <= 1.0f) {
						vertexMap[i + j * width] = vertices.Count;
						vertices.Add(depthVertex);
						uv.Add(new Vector2(u, v));
					}
				}
			}
		}

		List<int> indices = new List<int>();
		for (int i = 0; i < width - 1; ++i) {
			for (int j = 0; j < height - 1; ++j) {
				int vertexIndex = i + j * width;

				int index0 = vertexMap[vertexIndex];
				int index1 = vertexMap[vertexIndex + width];
				int index2 = vertexMap[vertexIndex + 1];
				int index3 = vertexMap[vertexIndex + width + 1];

				int validCount = (index0 != -1 ? 1 : 0) + (index1 != -1 ? 1 : 0)
					+ (index2 != -1 ? 1 : 0) + (index3 != -1 ? 1 : 0);
				
				if(validCount == 4) {
					indices.Add(index0);
					indices.Add(index2);
					indices.Add(index1);

					indices.Add(index2);
					indices.Add(index3);
					indices.Add(index1);
				}
				else if(validCount == 3) {
					if(index0 == -1) {
						indices.Add(index2);
						indices.Add(index3);
						indices.Add(index1);
					}
					else if(index1 == -1) {
						indices.Add(index0);
						indices.Add(index3);
						indices.Add(index2);
					}
					else if(index2 == -1) {
						indices.Add(index0);
						indices.Add(index1);
						indices.Add(index3);
					}
					else {
						indices.Add(index0);
						indices.Add(index2);
						indices.Add(index1);
					}
				}
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray ();
		mesh.SetIndices (indices.ToArray(), MeshTopology.Triangles, 0);

		Mesh temp = filter.sharedMesh;
		filter.mesh = mesh;

		if (temp != null) {
			Destroy(temp);
		}

		Vector3 fingerTipPosition = new Vector3 (0.0f, float.NegativeInfinity, 0.0f);
		foreach (var vertex in vertices) {
			if((vertex.y - vertex.z * 0.5f) > (fingerTipPosition.y - fingerTipPosition.z * 0.5f)) {
				fingerTipPosition = vertex;
			}
		}

		if (!float.IsNegativeInfinity (fingerTipPosition.y)) {
			fingerTip.localPosition = fingerTipPosition;
		}
	}

	private Vector3 DepthPixelToVertex(int u, int v, float depth, CameraParameters depthCamera) {
		float x = (u - depthCamera.CX) / depthCamera.FX;
		float y = (v - depthCamera.CY) / depthCamera.FY;

		//is this inverse okay?
//		float r2 = x * x + y * y;
//		float radialDistortion = 1.0f + r2 * depthCamera.K1
//			+ r2 * r2 * depthCamera.K2 + r2 * r2 * r2 * depthCamera.K3;
//
//		x /= radialDistortion;
//		y /= radialDistortion;
		
		return new Vector3(x * depth, y * depth, depth);
	}

	private Vector3 DepthVertexToColorVertex(Vector3 depthVertex) {
		float r11 = 0.99999738f;
		float r12 = 0.0012669859f;
		float r13 = -0.0019156976f;
		float r21 = 0.0012539299f;
		float r22 = -0.99997610f;
		float r23 = -0.0068011540f;
		float r31 = 0.0019242687f;
		float r32 = -0.0067987340f;
		float r33 = 0.99997503f;
		float t1 = 0.024492104f;
		float t2 = -0.00050799217f;
		float t3 = -0.00086258771f;

		float dx = depthVertex.x;
		float dy = depthVertex.y;
		float dz = -depthVertex.z;

		//TODO: check if this is right
		float cx = r11 * dx + r12 * dy + r13 * dz + t1;
		float cy = r21 * dx + r22 * dy + r23 * dz + t2;
		float cz = r31 * dx + r32 * dy + r33 * dz + t3;
		cy *= -1.0f;
		cz *= -1.0f;

		return new Vector3(cx, cy, cz);
	}

	private void ColorVertexToPixel(Vector3 colorVertex, CameraParameters colorCamera,
                                    out float u, out float v) {
		float x = colorVertex.x / colorVertex.z;
		float y = colorVertex.y / colorVertex.z;

//		float r2 = x * x + y * y;
//		float radialDistortion = 1.0f + colorCamera.K1 * r2
//			+ colorCamera.K2 * r2 * r2 + colorCamera.K3 * r2 * r2 * r2;
//
//		x *= radialDistortion;
//		y *= radialDistortion;

		u = x * colorCamera.FX + colorCamera.CX;
		v = y * colorCamera.FY + colorCamera.CY;
	}
	
	public void SetTexture(Texture texture) {
		meshRenderer.material.mainTexture = texture;
	}
}
