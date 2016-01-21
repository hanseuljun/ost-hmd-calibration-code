using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Iisu.Data;

/// <summary>
/// Helper class to convert iisu images to Unity images. 
/// </summary>
public class MyImageConvertor
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
	
	public static bool generateDepthImage(IImageData image, int width, int height, ref ushort[] values)
	{
		if (image == null)
			return false;
		
		if (image.Raw == IntPtr.Zero)
			return false;

		byte[] imageRaw = new byte[image.ImageInfos.BytesRaw];
		
		uint byte_size = (uint)image.ImageInfos.BytesRaw;
		
		// copy image content into a managed array
		Marshal.Copy(image.Raw, imageRaw, 0, (int)byte_size);
		
		int destinationU, destinationV;
		int sourceU, sourceV;
		int sourceIndex;
		
		int imageWidth = (int)image.ImageInfos.Width;
		int imageHeight = (int)image.ImageInfos.Height;

		values = new ushort[width * height];

		//build up the user mask
		for (int destinationIndex = 0; destinationIndex < values.Length; ++destinationIndex)
		{
			//get the UV coordinates from the final texture that will be displayed
			getUV(destinationIndex, width, height, out destinationU, out destinationV);
			
			//the resolutions of the depth and label image can differ from the final texture, 
			//so we have to apply some remapping to get the equivalent UV coordinates in the depth and label image.
			getUVEquivalent(width, height, destinationU, destinationV, imageWidth, imageHeight, out sourceU, out sourceV, out sourceIndex);
			
			// reconstruct ushort value from 2 bytes (low indian)
			values[destinationIndex] = (ushort)(imageRaw[sourceIndex * 2] + (imageRaw[sourceIndex * 2 + 1] << 8));
		}
		
		return true;
		
	}
	
}