using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BlockGlass : Block {
	
	public BlockGlass()
		:base()
	{
		transparent = true;	
	}

	public BlockGlass(int colorIndex)
		:base(colorIndex)
	{
		transparent = true;	
	}

	public override Vector2[] FaceUVs(Direction direction, int width, int height)
	{
		Vector2[] UVs = new Vector2[4];
		TileIndex tilePos = TexturePosition(direction);

		// relative coords of section within 16x16 color area;
		int x1;
		int y1;
		int x2;
		int y2;

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

	public override bool IsSolid(Block.Direction direction)
	{
		return false;
	}
}
