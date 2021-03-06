﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Block {

	public static ushort Null = UInt16.MaxValue;
	public static ushort Air = UInt16.MaxValue - 1;
	public enum Type { rock, glass, air, undefined };
	public enum Direction { north, east, south, west, up, down };
	public struct TileIndex { public int x; public int y; }

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
		Color color = Tile.Colors[tileIndex];
		this.color.r = color.r;
		this.color.g = color.g;
		this.color.b = color.b;

		type = Type.rock;
	}

	public virtual TileIndex TexturePosition(Direction direction)
	{
		TileIndex tile = new TileIndex();
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
		TileIndex tilePos = TexturePosition(direction);

		// relative coords of section within 16x16 color area;
		int x1;
		int y1;
		int x2;
		int y2;

		// remove tile border artifacts
        if (width == Chunk.Size)
            width--;

        if (height == Chunk.Size) 
            height--;


        if (direction == Direction.up || direction == Direction.north || direction == Direction.west)
        {
            x1 = Chunk.HalfSize - Mathf.CeilToInt(height / 2f);
            x2 = Chunk.HalfSize + Mathf.FloorToInt(height / 2f) - 1;
            y1 = Chunk.HalfSize - Mathf.CeilToInt(width / 2f);
            y2 = Chunk.HalfSize + Mathf.FloorToInt(width / 2f) - 1;
        }
        else
        {
            x1 = Chunk.HalfSize - Mathf.CeilToInt(width / 2f);
            x2 = Chunk.HalfSize + Mathf.FloorToInt(width / 2f) - 1;
            y1 = Chunk.HalfSize - Mathf.CeilToInt(height / 2f);
            y2 = Chunk.HalfSize + Mathf.FloorToInt(height / 2f) - 1;
        }

        UVs[0] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x2) + tileXSize, 
			tileYSize * tilePos.y + (tileYSize * y1)
		);
		UVs[1] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x2) + tileXSize, 
			tileYSize * tilePos.y + (tileYSize * y2) + tileYSize
		);
		UVs[2] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x1), 
			tileYSize * tilePos.y + (tileYSize * y2) + tileYSize
		);
		UVs[3] = new Vector2(
			tileXSize * tilePos.x + (tileXSize * x1), 
			tileYSize * tilePos.y + (tileYSize * y1)
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
