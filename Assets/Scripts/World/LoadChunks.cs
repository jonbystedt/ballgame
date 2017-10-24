using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class LoadChunks : MonoBehaviour 
{	
	public InterpolatedNoise noise;

	public bool loading = false;
	public bool spawning = false;
	public bool building = false;
	public float progress = 0;

	bool pipelineActive = false;

	List<World3> updateList = new List<World3>();
	List<World3> buildList = new List<World3>();
	List<World3> currentBuilds = new List<World3>();

	World3 playerChunkPos = new World3();
	World3 oldPosition = new World3();
	World3 center;
	World3 pos;
	Chunk testChunk;
	float spawnTime;

	void Update() 
	{
		if (pipelineActive)
		{
			FindChunksToLoad();
			BuildChunks();
			UpdateChunks();
		}

		if (loading && spawning && spawnTime <= Time.time) 
		{
			spawnTime = Time.time + Config.SpawnTiming;
			ExecuteSpawn();
		}
	}

	public void _start()
	{
		pipelineActive = true;
		spawnTime = Time.time + Config.SpawnTiming;
	}

	public void Reset()
	{
		StopAllCoroutines();
		pipelineActive = false;
		loading = false;
		spawning = false;
		building = false;
		progress = 0;
		Game.ChunksLoaded = 0;

		updateList = new List<World3>();
		buildList = new List<World3>();

		noise.Clear();
	}

	void FindChunksToLoad()
	{
		if (!loading)
		{
			return;
		}

		//Get the position of this gameobject to generate around
		playerChunkPos = World.GetChunkPosition(transform.position);

		// Update the positional display
		//if (playerChunkPos != oldPosition)
		//{
		//	Game.UpdatePosition(playerChunkPos);
		//}
		
		if (buildList.Count == 0 && updateList.Count == 0)
		{	
			for (int i = 0; i < ChunkData.LoadOrder.Count(); i++)
			{
				//translate the player position and array position into chunk position
				World3 chunkPosition = new World3(
					ChunkData.LoadOrder[i].x * Chunk.Size + playerChunkPos.x, 
					0, 
					ChunkData.LoadOrder[i].z * Chunk.Size + playerChunkPos.z
				);

				if (updateList.Contains(chunkPosition))
				{
					continue;
				}
				
				//Get the chunk in the defined position
				testChunk = World.GetChunk(chunkPosition);
				
				//If the chunk already exists and it's already rendered continue
				if (testChunk != null && (testChunk.rendered || testChunk.updating))
				{
					continue;
				}

				//load a column of chunks in this position
				// build ring of columns around center column, then build center
				int xMin = chunkPosition.x - Chunk.Size;
				int xMax = chunkPosition.x + Chunk.Size;
				int zMin = chunkPosition.z - Chunk.Size;
				int zMax = chunkPosition.z + Chunk.Size;

				// Add the center to the build list. We want it to build last.
				if (testChunk == null)
				{
					for (int y = -3; y < 1; y++)
					{
						center = new World3(chunkPosition.x, y * Chunk.Size, chunkPosition.z);
						buildList.Insert(0, center);
					}
				}

				// ring
				for (int y = -3; y < 1; y++)
				{
					pos = new World3(chunkPosition.x, y * Chunk.Size, zMin);
					testChunk = World.GetChunk(pos);
					if (testChunk == null)
					{
						buildList.Insert(0, pos);
					}
				}
				for (int y = -3; y < 1; y++)
				{
					pos = new World3(xMax, y * Chunk.Size, chunkPosition.z);
					testChunk = World.GetChunk(pos);
					if (testChunk == null)
					{
						buildList.Insert(0, pos);
					}

				}
				for (int y = -3; y < 1; y++)
				{
					pos = new World3(chunkPosition.x, y * Chunk.Size, zMax);
					testChunk = World.GetChunk(pos);
					if (testChunk == null)
					{
						buildList.Insert(0, pos);
					}
				}
				for (int y = -3; y < 1; y++)
				{
					pos = new World3(xMin, y * Chunk.Size, chunkPosition.z);
					testChunk = World.GetChunk(pos);
					if (testChunk == null)
					{
						buildList.Insert(0, pos);
					}
				}

				// Finally, add the center to the update list to render it.
				for (int y = -3; y < 1; y++)
				{
					center = new World3(chunkPosition.x, y * Chunk.Size, chunkPosition.z);
					updateList.Insert(0, center);
				}
					
				//exit after queuing 1 column
				oldPosition = playerChunkPos;
				break;
			}
		}
	}
	
	void BuildChunks()
	{
		if (buildList.Count == 0 || !loading || building)
		{
			return;
		}

		currentBuilds.Clear();
		building = true;

		// Pass the chunks' positions in to queue the build processes
		for (int i = buildList.Count - 1; i >= 0; i--)
		{
			World.CreateChunk(buildList[i]);
			currentBuilds.Add(buildList[i]);

			buildList.RemoveAt(i);

			if (currentBuilds.Count > 3)
			{
				break;
			}
		}

		StartCoroutine(AwaitBuildComplete());
	}

	IEnumerator AwaitBuildComplete()
	{
		while (true)
		{
			yield return null;

			bool complete = true;

			for (int i = currentBuilds.Count - 1; i >= 0; i--)
			{
				World3 pos = currentBuilds[i];

				Chunk chunk = World.GetChunk(pos);

				if (chunk == null)
				{
					currentBuilds.RemoveAt(i);
				}
				else if (!chunk.built)
				{
					complete = false;
					break;
				}
			}

			if (complete)
			{
				building = false;
				break;
			}

		}
	}
	
	void UpdateChunks ()
	{
		// got to wait for all chunks to build before updating. 
		// the update chunk may already be built so it would otherwise race ahead.
		if (updateList.Count == 0 || !loading || building)
		{
			return;
		}

		int updateListCount = updateList.Count;

		for (int i = updateList.Count - 1; i >= 0; i--)
		{
			// Check first that this chunk is still in range and we should be instantiating it.
			float distance = Vector3.Distance
			(
				new Vector3(updateList[i].x, 0, updateList[i].z),
				new Vector3(transform.position.x, 0, transform.position.z
			));

			// If not, remove it from the list as it will be deleted immediately anyway
			if (distance > (Config.ChunkDeleteRadius - 1) * Chunk.Size)
			{
				updateList.RemoveAt(i);
				continue;
			}

			Chunk chunk = World.GetChunk(updateList[i]);

			bool neighbors = CheckNeighbors(chunk);

			if (chunk != null && chunk.built && neighbors)
			{
				if (chunk.isActive)
				{
					chunk.update = true;
				}
				updateList.RemoveAt(i);
			}
		}
	}

	bool CheckNeighbors(Chunk chunk)
	{
		// check around
		for (int xi = -1; xi <= 1; xi += 2)
		{
			World3 pos = new World3(
				chunk.pos.x + (xi * Chunk.Size), 
				chunk.pos.y, 
				chunk.pos.z);

			Chunk neighbor = World.GetChunk(pos);

			if (neighbor == null || !neighbor.built)
			{
				return false;
			}
		}

		for (int zi = -1; zi <= 1; zi += 2)
		{
			World3 pos = new World3(
				chunk.pos.x, 
				chunk.pos.y, 
				chunk.pos.z + (zi * Chunk.Size));

			Chunk neighbor = World.GetChunk(pos);

			if (neighbor == null || !neighbor.built)
			{
				return false;
			}

		}

		int yStart = chunk.pos.y == 0 ? -1 : 1;
		int yEnd = chunk.pos.y == -((Config.WorldSize - 1f) * Chunk.Size) ? 1 : -1;

		// check above and/or below
		for (int yi = yStart; yi <= yEnd; yi++)
		{
				World3 pos = new World3
				(
					chunk.pos.x, 
					chunk.pos.y + (yi * Chunk.Size), 
					chunk.pos.z
				);

				Chunk neighbor = World.GetChunk(pos);

				if (neighbor == null || !neighbor.built)
				{
					return false;
				}
		}

		return true;
	}

	void ExecuteSpawn()
	{
		if (!Game.Active)
		{
			return;
		}

		if (World.Columns.Count > 0)
		{
			for (int i = 0; i < ChunkData.SpawnOrder.Count(); i++)
			{
				//translate the player position and array position into chunk position
				World3 spawnPosition = new World3
				(
					ChunkData.SpawnOrder[i].x * Chunk.Size + playerChunkPos.x,
					0, 
					ChunkData.SpawnOrder[i].z * Chunk.Size + playerChunkPos.z
				);

				Column column;
				if (World.Columns.TryGetValue(spawnPosition.GetHashCode(), out column))
				{
					if (!column.spawned && column.rendered)
					{
						// Only spawn if the player is still around
						World3 currentPosition = World.GetChunkPosition(new Vector3
						(
							Game.Player.transform.position.x,
							0,
							Game.Player.transform.position.z
						));
						World3 columnPosition = World.GetChunkPosition(new Vector3
						(
							column.region.min.x,
							0,
							column.region.min.z
						));

						float distance = Vector3.Distance(columnPosition.ToVector3(), currentPosition.ToVector3());

						if (distance <= Config.SpawnRadius * Chunk.Size)
						{
							column.SpawnColumn(spawnPosition, World.Spawn); 
						}
					}
				}
			}
		}
	}

	// void ExecuteDespawn()
	// {
	// 	if (despawnList.Count == 0)
	// 	{
	// 		foreach(Column column in World.Columns.Values)
	// 		{
	// 			despawnList.Add(new World3(column.chunks[0].x, 0, column.chunks[0].z));
	// 		}
	// 	}
	// 	else
	// 	{
	// 		Column column;
	// 		if (World.Columns.TryGetValue(despawnList[0].GetHashCode(), out column))
	// 		{
	// 			for (int i = column.spawns.Count - 1; i >= 0; i--)
	// 			{
	// 				PooledObject obj = column.spawns[i];
					
	// 				if (obj != null && obj.isActive)
	// 				{
	// 					int distance = Mathf.FloorToInt(Vector2.Distance(
	// 						new Vector2(transform.position.x, transform.position.z),
	// 						new Vector2(obj.transform.position.x, obj.transform.position.z)
	// 						));
						
	// 					if (distance > Config.DespawnRadius * Chunk.Size)
	// 					{
	// 						obj.ReturnToPool();
	// 						column.spawns.RemoveAt(i);
	// 					}
	// 				}
	// 				else
	// 				{
	// 					column.spawns.RemoveAt(i);
	// 				}
	// 			}

	// 			if (column.spawns.Count == 0)
	// 			{
	// 				column.spawned = false;
	// 			}
	// 		}

	// 		despawnList.RemoveAt(0);
	// 	}
	// }

	IEnumerator Wait(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback();
	}

}
