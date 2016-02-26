using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointCloud {

	public List<Vector3> Points { get; private set; }
	public List<Color> Colors { get; private set; }

	public PointCloud(Mesh mesh) {
		bool[] used = new bool[mesh.vertexCount];

		for(int i = 0; i < used.Length; ++i) {
			used[i] = false;
		}

		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length; ++i) {
			used[triangles[i]] = true;
		}

		Vector3[] meshVertices = mesh.vertices;
		Color[] meshColors = mesh.colors;

		Points = new List<Vector3> ();
		Colors = new List<Color> ();

		for(int i = 0; i < used.Length; ++i) {
			if(used[i]) {
				Points.Add(meshVertices[i]);
				Colors.Add(meshColors[i]);
			}
		}
	}
}
