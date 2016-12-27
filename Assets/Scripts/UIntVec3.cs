using UnityEngine;

public struct UIntVec3
{
	public uint x;
	public uint y;
	public uint z;

	public UIntVec3(uint x, uint y, uint z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static explicit operator Vector3(UIntVec3 vec)
	{
		return new Vector3(vec.x, vec.y, vec.z);
	}
}


