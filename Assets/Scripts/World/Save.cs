using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Save 
{
	public Dictionary<WorldPosition, ushort> blocks = new Dictionary<WorldPosition, ushort>();

	public Save(Chunk chunk)
	{
		for (int x = 0; x < Chunk.Size; x++)
		{
			for (int y = 0; y < Chunk.Size; y++)
			{
				for (int z = 0; z < Chunk.Size; z++)
				{
					uint index = Chunk.BlockIndex(x, y, z);
					if (chunk._blocks[index] == Block.Null || !chunk._changes[index]) 
					{
						continue;
					}

					WorldPosition pos = new WorldPosition(x, y, z);
					blocks.Add(pos, chunk._blocks[index]);
				}
			}
		}
	}
}
