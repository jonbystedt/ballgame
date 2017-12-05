using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class World : MonoBehaviour {
	
	public static Dictionary<int, Chunk> Chunks = new Dictionary<int, Chunk>();
	public static Dictionary<int, Column> Columns = new Dictionary<int, Column>();
	public static List<Chunk> ChunkList = new List<Chunk>();

	// Inspector attributes
	public PooledObject chunkPrefab;
	public PooledObject transparentChunkPrefab;
	public SpawnManager spawn;
	public TerrainGenerator generator;
	public GreedyMesher mesher;

	static Chunk[] columnChunks = new Chunk[Config.WorldHeight];

	static string _seed;

	public static string Seed 
	{
		get { return _seed; }
		set
		{
			_seed = value;
		}
	}

	public static SpawnManager Spawn
	{
		get { return _instance.spawn; }
	}

	public static  TerrainGenerator Generator
	{
		get { return _instance.generator; }
	}

	public static GreedyMesher Mesher
	{
		get { return _instance.mesher; }
	}

	public static bool ChunkShadows
	{
		set
		{
			if (value)
			{
				_instance.chunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
				_instance.transparentChunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}
			else
			{
				_instance.chunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				_instance.transparentChunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
		}
	}

	static World _instance;

	void Start() 
	{
		spawn = GetComponent<SpawnManager>();

		if (_instance == null)
		{
			_instance = this;
		}
        _instance.chunkPrefab.SetPoolSize(-1);
        _instance.transparentChunkPrefab.SetPoolSize(-1);


		_instance.chunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		_instance.transparentChunkPrefab.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
	}

	void Update()
	{
		for (int i = 0; i < ChunkList.Count; i++)
		{
			if (ChunkList[i].isActive)
			{
				ChunkList[i].CheckUpdate();
			}
		}
	}

	public void Reset()
	{
		DestroyChunks();
        Serialization.WriteWorldConfig();
        Serialization.WriteWorldHash();
        Serialization.WriteWorldColors();
        Serialization.Compress();

		Chunks = new Dictionary<int, Chunk>();
		ChunkList = new List<Chunk>();

		foreach(KeyValuePair<int,Column> item in Columns)
		{
			foreach(PooledObject spawn in item.Value.spawns)
			{
				spawn.ReturnToPool();
			}
		}

		Columns.Clear();

		Seed = "";
		generator.initialized = false;
	}

	public static World3 GetChunkPosition(Vector3 pos)
	{
		return new World3(
			Mathf.FloorToInt(pos.x / Chunk.Size) * Chunk.Size,
			Mathf.FloorToInt(pos.y / Chunk.Size) * Chunk.Size,
			Mathf.FloorToInt(pos.z / Chunk.Size) * Chunk.Size
		);
	}

	public static World3 GetChunkPosition(World3 pos)
	{
		return GetChunkPosition(new Vector3(pos.x, pos.y, pos.z));
	}

	public static World3 GetBlockPosition(Vector3 pos)
	{
		return new World3(
			Mathf.FloorToInt(pos.x),
			Mathf.FloorToInt(pos.y),
			Mathf.FloorToInt(pos.z)
		);
	}

	public static World3 GetRoundedBlockPosition(Vector3 pos)
	{
		return new World3(
			Mathf.RoundToInt(pos.x),
			Mathf.RoundToInt(pos.y),
			Mathf.RoundToInt(pos.z)
		);
	}

	public static Chunk GetChunk(World3 pos)
	{
		Chunk chunk = null;
		Chunks.TryGetValue(pos.GetHashCode(), out chunk);

		return chunk;
	}

	public static Chunk GetChunk(Vector3 pos)
	{
		World3 worldPos = World.GetChunkPosition(pos);

		return GetChunk(worldPos);
	}

	public static Chunk InstantiateChunk()
	{
		Chunk newChunk = World._instance.chunkPrefab.GetPooledInstance<Chunk>();
		TransparentChunk newTransparentChunk = World._instance.transparentChunkPrefab.GetPooledInstance<TransparentChunk>();

		newChunk.transparentChunk = (PooledObject)newTransparentChunk;

		newChunk.GetComponent<MeshRenderer>().material.mainTexture = TileFactory.stoneTexture;

		var transMat = newChunk.transparentChunk.GetComponent<MeshRenderer>().material;
		transMat.mainTexture = TileFactory.glassTexture;
		transMat.SetFloat("_EmissionScaleUI", 1);
		transMat.SetTexture("_EmissionMap", TileFactory.glassTexture);

		newChunk.Initialize();

		return newChunk;
	}
		
	public static void CreateChunk(int x, int y, int z)
	{
		// Get a chunk, either from the pool or brand new
		Chunk newChunk = InstantiateChunk();

		// Set the position of the chunk
		World3 pos = new World3(x, y, z);
		newChunk.transform.position = pos.ToVector3();
		newChunk.transparentChunk.transform.position = pos.ToVector3();
		newChunk.pos = pos;

		// Add the chunk to the world chunks
		World.Chunks.Add(pos.GetHashCode(), newChunk);
		World.ChunkList.Add(newChunk);

		// Look for the rest of the chunks in this column of chunks
		bool columnBuilt = true;
		for (int i = 1 - Config.WorldHeight; i < 1; i++)
		{
			World3 chunkPos = new World3(pos.x, i * Chunk.Size, pos.z);

			Chunk chunk;
			if (Chunks.TryGetValue(chunkPos.GetHashCode(), out chunk))
			{
				columnChunks[i + Config.WorldHeight - 1] = chunk;
			}
			else
			{
				columnBuilt = false;
				break;
			}
		}

		// If they exist we are ready to generate the column
		if (columnBuilt)
		{
			// Does this column already exist?
			World3 columnLocation = new World3(columnChunks[0].pos.x, 0, columnChunks[0].pos.z);
			if (!Columns.ContainsKey(columnLocation.GetHashCode()))
			{
				// Initialize the generator if it isn't
				if (!World._instance.generator.initialized)
				{
					World._instance.generator.Initialize();
				}

				// Kick off generation. We get a region back that encompasses these chunks.
				Region region = Generator.Generate(columnChunks);

				Column column = new Column(region, columnChunks);
				Columns.Add(columnLocation.GetHashCode(), column);
			}
		}
	}

	public static void CreateChunk(World3 pos)
	{
		CreateChunk(pos.x, pos.y, pos.z);
	}

	public static void DestroyChunkAt(int x, int y, int z)
	{
		Chunk chunk = null;
		if (Chunks.TryGetValue(new World3(x, y, z).GetHashCode(), out chunk))
		{
			DestroyChunk(chunk);
		}
	}

	public static void DestroyChunkAt(World3 pos)
	{
		Chunk chunk = null;
		if (Chunks.TryGetValue(pos.GetHashCode(), out chunk))
		{
			DestroyChunk(chunk);
		}
	}

	public static void DestroyChunk(Chunk chunk)
	{
		chunk.isActive = false;
		chunk.StopAllCoroutines();
		Chunks.Remove(chunk.pos.GetHashCode());
		ChunkList.Remove(chunk);
		Serialization.SaveChunk(chunk); 
		chunk.transparentChunk.ReturnToPool();
		chunk.ReturnToPool();
	}

	public static void DestroyChunks()
	{
		for (int i = ChunkList.Count - 1; i >= 0; i--)
		{
			DestroyChunk(ChunkList[i]);
		}
	}

	static Column GetColumn(int x, int z)
	{
		Column column = null;
		Columns.TryGetValue(new World3(x, 0, z).GetHashCode(), out column);

		return column;
	}

	public static Column GetColumn(World3 pos)
	{
		pos = GetChunkPosition(pos);
		return GetColumn(pos.x, pos.z);
	}

	public void SetColumnSpawned(int x, int z)
	{
		Column column;
		if (Columns.TryGetValue(new World3(x, 0, z).GetHashCode(), out column))
		{
			column.spawned = true;
		}
	}

	public static ushort GetBlock(int x, int y, int z)
	{
		Chunk containerChunk = GetChunk(new Vector3(x, y, z));

		if (containerChunk != null)
		{
			return containerChunk.GetBlock(
				x - containerChunk.pos.x,
				y - containerChunk.pos.y,
				z - containerChunk.pos.z);
		}
		else
		{
			return Block.Null;
		}
	}

	public static ushort GetBlock(World3 pos)
	{
		return GetBlock(pos.x, pos.y, pos.z);
	}

	public static void SetBlock(int x, int y, int z, ushort block, bool playerHit)
	{
		Chunk chunk = GetChunk(new Vector3(x, y, z));

		if (chunk != null)
		{
			chunk.SetBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);
			if (playerHit) 
			{
				chunk._changes.Add(Chunk.BlockIndex(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z));
			}
			chunk.update = true;

			// Check if adjacent chunks will need an update
			UpdateIfEqual(x - chunk.pos.x, 0, new World3(x - 1, y, z), playerHit);
			UpdateIfEqual(x - chunk.pos.x, Chunk.Size - 1, new World3(x + 1, y, z), playerHit);
			UpdateIfEqual(y - chunk.pos.y, 0, new World3(x, y - 1, z), playerHit);
			UpdateIfEqual(y - chunk.pos.y, Chunk.Size - 1, new World3(x, y + 1, z), playerHit);
			UpdateIfEqual(z - chunk.pos.z, 0, new World3(x, y, z -1), playerHit);
			UpdateIfEqual(z - chunk.pos.z, Chunk.Size - 1, new World3(x, y, z + 1), playerHit);
		}
		else
		{
			//ChunkBuffer.SetBlock(int x, int y, int z, Block block);
		}
	}

	public static void SetBlock(World3 pos, ushort block, bool playerHit)
	{
		SetBlock(pos.x, pos.y, pos.x, block, playerHit);
	}

	public static void SetBlock(World3 pos, ushort block)
	{
		SetBlock(pos, block, false);
	}

	public static void SetBlock(int x, int y, int z, ushort block)
	{
		SetBlock(x, y, z, block, false);
	}	

	static void UpdateIfEqual(int value1, int value2, World3 pos, bool playerHit)
	{
		if(value1 == value2)
		{
			Chunk chunk = GetChunk(GetChunkPosition(pos));
			if (chunk != null) 
			{
				chunk.update = true;
			}
		}
	}
}
