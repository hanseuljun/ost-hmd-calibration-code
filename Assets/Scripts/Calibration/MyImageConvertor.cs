using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Iisu.Data;

/// <summary>
/// Helper class to convert iisu images to Unity images. 
/// </summary>
public class MyImageConvertor
{
	private Color[] _colored_image;
	
	private int _width;
	private int _height;
	
	private byte[] imageRaw;
	
	private float floatConvertor = 1f / 255f;
	
	public MyImageConvertor(int width, int height)
	{
		_width = width;
		_height = height;
	}
	
	public MyImageConvertor()
	{
		_width = 160;
		_height = 120;
	}
	
	private void getUVEquivalent(int fromWidth, int fromHeight, int fromU, int fromV, int toWidth, int toHeight, out int toU, out int toV, out int toIndex)
	{
		float uNorm = (float)fromU / (float)fromWidth;
		float vNorm = (float)fromV / (float)fromHeight;
		
		toU = (int)(uNorm * toWidth);
		toV = (int)(vNorm * toHeight);
		
		toIndex = toU + toV * toWidth;
	}
	
	private void getUV(int index, int width, int height, out int u, out int v)
	{
		u = index % width;
		v = index / width;
	}
	
	private Color getColor(ushort depthValue)
	{
		return new Color(depthValue * floatConvertor, depthValue * floatConvertor, depthValue * floatConvertor, 1);
	}
	
	public bool generateDepthImage(IImageData image, IImageData idImage, ref ushort[] values, ref Texture2D destinationImage)
	{
		if (image == null || idImage == null)
			return false;
		
		if (image.Raw == IntPtr.Zero || idImage.Raw == IntPtr.Zero)
			return false;
		
		if (_colored_image == null || _colored_image.Length != image.ImageInfos.BytesRaw / 2)
		{
			_colored_image = new Color[_width * _height];
			imageRaw = new byte[image.ImageInfos.BytesRaw];
		}
		
		uint byte_size = (uint)image.ImageInfos.BytesRaw;
		uint labelImageSize = (uint)idImage.ImageInfos.BytesRaw;
		
		// copy image content into a managed array
		Marshal.Copy(image.Raw, imageRaw, 0, (int)byte_size);
		
		int destinationU, destinationV;
		int sourceU, sourceV;
		int sourceIndex;
		
		int imageWidth = (int)image.ImageInfos.Width;
		int imageHeight = (int)image.ImageInfos.Height;
		int idImageWidth = (int)idImage.ImageInfos.Width;
		int idImageHeight = (int)idImage.ImageInfos.Height;

		values = new ushort[_width * _height];

		//build up the user mask
		for (int destinationIndex = 0; destinationIndex < _colored_image.Length; ++destinationIndex)
		{
			//get the UV coordinates from the final texture that will be displayed
			getUV(destinationIndex, _width, _height, out destinationU, out destinationV);
			
			//the resolutions of the depth and label image can differ from the final texture, 
			//so we have to apply some remapping to get the equivalent UV coordinates in the depth and label image.
			getUVEquivalent(_width, _height, destinationU, destinationV, imageWidth, imageHeight, out sourceU, out sourceV, out sourceIndex);
			
			// reconstruct ushort value from 2 bytes (low indian)
			values[destinationIndex] = (ushort)(imageRaw[sourceIndex * 2] + (imageRaw[sourceIndex * 2 + 1] << 8));
		}
		
		return true;
		
	}
	
}