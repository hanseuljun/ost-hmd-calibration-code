using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	public float radius;
	public float innerRadius;

	// Use this for initialization
	void Start () {
		
		Mesh mesh = new Mesh ();
		
		const int FREQUENCY = 50;
		
		Vector3[] vertices = new Vector3[FREQUENCY * 2];
		
		for(int i = 0; i < FREQUENCY; ++i)
		{
			float degree = Mathf.PI * 2.0f * i / FREQUENCY;
			vertices[i * 2] = new Vector3(Mathf.Sin(degree) * radius, Mathf.Cos(degree) * radius, 0.0f);
			vertices[i * 2 + 1] = new Vector3(Mathf.Sin(degree) * innerRadius, Mathf.Cos(degree) * innerRadius, 0.0f);
		}
		
		Vector3[] normals = new Vector3[FREQUENCY * 2];
		for(int i = 0; i < normals.Length; ++i)
		{
			normals[i] = Vector3.back;
		}
		
		int[] indices = new int[FREQUENCY * 6];
		for(int i = 0; i < FREQUENCY - 1; ++i)
		{
			indices[i * 6 + 0] = i * 2;
			indices[i * 6 + 1] = i * 2 + 2;
			indices[i * 6 + 2] = i * 2 + 1;
			indices[i * 6 + 3] = i * 2 + 2;
			indices[i * 6 + 4] = i * 2 + 3;
			indices[i * 6 + 5] = i * 2 + 1;
		}
		indices[FREQUENCY * 6 - 6] = FREQUENCY * 2 - 2;
		indices[FREQUENCY * 6 - 5] = 0;
		indices[FREQUENCY * 6 - 4] = FREQUENCY * 2 - 1;
		indices[FREQUENCY * 6 - 3] = 0;
		indices[FREQUENCY * 6 - 2] = 1;
		indices[FREQUENCY * 6 - 1] = FREQUENCY * 2 - 1;

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = indices;

		GetComponent<MeshFilter> ().mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
