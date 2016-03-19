using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class DepthMesh : MonoBehaviour {
	
	public MeshFilter filter;
	public MeshRenderer meshRenderer;
	public Transform fingerTip;
	public Shader vertexColorShader;

	public MeshFilter Save() {
		print ("Save()");
		GameObject go = new GameObject ();
		MeshFilter savedFilter = go.AddComponent<MeshFilter> ();
		MeshRenderer savedMeshRenderer = go.AddComponent<MeshRenderer> ();
		savedFilter.mesh = filter.mesh;
		savedMeshRenderer.material = new Material (vertexColorShader);
		
		Texture2D texture2D = meshRenderer.material.mainTexture as Texture2D;

		Vector2[] uvs = savedFilter.mesh.uv;
		Color[] colors = new Color[uvs.Length];

		for (int i = 0; i < uvs.Length; ++i) {
			float x = uvs[i].x * texture2D.width;
			float y = uvs[i].y * texture2D.height;

			colors[i] = texture2D.GetPixel((int) x, (int) y);
		}

		savedFilter.sharedMesh.colors = colors;

		go.transform.parent = transform.parent;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		return savedFilter;
	}

	public void SetFloatMat(Mat handMat) {
		int depthWidth = handMat.Width;
		int depthHeight = handMat.Height;

		MatOfFloat matFloat = new MatOfFloat (handMat);
		var indexer = matFloat.GetIndexer ();

		CameraParameters depthCamera = CameraParameters.CreateMetaDepth ();
		CameraParameters colorCamera = CameraParameters.CreateMetaColor ();

		if (depthWidth != depthCamera.Width || depthHeight != depthCamera.Height) {
			Debug.LogError("Wrong Parameters!");
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2> ();
		int[] vertexMap = new int[depthWidth * depthHeight];
		for (int i = 0; i < depthWidth; ++i) {
			for (int j = 0; j < depthHeight; ++j) {
				vertexMap[i + j * depthWidth] = -1;

				float depth = indexer[j, i];
				if(depth > 0.0f && !float.IsPositiveInfinity(depth) && vertices.Count < 65000) {
					//Pixels ([0, depth width] x [0, depth height]) to Meters
					Vector3 depthVertex = DepthPixelToVertex(i, depthHeight - 1 - j, depth, depthCamera);
					
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
						vertexMap[i + j * depthWidth] = vertices.Count;
						vertices.Add(depthVertex);
						uv.Add(new Vector2(u, v));
					}
				}
			}
		}

		List<int> triangles = new List<int>();
		for (int i = 0; i < depthWidth - 1; ++i) {
			for (int j = 0; j < depthHeight - 1; ++j) {
				int vertexIndex = i + j * depthWidth;

				int index0 = vertexMap[vertexIndex];
				int index1 = vertexMap[vertexIndex + depthWidth];
				int index2 = vertexMap[vertexIndex + 1];
				int index3 = vertexMap[vertexIndex + depthWidth + 1];

				int validCount = (index0 != -1 ? 1 : 0) + (index1 != -1 ? 1 : 0)
					+ (index2 != -1 ? 1 : 0) + (index3 != -1 ? 1 : 0);
				
				if(validCount == 4) {
					triangles.Add(index0);
					triangles.Add(index2);
					triangles.Add(index1);

					triangles.Add(index2);
					triangles.Add(index3);
					triangles.Add(index1);
				}
				else if(validCount == 3) {
					if(index0 == -1) {
						triangles.Add(index2);
						triangles.Add(index3);
						triangles.Add(index1);
					}
					else if(index1 == -1) {
						triangles.Add(index0);
						triangles.Add(index3);
						triangles.Add(index2);
					}
					else if(index2 == -1) {
						triangles.Add(index0);
						triangles.Add(index1);
						triangles.Add(index3);
					}
					else {
						triangles.Add(index0);
						triangles.Add(index2);
						triangles.Add(index1);
					}
				}
			}
		}

		Mesh mesh = filter.sharedMesh;
		if(mesh == null) {
			mesh = new Mesh();
			filter.mesh = mesh;
		}
		mesh.Clear ();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray ();
		mesh.triangles = triangles.ToArray ();

		Vector3 fingerTipPosition = new Vector3 (0.0f, float.NegativeInfinity, 0.0f);
		foreach (var vertex in mesh.vertices) {
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
	
	public void SetTexture(Texture2D texture) {
		meshRenderer.material.mainTexture = texture;
	}
}
