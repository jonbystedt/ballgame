using UnityEngine;

public struct ValueRange
{
	public float max;
	public float min;
	public float value;

	public ValueRange(float ma, float mi, float v)
	{
		max = ma;
		min = mi;
		value = v;
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

	public static void Initialize()
	{
		var Seed = GameUtils.Seed;

		float s = Seed;
		rainBreakValue = rainBreakValue + Mathf.FloorToInt(s * s * 350);
		lightningBreakValue = lightningBreakValue + Mathf.FloorToInt(s * s * 250);

		terrain = new NoiseOptions(
			new ValueRange(
				Config.Noise.terrain.frequency.high, 
				Config.Noise.terrain.frequency.low,
				Mathf.Lerp(
					Config.Noise.terrain.frequency.low, 
					Config.Noise.terrain.frequency.high, 
					Mathf.Pow(Seed, 1.2f) 
				)),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.terrain.octaves.low,
				Config.Noise.terrain.octaves.high,
				Seed
				)),
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
			Seed//Mathf.Pow(Seed - 0.5f, 10 + Mathf.FloorToInt(Seed * 10))
			); 

		mountain = new NoiseOptions(
			new ValueRange(
				Config.Noise.mountain.frequency.high, 
				Config.Noise.mountain.frequency.low,
				Mathf.Lerp(
					Config.Noise.mountain.frequency.low, 
					Config.Noise.mountain.frequency.high, 
					Mathf.Pow(Seed, 2f)
				)),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.mountain.octaves.low,
				Config.Noise.mountain.octaves.high,
				Seed
				)),
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
			64 - Mathf.FloorToInt(Mathf.Lerp(0, 16, Mathf.Pow(Seed, 2))),
			Seed//Mathf.Pow(Seed - 0.5f, 8 + Mathf.FloorToInt(Seed * 8))
			); 

		cave = new NoiseOptions(
			new ValueRange(
				Config.Noise.cave.frequency.high, 
				Config.Noise.cave.frequency.low,
				Mathf.Lerp(
					Config.Noise.cave.frequency.low, 
					Config.Noise.cave.frequency.high, 
					Mathf.Pow(Seed, 2f)
				)),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.cave.octaves.low,
				Config.Noise.cave.octaves.high,
				Seed
				)),
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
			Seed//Mathf.Pow(Seed - 0.5f, 20 + Mathf.FloorToInt(Seed * 10))
			);

		pattern = new NoiseOptions(
			new ValueRange(
				Config.Noise.pattern.frequency.high, 
				Config.Noise.pattern.frequency.low,
				Mathf.Lerp(
					Config.Noise.pattern.frequency.low, 
					Config.Noise.pattern.frequency.high, 
					Mathf.Pow(Seed, 3f)
				)),
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.pattern.octaves.low, 
				Config.Noise.pattern.octaves.high, 
				Seed)
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
			Seed//Mathf.Pow(Seed - 0.5f, 16 + Mathf.FloorToInt(Seed * 10))
			);

		stripe = new NoiseOptions(
			new ValueRange(
				Config.Noise.stripe.frequency.high, 
				Config.Noise.stripe.frequency.low,
				Mathf.Lerp(
					Config.Noise.stripe.frequency.low, 
					Config.Noise.stripe.frequency.high, 
					Seed
				)), 
			Mathf.FloorToInt(Mathf.Lerp(
				Config.Noise.stripe.octaves.low,
				Config.Noise.stripe.octaves.high,
				Seed)
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
			Seed//Mathf.Pow(Seed - 0.5f, Mathf.FloorToInt(Seed * 8))
			);

		s = Seed;
		spawnTypes = new NoiseOptions(
			new ValueRange(0.05f, 0.008f, Mathf.Lerp(0.008f, 0.05f, s * s)), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,Seed)),//1, 
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			10000,
			Seed//0f
			);

		spawnFrequency = new NoiseOptions(
			new ValueRange(2f, 0.04f, Mathf.Lerp(0.04f, 2f, Seed)), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,Seed)),//1, 
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed//0f
			);

		s = Seed;
		spawnIntensity = new NoiseOptions(
			new ValueRange(0.05f, 0.004f, Mathf.Lerp(0.004f, 0.05f, s * s)), 
			Mathf.FloorToInt(Mathf.Lerp(1,2,Seed)) + 2,//1, 
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed//0f
			);

		rainIntensity = new NoiseOptions(
			new ValueRange(0.001f, 0.002f, Mathf.Lerp(0.002f, 0.001f, Seed)),
			Mathf.FloorToInt(Mathf.Lerp(1,4, Seed)),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			1000,
			0f
			);

		lightningIntensity = new NoiseOptions(
			rainIntensity.frequency,
			rainIntensity.octaves,
			rainIntensity.lacunarity + Mathf.Lerp(0.01f, 0.0025f, Seed),
			rainIntensity.persistance + Mathf.Lerp(0.01f, 0.0025f, Seed),
			1000,
			0f
			);
		
		boidInteraction = new NoiseOptions(
			new ValueRange(0.01f, 0.0002f, Mathf.Lerp(0.0002f, 0.01f, Seed)),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
			);
		
		boidDistance = new NoiseOptions(
			new ValueRange(0.01f, 0.0002f, Mathf.Lerp(0.0002f, 0.01f, Seed)),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
			);
		
		boidAlignment = new NoiseOptions(
			new ValueRange(0.01f, 0.0002f, Mathf.Lerp(0.0002f, 0.01f, Seed)),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f
			);

		worldKey = new NoiseOptions(
			new ValueRange(0.01f, 0.0002f, Mathf.Lerp(0.0002f, 0.01f, Seed)),
			2,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			12,
			Seed
			);

		driftMap = new NoiseOptions(
			new ValueRange(0.01f, 0.0002f, Mathf.Lerp(0.0002f, 0.01f, Seed)),
			Mathf.FloorToInt(Mathf.Lerp(1,4, Seed)),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			1,
			0f
		);

		// Noise Methods
		int index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.caveTypes.Length - 0.001f, Seed));
		caveMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.caveTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.patternTypes.Length - 0.001f, Seed));
		patternMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.patternTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.stripeTypes.Length - 0.001f, Seed));
		stripeMethod = NoiseGenerator.noiseMethods[(int)Config.Noise.stripeTypes[index]][2];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.terrainTypes.Length - 0.001f, Seed));
		terrainType = Config.Noise.terrainTypes[index];

		index = Mathf.FloorToInt(Mathf.Lerp(0, Config.Noise.mountainTypes.Length - 0.001f, Seed));
		mountainType = Config.Noise.mountainTypes[index];

		terrain.scale = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.terrainScale.low, Config.Noise.terrainScale.high, Seed));
	}

	public static void ApplyDefaults()
	{
		Config.Noise = new NoiseSettings();

		Range frequency = new Range(0.00005f,0.025f);
		Range octaves = new Range(1f, 3f);
		Range lacunarity = new Range(0f, 4f);
		Range persistance = new Range(0f, 0.5f);

		Config.Noise.terrain = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.0005f, 0.025f);
		octaves = new Range(1f, 3f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.mountain = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.008f, 0.02f);
		octaves = new Range(1f, 1f);
		lacunarity = new Range(0f, 0f);
		persistance = new Range(0f, 0f);

		Config.Noise.cave = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.01f, 0.04f);
		octaves = new Range(1f, 4f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.pattern = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.00001f, 0.1f);
		octaves = new Range(1f, 4f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 2f);

		Config.Noise.stripe = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		Config.Noise.terrainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.mountainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.caveTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.patternTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.stripeTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};

		Config.Noise.terrainScale = new Range(1f, 33f);
		Config.Noise.beachHeight = new Range(4f, 32f);
		Config.Noise.cloudEasing = new Range(16f, 32f);

		Config.Noise.caveBreak = new Range(256f, 768f);
		Config.Noise.patternBreak = new Range(128f, 768f);
		Config.Noise.stripeBreak = new Range(64f, 940f);
		Config.Noise.patternStripeBreak = new Range(0f, 1024f);

		Config.Noise.cloudBreak = new Range(512f, 768f);
		Config.Noise.islandBreak = new Range(0f, 256f);

		Config.Noise.glass1 = new Range(0f, 128f);
		Config.Noise.glass2 = new Range(0f, 128f);

		Config.Noise.modScale = new Range(2f, 128f);

		Config.Noise.stretch = new Range(0f, 1000f);
		Config.Noise.squish = new Range(0f, 100f);
	}
}
