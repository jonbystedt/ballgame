using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BlockAir : Block {

	public BlockAir()
		: base()
	{
		type =  Block.Type.air;
		initialized = true;
	}

//	public override MeshData BlockData (Chunk chunk, int x, int y, int z, MeshData meshData)
//	{
//		return meshData;
//	}

	public override bool IsSolid(Block.Direction direction)
	{
		return false;
	}

}
