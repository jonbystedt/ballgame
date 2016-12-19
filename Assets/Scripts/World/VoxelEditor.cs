using UnityEngine;
using System.Collections;

public static class VoxelEditor 
{
	public static void SetBlock(WorldPosition pos, Block block, bool playerHit)
	{
		Block current = World.GetBlock(pos);

		if (current is BlockAir && !(block is BlockAir))
		{
			World.SetBlock(pos.x, pos.y, pos.z, block, playerHit);
		}
		else if (!(current is BlockAir) && block is BlockAir && pos.y != -48)
		{
			World.SetBlock(pos.x, pos.y, pos.z, block, playerHit);
		}
	}

	public static void SetBlock(WorldPosition pos, Block block)
	{
		SetBlock(pos, block, false);
	}

	public static void SetSphere(WorldPosition pos, Block block, int diameter)
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
						SetBlock(new WorldPosition(pos.x + x, pos.y + y, pos.z + z), block);
					}
				}
			}
		}
	}

	public static void SetBox(Region region, Block block)
	{
		for (int z = region.min.z; z < region.max.z + 1; z = z + region.sizeZ)
		{
			for (int y = region.min.y; y < region.max.y + 1; y = y + region.sizeY)
			{
				for (int x = region.min.x; x < region.max.x + 1; x = x + region.sizeX)
				{
					SetBlock(new WorldPosition(x, y, z), block);
				}
			}
		}
	}
}
