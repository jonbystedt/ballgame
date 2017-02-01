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
public class NoiseSetting
{
	public Range frequency { get; set; }
	public Range octaves { get; set; }
	public Range lacunarity { get; set; }
	public Range persistance { get; set; }

	public NoiseSetting(Range f, Range o, Range l, Range p)
	{
		frequency = f;
		octaves = o;
		lacunarity = l;
		persistance = p;
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
	public NoiseType[] terrainTypes { get; set; }
	public NoiseType[] mountainTypes { get; set; }
	public NoiseType[] caveTypes { get; set; }
	public NoiseType[] patternTypes { get; set; }
	public NoiseType[] stripeTypes { get; set; }
	public Range terrainScale { get; set; }
	public Range beachHeight { get; set; }
	public Range cloudEasing { get; set; }
	public Range caveBreak { get; set; }
	public Range patternBreak { get; set; }
	public Range stripeBreak { get; set; }
	public Range patternStripeBreak { get; set; }
	public Range cloudBreak { get; set; }
	public Range islandBreak { get; set; }
	public Range glass1 { get; set; }
	public Range glass2 { get; set; }
	public Range modScale { get; set; }
	public Range stretch { get; set; }
	public Range squish { get; set; }

}
