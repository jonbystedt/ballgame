using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class ValueRange
{
	public float max { get; set; }
	public float min { get; set; }
	public float value { get; set; }

	public ValueRange(float n, float x, float v)
	{
		min = n;
		max = x;
        value = v;
	}

	public ValueRange(float n, float x)
	{
		min = n;
		max = x;
	}
    
    public ValueRange Init(float pow)
    {
        value = Mathf.Lerp(min, max, Mathf.Pow(GameUtils.Seed, pow));
        return this;
    }
}

[Serializable]
public class NoiseOptions
{
	public ValueRange frequency { get; set; }
	public int octaves { get; set; }
    public float lacunarity { get; set; }
    public float persistance { get; set; }
    public int scale { get; set; }
    public float drift { get; set; }
    public float driftScale { get; set; }
    public int id { get; set; }
    public int driftMapId { get; set; }

    public NoiseOptions() { }

	public NoiseOptions(ValueRange f, int o, float l, float p, int s, float d, float ds, int i, int di)
	{
		frequency = f;
		octaves = o;
		lacunarity = l;
		persistance = p;
		scale = s;
		drift = d;
		driftScale = ds;
		id = i;
		driftMapId = di;
	}
}

public static class NoiseConfig
{
	public static NoiseMethod caveMethod;
	public static NoiseMethod patternMethod;
	public static NoiseMethod stripeMethod;
	public static NoiseMethod driftMapMethod;

    public static NoiseOptions[] options;

    static float Seed { get { return GameUtils.Seed; }}

	static int optionCount = 0;
	static float copyChance = 0.5f;

    static int rainBreakValue = 650;
    static int lightningBreakValue = 750;

    static List<NoiseOptions> opts = new List<NoiseOptions>();
    static List<NoiseOptions> drifts = new List<NoiseOptions>();

    public static void Initialize()
	{
        Config.Instance = new WorldSettings();

		opts = new List<NoiseOptions>();
        drifts = new List<NoiseOptions>();
        optionCount = 0;

		float s = Seed;
		Config.Instance.environment.rainBreak = rainBreakValue + Mathf.FloorToInt(s * s * 350);
		Config.Instance.environment.lightningBreak = lightningBreakValue + Mathf.FloorToInt(s * s * 250);

		int driftIndex = GetDriftIndex(Config.Noise.terrain.drift);
		Config.Instance.terrain.hills = new NoiseOptions
		(
			new ValueRange
            (
				Config.Noise.terrain.frequency.low, 
				Config.Noise.terrain.frequency.high
		    )
            .Init(1.2f),
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
				? Seed
				: 0f,
			Config.Noise.terrain.drift 
				? Mathf.Lerp(1f, Config.Noise.driftFactor, Mathf.Pow(Seed, 3))
				: 0f,
			optionCount++,
			driftIndex
		); 

		opts.Add(Config.Instance.terrain.hills);

		driftIndex = GetDriftIndex(Config.Noise.mountain.drift);
		Config.Instance.terrain.mountain = new NoiseOptions
		(
			new ValueRange
            (
				Config.Noise.mountain.frequency.low, 
				Config.Noise.mountain.frequency.high
			)
            .Init(2f),
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
				? Mathf.Pow(Seed, 4) 
				: 0f,
			Config.Noise.mountain.drift 
				? Mathf.Lerp(1f, Config.Noise.driftFactor, Mathf.Pow(Seed, 3))
				: 0f,
			optionCount++,
			driftIndex
		); 

		opts.Add(Config.Instance.terrain.mountain);

		driftIndex = GetDriftIndex(Config.Noise.cave.drift);
        Config.Instance.terrain.cave = new NoiseOptions
		(
			new ValueRange
            (
				Config.Noise.cave.frequency.low, 
				Config.Noise.cave.frequency.high
			)
            .Init(2f),
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
				: 0f,
			Config.Noise.cave.drift 
				? Mathf.Lerp(1f, Config.Noise.driftFactor, Mathf.Pow(Seed, 2))
				: 0f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.terrain.cave);

		driftIndex = GetDriftIndex(Config.Noise.pattern.drift);
        Config.Instance.terrain.pattern = new NoiseOptions
		(
			new ValueRange
            (
				Config.Noise.pattern.frequency.low, 
				Config.Noise.pattern.frequency.high,
				3f
			)
            .Init(3f),
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
				: 0f,
			Config.Noise.pattern.drift 
				? Mathf.Lerp(1f, Config.Noise.driftFactor, Mathf.Pow(Seed, 2))
				: 0f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.terrain.pattern);

		driftIndex = GetDriftIndex(Config.Noise.stripe.drift);
        Config.Instance.terrain.stripe = new NoiseOptions
		(
			new ValueRange
            (
				Config.Noise.stripe.frequency.low, 
				Config.Noise.stripe.frequency.high
			)
            .Init(1f), 
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
				? Mathf.Pow(Seed, 3)
				: 0f,
			Config.Noise.stripe.drift 
				? Mathf.Lerp(1f, Config.Noise.driftFactor, Mathf.Pow(Seed, 3))
				: 0f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.terrain.stripe);

		driftIndex = GetDriftIndex(true);
        Config.Instance.spawns.type = new NoiseOptions
		(
			new ValueRange(0.008f, 0.05f).Init(2f), 
			GameUtils.IntLerp(1, 2, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			10000,
			Seed,
			1f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.spawns.type);

		driftIndex = GetDriftIndex(true);
		Config.Instance.spawns.frequency = new NoiseOptions
		(
			new ValueRange(0.04f, 0.2f).Init(1f), 
			GameUtils.IntLerp(1, 2, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed,
			1f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.spawns.frequency);

		driftIndex = GetDriftIndex(true);
		Config.Instance.spawns.intensity = new NoiseOptions
		(
			new ValueRange(0.004f, 0.05f, 2f).Init(1f), 
			GameUtils.IntLerp(1, 2, Seed) + 2,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			100,
			Seed,
			1f,
			optionCount++,
			driftIndex
		);

		opts.Add(Config.Instance.spawns.intensity);

		Config.Instance.environment.rain = new NoiseOptions
		(
			new ValueRange(0.001f, 0.002f).Init(1f),
			GameUtils.IntLerp(1, 4, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			1000,
			0f,
			0f,
			optionCount++,
			-1
		);

		opts.Add(Config.Instance.environment.rain);

		Config.Instance.environment.lightning = new NoiseOptions
		(
			new ValueRange
            (
                Config.Instance.environment.rain.frequency.min,
                Config.Instance.environment.rain.frequency.max,
                Config.Instance.environment.rain.frequency.value
            ),
            Config.Instance.environment.rain.octaves,
            Config.Instance.environment.rain.lacunarity + Mathf.Lerp(0.01f, 0.0025f, Seed),
            Config.Instance.environment.rain.persistance + Mathf.Lerp(0.01f, 0.0025f, Seed),
			1000,
			0f,
			0f,
			optionCount++,
			-1
		);

		opts.Add(Config.Instance.environment.lightning);
		
		Config.Instance.environment.boids.interaction = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f).Init(1f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f,
			0f,
			optionCount++,
			-1
		);

		opts.Add(Config.Instance.environment.boids.interaction);


        Config.Instance.environment.boids.distance = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f).Init(1f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f,
			0f,
			optionCount++,
			-1
		);
		
		opts.Add(Config.Instance.environment.boids.distance);

        Config.Instance.environment.boids.alignment = new NoiseOptions
		(
			new ValueRange(0.0002f, 0.01f).Init(1f),
			1,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			20,
			0f,
			0f,
			optionCount++,
			-1
		);

		opts.Add(Config.Instance.environment.boids.alignment);

        Config.Instance.environment.key = new NoiseOptions(
			new ValueRange(0.0002f, 0.01f).Init(1f),
			2,
			Mathf.Lerp(0f, 2f, Seed),
			Mathf.Lerp(0f, 2f, Seed),
			12,
			0f,
			0f,
			optionCount++,
			-1
		);

		opts.Add(Config.Instance.environment.key);
		options = opts.ToArray();

        Config.Instance.terrain.driftMaps = drifts.ToArray();

		// Noise Methods
		int index = GameUtils.IntLerp(0, Config.Noise.caveTypes.Length - 1, Seed);
        Config.Instance.terrain.caveType = Config.Noise.caveTypes[index];
        caveMethod = NoiseGenerator.noiseMethods[(int)Config.Instance.terrain.caveType][2];

		index = GameUtils.IntLerp(0, Config.Noise.patternTypes.Length - 1, Seed);
        Config.Instance.terrain.patternType = Config.Noise.patternTypes[index];
        patternMethod = NoiseGenerator.noiseMethods[(int)Config.Instance.terrain.patternType][2];

		index = GameUtils.IntLerp(0, Config.Noise.stripeTypes.Length - 1, Seed);
        Config.Instance.terrain.stripeType = Config.Noise.stripeTypes[index];
        stripeMethod = NoiseGenerator.noiseMethods[(int)Config.Instance.terrain.stripeType][2];

		index = GameUtils.IntLerp(0, Config.Noise.terrainTypes.Length - 1, Seed);
		Config.Instance.terrain.hillType = Config.Noise.terrainTypes[index];

		index = GameUtils.IntLerp(0, Config.Noise.mountainTypes.Length - 1, Seed);
        Config.Instance.terrain.mountainType = Config.Noise.mountainTypes[index];

		index = GameUtils.IntLerp(0, Config.Noise.driftMapTypes.Length - 1, Seed);
        Config.Instance.terrain.driftMapType = Config.Noise.driftMapTypes[index];
        driftMapMethod = NoiseGenerator.noiseMethods[(int)Config.Instance.terrain.driftMapType][1];

        Config.Instance.terrain.hills.scale = GameUtils.IntLerp
        (
            Config.Noise.terrainScale.low, 
            Config.Noise.terrainScale.high, 
            Seed
        );
	}

	static int GetDriftIndex(bool drift)
	{
		int driftIndex = -1;

		if (drift)
		{
			foreach (NoiseOptions o in opts)
			{
				if (o.driftMapId != -1 && Seed < copyChance)
				{
					driftIndex = o.driftMapId;
				}
			}
			
			if (driftIndex == -1)
			{
				driftIndex = optionCount;
                var driftMap = GetDriftMap();
				opts.Add(driftMap);
                drifts.Add(driftMap);
			}
		}

		return driftIndex;
	}

	static NoiseOptions GetDriftMap()
	{
		return new NoiseOptions(
			new ValueRange
            (
				Config.Noise.driftMap.frequency.low, 
				Config.Noise.driftMap.frequency.high
			)
            .Init(1f),
			GameUtils.IntLerp
            (
				Config.Noise.driftMap.octaves.low,
				Config.Noise.driftMap.octaves.high, 
				Seed
			),
			Mathf.Lerp
            (
				Config.Noise.driftMap.lacunarity.low, 
				Config.Noise.driftMap.lacunarity.high, 
				Seed
			),
			Mathf.Lerp
            (
				Config.Noise.driftMap.persistance.low, 
				Config.Noise.driftMap.persistance.high, 
				Seed
			),
			1, 
			0f,
			0f, 
			optionCount++, 
			-1
		);
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
