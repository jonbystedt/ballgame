using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CielaSpike;

public class GreedyMesher : MonoBehaviour 
{
	List<int[,]> MaskPool = new List<int[,]>();
	List<int[]> Int3Pool = new List<int[]>();

	public void Create(MeshData meshData, ushort[] blocks, WorldPosition pos, bool transparent)
	{
		if (Config.Multithreaded)
		{
			this.StartCoroutineAsync(CreateMeshData(meshData, blocks, pos, transparent));
		}
		else
		{
			StartCoroutine(CreateMeshData(meshData, blocks, pos, transparent));
		}
	}

	public void CreateCollider(MeshData meshData, ushort[] blocks, WorldPosition pos)
	{
		if (Config.Multithreaded)
		{
			this.StartCoroutineAsync(CreateCollisionMeshData(meshData, blocks, pos));
		}
		else
		{
			StartCoroutine(CreateCollisionMeshData(meshData, blocks, pos));
		}
		
	}

	IEnumerator CreateMeshData(MeshData meshData, ushort[] blocks, WorldPosition pos,  bool transparent)
	{
		// int[,] mask = GetMask();
		// int[] x = GetInt3();
		// int[] q = GetInt3();
		// int[] du = GetInt3();
		// int[] dv = GetInt3();
		int[,] mask = new int[Chunk.Size,Chunk.Size];
		int[] x = new int[3];
		int[] q = new int[3];
		int[] du = new int[3];
		int[] dv = new int[3];

		// Sweep over 3 axes, 0..2
		for (int axis = 0; axis < 3; axis++)
		{
			// u and v are orthogonal directions to the main axis
			int u = (axis + 1) % 3; 
			int v = (axis + 2) % 3;

			q[axis] = 1;

			// Include each side to compute outer visibility
			for (x[axis] = -1; x[axis] < Chunk.Size; )
			{
				// Compute mask for this face
				for (x[v] = 0; x[v] < Chunk.Size; x[v]++)
				{
					for (x[u] = 0; x[u] < Chunk.Size; x[u]++)
					{
						ushort front_block = Block.Null;
						ushort back_block = Block.Null;

						// Edge cases. Grab a block from the world to check visibility
						if (x[axis] == -1)
						{
							ushort block = Block.Null;
							block = World.GetBlock(new WorldPosition(pos.x + x[0], pos.y + x[1], pos.z + x[2]));

							Block.Type type = Blocks.GetType(block);
							if ((!transparent && type == Block.Type.rock) || (transparent && type == Block.Type.glass))
							{
								front_block = block;
							}
						}

						if (x[axis] == Chunk.Size - 1)
						{
							ushort block = Block.Null;
							block = World.GetBlock(new WorldPosition(pos.x + x[0] + q[0], pos.y + x[1] + q[1], pos.z + x[2] + q[2]));

							Block.Type type = Blocks.GetType(block);
							if ((!transparent && type == Block.Type.rock) || (transparent && type == Block.Type.glass))
							{
								back_block = block;
							}
						}

						// Check visibility within chunk
						if (0 <= x[axis]
							&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0], x[1], x[2])]) == Block.Type.rock)
						{
							front_block = blocks[Chunk.BlockIndex(x[0], x[1], x[2])];
						}
						if (x[axis] < Chunk.Size - 1 
							&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])]) == Block.Type.rock)
						{
							back_block = blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])];
						}
	
						bool maskAssigned = false;
						if (transparent)
						{
							if (0 <= x[axis] 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0], x[1], x[2])]) == Block.Type.glass)
							{
								front_block = blocks[Chunk.BlockIndex(x[0], x[1], x[2])];
							}
							if (x[axis] < Chunk.Size - 1 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])]) == Block.Type.glass)
							{
								back_block = blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])];
							}

							Block.Type frontType = Blocks.GetType(front_block);
							Block.Type backType = Blocks.GetType(back_block);

							// if this is transparent and one block is rock and one is glass, this cannot be seen.
							if (frontType == Block.Type.glass && backType == Block.Type.rock || frontType == Block.Type.rock && backType == Block.Type.glass)
							{
								mask[x[u], x[v]] = 0;
								maskAssigned = true;
							}

							if (frontType == Block.Type.rock)
							{
								front_block = Block.Null;
							}

							if (backType == Block.Type.rock)
							{
								back_block = Block.Null;
							}
						}

						if (!maskAssigned)
						{
							// if both blocks are something, or both or nothing assign 0 to the mask. this cannot be seen.
							if ((front_block == Block.Null && back_block == Block.Null) || (front_block != Block.Null && back_block != Block.Null) )
							{
								mask[x[u], x[v]] = 0;
							}

							// the front block only is nothing
							else if (front_block != Block.Null)
							{
								// don't include the frontside mesh if x[axis] = -1 as this lies outside the chunk
								if (x[axis] >= 0)
								{
									mask[x[u], x[v]] = (int)(front_block + 1);
								}
								else
								{
									mask[x[u], x[v]] = 0;
								}
							}
							else
							{
								// don't include the backside mesh if x[axis] = Chunk.Size - 1 as this lies outside the chunk
								if (x[axis] < Chunk.Size - 1)
								{
									// The sign indicates the side the mesh is on
									mask[x[u], x[v]] = -(int)(back_block + 1);
								}
								else
								{
									mask[x[u], x[v]] = 0;
								}
							}
						}
					}
				}

				// Increment x[axis]
				x[axis]++;

				// Generate mesh for mask using lexicographic ordering
				for (int j = 0; j < Chunk.Size; j++)
				{
					for (int i = 0; i < Chunk.Size; )
					{
						// this is the block code, signed according to what side the mesh is on
						int block = mask[i, j];

						if (block != 0)
						{
							// compute width. expand as long as the same block code is encountered in the mask
							int width = 1;
							for ( ; i + width < Chunk.Size && block == mask[i + width, j]; width++) {}

							// compute height. expand as long as the total height and width have the same block code
							bool done = false;
							int height = 1;
							for ( ; j + height < Chunk.Size; height++)
							{
								for (int k = 0; k < width; k++)
								{
									if (block != mask[i + k, j + height])
									{
										done = true;
										break;
									}
								}
								if (done)
								{
									break;
								}
							}

							// Add quad
							x[u] = i;
							x[v] = j;

							for (int ix = 0; ix <= 2; ix++)
							{
								dv[ix] = 0;
								du[ix] = 0;
							}

							Block.Direction dir;

							if (block > 0)
							{
								dv[v] = height;
								du[u] = width;
								dir = Block.Direction.up;
							}
							else
							{
								block = -block;
								du[v] = height;
								dv[u] = width;
								dir = Block.Direction.down;
							}

							meshData.AddVertex(new Vector3(x[0], x[1], x[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]));
							meshData.AddVertex(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]));

							meshData.AddQuadTriangles();

							meshData.uv.AddRange(Blocks.GetFaceUVs((ushort)block, dir, width, height)); 

							// Clear this portion of the mask
							for (int l = 0; l < height; l++)
							{
								for (int k = 0; k < width; k++)
								{
									mask[i + k, j + l] = 0;
								}
							}

							// Increment and continue
							i += width;
						}
						else
						{
							i++;
						}
					}
				}
			}

			for (int ix = 0; ix <= 2; ix++)
			{
				x[ix] = 0;
				q[ix] = 0;
			}	
		}

		// ReturnInt3(x);
		// ReturnInt3(q);
		// ReturnInt3(du);
		// ReturnInt3(dv);
		// ReturnMask(mask);

		meshData.complete = true;
		yield return null;
	}

	// This is basically the same as above, but doesn't track block types to create an optimized collision mesh
	IEnumerator CreateCollisionMeshData(MeshData meshData, ushort[] blocks, WorldPosition pos)
	{
		// int[,] mask = GetMask();
		// int[] x = GetInt3();
		// int[] q = GetInt3();
		// int[] du = GetInt3();
		// int[] dv = GetInt3();
		int[,] mask = new int[Chunk.Size,Chunk.Size];
		int[] x = new int[3];
		int[] q = new int[3];
		int[] du = new int[3];
		int[] dv = new int[3];

		// Sweep over 3 axes, 0..2
		for (int axis = 0; axis < 3; axis++)
		{
			// u and v are orthogonal directions to the main axis
			int u = (axis + 1) % 3; 
			int v = (axis + 2) % 3;

			q[axis] = 1;

			// Include each side to compute outer visibility
			for (x[axis] = -1; x[axis] < Chunk.Size; )
			{
				// Compute mask for this face
				for (x[v] = 0; x[v] < Chunk.Size; x[v]++)
				{
					for (x[u] = 0; x[u] < Chunk.Size; x[u]++)
					{
						ushort front_block = Block.Null;
						ushort back_block = Block.Null;
						ushort block = Block.Null;

						// Edge cases. Grab a block from the world to check visibility
						if (x[axis] == -1)
						{		
							block = World.GetBlock(new WorldPosition(pos.x + x[0], pos.y + x[1], pos.z + x[2]));

							if (block != Block.Null && block != Block.Air)
							{
								front_block = block;
							}
						}

						if (x[axis] == Chunk.Size - 1)
						{
							block = World.GetBlock(new WorldPosition(pos.x + x[0] + q[0], pos.y + x[1] + q[1], pos.z + x[2] + q[2]));

							if (block != Block.Null && block != Block.Air)
							{
								back_block = block;
							}
						}

						// Check visibility within chunk
						if (0 <= x[axis])
						{
							block = blocks[Chunk.BlockIndex(x[0], x[1], x[2])];
							if (block != Block.Air && block != Block.Null)
							{
								front_block = block;
							}
						}
						if (x[axis] < Chunk.Size - 1)
						{
							block = blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])];
							if (block != Block.Air && block != Block.Null)
							{
								back_block = block;
							}
						} 

						// if both blocks are something, or both or nothing assign 0 to the mask. this cannot be seen.
						if ((front_block == Block.Null && back_block == Block.Null) || (front_block != Block.Null && back_block != Block.Null) )
						{
							mask[x[u], x[v]] = 0;
						}
						// the front block only is nothing
						else if (front_block != Block.Null)
						{
							// We don't include the frontside mesh if x[axis] = -1 as this lies outside the chunk
							if (x[axis] >= 0)
							{
								mask[x[u], x[v]] = 1;
							}
							else
							{
								mask[x[u], x[v]] = 0;
							}
						}
						else
						{
							// We don't include the backside mesh if x[axis] = Chunk.Size - 1 as this lies outside the chunk
							if (x[axis] < Chunk.Size - 1)
							{
								// The sign indicates the side the mesh is on
								mask[x[u], x[v]] = -1;
							}
							else
							{
								mask[x[u], x[v]] = 0;
							}
						}
					}
				}

				// Increment x[axis]
				x[axis]++;

				// Generate mesh for mask using lexicographic ordering
				for (int j = 0; j < Chunk.Size; j++)
				{
					for (int i = 0; i < Chunk.Size; )
					{
						// this is the block code, signed according to what side the mesh is on
						int block = mask[i, j];

						if (block != 0)
						{
							// compute width. expand as long as the same block code is encountered in the mask
							int width = 1;
							for ( ; i + width < Chunk.Size && block == mask[i + width, j]; width++) {}

							// compute height. expand as long as the total height and width have the same block code
							bool done = false;
							int height = 1;
							for ( ; j + height < Chunk.Size; height++)
							{
								for (int k = 0; k < width; k++)
								{
									if (block != mask[i + k, j + height])
									{
										done = true;
										break;
									}
								}
								if (done)
								{
									break;
								}
							}

							// Add quad
							x[u] = i;
							x[v] = j;

							for (int ix = 0; ix <= 2; ix++)
							{
								dv[ix] = 0;
								du[ix] = 0;
							}

							if (block > 0)
							{
								dv[v] = height;
								du[u] = width;
							}
							else
							{
								du[v] = height;
								dv[u] = width;
							}

							meshData.AddVertex(new Vector3(x[0], x[1], x[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]));
							meshData.AddVertex(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]));

							meshData.AddQuadTriangles();

							// Clear this portion of the mask
							for (int l = 0; l < height; l++)
							{
								for (int k = 0; k < width; k++)
								{
									mask[i + k, j + l] = 0;
								}
							}

							// Increment and continue
							i += width;
						}
						else
						{
							i++;
						}
					}
				}
			}

			for (int ix = 0; ix <= 2; ix++)
			{
				x[ix] = 0;
				q[ix] = 0;
			}	
		}

		// ReturnInt3(x);
		// ReturnInt3(q);
		// ReturnInt3(du);
		// ReturnInt3(dv);
		// ReturnMask(mask);

		meshData.complete = true;
		yield return null;
	}

	int[,] GetMask()
	{
		int[,] mask;
		int lastAvailableIndex = MaskPool.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			mask = MaskPool[lastAvailableIndex];
			MaskPool.RemoveAt(lastAvailableIndex);
			for (int i = 0; i < Chunk.Size; i++)
			{
				for (int j = 0; j < Chunk.Size; j++)
				{
					mask[i,j] = 0;
				}
			}
		}
		else
		{
			mask = new int[Chunk.Size,Chunk.Size];
		}

		return mask;
	}

	void ReturnMask(int[,] mask)
	{
		MaskPool.Add(mask);
	}

	int[] GetInt3()
	{
		int[] directions;
		int lastAvailableIndex = Int3Pool.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			directions = Int3Pool[lastAvailableIndex];
			Int3Pool.RemoveAt(lastAvailableIndex);
			for (int i = 0; i < 3; i++)
			{
				directions[i] = 0;
			}
		}
		else
		{
			directions = new int[3];
		}

		return directions;
	}

	void ReturnInt3(int[] directions)
	{
		Int3Pool.Add(directions);
	}
}
