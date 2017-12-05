using UnityEngine;
using System.Collections.Generic;
using System;

public enum GradientType
{
	SingleColor,
	WarmCool,
	DarkLight,
	ValueColor
}

public class TileFactory : MonoBehaviour
{

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

    static List<Action<Texture2D, int, int, Color>> cloudPaint = new List<Action<Texture2D, int, int, Color>>();

    public static void Clear()
	{
		Destroy(_stoneTexture);
		Destroy(_glassTexture);
		_stoneTexture = null;
		_glassTexture = null;
	}

    private static void AddPainters()
    {
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchGrid(tex, xoff, yoff, 1, 4, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchGrid(tex, xoff, yoff, 1, 2, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchGrid(tex, xoff, yoff, 2, 1, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchGrid(tex, xoff, yoff, 2, 2, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchGrid(tex, xoff, yoff, 1, 4, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchCheckerboard(tex, xoff, yoff, 1, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchCheckerboard(tex, xoff, yoff, 2, color);
        });
        cloudPaint.Add((Texture2D tex, int xoff, int yoff, Color color) =>
        {
            FillSwatchCheckerboard(tex, xoff, yoff, 4, color);
        });
    }


    public static Texture2D CreateTexture(int offset, string name, bool transparent)
	{
        if (transparent && cloudPaint.Count == 0)
        {
            AddPainters();
        }

        Texture2D texture = new Texture2D
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
                        cloudPaint[i % 8]
                        (
                            texture,
                            xoffset + x * TileSize,
                            yoffset + y * TileSize,
                            Tile.Colors[i]
                        ); 
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
        int half, 
        Color color
    )
	{
		Color trans = new Color(color.r, color.g, color.b, 0f);
        int size = half * 2;

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
}
