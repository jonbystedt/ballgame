using UnityEngine;
using System;

[Serializable]
public struct World3
{
	public int x, y, z;

	public World3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public World3(Vector3 gamePosition)
	{
		this.x = Mathf.FloorToInt(gamePosition.x);
		this.y = Mathf.FloorToInt(gamePosition.y);
		this.z = Mathf.FloorToInt(gamePosition.z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x,y,z);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is World3))
		{
			return false;
		}

		return GetHashCode() == obj.GetHashCode();
	}

	public bool Equals(World3 p)
	{
		return GetHashCode() == p.GetHashCode();
	}

	public static bool operator == (World3 a, World3 b)
	{
		return a.GetHashCode() == b.GetHashCode();
	}

	public static bool operator != (World3 a, World3 b)
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