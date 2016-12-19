using UnityEngine;
using System;

[Serializable]
public struct WorldPosition
{
	public int x, y, z;

	public WorldPosition(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x,y,z);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is WorldPosition))
		{
			return false;
		}

		return GetHashCode() == obj.GetHashCode();
	}

	public bool Equals(WorldPosition p)
	{
		return GetHashCode() == p.GetHashCode();
	}

	public static bool operator == (WorldPosition a, WorldPosition b)
	{
		return a.GetHashCode() == b.GetHashCode();
	}

	public static bool operator != (WorldPosition a, WorldPosition b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 47;
			
			hash = hash * 227 + x.GetHashCode();
			hash = hash * 227 + y.GetHashCode();
			hash = hash * 227 + z.GetHashCode();
			
			return hash;
		}
	}
}