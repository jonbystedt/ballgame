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

//	public override MeshData TransparentBlockData (Chunk chunk, int x, int y, int z, MeshData meshData)
//	{
//		meshData.useRenderDataForCol = true;
//		Block block;
//
//		block = chunk.GetBlock (x, y + 1, z);
//		if (!block.IsSolid(Direction.down) && !(block.transparent))
//		{
//			meshData = FaceDataUp(chunk, x, y, z, meshData);
//		}
//
//		block = chunk.GetBlock(x, y - 1, z);
//		if (!block.IsSolid(Direction.up) && !(block.transparent))
//		{
//			meshData = FaceDataDown(chunk, x, y, z, meshData);
//		}
//
//		block = chunk.GetBlock(x, y, z + 1);
//		if (!block.IsSolid(Direction.south) && !(block.transparent))
//		{
//			meshData = FaceDataNorth(chunk, x, y, z, meshData);
//		}
//
//		block = chunk.GetBlock(x, y, z - 1);
//		if (!block.IsSolid(Direction.north) && !(block.transparent))
//		{
//			meshData = FaceDataSouth(chunk, x, y, z, meshData);
//		}
//
//		block = chunk.GetBlock(x + 1, y, z);
//		if (!block.IsSolid(Direction.west) && !(block.transparent))
//		{
//			meshData = FaceDataEast(chunk, x, y, z, meshData);
//		}
//
//		block = chunk.GetBlock(x - 1, y, z);
//		if (!block.IsSolid(Direction.east) && !(block.transparent))
//		{
//			meshData = FaceDataWest(chunk, x, y, z, meshData);
//		}
//		
//		return meshData;
//	}

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
