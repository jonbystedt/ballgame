using UnityEngine;
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

	public static void Clear()
	{
		Destroy(_stoneTexture);
		Destroy(_glassTexture);
		_stoneTexture = null;
		_glassTexture = null;
	}

	public static Texture2D CreateTexture(int offset, string name, bool transparent)
	{
		Texture2D texture;

		texture = new Texture2D
        (
            Chunk.Size * TileSize * TileGridWidth, 
            Chunk.Size * TileSize * TileGridHeight, 
            TextureFormat.RGBA32, 
            false
        );

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
						FillSwatchSolid
                        (
                            texture, 
                            xoffset + x * TileSize, 
                            yoffset + y * TileSize, 
                            Tile.Colors[i]
                        );
					}
					else
                    {
                        int inc = Mathf.FloorToInt(TileGridWidth / TileSize);

                        if (column < inc)
                        {
                            FillSwatchGrid
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                1, 
                                4, 
                                Tile.Colors[i]
                            );
                        }
                        else if (column < 2 * inc)
                        {
                            FillSwatchGrid
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                1, 
                                2, 
                                Tile.Colors[i]
                            );
                        }
                        else if (column < 3 * inc)
                        {
                            FillSwatchGrid
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                2, 
                                1, 
                                Tile.Colors[i]
                            );
                        }
                        else if (column < 4 * inc)
                        {
                            FillSwatchGrid
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                2, 
                                2, 
                                Tile.Colors[i]
                            );
                        }
                        // covers 2 columns
                        else if (column < 6 * inc)
                        {
                            FillSwatchCheckerboard
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                1, 
                                Tile.Colors[i]
                            );
                        }
                        else if (column < 7 * inc)
                        {
                            FillSwatchCheckerboard
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                2, 
                                Tile.Colors[i]
                            );
                        }
                        else
                        {
                            FillSwatchCheckerboard
                            (
                                texture, 
                                xoffset + x * TileSize, 
                                yoffset + y * TileSize, 
                                4, 
                                Tile.Colors[i]
                            );
                        }
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
		for (int x = offsetX; x < offsetX + TileSize; x++)
		{
			for (int y = offsetY; y < offsetY + TileSize; y++)
			{
				texture.SetPixel(x, y, color);
			}	
		}
	}

	static void FillSwatchGrid
    (
        Texture2D texture, 
        int offsetX, 
        int offsetY, 
        int tile, 
        int border, 
        Color color
    )
	{
		int gridSize = Mathf.FloorToInt(TileSize / (float)tile); 
		Color trans = new Color(color.r, color.g, color.b, 0f);

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

						if 
                        ( 
                            y < yStart + border || 
                            y >= yStart + gridSize - border - 1 || 
                            x < xStart + border || 
                            x >= xStart + gridSize - border - 1 
                        )
						{
							texture.SetPixel(x, y, color);
						}
						else
						{
							texture.SetPixel(x, y, trans);
						}
					}	
				}
			}
		}
	}

	public static void FillSwatchCheckerboard
    (
        Texture2D texture, 
        int offsetX, 
        int offsetY, 
        int size, 
        Color color
    )
	{
		Color trans = new Color(color.r, color.g, color.b, 0f);
        int half = Mathf.FloorToInt(size / 2f);

		for (int x = offsetX; x < offsetX + TileSize; x++)
		{
			for (int y = offsetY; y < offsetY + TileSize; y++)
			{
				if (x % size < half)
				{
					if (y % size < half)
					{
						texture.SetPixel(x, y, trans);
					}
					else
					{
						texture.SetPixel(x, y, color);
					}
				}
				else
				{
					if (y % size < half)
					{
						texture.SetPixel(x, y, color);
					}
					else
					{
						texture.SetPixel(x, y, trans);
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

		GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(Tile.Colors, 0);

		valueCap = Random.Range(0.75f, 1.0f);
		saturationCap = Random.Range(0.5f, 1.0f);
		type = (GradientType)Random.Range(0,2);
		seedColor = Tile.Inverse(Tile.Colors[16]);

		GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(Tile.Colors, 32);

		//lightColor = Color.Lerp(Tile.colors[30], Color.white, 0.85f);
		//darkColor = Color.Lerp(Tile.colors[1], Color.black, 0.85f);

		// for (int i = 0; i < 64; i++)
		// {
		// 	Tile.colors[64 + i] = Inverse(Tile.colors[i]);
		// }
	}

	static List<Color> GenerateGradients(
		GradientType type, 
		Color seedColor, 
		float saturationCap, 
		float valueCap, 
		int size
		)
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
		float darkest = Random.value;
		float lightest = Random.value;
		float desaturateMax = Random.value;
		float dark;
		float light;
		float desat;
		float coin;

		Color color = new Color();

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
			dark = Mathf.Lerp(0, darkest, Random.value);
			light = Mathf.Lerp(0, lightest, Random.value);

			if (type == GradientType.WarmCool)
			{
				color = Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueEnd, valueStart, i / (float)(size - 1)), v),
						Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationStart, saturationEnd, i / (float)(size - 1))), 0.5f);

				if (coin < 0.5f)
				{
					color = Tile.Darken(color, dark);
				}
				else if (coin < 0.75f)
				{
					color = Tile.Lighten(color, light);
				}
			}
			else if (type == GradientType.SingleColor)
			{
				color = Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)));

				if (coin < 0.5f)
				{
					color = Tile.Darken(color, dark);
				}
				else if (coin < 0.75f)
				{
					color = Tile.Lighten(color, light);
				}			
			}
			else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
			{
				color = Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v);

				if (coin < 0.5f)
				{
					color = Tile.Darken(color, dark);
				}
				else if (coin < 0.75f)
				{
					color = Tile.Lighten(color, light);
				}
			}

			// desaturate world
			if (Random.value < 0.75f)
			{
				desat = Mathf.Lerp(0, desaturateMax, Random.value);
				color = Tile.Desaturate(color, desat);
			}

			gradients.Add(color);
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
				color = Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)), v),
					Color.HSVToRGB(hue, s,  Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1))), 0.5f);
			}
			else if (type == GradientType.SingleColor)
			{
				color = Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v);
			}
			else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
			{
				color = Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)));
			}

			gradients.Add(color);
			
		}

		return gradients;
	}
}
