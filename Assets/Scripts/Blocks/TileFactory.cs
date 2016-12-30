﻿using UnityEngine;
using System.Collections.Generic;

public enum GradientType
{
	SingleColor,
	WarmCool,
	DarkLight,
	ValueColor
}

public class TileFactory : MonoBehaviour {

	static private Texture2D _stoneTexture;
	static private Texture2D _glassTexture;

	static public Texture2D stoneTexture 
	{
		get 
		{
			if (_stoneTexture == null)
			{
				_stoneTexture = CreateTexture(0, "Stone", false);
			}	

			return _stoneTexture;
		}
	}

	static public Texture2D glassTexture 
	{
		get 
		{
			if (_glassTexture == null)
			{
				_glassTexture = CreateTexture(32, "Glass", true);
			}	

			return _glassTexture;
		}
	}

	public static int TileSize = 16;
	public static int TileGridHeight = 4;
	public static int TileGridWidth = 8;

	const int TILE_BORDER = 4;

	public static Color[] colors = new Color[64];
	public static Color lightColor = Color.white;
	public static Color darkColor = Color.black;

	public static void Clear()
	{
		Destroy(_stoneTexture);
		Destroy(_glassTexture);
		_stoneTexture = null;
		_glassTexture = null;
	}

	public static Color Darken(Color color, float amount)
	{
		float a = color.a;
		color = Color.Lerp(color, darkColor, amount);
		color.a = a;
		return color;
	}

	public static Color Lighten(Color color, float amount)
	{
		float a = color.a;
		color = Color.Lerp(color, lightColor, amount);
		color.a = a;
		return color;
	}

	public static Color Brighten(Color color, float amount)
	{
		float h;
		float s;
		float v;
		float a = color.a;

		Color.RGBToHSV(color, out h, out s, out v);

		s = Mathf.Lerp(s, 1.0f, amount);
		v = Mathf.Lerp(v, 1.0f, amount);
		color = Color.HSVToRGB(h, s, v);
		color.a = a;

		return color;
	}

	public static Color Desaturate(Color color, float amount)
	{
		float h;
		float s;
		float v;
		float a = color.a;

		Color.RGBToHSV(color, out h, out s, out v);
		s = Mathf.Lerp(s, 0.0f, amount);
		color = Color.HSVToRGB(h, s, v);
		color.a = a;

		return color;
	}

	public static Color Inverse(Color color)
	{
		float h;
		float s;
		float v;

		Color.RGBToHSV(color, out h, out s, out v);

		h = h + 0.5f;
		h = h + Random.Range(0,0.2f) - 0.1f;
		if (h > 1) h -= 1;

		return Color.HSVToRGB(h, s, v);
	}


	public static Texture2D CreateTexture(int offset, string name, bool transparent)
	{
		Texture2D texture;

		texture = new Texture2D(Chunk.Size * TileSize * TileGridWidth, Chunk.Size * TileSize * TileGridHeight, TextureFormat.RGBA32, false);

		texture.name = name;
		texture.wrapMode = TextureWrapMode.Repeat;
		texture.filterMode = FilterMode.Point;

		int ci = transparent ? 32 : 0;

		for (int i = ci; i < ci + 32; i++)
		{
			int row = Mathf.FloorToInt((i - ci) / (float)TileGridWidth);
			int column = i % TileGridWidth;

			int xoffset = column * TileSize * Chunk.Size;
			int yoffset = row * TileSize * Chunk.Size;

			for (int y = 0; y < Chunk.Size; y++)
			{
				for (int x = 0; x < Chunk.Size; x++)
				{
					if (!transparent)
					{
						FillSwatchSolid(texture, xoffset + x * TileSize, yoffset + y * TileSize, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < Mathf.FloorToInt(TileGridWidth / 8f))
					{
						//FillSwatchGrid(texture, xoffset + x * TileSize, yoffset + y * TileSize, 1, 4, colors[i]);
						FillSwatchTransparent(texture, xoffset + x * TileSize, yoffset + y * TileSize, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 2 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						//FillSwatchGrid(texture, xoffset + x * TileSize, yoffset + y * TileSize, 1, 2, colors[i]);
						FillSwatchTransparent(texture, xoffset + x * TileSize, yoffset + y * TileSize, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 3 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						//FillSwatchGrid(texture, xoffset + x * TileSize, yoffset + y * TileSize, 1, 1, colors[i]);
						FillSwatchTransparent(texture, xoffset + x * TileSize, yoffset + y * TileSize, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 4 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						FillSwatchGrid(texture, xoffset + x * TileSize, yoffset + y * TileSize, 2, 2, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 5 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						FillSwatchGrid(texture, xoffset + x * TileSize, yoffset + y * TileSize, 1, 4, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 6 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						FillSwatchCheckerboard(texture, xoffset + x * TileSize, yoffset + y * TileSize, 1, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 7 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						FillSwatchCheckerboard(texture, xoffset + x * TileSize, yoffset + y * TileSize, 2, colors[i]);
					}
					else if (row < TileGridHeight / 2f && column < 7 * Mathf.FloorToInt(TileGridWidth / 8f))
					{
						FillSwatchCheckerboard(texture, xoffset + x * TileSize, yoffset + y * TileSize, 4, colors[i]);
					}
					else
					{
						FillSwatchTransparent(texture, xoffset + x * TileSize, yoffset + y * TileSize, colors[i]);
					}
				}
			}
		}
		
		texture.Apply();
		return texture;
	}

	static void FillSwatchSolid(Texture2D texture, int offsetX, int offsetY, Color color)
	{
		for (int x = offsetX; x < offsetX + TileSize; x++)
		{
			for (int y = offsetY; y < offsetY + TileSize; y++)
			{
				texture.SetPixel(x, y, color);
			}	
		}
	}

	static void FillSwatchTransparent(Texture2D texture, int offsetX, int offsetY, Color color)
	{
		Color transColor = new Color(color.r, color.g, color.b, 0.5f);

		for (int x = offsetX; x < offsetX + TileSize; x++)
		{
			for (int y = offsetY; y < offsetY + TileSize; y++)
			{
				texture.SetPixel(x, y, transColor);
			}	
		}
	}

	static void FillSwatchGrid(Texture2D texture, int offsetX, int offsetY, int tile, int border, Color color)
	{
		Color transColor = new Color(color.r, color.g, color.b, 0.35f);

		int gridSize = Mathf.FloorToInt(TileSize / (float)tile); 

		for (int ix = 0; ix < tile; ix++)
		{
			for (int iy = 0; iy < tile; iy++)
			{
				int xStart = offsetX + (gridSize * ix);
				int yStart = offsetY + (gridSize * iy);

				for (int x = xStart; x < xStart + gridSize; x++)
				{
					for (int y = yStart; y < yStart + gridSize; y++)
					{

						if (y < yStart + border
							|| y >= yStart + gridSize - border - 1 
							|| x < xStart + border
							|| x >= xStart + gridSize - border - 1)
						{
							texture.SetPixel(x, y, color);
						}
						else
						{
							texture.SetPixel(x, y, transColor);
						}

					}	
				}
			}
		}
	}

	public static void FillSwatchCheckerboard(Texture2D texture, int offsetX, int offsetY, int size, Color color)
	{
		Color transColor = new Color(color.r, color.g, color.b, 0.35f);

		for (int x = offsetX; x < offsetX + TileSize; x++)
		{
			for (int y = offsetY; y < offsetY + TileSize; y++)
			{
				if (x % 2 == 0)
				{
					if (x % (size * 2) < size && y % (size * 2) >= size)
					{
						texture.SetPixel(x, y, color);
					}
					else
					{
						texture.SetPixel(x, y, transColor);
					}
				}
				else
				{
					if (x % (size * 2) >= size && y % (size * 2) < size)
					{
						texture.SetPixel(x, y, color);
					}
					else
					{
						texture.SetPixel(x, y, transColor);
					}
				}

			}	
		}
	}

	// Size should probably be divisible by two :)
	public static void GenerateColorPalette()
	{
		float valueCap = Random.Range(0.75f, 1.0f);
		float saturationCap = Random.Range(0.75f, 1.0f);

		// Randomized on hue only
		Color seedColor = Random.ColorHSV(0f, 1f, saturationCap, saturationCap, valueCap, valueCap);

		// SingleColor: a color with a smooth tone gradient
		// WarmCool: a color and its complimentary opposites
		// DarkLight: one color with dark and light tones
		// ValueColor: a combination of the two
		GradientType type = (GradientType)Random.Range(2,4);

		GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(colors, 0);

		valueCap = Random.Range(0.75f, 1.0f);
		saturationCap = Random.Range(0.5f, 1.0f);
		type = (GradientType)Random.Range(0,2);
		seedColor = Inverse(colors[16]);

		GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(colors, 32);

		//lightColor = Color.Lerp(colors[30], Color.white, 0.85f);
		//darkColor = Color.Lerp(colors[1], Color.black, 0.85f);

		// for (int i = 0; i < 64; i++)
		// {
		// 	colors[64 + i] = Inverse(colors[i]);
		// }
	}

	static List<Color> GenerateGradients(GradientType type, Color seedColor, float saturationCap, float valueCap, int size)
	{
		List<Color> gradients = new List<Color>();

		float valueStart = 0.0f;
		float valueEnd = valueCap;
		float saturationStart = 0.0f;
		float saturationEnd = saturationCap;

		if (type == GradientType.WarmCool || type == GradientType.SingleColor)
		{
			valueStart = Random.Range(0.5f, 0.75f);
			valueEnd = valueCap;
			saturationStart = valueStart;
			saturationEnd = saturationCap;
		} 
		else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
		{
			valueStart = Random.Range(0f, 0.5f);
			valueEnd = Random.Range(valueStart, 1f);
			saturationStart = valueStart - 0.25f;
			saturationEnd = saturationCap;
		}

		float h;
		float s;
		float v;

		Color.RGBToHSV(seedColor, out h, out s, out v);

		//float hueRange = Random.Range(0f,0.25f);
		float hueRange = Mathf.Lerp(0f, 1f, Random.value * Random.value * Random.value);
		float hue;
		float darkest = Random.value / 2f;
		float lightest = Random.value / 2f;
		float dark;
		float light;
		float coin;

		// Fill in the first range
		for (int i = 0; i < size; i++)
		{
			if (type == GradientType.SingleColor)
			{
				hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)((size * 2f) - 1f));
			}
			else
			{
				hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)(size - 1f));
			}

			if (hue > 1) hue -= 1;
			if (hue < 0) hue += 1;

			coin = Random.value;

			if (type == GradientType.WarmCool)
			{
				if (coin < 0.25)
				{
					dark = Mathf.Lerp(0, darkest, Random.value);
					// Run from saturation to value
					gradients.Add(Darken(Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueEnd, valueStart, i / (float)(size - 1)), v),
						Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationStart, saturationEnd, i / (float)(size - 1))), 0.5f), dark));
				} 
				else if (coin < 0.5)
				{
					light = Mathf.Lerp(0, lightest, Random.value);
					// Run from saturation to value
					gradients.Add(Lighten(Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueEnd, valueStart, i / (float)(size - 1)), v),
						Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationStart, saturationEnd, i / (float)(size - 1))), 0.5f), light));
				}
				else
				{
					gradients.Add(Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueEnd, valueStart, i / (float)(size - 1)), v),
						Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationStart, saturationEnd, i / (float)(size - 1))), 0.5f));
				}


			}
			else if (type == GradientType.SingleColor)
			{
				if (coin < 0.25)
				{
					dark = Mathf.Lerp(0, darkest, Random.value * Random.value);
					gradients.Add(Darken(Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1))), dark));
				}
				else if (coin < 0.5)
				{
					light = Mathf.Lerp(0, lightest, Random.value * Random.value);
					gradients.Add(Lighten(Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1))), light));
				}
				else
				{
					gradients.Add(Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1))));
				}
				
			}
			else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
			{
				gradients.Add(Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v));
			}

		}

		// Flip the color if needed, and apply a small skew
		if (type == GradientType.WarmCool || type == GradientType.ValueColor)
		{
			h = h + 0.5f;
			h = h + Random.Range(0,0.25f) - 0.125f;
			if (h > 1) h -= 1;
		}

		// Fill in the second range
		for (int i = 0; i < size; i++)
		{
			if (type == GradientType.SingleColor)
			{
				hue = Mathf.Lerp(h + hueRange, h - hueRange, i + size / (float)((size * 2f) - 1f));
			}
			else
			{
				hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)(size - 1f));
			}

			if (hue > 1) hue -= 1;
			if (hue < 0) hue += 1;

			if (type == GradientType.WarmCool)
			{
				// Use the range to vary the value, then flip it and vary the saturation
				gradients.Add(Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)), v),
					Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1))), 0.5f));
			}
			else if (type == GradientType.SingleColor)
			{
				gradients.Add(Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v));
			}
			else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
			{
				gradients.Add(Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1))));
			}
		}

		return gradients;
	}
}