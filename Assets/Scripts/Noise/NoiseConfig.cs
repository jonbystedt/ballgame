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
			Mathf.Lerp(
				Config.Noise.terrain.frequency.low, 
				Config.Noise.terrain.frequency.high, 
				Mathf.Pow(GameUtils.Seed, 1.2f)
				),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.terrain.octaves.low,
				Config.Noise.terrain.octaves.high,
				GameUtils.Seed
				)),
			Mathf.Lerp(
				Config.Noise.terrain.lacunarity.low, 
				Config.Noise.terrain.lacunarity.high, 
				GameUtils.Seed
				), 
			Mathf.Lerp(
				Config.Noise.terrain.persistance.low, 
				Config.Noise.terrain.persistance.high, 
				GameUtils.Seed
				), 
			32,
			Mathf.Pow(GameUtils.Seed - 0.5f, 10 + Mathf.FloorToInt(GameUtils.Seed * 10) % 10)
			); 

		mountain = new NoiseOptions(
			Mathf.Lerp(
				Config.Noise.mountain.frequency.low,
				Config.Noise.mountain.frequency.high, 
				Mathf.Pow(GameUtils.Seed, 2f)
				),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.mountain.octaves.low,
				Config.Noise.mountain.octaves.high,
				GameUtils.Seed
				)),
			Mathf.Lerp(
				Config.Noise.mountain.lacunarity.low, 
				Config.Noise.mountain.lacunarity.high,
				GameUtils.Seed
				), 
			Mathf.Lerp(
				Config.Noise.mountain.persistance.low, 
				Config.Noise.mountain.persistance.high, 
				GameUtils.Seed
				), 
			64 - Mathf.FloorToInt(Mathf.Lerp(0, 16, Mathf.Pow(GameUtils.Seed, 2))),
			Mathf.Pow(GameUtils.Seed - 0.5f, 8 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			); 

		cave = new NoiseOptions(
			Mathf.Lerp(
				Config.Noise.cave.frequency.low, 
				Config.Noise.cave.frequency.high, 
				Mathf.Pow(GameUtils.Seed,2)
				), 
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.cave.octaves.low,
				Config.Noise.cave.octaves.high,
				GameUtils.Seed
				)),
			Mathf.Lerp(
				Config.Noise.cave.lacunarity.low, 
				Config.Noise.cave.lacunarity.high, 
				GameUtils.Seed
				), 
			Mathf.Lerp(
				Config.Noise.cave.persistance.low, 
				Config.Noise.cave.persistance.high, 
				GameUtils.Seed
				), 
			1024,
			Mathf.Pow(GameUtils.Seed - 0.5f, 12 + Mathf.FloorToInt(GameUtils.Seed * 10) % 10)
			);

		pattern = new NoiseOptions(
			Mathf.Lerp(
				Config.Noise.pattern.frequency.low, 
				Config.Noise.pattern.frequency.high, 
				Mathf.Pow(GameUtils.Seed,3)
				), 
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.pattern.octaves.low, 
				Config.Noise.pattern.octaves.high, 
				GameUtils.Seed)
				),
			Mathf.Lerp(
				Config.Noise.pattern.lacunarity.low, 
				Config.Noise.pattern.lacunarity.high, 
				GameUtils.Seed
				), 
			Mathf.Lerp(
				Config.Noise.pattern.persistance.low, 
				Config.Noise.pattern.persistance.high, 
				GameUtils.Seed
				), 
			1024,
			Mathf.Pow((GameUtils.Seed * 0.25f) - 0.125f, 12 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			);

		stripe = new NoiseOptions(
			Mathf.Lerp(
				Config.Noise.stripe.frequency.low, 
				Config.Noise.stripe.frequency.high, 
				GameUtils.Seed
				),  
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.stripe.octaves.low,
				Config.Noise.stripe.octaves.high,
				GameUtils.Seed)
				), 
			Mathf.Lerp(
				Config.Noise.stripe.lacunarity.low, 
				Config.Noise.stripe.lacunarity.high,
				GameUtils.Seed
				),
			Mathf.Lerp(
				Config.Noise.stripe.persistance.low, 
				Config.Noise.stripe.persistance.high, 
				GameUtils.Seed
				),
			1024,
			Mathf.Pow(GameUtils.Seed - 0.5f, 6 + Mathf.FloorToInt(GameUtils.Seed * 8) % 8)
			);

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

		// Noise Methods
		int index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.caveTypes.Length - 0.001f, GameUtils.Seed));
		caveMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.caveTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.patternTypes.Length - 0.001f, GameUtils.Seed));
		patternMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.patternTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.stripeTypes.Length - 0.001f, GameUtils.Seed));
		stripeMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.stripeTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.terrainTypes.Length - 0.001f, GameUtils.Seed));
		terrainType = Config.Noise.terrainTypes[index];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.mountainTypes.Length - 0.001f, GameUtils.Seed));
		mountainType = Config.Noise.mountainTypes[index];

		terrain.scale = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.terrainScale.low, Config.Noise.terrainScale.high, GameUtils.Seed));
	}
}
