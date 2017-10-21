using System.Collections.Generic;
using System;

[Serializable]
public class Save 
{
	public Dictionary<World3, ushort> blocks = new Dictionary<World3, ushort>();

	public Save(Chunk chunk)
	{
		for (int i = 0; i < chunk._changes.Count; i++)
		{
			uint index = chunk._changes[i];
			World3 pos = Chunk.BlockPosition(index);
			blocks.Add(pos, chunk._blocks[index]);
		}
	}
}
