using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Blob;

public static class HandExtractor {

	public static MatOfByte GenerateValidMat(MatOfFloat depthMat, float gap, float maxDistance) {
		DateTime time1 = DateTime.Now;

		int width = depthMat.Width;
		int height = depthMat.Height;
		var indexer = depthMat.GetIndexer ();

		// number of 1cm connections
		int[,] connections = new int[depthMat.Width, depthMat.Height];

		float gapSquare = gap * gap;
		
		for (int i = 0; i < width - 1; ++i) {
			for (int j = 0; j < height - 1; ++j) {
				float depth = indexer[j, i];
				if(depth > maxDistance) {
					continue;
				}
				float rightDiff = indexer[j, i + 1] - depth;
				//right
				if(rightDiff * rightDiff < gapSquare) {
					++connections[i, j];
					++connections[i + 1, j];
				}
				
				float downDiff = indexer[j + 1, i] - depth;
				//down
				if(downDiff * downDiff < gapSquare) {
					++connections[i, j];
					++connections[i, j + 1];
				}
			}
		}

		DateTime time2 = DateTime.Now;
		Debug.Log ("GenerateValidMat() 1-2: " + (time2 - time1).Milliseconds);
		
		MatOfByte validMat = new MatOfByte (height, width);
		var validIndexer = validMat.GetIndexer ();
		
		int invalidCount = 0;
		
		for (int i = 0; i < width; ++i) {
			for(int j = 0; j < height; ++j) {
				// pixels from a hand does not have too many gaps and they aren't further than 1 m
				if(connections[i, j] >= 3) {
					validIndexer[j, i] = (byte)255;
				}
				else {
					++invalidCount;
					validIndexer[j, i] = (byte)0;
				}
			}
		}
		
		DateTime time3 = DateTime.Now;
		Debug.Log ("GenerateValidMat() 2-3: " + (time3 - time2).Milliseconds);

		return validMat;
	}

	public static CvBlob ExtractHandBlob(MatOfFloat depthMat, CvBlobs validMatBlobs) {
		var indexer = depthMat.GetIndexer ();
		
		validMatBlobs.FilterByArea (validMatBlobs.LargestBlob ().Area / 10, int.MaxValue);

		float meanDepthToNearestBlob = float.PositiveInfinity;
		CvBlob nearestBlob = null;
		
		foreach (var blob in validMatBlobs.Values) {
			int label = blob.Label;
			int count = 0;
			float depthSum = 0.0f;
			
			for(int i = blob.MinX; i < blob.MaxX; ++i) {
				for(int j = blob.MinY; j < blob.MaxY; ++j) {
					if(validMatBlobs.GetLabel(i, j) == label) {
						++count;
						depthSum += indexer[j, i];
					}
				}
			}
			
			float meanDepth = depthSum / count;
			
			if(meanDepth < meanDepthToNearestBlob) {
				meanDepthToNearestBlob = meanDepth;
				nearestBlob = blob;
			}
		}

		return nearestBlob;
	}

	public static MatOfByte GenerateBlobMat(CvBlobs blobs, CvBlob blob) {
		OpenCvSharp.Rect rect = blob.Rect;
		int width = rect.Right - rect.Left;
		int height = rect.Bottom - rect.Top;
		
		MatOfByte mat = new MatOfByte (height, width);
		var indexer = mat.GetIndexer ();
		int left = rect.Left;
		int top = rect.Top;

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if(blobs.GetLabel(i + left, j + top) == blob.Label) {
					indexer[j, i] = (byte) 255;
				}
				else {
					indexer[j, i] = (byte) 0;
				}
			}
		}

		return mat;
	}
	
	public static MatOfFloat GenerateHandMat(MatOfFloat depthMat, MatOfByte handBlobMat, CvBlob handBlob) {
		DateTime time1 = DateTime.Now;

		int depthWidth = depthMat.Width;
		int depthHeight = depthMat.Height;
		int handWidth = handBlobMat.Width;
		int handHeight = handBlobMat.Height;

		var depthIndexer = depthMat.GetIndexer ();
		var handBlobIndexer = handBlobMat.GetIndexer ();
		
		MatOfFloat medianMat = new MatOfFloat(depthHeight, depthWidth);
		Cv2.MedianBlur (depthMat, medianMat, 5);
		var medianIndexer = medianMat.GetIndexer ();
		
		MatOfFloat handMat = new MatOfFloat (depthHeight, depthWidth, float.PositiveInfinity);
		var handIndexer = handMat.GetIndexer ();

		int handLeft = handBlob.Rect.Left;
		int handTop = handBlob.Rect.Top;

		for (int i = 0; i < handWidth; ++i) {
			for (int j = 0; j < handHeight; ++j) {
				if(handBlobIndexer[j, i] != 0) {
					handIndexer[j + handTop, i + handLeft] = medianIndexer[j + handTop, i + handLeft];
				}
				else {
					handIndexer[j + handTop, i + handLeft] = float.PositiveInfinity;
				}
			}
		}
		
		DateTime time2 = DateTime.Now;
		Debug.Log ("GenerateHandMat(): " + (time2 - time1).Milliseconds);

		return handMat;
	}

	public static MatOfByte GenerateFloodFilledMat(MatOfByte handBlobMat) {
		int width = handBlobMat.Width;
		int height = handBlobMat.Height;

		MatOfByte floodFilledMat = new MatOfByte (height, width);

		var handBlobIndexer = handBlobMat.GetIndexer ();
		var floodFilledIndexer = floodFilledMat.GetIndexer ();
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if(handBlobIndexer[j, i] != 0) {
					floodFilledIndexer[j, i] = (byte)255;
				}
				else {
					floodFilledIndexer[j, i] = (byte)0;
				}
			}
		}

		if (floodFilledIndexer [0, 0] != (byte)0) {
			return null;
		}

		Cv2.FloodFill (floodFilledMat, new Point (0, 0), (byte)128);
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if(floodFilledIndexer[j, i] != (byte)128) {
					floodFilledIndexer[j, i] = (byte)255;
				}
				else {
					floodFilledIndexer[j, i] = (byte)0;
				}
			}
		}

		return floodFilledMat;
	}
	
	public static List<Point> GenerateHandContourMat(MatOfByte handBlobMat) {
		DateTime time1 = DateTime.Now;

		int width = handBlobMat.Width;
		int height = handBlobMat.Height;
		
		MatOfByte dilateMat = new MatOfByte (height, width);
		byte[] kernelValues = {0, 1, 0, 1, 1, 1, 0, 1, 0}; // cross (+)  
		MatOfByte kernel = new MatOfByte(3, 3, kernelValues, 0); 
		
		Cv2.MorphologyEx (handBlobMat, dilateMat, MorphTypes.DILATE, kernel, null, 1, BorderTypes.Constant, 0);
		var dilateIndexer = dilateMat.GetIndexer ();

		var handBlobIndexer = handBlobMat.GetIndexer ();
		MatOfByte handContourMat = new MatOfByte (height, width);
		var handContourIndexer = handContourMat.GetIndexer ();

		var contourPoints = new List<Point> ();

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if(handBlobIndexer[j, i] != dilateIndexer[j, i]) {
					contourPoints.Add(new Point(i, j));
				}
			}
		}
		
		DateTime time2 = DateTime.Now;
		Debug.Log ("GenerateHandContourMat(): " + (time2 - time1).Milliseconds);

		return contourPoints;
	}

	public static Point? ExtractHandCenter(MatOfByte handBlobMat, List<Point> contourPoints) {
		DateTime time1 = DateTime.Now;

		var sampledContourPoints = new List<Point> ();
		for(int i = 0; i < contourPoints.Count / 3; ++i) {
			sampledContourPoints.Add(contourPoints[i * 3]);
		}
		contourPoints = sampledContourPoints;

		int width = handBlobMat.Width;
		int height = handBlobMat.Height;

		var handBlobIndexer = handBlobMat.GetIndexer ();

		float maxChamferDistanceSquare = float.NegativeInfinity;
		Point? maxChamferDistancePoint = null;

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if(handBlobIndexer[j, i] == 0) {
					continue;
				}

				float chamferDistanceSquare = float.PositiveInfinity;
				bool continueFor = false;

				foreach(var contourPoint in contourPoints) {
					float xDiff = contourPoint.X - i;
					float yDiff = contourPoint.Y - j;
					float distanceSquare = xDiff * xDiff + yDiff * yDiff;
					if(distanceSquare < maxChamferDistanceSquare) {
						continueFor = true;
						break;
					}
					else if(distanceSquare < chamferDistanceSquare) {
						chamferDistanceSquare = distanceSquare;
					}
				}

				if(continueFor) {
					continue;
				}

				if(chamferDistanceSquare > maxChamferDistanceSquare) {
					maxChamferDistanceSquare = chamferDistanceSquare;
					maxChamferDistancePoint = new Point(i, j);
				}
			}
		}
		DateTime time2 = DateTime.Now;
		Debug.Log ("ExtractHandCenter(): " + (time2 - time1).Milliseconds);

		return maxChamferDistancePoint;
	}
	
	public static Vector3?[,] GenerateHandPointCloud(MatOfFloat handMat, CvBlob handBlob) {
		DateTime time1 = DateTime.Now;
		int width = handMat.Width;
		int height = handMat.Height;
		int left = handBlob.Rect.Left;
		int top = handBlob.Rect.Top;
		int bottom = handBlob.Rect.Bottom;

		CameraParameters depthCamera = CameraParameters.CreateMetaDepth ();
		
		var handIndexer = handMat.GetIndexer ();
		
		Vector3?[,] vertices = new Vector3?[width, height];
		
		float fxDenominator = 1.0f / depthCamera.FX;
		float fyDenominator = 1.0f / depthCamera.FY;
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				float depth = handIndexer[j, i];
				if(float.IsPositiveInfinity(depth)) {
					vertices[i, j] = null;
					continue;
				}
				
				int u = i + left;
				int v = bottom - 1 - (j + top);
				
				float x = (u - depthCamera.CX) * fxDenominator;
				float y = (v - depthCamera.CY) * fyDenominator;
				
				vertices[i, j] = new Vector3(x * depth, y * depth, depth);
			}
		}
		DateTime time2 = DateTime.Now;
		Debug.Log ("GenerateHandPointCloud(): " + (time2 - time1).Milliseconds);
		
		return vertices;
	}
}
