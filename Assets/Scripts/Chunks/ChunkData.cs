using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class ChunkData
{
	public static World3[] LoadOrder;
	public static World3[] SpawnOrder;
	
	static ChunkData() {}

	public static void SetLoadOrder()
	{
		var chunkOffsets = new List<World3>();
		for (int x = -Config.ChunkDeleteRadius; x <= Config.ChunkDeleteRadius; x++)
		{
			for (int z = -Config.ChunkDeleteRadius; z <= Config.ChunkDeleteRadius; z++)
			{
				chunkOffsets.Add(new World3(x, 0, z));
			}
		}

		// limit how far away the blocks can be to achieve a circular loading pattern
		float maxChunkRadius = Config.ChunkLoadRadius * 1.55f;
		float maxSpawnRadius = Config.SpawnRadius * 1.55f;
		//float maxDeleteRadius = Config.ChunkDeleteRadius * 1.55f;

		//sort 2d vectors by closeness to center
		LoadOrder = chunkOffsets
			.Where(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z) < maxChunkRadius)
			.OrderBy(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z)) //smallest magnitude vectors first
			.ThenBy(pos => Mathf.Abs(pos.x)) //make sure not to process e.g (-10,0) before (5,5)
			.ThenBy(pos => Mathf.Abs(pos.z))
			.ToArray();

		SpawnOrder = chunkOffsets
			.Where(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z) < maxSpawnRadius)
			.OrderBy(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z)) //smallest magnitude vectors first
			.ThenBy(pos => Mathf.Abs(pos.x)) //make sure not to process e.g (-10,0) before (5,5)
			.ThenBy(pos => Mathf.Abs(pos.z))
			.ToArray();

		// for (int i = 1; i < LoadOrder.Count() * 4; i++)
		// {
		// 	prefab = World.InstantiateChunk();
		// 	prefab.transparentChunk.ReturnToPool();
		// 	prefab.ReturnToPool();
		// }
	}
	
}