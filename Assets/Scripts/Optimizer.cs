using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

public class Optimizer : MonoBehaviour {

	private Optimizer() {
	}

	public static void Optimize(List<Vector3> p, List<Vector3> v,
	                            out float s, out Quaternion q, out Vector3 t) {
		if (p.Count != v.Count) {
			Debug.LogError ("sizes don't match");
			throw new ArgumentException();
		}

		int size = p.Count;
		Vector3 pCenter = Vector3.zero;
		Vector3 vCenter = Vector3.zero;

		for(int i = 0; i < size; ++i) {
			pCenter += p[i];
			vCenter += v[i];
		}
		pCenter /= size;
		vCenter /= size;

		//normalized ones
		Vector3[] pp = new Vector3[size];
		Vector3[] vv = new Vector3[size];

		for (int i = 0; i < size; ++i) {
			pp[i] = p[i] - pCenter;
			vv[i] = v[i] - vCenter;
		}

		float sNu = 0.0f;
		float sDe = 0.0f;

		for(int i = 0; i < size; ++i) {
			sNu += vv[i].sqrMagnitude;
			sDe += pp[i].sqrMagnitude;
		}

		s = Mathf.Sqrt (sNu / sDe);
		print ("s: " + s);

		float m11 = 0.0f;	float m12 = 0.0f;	float m13 = 0.0f;
		float m21 = 0.0f;	float m22 = 0.0f;	float m23 = 0.0f;
		float m31 = 0.0f;	float m32 = 0.0f;	float m33 = 0.0f;

		for(int i = 0; i < size; ++i) {
			m11 += pp[i].x * vv[i].x;	m12 += pp[i].x * vv[i].y;	m13 += pp[i].x * vv[i].z;
			m21 += pp[i].y * vv[i].x;	m22 += pp[i].y * vv[i].y;	m23 += pp[i].y * vv[i].z;
			m31 += pp[i].z * vv[i].x;	m32 += pp[i].z * vv[i].y;	m33 += pp[i].z * vv[i].z;
		}

		Mat n = new Mat (4, 4, MatType.CV_32F);
		n.Set (0, 0, m11 + m22 + m33);	n.Set (0, 1, m23 - m32);		n.Set (0, 2, m31 - m13);		n.Set (0, 3, m12 - m21);
		n.Set (1, 0, m23 - m32);		n.Set (1, 1, m11 - m22 - m33);	n.Set (1, 2, m12 + m21);		n.Set (1, 3, m31 + m13);
		n.Set (2, 0, m31 - m13);		n.Set (2, 1, m12 + m21);		n.Set (2, 2, -m11 + m22 - m33);	n.Set (2, 3, m23 + m32);
		n.Set (3, 0, m12 - m21);		n.Set (3, 1, m31 + m13);		n.Set (3, 2, m23 + m32);		n.Set (3, 3, -m11 - m22 + m33);

		InputArray nArray = InputArray.Create (n);

		Mat eigenValues = new Mat (1, 4, MatType.CV_32F);
		OutputArray eigenValueArray = OutputArray.Create (eigenValues);

		Mat eigenVectors = new Mat (4, 4, MatType.CV_32F);
		OutputArray eigenVectorArray = OutputArray.Create (eigenVectors);

		Cv2.Eigen (nArray, eigenValueArray, eigenVectorArray);

		for(int i = 0; i < 4; ++i) {
			print ("eigen " + i);
			print ("value: " + eigenValues.Get<float> (0, i));
			print (string.Format("vector: {0}, {1}, {2}, {3}", eigenVectors.Get<float> (0, i),
			                     eigenVectors.Get<float> (1, i), eigenVectors.Get<float> (2, i),
			                     eigenVectors.Get<float> (3, i)));
		}

		q = new Quaternion (eigenVectors.Get<float> (1, 0),
                            eigenVectors.Get<float> (2, 0),
                            eigenVectors.Get<float> (3, 0), 
		                    -eigenVectors.Get<float> (0, 0));

		print ("q: " + q);

		t = vCenter / s - q * pCenter;
		print ("t: " + t);
	}
}
