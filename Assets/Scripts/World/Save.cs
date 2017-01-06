using System.Collections.Generic;
using System;

[Serializable]
public class Save 
{
	public Dictionary<WorldPosition, ushort> blocks = new Dictionary<WorldPosition, ushort>();

	public Save(Chunk chunk)
	{
		for (int i = 0; i < chunk._changes.Count; i++)
		{
			uint index = chunk._changes[i];
			WorldPosition pos = Chunk.BlockPosition(index);
			blocks.Add(pos, chunk._blocks[index]);
		}
	}
}
