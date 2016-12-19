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
		float variance = GameUtils.Variance;
		rainBreakValue = rainBreakValue + Mathf.FloorToInt(variance * variance * 350);
		lightningBreakValue = lightningBreakValue + Mathf.FloorToInt(variance * variance * 250);

		variance = GameUtils.Variance;
		terrain = new NoiseOptions(
			Mathf.Lerp(0.000001f, 0.05f, Mathf.Pow(variance, 1.2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Variance)),//1, 
			Mathf.Lerp(0f, 4f, GameUtils.Variance), 
			Mathf.Lerp(0f, 0.5f, GameUtils.Variance), 
			32,
			Mathf.Pow(GameUtils.Variance - 0.5f, Mathf.FloorToInt(GameUtils.Variance * 8) % 8)
			); 

		variance = GameUtils.Variance;
		mountain = new NoiseOptions(
			Mathf.Lerp(0.00005f, 0.025f, Mathf.Pow(variance, 1.2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Variance)),//4, 
			Mathf.Lerp(0f, 4f, GameUtils.Variance), 
			Mathf.Lerp(0f, 1f, GameUtils.Variance), 
			64,
			Mathf.Pow(GameUtils.Variance - 0.5f, 4 + Mathf.FloorToInt(GameUtils.Variance * 8) % 8)
			); 

		cave = new NoiseOptions(
			Mathf.Lerp(0.0008f, 0.01f, GameUtils.Variance), 
			1,//2, 
			Mathf.Lerp(0f, 4f, GameUtils.Variance), 
			Mathf.Lerp(0f, 1f, GameUtils.Variance), 
			1024,
			Mathf.Pow(GameUtils.Variance - 0.5f, 8 + Mathf.FloorToInt(GameUtils.Variance * 8) % 8)
			);
		if (cave.frequency > 0.005) 
		{
			cave.octaves = 1;//Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Variance));
		}
		else if (cave.frequency <= 0.001)
		{
			cave.octaves = Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Variance));
		}
		else
		{
			cave.octaves = Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Variance));
		}


		variance = GameUtils.Variance;
		pattern = new NoiseOptions(
			Mathf.Lerp(0.000005f, 0.05f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Variance)),//4, 
			Mathf.Lerp(0f, 4f, GameUtils.Variance), 
			Mathf.Lerp(0f, 1f, GameUtils.Variance), 
			1024,
			Mathf.Pow(GameUtils.Variance - 0.5f, 4 + Mathf.FloorToInt(GameUtils.Variance * 8) % 8)
			);

		variance = GameUtils.Variance;
		stripe = new NoiseOptions(
			Mathf.Lerp(0.00000001f, 5f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Variance)),//2, 
			Mathf.Lerp(0f, 4f, GameUtils.Variance),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			1024,
			Mathf.Pow(GameUtils.Variance - 0.5f, Mathf.FloorToInt(GameUtils.Variance * 8) % 8)
			);

		Game.Log(stripe.drift.ToString() + " " + pattern.drift.ToString() + " " + terrain.drift.ToString() + " " + mountain.drift.ToString());

		variance = GameUtils.Variance;
		spawnTypes = new NoiseOptions(
			Mathf.Lerp(0.008f, 0.05f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Variance)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			10000,
			0f
			);

		spawnFrequency = new NoiseOptions(
			Mathf.Lerp(0.04f, 2f, GameUtils.Variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Variance)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			100,
			0f
			);

		variance = GameUtils.Variance;
		spawnIntensity = new NoiseOptions(
			Mathf.Lerp(0.004f, 0.05f, variance * variance), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Variance)) + 2,//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			100,
			0f
			);

		rainIntensity = new NoiseOptions(
			Mathf.Lerp(0.002f, 0.001f, GameUtils.Variance),
			Mathf.FloorToInt(Mathf.Lerp(1,4, GameUtils.Variance)),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			Mathf.Lerp(0f, 2f, GameUtils.Variance),
			1000,
			0f
			);

		lightningIntensity = new NoiseOptions(
			rainIntensity.frequency,
			rainIntensity.octaves,
			rainIntensity.lacunarity + Mathf.Lerp(0.01f, 0.0025f, GameUtils.Variance),
			rainIntensity.persistance + Mathf.Lerp(0.01f, 0.0025f, GameUtils.Variance),
			1000,
			0f
			);

		float noiseValue = GameUtils.Variance;
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

		noiseValue = GameUtils.Variance;
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

		noiseValue = GameUtils.Variance;
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

		noiseValue = GameUtils.Variance;
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

		noiseValue = GameUtils.Variance;
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

		terrain.scale = Mathf.FloorToInt(Mathf.Lerp(1f, 33f, GameUtils.Variance));
	}
}
