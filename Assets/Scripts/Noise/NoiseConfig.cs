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

	public static NoiseOptions boidInteraction;
	public static NoiseOptions boidSeparation;
	public static NoiseOptions boidDistance;
	public static NoiseOptions boidAlignment;
	public static NoiseOptions boidInner;
	public static NoiseOptions boidOuter;

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
		float seed = GameUtils.Seed;
		rainBreakValue = rainBreakValue + Mathf.FloorToInt(seed * seed * 350);
		lightningBreakValue = lightningBreakValue + Mathf.FloorToInt(seed * seed * 250);

		terrain = new NoiseOptions(
			Mathf.Lerp(0.00005f, 0.025f, Mathf.Pow(GameUtils.Seed, 1.2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Seed)),
			Mathf.Lerp(0f, 4f, GameUtils.Seed), 
			Mathf.Lerp(0f, 0.5f, GameUtils.Seed), 
			32,
			Mathf.Pow(GameUtils.Seed - 0.5f, 10 + Mathf.FloorToInt(GameUtils.Seed * 10) % 10)
			); 

		mountain = new NoiseOptions(
			Mathf.Lerp(0.0005f, 0.025f, Mathf.Pow(GameUtils.Seed, 2f)),
			Mathf.FloorToInt(Mathf.Lerp(1,3,GameUtils.Seed)),
			Mathf.Lerp(0f, 4f, GameUtils.Seed), 
			Mathf.Lerp(0f, 1f, GameUtils.Seed), 
			64 - Mathf.FloorToInt(Mathf.Lerp(0, 16, Mathf.Pow(GameUtils.Seed, 2))),
			Mathf.Pow(GameUtils.Seed - 0.5f, 8 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			); 

		cave = new NoiseOptions(
			Mathf.Lerp(0.008f, 0.02f, Mathf.Pow(GameUtils.Seed,2)), 
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed), 
			Mathf.Lerp(0f, 1f, GameUtils.Seed), 
			1024,
			Mathf.Pow(GameUtils.Seed - 0.5f, 12 + Mathf.FloorToInt(GameUtils.Seed * 10) % 10)
			);

		pattern = new NoiseOptions(
			Mathf.Lerp(0.01f, 0.04f, Mathf.Pow(GameUtils.Seed,3)), 
			Mathf.FloorToInt(Mathf.Lerp(1, 4, GameUtils.Seed)),
			Mathf.Lerp(0f, 4f, GameUtils.Seed), 
			Mathf.Lerp(0f, 1f, GameUtils.Seed), 
			1024,
			Mathf.Pow((GameUtils.Seed * 0.25f) - 0.125f, 12 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			);
		
		//Game.Log(pattern.frequency.ToString() + " : " + pattern.octaves.ToString() + " : " + pattern.lacunarity.ToString() + " : " + pattern.persistance.ToString());
		//Game.Log(cave.frequency.ToString() + " : " + cave.octaves.ToString() + " : " + cave.lacunarity.ToString() + " : " + cave.persistance.ToString());

		stripe = new NoiseOptions(
			Mathf.Lerp(0.00001f, 0.1f, GameUtils.Seed),  
			Mathf.FloorToInt(Mathf.Lerp(1,4,GameUtils.Seed)), 
			Mathf.Lerp(0f, 4f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			1024,
			Mathf.Pow(GameUtils.Seed - 0.5f, 6 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			);

		//Game.Log(stripe.drift.ToString() + " " + pattern.drift.ToString() + " " + terrain.drift.ToString() + " " + mountain.drift.ToString());

		seed = GameUtils.Seed;
		spawnTypes = new NoiseOptions(
			Mathf.Lerp(0.008f, 0.05f, seed * seed), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Seed)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			10000,
			0f
			);

		spawnFrequency = new NoiseOptions(
			Mathf.Lerp(0.04f, 2f, GameUtils.Seed), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Seed)),//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			100,
			0f
			);

		seed = GameUtils.Seed;
		spawnIntensity = new NoiseOptions(
			Mathf.Lerp(0.004f, 0.05f, seed * seed), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,GameUtils.Seed)) + 2,//1, 
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			100,
			0f
			);

		rainIntensity = new NoiseOptions(
			Mathf.Lerp(0.002f, 0.001f, GameUtils.Seed),
			Mathf.FloorToInt(Mathf.Lerp(1,4, GameUtils.Seed)),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			1000,
			0f
			);

		lightningIntensity = new NoiseOptions(
			rainIntensity.frequency,
			rainIntensity.octaves,
			rainIntensity.lacunarity + Mathf.Lerp(0.01f, 0.0025f, GameUtils.Seed),
			rainIntensity.persistance + Mathf.Lerp(0.01f, 0.0025f, GameUtils.Seed),
			1000,
			0f
			);
		
		boidInteraction = new NoiseOptions(
			Mathf.Lerp(0.00002f, 0.001f, GameUtils.Seed),
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			20,
			0f
			);
		
		boidDistance = new NoiseOptions(
			Mathf.Lerp(0.00002f, 0.001f, GameUtils.Seed),
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			20,
			0f
			);
		
		boidAlignment = new NoiseOptions(
			Mathf.Lerp(0.00002f, 0.001f, GameUtils.Seed),
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			20,
			0f
			);

		boidInner = new NoiseOptions(
			Mathf.Lerp(0.0002f, 0.001f, GameUtils.Seed),
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			30,
			0f
			);

		boidOuter = new NoiseOptions(
			Mathf.Lerp(0.0002f, 0.001f, GameUtils.Seed),
			1,
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			Mathf.Lerp(0f, 2f, GameUtils.Seed),
			30,
			0f
			);

		float noiseValue = GameUtils.Seed;
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

		noiseValue = GameUtils.Seed;
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

		noiseValue = GameUtils.Seed;
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

		noiseValue = GameUtils.Seed;
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

		noiseValue = GameUtils.Seed;
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

		terrain.scale = Mathf.FloorToInt(Mathf.Lerp(1f, 33f, GameUtils.Seed));
	}
}
