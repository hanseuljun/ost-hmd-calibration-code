using UnityEngine;
using System.Collections;

public class SKLogo : MonoBehaviour {
	
	public Texture2D LogoTexture;
	
	private float _width;
	private float _height;
	
	void Start()
	{
		_width = Screen.width/3;
		_height = _width * (float)LogoTexture.height/(float)LogoTexture.width;
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(Screen.width - _width, Screen.height - _height, _width, _height), LogoTexture);
	}
	
}
