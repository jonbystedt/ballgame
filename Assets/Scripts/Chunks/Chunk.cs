using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : PooledObject {

	public static int Size = 16;
	public static int HalfSize = Mathf.FloorToInt(Size * 0.5f);
	public static int NoSpawn = -9999;
	private const byte VOXEL_Y_SHIFT = 4;
	private const byte VOXEL_Z_SHIFT = 8;

	public bool update = false;
	public bool updating = false;
	public bool built = false;
	public bool rendered = false;

	public bool surrounded = false;
	public bool outofrange = false;
	public bool playerHit = false;

	private WorldPosition _pos;
	public WorldPosition pos 
	{
		get { return _pos; }
		set 
		{
			_pos = value;
			chunkOrder = Mathf.Abs(_pos.x) + Mathf.Abs(_pos.z);
		}
	}
	public PooledObject transparentChunk;
	public Column column;
	public ushort[] _blocks;
	public List<uint> _changes;
	
	private MeshFilter filter;
	private MeshCollider col;

	private Vector3 chunkCenter = Vector3.zero;
	private Vector3 playerHPos = Vector3.zero;
	private float chunkDistance;
	private WorldPosition chunkPosition;

	public static List<MeshData> MeshDataPool = new List<MeshData>();
	private Block emptyBlock = new Block();
	private int chunkOrder;
	private float slowUpdateTimer = 0f;

	[HideInInspector] public MeshRenderer _renderer;
	[HideInInspector] public MeshRenderer _glassrenderer;

	public void Initialize()
	{
		filter = gameObject.GetComponent<MeshFilter>();
		filter.mesh.Clear();
		transparentChunk.GetComponent<MeshFilter>().mesh.Clear();
		col = gameObject.GetComponent<MeshCollider>();

		_glassrenderer = transparentChunk.GetComponent<MeshRenderer>();
		_renderer = gameObject.GetComponent<MeshRenderer>();

		_blocks = new ushort[Size * Size * Size];
		_changes = new List<uint>();

		update = false;
		updating = false;
		built = false;
		rendered = false;
		surrounded = false;
		outofrange = false;

		if (column != null)
		{
			column.spawned = false;
			column.rendered = false;
			column.chunks.Clear();
		}

		for (int i = 0; i < _blocks.Length; i++)
		{
			_blocks[i] = Block.Null;
		}

		StartCoroutine(SlowUpdate());
	}
		
	public void CheckUpdate()
	{
		if (outofrange || updating)
		{
			return;
		}

		if (update)
		{
			update = false;
			updating = true;

			MeshData meshData = GetMeshData();
			MeshData transparentMeshData = GetMeshData();
			MeshData colliderMeshData = GetMeshData();

			World.Mesher.Create(meshData, _blocks, _pos, false, playerHit);
			World.Mesher.Create(transparentMeshData, _blocks, _pos, true, playerHit);
			World.Mesher.CreateCollider(colliderMeshData, _blocks, _pos, playerHit);

			if (playerHit)
			{
				playerHit = false;
			}

			StartCoroutine(AwaitMeshData(meshData,transparentMeshData, colliderMeshData));
		}
	}

	public void ApplyOcclusion()
	{
		if (rendered && Time.time > slowUpdateTimer && Time.frameCount % 100 == chunkOrder % 100)
		{
			slowUpdateTimer = Time.time + 60f;
			ApplyDistanceOcclusion();
		}
	}

	IEnumerator SlowUpdate()
	{
		for (;;)
		{
			if (!isActive)
			{
				break;
			}
			
			// Space out updates evenly
			if (Time.frameCount % 100 == chunkOrder % 100)
			{
				if (rendered)
				{
					ApplyDistanceOcclusion();
				}

				yield return new WaitForSeconds(0.1f);
			}

			yield return null;	
			
		}

	}

	void ApplyDistanceOcclusion()
	{
		// Chunk center on the horizontal plane
		chunkCenter.x = transform.position.x + Chunk.Size / 2f;
		chunkCenter.z = transform.position.z + Chunk.Size / 2f;

		playerHPos.x = Game.Player.transform.position.x;
		playerHPos.z = Game.Player.transform.position.z;

		chunkDistance = Mathf.FloorToInt(Vector3.Distance(chunkCenter, playerHPos));

		// distance based occlusion
		if (chunkDistance > Config.MaxRenderDistance)
		{
			if (_renderer.enabled)
			{
				_renderer.enabled = false;
			}
			if (_glassrenderer.enabled)
			{
				_glassrenderer.enabled = false;
			}			
			outofrange = true;
		}
		else if (chunkDistance <= Config.MaxRenderDistance)
		{
			if (!_renderer.enabled)
			{
				_renderer.enabled = true;
			}
			if (!_glassrenderer.enabled)
			{
				_glassrenderer.enabled = true;
			}	

			outofrange = false;
		} 

		// Chunk Deletion
		if (chunkDistance > Config.ChunkDeleteRadius * Chunk.Size)
		{
			World.Generator.RemoveResults(column.region);
			World.Columns.Remove(new WorldPosition(column.chunks[0].x, 0, column.chunks[0].z).GetHashCode());

			if (column.spawns.Count > 0)
			{
				for (int i = column.spawns.Count - 1; i >= 0; i--)
				{
					PooledObject spawn = column.spawns[i];
					spawn.ReturnToPool();
				}
				column.spawns.Clear();
			}

			for (int i = 0; i < column.chunks.Count; i++)
			{
				if (column.chunks[i] != null)
				{
					World.DestroyChunkAt(column.chunks[i]);
				}
			}
		}
	}

	IEnumerator AwaitMeshData(MeshData meshData, MeshData transparentMeshData, MeshData colliderMeshData)
	{
		for(;;)
		{
			if (meshData.complete && transparentMeshData.complete && colliderMeshData.complete)
			{
				break;
			}

			yield return null;
		}

		RenderMesh(meshData);
		SetColliderMesh(colliderMeshData);
		transparentChunk.GetComponent<TransparentChunk>().RenderMesh(transparentMeshData);

		ReturnMeshData(meshData);
		ReturnMeshData(transparentMeshData);
		ReturnMeshData(colliderMeshData);

		// Update flags
		if (!rendered)
		{
			rendered = true;

			Game.ChunksLoaded++;

			ApplyDistanceOcclusion();
		}

		if (column != null)
		{
			column.rendered = true;
		}

		updating = false;
	}

	void RenderMesh(MeshData meshData)
	{
		// renderer mesh
		filter.mesh.Clear();

		filter.mesh.vertices = meshData.vertices.ToArray();
		filter.mesh.triangles = meshData.triangles.ToArray();
		filter.mesh.uv = meshData.uv.ToArray();

		NormalCalculator.RecalculateNormals(filter.mesh, 60);
		filter.mesh.RecalculateBounds();
	}

	void SetColliderMesh(MeshData meshData)
	{
		// collider mesh
		col.sharedMesh = null;
		Mesh mesh = new Mesh();

		mesh.vertices = meshData.colliderVerts.ToArray();
		mesh.triangles = meshData.colliderTris.ToArray();

		NormalCalculator.RecalculateNormals(mesh, 60);
		col.sharedMesh = mesh;
	}

	public ushort GetBlock(int x, int y, int z)
	{
		if (InRange(x) && InRange(y) && InRange(z))
		{
			return _blocks[GetBlockDataIndex((uint)x, (uint)y, (uint)z)];
		}
		
		return World.GetBlock(_pos.x + x, _pos.y + y, _pos.z + z);
	}
	
	public void SetBlock(int x, int y, int z, ushort block)
	{
		if (InRange(x) && InRange(y) && InRange(z))
		{
			_blocks[GetBlockDataIndex((uint)x, (uint)y, (uint)z)] = block;
		}
		else
		{
			World.SetBlock(_pos.x + x, _pos.y + y, _pos.z + z, block);
		}
	}

	public bool IsSurrounded()
	{
		// only on the x-z axes
		Chunk chunk;

		chunkPosition.x = _pos.x + Chunk.Size;
		chunkPosition.y = _pos.y;
		chunkPosition.z = _pos.z;

		World.Chunks.TryGetValue(chunkPosition.GetHashCode(), out chunk);
		if (chunk == null || !chunk.rendered)
			return false;

		chunkPosition.x = _pos.x - Chunk.Size;
		chunkPosition.y = _pos.y;
		chunkPosition.z = _pos.z;

		World.Chunks.TryGetValue(chunkPosition.GetHashCode(), out chunk);
		if (chunk == null || !chunk.rendered)
			return false;

		chunkPosition.x = _pos.x;
		chunkPosition.y = _pos.y;
		chunkPosition.z = _pos.z + Chunk.Size;;

		World.Chunks.TryGetValue(chunkPosition.GetHashCode(), out chunk);
		if (chunk == null || !chunk.rendered)
			return false;

		chunkPosition.x = _pos.x;
		chunkPosition.y = _pos.y;
		chunkPosition.z = _pos.z - Chunk.Size;

		World.Chunks.TryGetValue(chunkPosition.GetHashCode(), out chunk);
		if (chunk == null || !chunk.rendered)
			return false;

		return true;
	}

	public void SetBlocksUnmodified()
	{
		_changes.Clear();
	}
	
	public static bool InRange(int index)
	{
		if (index < 0 || index >= Size)
		{
			return false;
		}
		
		return true;
	}

	MeshData GetMeshData()
	{
		MeshData meshData;
		int lastAvailableIndex = MeshDataPool.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			meshData = MeshDataPool[lastAvailableIndex];
			MeshDataPool.RemoveAt(lastAvailableIndex);
			meshData.Clear();
		}
		else
		{
			meshData = new MeshData();
		}

		return meshData;
	}

	void ReturnMeshData(MeshData meshData)
	{
		MeshDataPool.Add(meshData);
	}

	static uint GetBlockDataIndex(uint x, uint y, uint z)
	{
		return x | y << VOXEL_Y_SHIFT | z << VOXEL_Z_SHIFT;
	}

	public static uint BlockIndex(int x, int y, int z)
	{
		return GetBlockDataIndex((uint)x, (uint)y, (uint)z);
	}

	static UIntVec3 GetBlockDataPosition(uint index)
	{
		uint blockX = index & 0xF;
		uint blockY = (index >> VOXEL_Y_SHIFT) & 0xF;
		uint blockZ = (index >> VOXEL_Z_SHIFT) & 0xF;

		return new UIntVec3(blockX, blockY, blockZ);
	}

	public static WorldPosition BlockPosition(uint index)
	{
		return new WorldPosition((Vector3)GetBlockDataPosition(index));
	}

}
