using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public static class Blocks {

	private static Dictionary<ushort, Block> BlockLookup = new Dictionary<ushort, Block>();
	private static ConcurrentDictionary<int, Vector2[]> UVLookup = new ConcurrentDictionary<int, Vector2[]>();

	public static void Initialize()
	{
		BlockLookup = new Dictionary<ushort, Block>();
		UVLookup = new ConcurrentDictionary<int, Vector2[]>();

		ushort blockId = 0;
		for (int i = 0; i < Tile.Colors.Length; i++, blockId++ )
		{
			BlockLookup.Add(blockId, new Block(i));
		}

		for (int i = 0; i < Tile.Colors.Length; i++, blockId++ )
		{
			BlockLookup.Add(blockId, new BlockGlass(i));
		}
	}

	public static ushort Rock(int index)
	{
		return (ushort)index;
	}

	public static ushort Glass(int index)
	{
		return (ushort)(Tile.Colors.Length + index);
	}

	public static Block.Type GetType(ushort blockId)
	{
		if (blockId == Block.Null)
		{
			return Block.Type.undefined;
		}

		if (blockId == Block.Air)
		{
			return Block.Type.air;
		}

		if (blockId < Tile.Colors.Length)
		{
			return Block.Type.rock;
		}

		return Block.Type.glass;
	}

	// TODO: This should be updated to account for any pattern on this tile
	public static int GetTileCode(ushort blockId)
	{
		return GetColor(blockId).GetHashCode();
	}

	public static TileColor GetColor(ushort blockId)
	{
		Block block;
		TileColor color = new TileColor();

		BlockLookup.TryGetValue(blockId, out block);
		if (block != null)
		{
			color = block.color;
		}

		return color;
	}

	public static Vector2[] GetFaceUVs(ushort blockId, Block.Direction direction, int width, int height)
	{
		Vector2[] uvs;
		int sector = (direction == Block.Direction.up || direction == Block.Direction.north || direction == Block.Direction.west) ? 0 : 1;
		int hash = GetUVHash(GetTileCode(blockId), sector, width, height);
		UVLookup.TryGetValue(hash, out uvs);
		if (uvs != null)
		{
			return uvs;
		}

		Block block;
		BlockLookup.TryGetValue(blockId, out block);
		if (block != null)
		{
			uvs = BlockLookup[blockId].FaceUVs(direction, width, height);
			UVLookup.TryAdd(hash, uvs);
		}
		return uvs;
	}

	private static int GetUVHash(int tileCode, int sector, int width, int height)
	{
		unchecked 
		{
			int hash = 17;
			hash = hash * 23 + tileCode;
			hash = hash * 23 + sector;
			hash = hash * 23 + width;
			hash = hash * 23 + height;
			return hash;
		}
	}
}
