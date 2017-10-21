using System;

public struct Region 
{
	public World3 min;
	public World3 max;
	public int sizeX;
	public int sizeY;
	public int sizeZ;

	public Region(World3 min, World3 max)
	{
		this.min = min;
		this.max = max;
		this.sizeX = max.x - min.x + 1;
		this.sizeY = max.y - min.y + 1;
		this.sizeZ = max.z - min.z + 1;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Region))
		{
			return false;
		}

		return GetHashCode() == obj.GetHashCode();
	}

	public bool Equals(Region r)
	{
		return GetHashCode() == r.GetHashCode();
	}

	public static bool operator == (Region a, Region b)
	{
		return a.GetHashCode() == b.GetHashCode();
	}

	public static bool operator != (Region a, Region b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 47;

			hash = hash * 227 + min.x.GetHashCode();
			hash = hash * 227 + min.y.GetHashCode();
			hash = hash * 227 + min.z.GetHashCode();
			hash = hash * 227 + max.x.GetHashCode();
			hash = hash * 227 + max.y.GetHashCode();
			hash = hash * 227 + max.z.GetHashCode();

			return hash;
		}
	}
}

