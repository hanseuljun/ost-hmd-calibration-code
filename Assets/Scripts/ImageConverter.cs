using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Iisu.Data;
using OpenCvSharp;

public class ImageConverter
{
	public static bool GenerateColorMat(IImageData image, ref Mat mat) {
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;

		if ((image.ImageInfos.Width != mat.Width) || (image.ImageInfos.Height != mat.Height))
			return false;
		
		byte[] imageRaw = new byte[image.ImageInfos.BytesRaw];
		
		uint byte_size = (uint)image.ImageInfos.BytesRaw;
		
		// copy image content into a managed array
		Marshal.Copy (image.Raw, imageRaw, 0, (int)byte_size);

		MatOfByte3 matB3 = new MatOfByte3 (mat);
		var indexer = matB3.GetIndexer ();

		int width = mat.Width;
		int height = mat.Height;

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				int index = i + j * width;
			
				indexer [j, i] = new Vec3b (imageRaw [index * 4 + 2],
		                                  imageRaw [index * 4 + 1],
		                                  imageRaw [index * 4 + 0]);
			}
		}
		
		return true;
	}
	
	public static bool GenerateDepthMat(IImageData image, ref Mat mat) {
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;
		
		if ((image.ImageInfos.Width != mat.Width) || (image.ImageInfos.Height != mat.Height))
			return false;

		byte[] imageRaw = new byte[image.ImageInfos.BytesRaw];
		
		// copy image content into a managed array
		Marshal.Copy (image.Raw, imageRaw, 0, (int)image.ImageInfos.BytesRaw);

		MatOfFloat matFloat = new MatOfFloat (mat);
		var indexer = matFloat.GetIndexer ();

		int width = mat.Width;
		int height = mat.Height;

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				int index = i + j * width;
				ushort depth = (ushort)(imageRaw [index * 2] + (imageRaw [index * 2 + 1] << 8));
				indexer [j, i] = depth * 0.001f;
			}
		}
		
		return true;
	}
	
	public static void GenerateColorMap(Mat mat, ref Texture2D colorMap) {
		int width = mat.Width;
		int height = mat.Height;
		
		Color32[] pixels = new Color32[width * height];
		
		MatOfByte3 matB3 = new MatOfByte3 (mat);
		var indexer = matB3.GetIndexer ();
		
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				Vec3b color = indexer [j, i];
				pixels [i + (height - 1 - j) * width] = new Color32 (color.Item0, color.Item1, color.Item2, (byte)255);
			}
		}
		
		colorMap.SetPixels32 (pixels);
		colorMap.Apply ();
	}
}