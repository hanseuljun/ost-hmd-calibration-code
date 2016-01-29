using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class DepthMesh : MonoBehaviour {

	private MeshFilter filter;

	// Use this for initialization
	void Start () {
		filter = GetComponent<MeshFilter> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetFloatMat(Mat mat) {
		int width = mat.Width;
		int height = mat.Height;

		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		CameraParameters depthCamera = CameraParameters.CreateMetaDepth ();

		List<Vector3> vertices = new List<Vector3>();
		int[] vertexMap = new int[width * height];
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				float depth = indexer[j, i];
				if(indexer[j, i] > 0.0f && vertices.Count < 65000) {
					vertexMap[i + j * width] = vertices.Count;

					float x = (i * 2.0f / ((float) (width - 1))) - 1.0f;
					float y = 1.0f - (j * 2.0f / ((float) (height - 1)));

					x = (depthCamera.ScaleX * x + depthCamera.OffsetX) * depth;
					y = (depthCamera.ScaleY * y + depthCamera.OffsetY) * depth;

					vertices.Add(new Vector3(x, y, depth));
				}
				else {
					vertexMap[i + j * width] = -1;
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
		mesh.SetIndices (indices.ToArray(), MeshTopology.Triangles, 0);

		filter.mesh = mesh;
	}
}
