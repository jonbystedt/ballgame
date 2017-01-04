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
	public Text seed;

	static Chunk[] columnChunks = new Chunk[Config.WorldHeight];

	static string _seed;

	public static string Seed 
	{
		get { return _seed; }
		set
		{
			_seed = value;

			int n;
			if (int.TryParse(_seed, out n))
			{
				_instance.seed.text = n.ToString();
			}
			else
			{
				_instance.seed.text = _seed + " (" + _seed.GetHashCode().ToString() + ")";
			}
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

	static World _instance;
	static Block undefinedBlock = new Block();

	void Start() 
	{
		spawn = GetComponent<SpawnManager>();

		if (_instance == null)
		{
			_instance = this;
		}
        _instance.chunkPrefab.SetPoolSize(-1);
        _instance.transparentChunkPrefab.SetPoolSize(-1);
	}

	void Update()
	{
		for (int i = 0; i < ChunkList.Count; i++)
		{
			if (ChunkList[i].isActive)
			{
				ChunkList[i].CheckUpdate();
				//ChunkList[i].ApplyOcclusion();
			}
		}
	}

	public void Reset()
	{
		for (int i = ChunkList.Count - 1; i >= 0; i--)
		{
			DestroyChunk(ChunkList[i]);
		}

		Chunks.Clear();
		ChunkList.Clear();
		Chunk.MeshDataPool.Clear();

		ObjectPool chunkPool = ObjectPool.GetPool(chunkPrefab);
		chunkPool.ClearPool();
		ObjectPool transChunkPool = ObjectPool.GetPool(transparentChunkPrefab);
		transChunkPool.ClearPool();

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

	public static WorldPosition GetChunkPosition(Vector3 pos)
	{
		return new WorldPosition(
			Mathf.FloorToInt(pos.x / Chunk.Size) * Chunk.Size,
			Mathf.FloorToInt(pos.y / Chunk.Size) * Chunk.Size,
			Mathf.FloorToInt(pos.z / Chunk.Size) * Chunk.Size
		);
	}

	public static WorldPosition GetChunkPosition(WorldPosition pos)
	{
		return GetChunkPosition(new Vector3(pos.x, pos.y, pos.z));
	}

	public static WorldPosition GetBlockPosition(Vector3 pos)
	{
		return new WorldPosition(
			Mathf.FloorToInt(pos.x),
			Mathf.FloorToInt(pos.y),
			Mathf.FloorToInt(pos.z)
		);
	}

	public static WorldPosition GetRoundedBlockPosition(Vector3 pos)
	{
		return new WorldPosition(
			Mathf.RoundToInt(pos.x),
			Mathf.RoundToInt(pos.y),
			Mathf.RoundToInt(pos.z)
		);
	}

	public static Chunk GetChunk(WorldPosition pos)
	{
		Chunk chunk = null;
		Chunks.TryGetValue(pos.GetHashCode(), out chunk);

		return chunk;
	}

	public static Chunk GetChunk(Vector3 pos)
	{
		WorldPosition worldPos = World.GetChunkPosition(pos);

		return GetChunk(worldPos);
	}

	public static Chunk InstantiateChunk()
	{
		Chunk newChunk = World._instance.chunkPrefab.GetPooledInstance<Chunk>();
		TransparentChunk newTransparentChunk = World._instance.transparentChunkPrefab.GetPooledInstance<TransparentChunk>();

		newChunk.transparentChunk = (PooledObject)newTransparentChunk;

		newChunk.GetComponent<MeshRenderer>().material.mainTexture = TileFactory.stoneTexture;
		newChunk.transparentChunk.GetComponent<MeshRenderer>().material.mainTexture = TileFactory.glassTexture;

		newChunk.Initialize();

		return newChunk;
	}
		
	public static void CreateChunk(int x, int y, int z)
	{
		// Get a chunk, either from the pool or brand new
		Chunk newChunk = InstantiateChunk();

		// Set the position of the chunk
		WorldPosition pos = new WorldPosition(x, y, z);
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
			WorldPosition chunkPos = new WorldPosition(pos.x, i * Chunk.Size, pos.z);

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
			WorldPosition columnLocation = new WorldPosition(columnChunks[0].pos.x, 0, columnChunks[0].pos.z);
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

	public static void CreateChunk(WorldPosition pos)
	{
		CreateChunk(pos.x, pos.y, pos.z);
	}

	public static void DestroyChunkAt(int x, int y, int z)
	{
		Chunk chunk = null;
		if (Chunks.TryGetValue(new WorldPosition(x, y, z).GetHashCode(), out chunk))
		{
			DestroyChunk(chunk);
		}
	}

	public static void DestroyChunkAt(WorldPosition pos)
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
		Chunks.Remove(chunk.pos.GetHashCode());
		ChunkList.Remove(chunk);
		Serialization.SaveChunk(chunk); 
		chunk.transparentChunk.ReturnToPool();
		chunk.ReturnToPool();
	}

	static Column GetColumn(int x, int z)
	{
		Column column = null;
		Columns.TryGetValue(new WorldPosition(x, 0, z).GetHashCode(), out column);

		if (column == null)
		{
			Game.Log("Null Column Requested.");
		}

		return column;
	}

	public static Column GetColumn(WorldPosition pos)
	{
		pos = GetChunkPosition(pos);
		return GetColumn(pos.x, pos.z);
	}

	public void SetColumnSpawned(int x, int z)
	{
		Column column;
		if (Columns.TryGetValue(new WorldPosition(x, 0, z).GetHashCode(), out column))
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

	public static ushort GetBlock(WorldPosition pos)
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
				chunk._changes[Chunk.BlockIndex(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z)] = true;
			}
			chunk.playerHit = playerHit;
			chunk.update = true;

			// Check if adjacent chunks will need an update
			UpdateIfEqual(x - chunk.pos.x, 0, new WorldPosition(x - 1, y, z), playerHit);
			UpdateIfEqual(x - chunk.pos.x, Chunk.Size - 1, new WorldPosition(x + 1, y, z), playerHit);
			UpdateIfEqual(y - chunk.pos.y, 0, new WorldPosition(x, y - 1, z), playerHit);
			UpdateIfEqual(y - chunk.pos.y, Chunk.Size - 1, new WorldPosition(x, y + 1, z), playerHit);
			UpdateIfEqual(z - chunk.pos.z, 0, new WorldPosition(x, y, z -1), playerHit);
			UpdateIfEqual(z - chunk.pos.z, Chunk.Size - 1, new WorldPosition(x, y, z + 1), playerHit);
		}
		else
		{
			//ChunkBuffer.SetBlock(int x, int y, int z, Block block);
		}
	}

	public static void SetBlock(WorldPosition pos, ushort block, bool playerHit)
	{
		SetBlock(pos.x, pos.y, pos.x, block, playerHit);
	}

	public static void SetBlock(WorldPosition pos, ushort block)
	{
		SetBlock(pos, block, false);
	}

	public static void SetBlock(int x, int y, int z, ushort block)
	{
		SetBlock(x, y, z, block, false);
	}	

	static void UpdateIfEqual(int value1, int value2, WorldPosition pos, bool playerHit)
	{
		if(value1 == value2)
		{
			Chunk chunk = GetChunk(GetChunkPosition(pos));
			if (chunk != null) 
			{
				chunk.playerHit = playerHit;
				chunk.update = true;
			}
		}
	}
}
