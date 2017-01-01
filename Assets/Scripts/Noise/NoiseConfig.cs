using UnityEngine;

public struct NoiseOptions
{
	public float frequency;
	public int octaves;
	public float lacunarity;
	public float persistance;
	public int scale;
	public float drift;

	public NoiseOptions(float f, int o, float l, float p, int s, float d)
	{
		frequency = f;
		octaves = o;
		lacunarity = l;
		persistance = p;
		scale = s;
		drift = d;
	}
}

public static class NoiseConfig {

	public static NoiseOptions terrain;
	public static NoiseOptions mountain;
	public static NoiseOptions cave;
	public static NoiseOptions pattern;
	public static NoiseOptions stripe;
	public static NoiseOptions spawnTypes;
	public static NoiseOptions spawnFrequency;
	public static NoiseOptions spawnIntensity;
	public static NoiseOptions rainIntensity;
	public static NoiseOptions lightningIntensity;

	// Intensity will never be below this number.
	public static int spawnIntensityBase = -20;
	public static int rainBreakValue = 650;
	public static int lightningBreakValue = 750;

	public static NoiseType terrainType;
	public static NoiseType mountainType;
	public static NoiseMethod caveMethod;
	public static NoiseMethod patternMethod;
	public static NoiseMethod stripeMethod;

	public static void Initialize()
	{
		float variance = GameUtils.SeedValue;
		rainBreakValue = rainBreakValue + Mathf.FloorToInt(variance * variance * 350);
		lightningBreakValue = lightningBreakValue + Mathf.FloorToInt(variance * variance * 250);

		variance = GameUtils.SeedValue;
		terrain = new NoiseOptions(
			Mathf.Lerp(0.00001f, 0.025f, Mathf.Pow(variance, 1.2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.SeedValue)),
			Mathf.Lerp(0f, 4f, GameUtils.SeedValue), 
			Mathf.Lerp(0f, 0.5f, GameUtils.SeedValue), 
			32,
			Mathf.Pow(GameUtils.SeedValue - 0.5f, 10 + Mathf.FloorToInt(GameUtils.SeedValue * 10) % 10)
			); 

		variance = GameUtils.SeedValue;
		mountain = new NoiseOptions(
			Mathf.Lerp(0.00005f, 0.025f, Mathf.Pow(variance, 1.2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.SeedValue)),
			Mathf.Lerp(0f, 4f, GameUtils.SeedValue), 
			Mathf.Lerp(0f, 1f, GameUtils.SeedValue), 
			64,
			Mathf.Pow(GameUtils.SeedValue - 0.5f, 8 + Mathf.FloorToInt(GameUtils.SeedValue * 8) % 8)
			); 

		cave = new NoiseOptions(
			Mathf.Lerp(0.008f, 0.02f, Mathf.Pow(GameUtils.SeedValue,2)), 
			1,
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue), 
			Mathf.Lerp(0f, 1f, GameUtils.SeedValue), 
			1024,
			Mathf.Pow(GameUtils.SeedValue - 0.5f, 12 + Mathf.FloorToInt(GameUtils.SeedValue * 10) % 10)
			);

		pattern = new NoiseOptions(
			Mathf.Lerp(0.0008f, 0.04f, Mathf.Pow(GameUtils.SeedValue,3)), 
			1,
			Mathf.Lerp(0f, 4f, GameUtils.SeedValue), 
			Mathf.Lerp(0f, 1f, GameUtils.SeedValue), 
			1024,
			Mathf.Pow(GameUtils.SeedValue - 0.5f, 10 + Mathf.FloorToInt(GameUtils.SeedValue * 8) % 8)
			);

		stripe = new NoiseOptions(
			Mathf.Lerp(0.00000001f, 1f, Mathf.Pow(GameUtils.SeedValue,2)),  
			Mathf.FloorToInt(Mathf.Lerp(1,4,GameUtils.SeedValue)), 
			Mathf.Lerp(0f, 4f, GameUtils.SeedValue),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			1024,
			Mathf.Pow(GameUtils.SeedValue - 0.5f, 6 + Mathf.FloorToInt(GameUtils.SeedValue * 8) % 8)
			);

		Game.Log(stripe.drift.ToString() + " " + pattern.drift.ToString() + " " + terrain.drift.ToString() + " " + mountain.drift.ToString());

		variance = GameUtils.SeedValue;
		spawnTypes = new NoiseOptions(
			Mathf.Lerp(0.008f, 0.05f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.SeedValue)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			10000,
			0f
			);

		spawnFrequency = new NoiseOptions(
			Mathf.Lerp(0.04f, 2f, GameUtils.SeedValue), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.SeedValue)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			100,
			0f
			);

		variance = GameUtils.SeedValue;
		spawnIntensity = new NoiseOptions(
			Mathf.Lerp(0.004f, 0.05f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.SeedValue)) + 2,//1, 
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			100,
			0f
			);

		rainIntensity = new NoiseOptions(
			Mathf.Lerp(0.002f, 0.001f, GameUtils.SeedValue),
			Mathf.FloorToInt(Mathf.Lerp(1,4, GameUtils.SeedValue)),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			Mathf.Lerp(0f, 2f, GameUtils.SeedValue),
			1000,
			0f
			);

		lightningIntensity = new NoiseOptions(
			rainIntensity.frequency,
			rainIntensity.octaves,
			rainIntensity.lacunarity + Mathf.Lerp(0.01f, 0.0025f, GameUtils.SeedValue),
			rainIntensity.persistance + Mathf.Lerp(0.01f, 0.0025f, GameUtils.SeedValue),
			1000,
			0f
			);

		float noiseValue = GameUtils.SeedValue;
		if (noiseValue > 0.75f)
		{
			caveMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Value][2];
		}
		else if (noiseValue > 0.5f)
		{
			caveMethod = NoiseGenerator.noiseMethods[(int)NoiseType.SimplexValue][2];
		}
		else if (noiseValue > 0.25f)
		{
			caveMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Simplex][2];
		}
		else
		{
			caveMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Perlin][2];
		}

		noiseValue = GameUtils.SeedValue;
		if (noiseValue > 0.75f)//0.95f)
		{
			patternMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Value][2];
		}
		else if (noiseValue > 0.5f)//0.9f)
		{
			patternMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Perlin][2];
		}
		else if (noiseValue > 0.25f)//0.7f)
		{
			patternMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Simplex][2];
		}
		else
		{
			patternMethod = NoiseGenerator.noiseMethods[(int)NoiseType.SimplexValue][2];
		}

		noiseValue = GameUtils.SeedValue;
		if (noiseValue > 0.75f)//0.95f)
		{
			stripeMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Value][2];
		}
		else if (noiseValue > 0.5f)//0.9f)
		{
			stripeMethod = NoiseGenerator.noiseMethods[(int)NoiseType.SimplexValue][2];
		}
		else if (noiseValue > 0.25f)//0.7f)
		{
			stripeMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Perlin][2];
		}
		else
		{
			stripeMethod = NoiseGenerator.noiseMethods[(int)NoiseType.Simplex][2];
		}

		noiseValue = GameUtils.SeedValue;
		if (noiseValue > 0.75f)//0.95f)
		{
			mountainType = NoiseType.Value;
		}
		else if (noiseValue > 0.5f)//0.9f)
		{
			mountainType = NoiseType.SimplexValue;
		}
		else if (noiseValue > 0.25f)//0.7f)
		{
			mountainType = NoiseType.Perlin;
		}
		else
		{
			mountainType = NoiseType.Simplex;
		}

		noiseValue = GameUtils.SeedValue;
		if (noiseValue > 0.75f)//0.95f)
		{
			terrainType = NoiseType.Perlin;
		}
		else if (noiseValue > 0.5f)//0.9f)
		{
			terrainType = NoiseType.Value;
		}
		else if (noiseValue > 0.25f)//0.7f)
		{
			terrainType = NoiseType.Simplex;
		}
		else
		{
			terrainType = NoiseType.SimplexValue;
		}

		terrain.scale = Mathf.FloorToInt(Mathf.Lerp(1f, 33f, GameUtils.SeedValue));
	}
}
