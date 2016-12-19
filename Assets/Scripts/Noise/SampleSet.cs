using System;
using System.Collections.Generic;

public class SampleSet
{
	public Dictionary<String,SampleRegion> results;
	public Region region;
	public SpawnMap spawnMap;

	public SampleSet(Dictionary<String,SampleRegion> res)
	{
		results = res;
		spawnMap = new SpawnMap(Chunk.Size);
	}

	public void SetRegion(Region r)
	{
		region = r;
		results["caves"].region = r;
		results["patterns"].region = r;
		results["stripes"].region = r;
	}

	public void Initialize()
	{
		results["caves"].sampled = false;
		results["caves"].complete = false;
		results["patterns"].sampled = false;
		results["patterns"].complete = false;
		results["stripes"].sampled = false;
		results["stripes"].complete = false;
	}
}



