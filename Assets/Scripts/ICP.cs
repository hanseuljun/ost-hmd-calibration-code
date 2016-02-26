using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ICP {

	public void Solve(PointCloud source, PointCloud target) {
		int sourceCount = source.Points.Count;
		int targetCount = target.Points.Count;
	
		Vector3 translation = Vector3.zero;
		Quaternion rotation = Quaternion.identity;

		for (int i = 0; i < 1; ++i) {
			List<Vector3> transformedSourcePoints = new List<Vector3>();
			List<Vector3> closestTargetPoints = new List<Vector3>();

			for(int j = 0; j < sourceCount; ++j) {
				transformedSourcePoints.Add(rotation * source.Points[i] + translation);
			}

			Vector3 transformedSourceCenter = Vector3.zero;
			Vector3 closestTargetCenter = Vector3.zero;

			for (int j = 0; j < sourceCount; ++j) {
				int closestPoint = -1;
				float minDistance = float.MaxValue;

				Vector3 sourcePoint = transformedSourcePoints [j];

				for (int k = 0; k < targetCount; ++k) {
					float distance = (sourcePoint - target.Points [k]).magnitude;
					if (distance < minDistance) {
						closestPoint = k;
						minDistance = distance;
					}
				}

				closestTargetPoints.Add (target.Points[closestPoint]);
			}

			for(int j = 0; j < sourceCount; ++j) {
				transformedSourceCenter += transformedSourcePoints[j];
				closestTargetCenter += closestTargetPoints[j];
			}
			
			transformedSourceCenter /= sourceCount;
			closestTargetCenter /= sourceCount;

			List<Vector3> centeredSource = new List<Vector3>();
			List<Vector3> centeredTarget = new List<Vector3>();

			for(int j = 0; j < sourceCount; ++j) {
				centeredSource.Add(transformedSourcePoints[j] - transformedSourceCenter);
				centeredTarget.Add(closestTargetPoints[j] - closestTargetCenter);
			}

			float[,] cov = new float[3, 3];
			for(int j = 0; j < 3; ++j) {
				for(int l = 0; l < 3; ++l) {
					cov[j, l] = 0.0f;
				}
			}


			for(int j = 0; j < sourceCount; ++j) {
				cov[0, 0] += centeredSource[j].x * centeredTarget[j].x;
				cov[0, 1] += centeredSource[j].y * centeredTarget[j].x;
				cov[0, 2] += centeredSource[j].z * centeredTarget[j].x;
				cov[1, 0] += centeredSource[j].x * centeredTarget[j].y;
				cov[1, 1] += centeredSource[j].y * centeredTarget[j].y;
				cov[1, 2] += centeredSource[j].z * centeredTarget[j].y;
				cov[2, 0] += centeredSource[j].x * centeredTarget[j].z;
				cov[2, 1] += centeredSource[j].y * centeredTarget[j].z;
				cov[2, 2] += centeredSource[j].z * centeredTarget[j].z;
			}
			
			for(int j = 0; j < 3; ++j) {
				for(int l = 0; l < 3; ++l) {
					cov[j, l] /= sourceCount;
				}
			}

			float trace = cov[0, 0] + cov[1, 1] + cov[2, 2];

			double[,] Q = new double[4, 4];

			Q[0, 0] = trace;
			Q[0, 1] = cov[1, 2];
			Q[0, 2] = cov[2, 0];
			Q[0, 3] = cov[0, 1];
			Q[1, 0] = cov[1, 2];
			Q[2, 0] = cov[2, 0];
			Q[3, 0] = cov[0, 1];

			for(int j = 0; j < 3; ++j) {
				for(int k = 0; k < 3; ++k) {
					Q[j + 1, k + 1] = cov[j, k] + cov[k, j];
				}

				Q[j + 1, j + 1] -= trace;
			}

			double[] d = new double[0];
			double[,] v = new double[0, 0];
			alglib.evd.smatrixevd(Q, 4, 0, false, ref d, ref v);

			rotation = new Quaternion((float) v[0, 1], (float) v[0, 2], (float) v[0, 3], (float) v[0, 0]);
			translation = closestTargetCenter - rotation * transformedSourceCenter;
		}
	}
}
