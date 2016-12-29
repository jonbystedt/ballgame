using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Block {

	public static ushort Null = UInt16.MaxValue;
	public static ushort Air = UInt16.MaxValue - 1;
	public enum Type { rock, glass, air, undefined };
	public enum Direction { north, east, south, west, up, down };
	public struct Tile { public int x; public int y; }

	bool _transparent = false;
	public bool transparent
	{
		get { return _transparent; }
		protected set
		{
			if (value == true)
			{
				type = Type.glass;
			}
			else
			{
				type = Type.rock;
			}

			_transparent = value;
		}
	}

	public TileColor color;
	public Type type;

	private static float pixelXPercent = 1 / (float)(TileFactory.TileGridWidth * TileFactory.TileSize * Chunk.Size);
	private static float pixelYPercent = 1 / (float)(TileFactory.TileGridHeight * TileFactory.TileSize * Chunk.Size);

	protected static float tileXSize = pixelXPercent * TileFactory.TileSize;
	protected static float tileYSize = pixelYPercent * TileFactory.TileSize;

	private int tileX = 0;
	private int tileY = 0;

	public Block() 
	{
		type = Type.undefined;
	}

	public Block(int tileIndex) 
	{
		SetTile(tileIndex);
		Color color = TileFactory.colors[tileIndex];
		this.color.r = color.r;
		this.color.g = color.g;
		this.color.b = color.b;

		type = Type.rock;
	}

	public virtual Tile TexturePosition(Direction direction)
	{
		Tile tile = new Tile();
		tile.x = tileX;
		tile.y = tileY;

		return tile;
	}

	public virtual void SetTile(int tileIndex)
	{
		tileX = (tileIndex % TileFactory.TileGridWidth) * Chunk.Size;
		tileY = (3 - Mathf.FloorToInt(tileIndex / (float)TileFactory.TileGridWidth)) * Chunk.Size;
	}

	public virtual Vector2[] FaceUVs(Direction direction, int width, int height)
	{
		Vector2[] UVs = new Vector2[4];
		Tile tilePos = TexturePosition(direction);

		// relative coords of section within 16x16 color area;
		int x1;
		int y1;
		int x2;
		int y2;

		int tileXBorder = 0;
		int tileYBorder = 0;

		// remove tile border artifacts
		if (width > Mathf.FloorToInt(Chunk.Size*0.5f))
			width = Mathf.FloorToInt(width*0.5f);

		if (height > Mathf.FloorToInt(Chunk.Size*0.5f))
			height = Mathf.FloorToInt(height*0.5f);

		if (direction == Direction.up || direction == Direction.north || direction == Direction.west)
		{
			x1 = Mathf.FloorToInt((Chunk.Size - height) / 2f);
			y1 = Mathf.FloorToInt((Chunk.Size - width) / 2f);
			x2 = height + x1 - 1;
			y2 = width + y1 - 1;
		}
		else
		{
			x1 = Mathf.FloorToInt((Chunk.Size - width) / 2f);
			y1 = Mathf.FloorToInt((Chunk.Size - height) / 2f);
			x2 = width + x1 - 1;
			y2 = height + y1 - 1;
		}

		UVs[0] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x2) + tileXSize - tileXBorder, 
			tileYSize * tilePos.y + (tileYSize * y1) + tileYBorder
		);
		UVs[1] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x2) + tileXSize - tileXBorder, 
			tileYSize * tilePos.y + (tileYSize * y2) + tileYSize - tileYBorder
		);
		UVs[2] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x1) + tileXBorder, 
			tileYSize * tilePos.y + (tileYSize * y2) + tileYSize - tileYBorder
		);
		UVs[3] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x1) + tileXBorder, 
			tileYSize * tilePos.y + (tileYSize * y1) + tileYBorder
		);

		return UVs;
	}

	public virtual bool IsSolid(Direction direction)
	{
		switch(direction) {
			case Direction.north:
				return true;
			case Direction.east:
				return true;
			case Direction.south:
				return true;
			case Direction.west:
				return true;
			case Direction.up:
				return true;
			case Direction.down:
				return true;
		}

		return false;
	}
}
