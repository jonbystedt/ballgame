using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class GreedyMesher : MonoBehaviour 
{
	Dictionary<int,ushort> BlockLookup = new Dictionary<int,ushort>();
	List<int[,]> MaskPool = new List<int[,]>();
	List<int[]> DirectionsPool = new List<int[]>();

	public void Create(MeshData meshData, ushort[] blocks, WorldPosition pos, bool transparent, bool surrounded, bool fastMesh)
	{
		StartCoroutine(CreateMeshData(meshData, blocks, pos, transparent, surrounded, fastMesh));
	}

	IEnumerator CreateMeshData(MeshData meshData, ushort[] blocks, WorldPosition pos,  bool transparent, bool surrounded, bool fastMesh)
	{
		// Experimental
		fastMesh = true;

		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		int[,] mask = GetMask();
		int[] x = GetDirections();
		int[] q = GetDirections();

		// Sweep over 3 axes
		for (int d = 0; d < 3; d++)
		{
			// u and v are orthogonal directions to d
			int u = (d + 1) % 3; 
			int v = (d + 2) % 3;

			q[d] = 1;

			for (x[d] = -1; x[d] < Chunk.Size; )
			{
				// Compute mask
				for (x[v] = 0; x[v] < Chunk.Size; x[v]++)
				{
					for (x[u] = 0; x[u] < Chunk.Size; x[u]++)
					{
						ushort front_block = Block.Null;
						ushort back_block = Block.Null;

						if (surrounded || transparent)
						{
							// Edge cases. Grab a block from the world to check visibility
							if (x[d] == -1)
							{
								ushort block = Block.Null;
								block = World.GetBlock(new WorldPosition(pos.x + x[0], pos.y + x[1], pos.z + x[2]));
								if (block != Block.Null)
								{
									Block.Type type = Blocks.GetType(block);
									if ((!transparent && type == Block.Type.rock) || (transparent && type == Block.Type.glass))
									{
										front_block = block;
									}
								}
							}

							if (x[d] == Chunk.Size - 1)
							{
								ushort block = Block.Null;
								block = World.GetBlock(new WorldPosition(pos.x + x[0] + q[0], pos.y + x[1] + q[1], pos.z + x[2] + q[2]));
								if (block != Block.Null)
								{
									Block.Type type = Blocks.GetType(block);
									if ((!transparent && type == Block.Type.rock) || (transparent && type == Block.Type.glass))
									{
										back_block = block;
									}
								}
							}
						}

						// Check visibility within chunk
						if (!transparent)
						{
							if (0 <= x[d] 
								&& blocks[Chunk.BlockIndex(x[0], x[1], x[2])] != Block.Null 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0], x[1], x[2])]) == Block.Type.rock)
							{
								front_block = blocks[Chunk.BlockIndex(x[0], x[1], x[2])];
							}
							if (x[d] < Chunk.Size - 1 
								&& blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])] != Block.Null 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])]) == Block.Type.rock)
							{
								back_block = blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])];
							}
						}
						else
						{
							if (0 <= x[d] 
								&& blocks[Chunk.BlockIndex(x[0], x[1], x[2])] != Block.Null 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0], x[1], x[2])]) == Block.Type.glass)
							{
								front_block = blocks[Chunk.BlockIndex(x[0], x[1], x[2])];
							}
							if (x[d] < Chunk.Size - 1 
								&& blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])] != Block.Null 
								&& Blocks.GetType(blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])]) == Block.Type.glass)
							{
								back_block = blocks[Chunk.BlockIndex(x[0] + q[0], x[1] + q[1], x[2] + q[2])];
							}
						}

						int front_tile_code = Blocks.GetTileCode(front_block);
						if (front_block != Block.Null && !BlockLookup.ContainsKey(front_tile_code))
						{
							BlockLookup.Add(front_tile_code, front_block);
						}

						int back_tile_code = Blocks.GetTileCode(back_block);
						if (back_block != Block.Null && !BlockLookup.ContainsKey(back_tile_code))
						{
							BlockLookup.Add(back_tile_code, back_block);
						}

						// Check this code for errors!
						if ((front_block == Block.Null && back_block == Block.Null) || (front_block != Block.Null && back_block != Block.Null) )
						{
							mask[x[u], x[v]] = 0;
						}
						else if (front_block != Block.Null)
						{
							// Don't include meshes from blocks outside of this chunk
							if (x[d] >= 0)
							{
								mask[x[u], x[v]] = front_tile_code;
							}
							else
							{
								mask[x[u], x[v]] = 0;
							}
						}
						else
						{
							if (x[d] < Chunk.Size - 1)
							{
								mask[x[u], x[v]] = -back_tile_code;
							}
							else
							{
								mask[x[u], x[v]] = 0;
							}
						}
					}

					if (!fastMesh && stopwatch.ElapsedTicks > Config.CoroutineTiming / 2f)
					{
						yield return null;

						stopwatch.Reset();
						stopwatch.Start();
					}
				}

				// Increment x[d]
				x[d]++;

				// Generate mesh for mask using lexicographic ordering
				for (int j = 0; j < Chunk.Size; j++)
				{
					for (int i = 0; i < Chunk.Size; )
					{
						int c = mask[i, j];

						if (c != 0)
						{
							// Compute width
							int w = 1;
							for ( ; i + w < Chunk.Size && c == mask[i + w, j]; w++) {}

							// Compute height
							bool done = false;
							int h = 1;
							for ( ; j + h < Chunk.Size; h++)
							{
								for (int k = 0; k < w; k++)
								{
									if (c != mask[i + k, j + h])
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

							int[] du = new int[3];
							int[] dv = new int[3];

							Block.Direction dir;

							if (c > 0)
							{
								dv[v] = h;
								du[u] = w;
								dir = Block.Direction.up;
							}
							else
							{
								c = -c;
								du[v] = h;
								dv[u] = w;
								dir = Block.Direction.down;
							}

							meshData.AddVertex(new Vector3(x[0], x[1], x[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]));
							meshData.AddVertex(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]));
							meshData.AddVertex(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]));

							meshData.AddQuadTriangles();

							meshData.uv.AddRange(Blocks.GetFaceUVs(BlockLookup[c],dir, w, h)); 

							// Clear mask
							for (int l = 0; l < h; l++)
							{
								for (int k = 0; k < w; k++)
								{
									mask[i + k, j + l] = 0;
								}
							}

							// Increment and continue
							i += w;
						}
						else
						{
							i++;
						}
					}

					if (!fastMesh && stopwatch.ElapsedTicks > Config.CoroutineTiming / 2f)
					{
						yield return null;

						stopwatch.Reset();
						stopwatch.Start();
					}
				}
			}
				
			x = GetDirections();
			q = GetDirections();
		}

		ReturnDirections(x);
		ReturnDirections(q);
		ReturnMask(mask);

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

	int[] GetDirections()
	{
		int[] directions;
		int lastAvailableIndex = DirectionsPool.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			directions = DirectionsPool[lastAvailableIndex];
			DirectionsPool.RemoveAt(lastAvailableIndex);
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

	void ReturnDirections(int[] directions)
	{
		DirectionsPool.Add(directions);
	}
}
