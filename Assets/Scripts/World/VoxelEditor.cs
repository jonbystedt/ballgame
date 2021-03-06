﻿using UnityEngine;
using System.Collections;

public static class VoxelEditor 
{
	public static void SetBlock(World3 pos, ushort block, bool playerHit)
	{
		ushort current = World.GetBlock(pos);

		if (current == Block.Air && block != Block.Air)
		{
			World.SetBlock(pos.x, pos.y, pos.z, block, playerHit);
		}
		else if (current != Block.Air && block == Block.Air && pos.y != -48)
		{
			World.SetBlock(pos.x, pos.y, pos.z, block, playerHit);
		}
	}

	public static void SetBlock(World3 pos, ushort block)
	{
		SetBlock(pos, block, true);
	}

	public static void SetSphere(World3 pos, ushort block, int diameter)
	{
		int r = Mathf.FloorToInt(diameter / 2f);

		for (int z = -r; z < r + 1; z++)
		{
			for (int y = -r; y < r + 1; y++)
			{
				for (int x = -r; x < r + 1; x++)
				{
					if (Mathf.FloorToInt(Mathf.Sqrt(x * x + y * y + z * z)) == r)
					{
						SetBlock(new World3(pos.x + x, pos.y + y, pos.z + z), block);
					}
				}
			}
		}
	}

	public static void SetBox(Region region, ushort block)
	{
		for (int z = region.min.z; z < region.max.z + 1; z = z + region.sizeZ)
		{
			for (int y = region.min.y; y < region.max.y + 1; y = y + region.sizeY)
			{
				for (int x = region.min.x; x < region.max.x + 1; x = x + region.sizeX)
				{
					SetBlock(new World3(x, y, z), block);
				}
			}
		}
	}
}
