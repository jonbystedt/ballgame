using System;

[Serializable]
public class Range 
{
	public float low { get; set; }
	public float high { get; set; }

	public Range(float l, float h)
	{
		low = l;
		high = h;
	}

	public Range() {}
}

[Serializable]
public class IntRange 
{
	public int low { get; set; }
	public int high { get; set; }

	public IntRange(int l, int h)
	{
		low = l;
		high = h;
	}

	public IntRange() {}
}

[Serializable]
public class NoiseSetting
{
	public Range frequency { get; set; }
	public IntRange octaves { get; set; }
	public Range lacunarity { get; set; }
	public Range persistance { get; set; }
	public bool drift { get; set; }

	public NoiseSetting(Range f, IntRange o, Range l, Range p, bool d)
	{
		frequency = f;
		octaves = o;
		lacunarity = l;
		persistance = p;
		drift = d;
	}

	public NoiseSetting() {}
}

[Serializable]
public class NoiseSettings
{
	public NoiseSetting terrain { get; set; }
	public NoiseSetting mountain { get; set; }
	public NoiseSetting cave { get; set; }
	public NoiseSetting pattern { get; set; }
	public NoiseSetting stripe { get; set; }
	public NoiseSetting driftMap { get; set; }
	public NoiseType[] terrainTypes { get; set; }
	public NoiseType[] mountainTypes { get; set; }
	public NoiseType[] caveTypes { get; set; }
	public NoiseType[] patternTypes { get; set; }
	public NoiseType[] stripeTypes { get; set; }
	public NoiseType[] driftMapTypes { get; set; }
	public IntRange terrainScale { get; set; }
	public IntRange beachHeight { get; set; }
	public IntRange cloudEasing { get; set; }
	public IntRange caveBreak { get; set; }
	public IntRange patternBreak { get; set; }
	public IntRange stripeBreak { get; set; }
	public IntRange patternStripeBreak { get; set; }
	public IntRange cloudBreak { get; set; }
	public IntRange islandBreak { get; set; }
	public IntRange glass1 { get; set; }
	public IntRange glass2 { get; set; }
	public IntRange modScale { get; set; }
	public Range stretch { get; set; }
	public Range squish { get; set; }
	public float driftFactor { get; set; }
}
