using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct TileColor 
{ 
	public float r; 
	public float g; 
	public float b; 

	public TileColor(int r, int g, int b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TileColor))
		{
			return false;
		}

		return GetHashCode() == obj.GetHashCode();
	}

	public bool Equals(TileColor c)
	{
		return GetHashCode() == c.GetHashCode();
	}

	public static bool operator == (TileColor a, TileColor b)
	{
		return a.GetHashCode() == b.GetHashCode();
	}

	public static bool operator != (TileColor a, TileColor b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 47;

			hash = hash * 227 + r.GetHashCode();
			hash = hash * 227 + g.GetHashCode();
			hash = hash * 227 + b.GetHashCode();

			return Math.Abs(hash);
		}
	}
}

