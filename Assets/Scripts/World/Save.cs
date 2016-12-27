using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Save 
{
	public Dictionary<WorldPosition, Block> blocks = new Dictionary<WorldPosition, Block>();

	public Save(Chunk chunk)
	{
		for (int x = 0; x < Chunk.Size; x++)
		{
			for (int y = 0; y < Chunk.Size; y++)
			{
				for (int z = 0; z < Chunk.Size; z++)
				{
					uint index = Chunk.GetBlockDataIndex(x, y, z);
					if (chunk._blocks[index] == null || !chunk._blocks[index].changed) 
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
