using UnityEngine;
using System.Collections;
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

	public void SetFloatMat(Mat mat)
	{
		int width = mat.Width;
		int height = mat.Height;

		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		Vector3[] vertices = new Vector3[width * height];
		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				vertices[i + j * width] = new Vector3(i, j, indexer[j, i]);
			}
		}

		int[] indices = new int[(width - 1) * (height - 1) * 6];
		for(int i = 0; i < width - 1; ++i)
		{
			for(int j = 0; j < height - 1; ++j)
			{
				int planeIndex = (i + j * (width - 1)) * 6;
				int vertexIndex = i + j * width;
				indices[planeIndex + 0] = vertexIndex;
				indices[planeIndex + 1] = vertexIndex + width;
				indices[planeIndex + 2] = vertexIndex + 1;
				indices[planeIndex + 3] = vertexIndex + 1;
				indices[planeIndex + 4] = vertexIndex + width;
				indices[planeIndex + 5] = vertexIndex + width + 1;
			}
		}

		//TODO: reduce triangles
//		Mesh mesh = new Mesh ();
//		mesh.vertices = vertices;
//		mesh.SetIndices (indices, MeshTopology.Triangles, 0);
//
//		filter.mesh = mesh;
	}
}
