using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Iisu.Data;
using OpenCvSharp;

/// <summary>
/// Helper class to convert iisu images to Unity images. 
/// </summary>
public class ImageConvertor
{
	private static void getUVEquivalent(int fromWidth, int fromHeight, int fromU, int fromV, int toWidth, int toHeight, out int toU, out int toV, out int toIndex)
	{
		float uNorm = (float)fromU / (float)fromWidth;
		float vNorm = (float)fromV / (float)fromHeight;
		
		toU = (int)(uNorm * toWidth);
		toV = (int)(vNorm * toHeight);
		
		toIndex = toU + toV * toWidth;
	}
	
	private static void getUV(int index, int width, int height, out int u, out int v)
	{
		u = index % width;
		v = index / width;
	}
	
	public static bool generateColorImage(IImageData image, ref Mat mat)
	{
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;

		if ((image.ImageInfos.Width != mat.Width) || (image.ImageInfos.Height != mat.Height))
			return false;
		
		byte[] imageRaw = new byte[image.ImageInfos.BytesRaw];
		
		uint byte_size = (uint)image.ImageInfos.BytesRaw;
		
		// copy image content into a managed array
		Marshal.Copy(image.Raw, imageRaw, 0, (int)byte_size);

		MatOfByte3 matB3 = new MatOfByte3 (mat);
		var indexer = matB3.GetIndexer ();

		int width = mat.Width;
		int height = mat.Height;

		for (int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				int index = i + j * width;
			
				indexer[j, i] = new Vec3b(imageRaw[index * 4 + 2],
		                                  imageRaw[index * 4 + 1],
		                                  imageRaw[index * 4 + 0]);
			}
		}
		
		return true;
		
	}
	
	public static bool generateDepthImage(IImageData image, ref Mat mat)
	{
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;
		
		if ((image.ImageInfos.Width != mat.Width) || (image.ImageInfos.Height != mat.Height))
			return false;

		byte[] imageRaw = new byte[image.ImageInfos.BytesRaw];
		
		// copy image content into a managed array
		Marshal.Copy(image.Raw, imageRaw, 0, (int)image.ImageInfos.BytesRaw);

		MatOfUShort matUS = new MatOfUShort (mat);
		var indexer = matUS.GetIndexer ();

		int width = mat.Width;
		int height = mat.Height;

		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				int index = i + j * width;
				indexer[j, i] = (ushort)(imageRaw[index * 2] + (imageRaw[index * 2 + 1] << 8));
			}
		}
		
		return true;
	}
	
}