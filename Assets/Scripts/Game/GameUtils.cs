using UnityEngine;
using System.Collections;
using System;
using System.Text;
using Random = UnityEngine.Random;

public static class GameUtils 
{
	public const int SEED_TABLE_SIZE = 1024;
	public static float[] SeedTable = new float[SEED_TABLE_SIZE];

	static int _seedCount = 0;
	public static float Seed 
	{
		get
		{
			if (_seedCount == SEED_TABLE_SIZE)
			{
				_seedCount = 0;
			}
			return SeedTable[_seedCount++];
		}
		set
		{
			_seedCount = Mathf.FloorToInt(value);
		}
	}

	public static void SetHash()
	{
		int n = 256;
		int[] hashTable = new int[n];

		for (int i = 0; i < n; i++)
		{
			hashTable[i] = i;
		}

		while (n > 1)
		{
			n--;
			int k = Random.Range (0, n + 1);
			int value = hashTable[k];
			hashTable[k] = hashTable[n];
			hashTable[n] = value;
		}

		hashTable.CopyTo(Hash, 0);
		hashTable.CopyTo(Hash, hashTable.Length);
	}

	public static float GetImpactForce(Rigidbody r1, Rigidbody r2)
	{
		float impactVelocityX = r1.velocity.x - r2.velocity.x;
		impactVelocityX *= Mathf.Sign(impactVelocityX);

		float impactVelocityY = r1.velocity.y - r2.velocity.y;
		impactVelocityY *= Mathf.Sign(impactVelocityY);

		float impactVelocityZ = r1.velocity.z - r2.velocity.z;
		impactVelocityZ *= Mathf.Sign(impactVelocityZ);

		float impactVelocity = impactVelocityX + impactVelocityY + impactVelocityZ;
		float impactForce = impactVelocity * r1.mass * r2.mass;
		impactForce *= Mathf.Sign(impactForce);
		impactForce /= 1000f; // generally this will be below 1. mostly it will be closer to 0.

		return impactForce;
	}

	public static void CreateVarianceTable()
	{
		for (int i = 0; i < SEED_TABLE_SIZE; i++)
		{
			SeedTable[i] = Random.value;
		}
	}

	public static string GenerateSeed(int length) 
	{
		string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
		StringBuilder result = new StringBuilder(length);
		for (int i = 0; i < length; i++) 
		{
			result.Append(characters[Random.Range(0, characters.Length)]);
		}
		return result.ToString();
	}

	public static float BiLerp(float v00, float v01, float v10, float v11, float tx, float ty)
	{
		return Mathf.Lerp(
			Mathf.Lerp (v00, v01, tx), 
			Mathf.Lerp (v10, v11, tx), ty);
	}

	public static float TriLerp(
		float v000, float v100, float v010, float v110, float v001, float v101, float v011, float v111, float tx, float ty, float tz)
	{
		return Mathf.Lerp (
			BiLerp(v000, v100, v010, v110, tx, ty), 
			BiLerp(v001, v101, v011, v111, tx, ty), tz);
	}

	public static float Smooth (float t) 
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}
	
	public static float SmoothDerivative (float t) {
		return 30f * t * t * (t * (t - 2f) + 1f);
	}
	
	public static float Dot (Vector2 g, float x, float y) {
		return g.x * x + g.y * y;
	}
	
	public static float Dot (Vector3 g, float x, float y, float z) {
		return g.x * x + g.y * y + g.z * z;
	}

	public static int[] Hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};
}
