using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BlockAir : Block {

	public BlockAir(): base()
	{
		type =  Block.Type.air;
	}

	public override bool IsSolid(Block.Direction direction)
	{
		return false;
	}

}
