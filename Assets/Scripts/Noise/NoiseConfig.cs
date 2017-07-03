using UnityEngine;

public struct ValueRange
{
	public float max;
	public float min;
	public float value;

	public ValueRange(float n, float x, float pow)
	{
		min = n;
		max = x;
		value = Mathf.Lerp(n, x, Mathf.Pow(GameUtils.Seed, pow));
	}

	public ValueRange(float n, float x)
	{
		min = n;
		max = x;
		value = Mathf.Lerp(n, x, GameUtils.Seed);
	}
}

public struct NoiseOptions
{
	public ValueRange frequency;
	public int octaves;
	public float lacunarity;
	public float persistance;
	public int scale;
	public float drift;

	public NoiseOptions(ValueRange f, int o, float l, float p, int s, float d)
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
	public static NoiseOptions worldKey;
	public static NoiseOptions driftMap;

	// Intensity will never be below this number.
	public static int spawnIntensityBase = -20;
	public static int rainBreakValue = 650;
	public static int lightningBreakValue = 750;

	public static NoiseType terrainType;
	public static NoiseType mountainType;
	public static NoiseMethod caveMethod;
	public static NoiseMethod patternMethod;
	public static NoiseMethod stripeMethod;
	public static NoiseMethod driftMapMethod;

	public static float Seed { get { return GameUtils.Seed; }}

	public static void Initialize()
	{
		float s = Seed;
		rainBreakValue = rainBreakValue + Mathf.FloorToInt(s * s * 350);
		lightningBreakValue = lightningBreakValue + Mathf.FloorToInt(s * s * 250);

		terrain = new NoiseOptions(
			new ValueRange(
				Config.Noise.terrain.frequency.low, 
				Config.Noise.terrain.frequency.high,
				1.2f
				),
			GameUtils.IntLerp(
				Config.Noise.terrain.octaves.low,
				Config.Noise.terrain.octaves.high,
				Seed
				),
			Mathf.Lerp(
				Config.Noise.terrain.lacunarity.low, 
				Config.Noise.terrain.lacunarity.high, 
				Seed
				), 
			Mathf.Lerp(
				Config.Noise.terrain.persistance.low, 
				Config.Noise.terrain.persistance.high, 
				Seed
				), 
			32,
			Config.Noise.terrain.drift 
				? Mathf.Pow(Seed, 2f)
				: 0f
			); 

		mountain = new NoiseOptions(
			new ValueRange(
				Config.Noise.mountain.frequency.low, 
				Config.Noise.mountain.frequency.high,
				2f
				),
			GameUtils.IntLerp(
				Config.Noise.mountain.octaves.low,
				Config.Noise.mountain.octaves.high,
				Seed
				),
			Mathf.Lerp(
				Config.Noise.mountain.lacunarity.low, 
				Config.Noise.mountain.lacunarity.high,
				Seed
				), 
			Mathf.Lerp(
				Config.Noise.mountain.persistance.low, 
				Config.Noise.mountain.persistance.high, 
				Seed
				), 
			64 - GameUtils.IntLerp(
				0, 
				16, 
				Mathf.Pow(Seed, 2)
				),
			Config.Noise.mountain.drift 
				? Mathf.Pow(Seed, 2) 
				: 0f
			); 

		cave = new NoiseOptions(
			new ValueRange(
				Config.Noise.cave.frequency.low, 
				Config.Noise.cave.frequency.high,
				2f
				),
			GameUtils.IntLerp(
				Config.Noise.cave.octaves.low,
				Config.Noise.cave.octaves.high,
				Seed
				),
			Mathf.Lerp(
				Config.Noise.cave.lacunarity.low, 
				Config.Noise.cave.lacunarity.high, 
				Seed
				), 
			Mathf.Lerp(
				Config.Noise.cave.persistance.low, 
				Config.Noise.cave.persistance.high, 
				Seed
				), 
			1024,
			Config.Noise.cave.drift 
				? Mathf.Pow(Seed, 2) 
				: 0f
			);

		pattern = new NoiseOptions(
			new ValueRange(
				Config.Noise.pattern.frequency.low, 
				Config.Noise.pattern.frequency.high,
				3f
				),
			GameUtils.IntLerp(
				Config.Noise.pattern.octaves.low, 
				Config.Noise.pattern.octaves.high, 
				Seed
				),
			Mathf.Lerp(
				Config.Noise.pattern.lacunarity.low, 
				Config.Noise.pattern.lacunarity.high, 
				Seed
				), 
			Mathf.Lerp(
				Config.Noise.pattern.persistance.low, 
				Config.Noise.pattern.persistance.high, 
				Seed
				), 
			1024,
			Config.Noise.pattern.drift 
				? Mathf.Pow(Seed, 2)
				: 0f
			);

		stripe = new NoiseOptions(
			new ValueRange(
				Config.Noise.stripe.frequency.low, 
				Config.Noise.stripe.frequency.high
				), 
			GameUtils.IntLerp(
				Config.Noise.stripe.octaves.low,
				Config.Noise.stripe.octaves.high,
				Seed
				), 
			Mathf.Lerp(
				Config.Noise.stripe.lacunarity.low, 
				Config.Noise.stripe.lacunarity.high,
				Seed
				),
			Mathf.Lerp(
				Config.Noise.stripe.persistance.low, 
				Config.Noise.stripe.persistance.high, 
				Seed
				),
			1024,
			Config.Noise.stripe.drift 
				? Seed 
				: 0f
			);

		driftMap = new NoiseOptions(
			new ValueRange(
				Config.Noise.driftMap.frequency.low, 
				Config.Noise.driftMap.frequency.high
				),
			GameUtils.IntLerp(
				Config.Noise.driftMap.octaves.low,
				Config.Noise.driftMap.octaves.high, 
				Seed
				),
			Mathf.Lerp(
				Config.Noise.driftMap.lacunarity.low, 
				Config.Noise.driftMap.lacunarity.high, 
				Seed
				),
			Mathf.Lerp(
				Config.Noise.driftMap.persistance.low, 
				Config.Noise.driftMap.persistance.high, 
				Seed
				),
			1, 0f
		);

		spawnTypes = new NoiseOptions
		(
			new ValueRange(0.008f, 0.05f, 2f), 
			GameUtils.IntLerp(1, 2, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			10000,
			Seed
		);

		spawnFrequency = new NoiseOptions
		(
			new ValueRange(0.04f, 0.2f), 
			GameUtils.IntLerp(1, 2, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed
		);

		spawnIntensity = new NoiseOptions
		(
			new ValueRange(0.004f, 0.05f, 2f), 
			GameUtils.IntLerp(1, 2, Seed) + 2,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed
		);

		rainIntensity = new NoiseOptions
		(
			new ValueRange(0.001f, 0.002f),
			GameUtils.IntLerp(1, 4, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			1000,
			0f
		);

		lightningIntensity = new NoiseOptions
		(
			rainIntensity.frequency,
			rainIntensity.octaves,
			rainIntensity.lacunarity + Mathf.Lerp(0.01f, 0.0025f, Seed),
			rainIntensity.persistance + Mathf.Lerp(0.01f, 0.0025f, Seed),
			1000,
			0f
		);
		
		boidInteraction = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
		);
		
		boidDistance = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
		);
		
		boidAlignment = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
		);

		worldKey = new NoiseOptions(
			new ValueRange(0.0002f, 0.01f),
			2,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			12,
			Seed
		);

		// Noise Methods
		int index = GameUtils.IntLerp(0, Config.Noise.caveTypes.Length - 1, Seed);
		caveMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.caveTypes[index]][2];

		index = GameUtils.IntLerp(0, Config.Noise.patternTypes.Length - 1, Seed);
		patternMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.patternTypes[index]][2];

		index = GameUtils.IntLerp(0, Config.Noise.stripeTypes.Length - 1, Seed);
		stripeMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.stripeTypes[index]][2];

		index = GameUtils.IntLerp(0, Config.Noise.terrainTypes.Length - 1, Seed);
		terrainType = Config.Noise.terrainTypes[index];

		index = GameUtils.IntLerp(0, Config.Noise.mountainTypes.Length - 1, Seed);
		mountainType = Config.Noise.mountainTypes[index];

		index = GameUtils.IntLerp(0, Config.Noise.driftMapTypes.Length - 1, Seed);
		driftMapMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.driftMapTypes[index]][1];

		terrain.scale = GameUtils.IntLerp(Config.Noise.terrainScale.low, Config.Noise.terrainScale.high, Seed);
	}

	public static void ApplyDefaults()
	{
		Config.Noise = new NoiseSettings();

		Range frequency = new Range(0.00005f, 0.008f);
		IntRange octaves = new IntRange(1, 3);
		Range lacunarity = new Range(0f, 4f);
		Range persistance = new Range(0f, 0.5f);

		Config.Noise.terrain = new NoiseSetting(frequency, octaves, lacunarity, persistance, true);

		frequency = new Range(0.007f, 0.019f);
		octaves = new IntRange(1, 3);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.mountain = new NoiseSetting(frequency, octaves, lacunarity, persistance, true);

		frequency = new Range(0.004f, 0.0085f);
		octaves = new IntRange(1, 4);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.cave = new NoiseSetting(frequency, octaves, lacunarity, persistance, true);

		frequency = new Range(0.006f, 0.01f);
		octaves = new IntRange(1, 4);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.pattern = new NoiseSetting(frequency, octaves, lacunarity, persistance, true);

		frequency = new Range(0.00001f, 0.1f);
		octaves = new IntRange(1, 4);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 2f);

		Config.Noise.stripe = new NoiseSetting(frequency, octaves, lacunarity, persistance, true);

		frequency = new Range(0.0002f, 0.01f);
		octaves = new IntRange(1, 4);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 2f);

		Config.Noise.driftMap = new NoiseSetting(frequency, octaves, lacunarity, persistance, false);

		Config.Noise.terrainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.mountainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.caveTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.patternTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.stripeTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.driftMapTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};

		Config.Noise.terrainScale = new IntRange(6, 48);
		Config.Noise.beachHeight = new IntRange(4, 24);
		Config.Noise.cloudEasing = new IntRange(48, 60);

		Config.Noise.caveBreak = new IntRange(256, 768);
		Config.Noise.patternBreak = new IntRange(128, 768);
		Config.Noise.stripeBreak = new IntRange(64, 940);
		Config.Noise.patternStripeBreak = new IntRange(256, 768);

		Config.Noise.cloudBreak = new IntRange(128, 768);
		Config.Noise.islandBreak = new IntRange(128, 512);

		Config.Noise.glass1 = new IntRange(0, 32);
		Config.Noise.glass2 = new IntRange(0, 32);

		Config.Noise.modScale = new IntRange(2, 64);

		Config.Noise.stretch = new Range(0f, 1000f);
		Config.Noise.squish = new Range(0f, 10f);

		Config.Noise.driftFactor = 1f;
	}
}
