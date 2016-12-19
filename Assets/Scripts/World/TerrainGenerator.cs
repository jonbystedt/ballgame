using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class TerrainGenerator : MonoBehaviour
{
	public bool initialized = false;
	public int mountainBase = -42;
	public InterpolatedNoise generator;

	private List<SampleSet> SampleSetPool = new List<SampleSet>(); // TODO: Find expected size

	int glassRockBreakPoint;
	int stripeColorBreakPoint;
	int glassStripeColorBreakPoint;

	int caveBreakPoint = 512;
	int cloudBreakPoint;
	int lowerCloudBreakPoint;

	int glassIncrease1;
	int glassIncrease2;
	bool solid;
	bool striped;
	bool patterned;
	float patternAmount;

	float stretchFactor;
	float squishFactor;
	bool poleFlip;
	bool candyStriper;
	bool candyStriper2;
	bool candyStriper3;
	bool candyStriper4;
	bool glassy1;
	bool glassy2;
	bool freakyFriday;
	float hollow;

	int floor = -48;

	public void Initialize()
	{	
		SetupFlags();
		initialized = true;
	}

	public Region Generate(Chunk[] column)
	{
		SampleSet sampleSet = GetSampleSet();

		Region sampleRegion = new Region(
			new WorldPosition(column[0].pos.x, column.Min(chunk => chunk.pos.y), column[0].pos.z),
			new WorldPosition(
				column[0].pos.x + Chunk.Size - 1, 
				column.Max(chunk => chunk.pos.y) + Chunk.Size - 1, 
				column[0].pos.z + Chunk.Size - 1)
		);

		sampleSet.SetRegion(sampleRegion);

		generator.SampleNoise(sampleSet);
		PopulateSpawns(sampleSet, column[0].pos);

		StartCoroutine(AwaitSamples(sampleSet, column));

		return sampleSet.region;
	}

	SampleSet GetSampleSet()
	{
		SampleSet sampleSet;
		int lastAvailableIndex = SampleSetPool.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			sampleSet = SampleSetPool[lastAvailableIndex];
			SampleSetPool.RemoveAt(lastAvailableIndex);
			sampleSet.Initialize();
		}
		else
		{
			sampleSet = CreateNewSampleSet();
		}

		return sampleSet;
	}

	SampleSet CreateNewSampleSet()
	{
		SampleRegion caves = new SampleRegion(NoiseConfig.cave, NoiseConfig.caveMethod, 4, new Vector3(1,1,1));

		SampleRegion patterns = new SampleRegion(NoiseConfig.pattern, NoiseConfig.patternMethod, 4, new Vector3(1,1,1));

		SampleRegion stripes;

		if (!poleFlip)
		{
			stripes= new SampleRegion(
				NoiseConfig.stripe, 
				NoiseConfig.stripeMethod, 
				1, 
				new Vector3(1f / stretchFactor, squishFactor, 1f / stretchFactor));
		}
		else
		{
			stripes = new SampleRegion(
				NoiseConfig.stripe, 
				NoiseConfig.stripeMethod, 
				1, 
				new Vector3(squishFactor, 1f / stretchFactor, squishFactor));
		}

		Dictionary<string,SampleRegion> sampleDict = new Dictionary<string, SampleRegion>()
		{
			{"caves", caves},
			{"patterns", patterns},
			{"stripes", stripes}
		};

		SampleSet sampleSet = new SampleSet(sampleDict);

		// Still need to set region
		return sampleSet;
	}

	public void RemoveResults(Region region)
	{
		SampleSet sampleSet;
		InterpolatedNoise.Results.TryGetValue(region, out sampleSet);
		if (sampleSet != null)
		{
			SampleSetPool.Add(sampleSet);
			InterpolatedNoise.Results.Remove(region);
		}
	}


	IEnumerator AwaitSamples(SampleSet sampleSet, Chunk[] column)
	{
		for(;;)
		{	
			if (sampleSet.results.Values.All(x => x.complete))
			{
				Stopwatch stopwatch = Stopwatch.StartNew();

				for (int x = column[0].pos.x; x < column[0].pos.x + Chunk.Size; x++)
				{
					for (int z = column[0].pos.z; z < column[0].pos.z + Chunk.Size; z++)
					{
						GenerateColumn(x, z, sampleSet, column);

						if (stopwatch.ElapsedTicks > Config.CoroutineTiming)
						{
							yield return null;

							stopwatch.Reset();
							stopwatch.Start();
						}
					}
				}

				for (int i = 0; i < column.Length; i++)
				{
					Chunk chunk = column[i];
					chunk.SetBlocksUnmodified();
					Serialization.Load(chunk);
					chunk.built = true;
				}

				break;
			} 
			else
			{
				yield return null;
			}
		}

		yield return null;
	}

	public void GenerateColumn(int x, int z, SampleSet sampleSet, Chunk[] column)
	{
		int[,,] caves = sampleSet.results["caves"].interpolates;
		int[,,] patterns;
		int[,,] stripes;

		if (!freakyFriday)
		{
			patterns = sampleSet.results["patterns"].interpolates;
			stripes = sampleSet.results["stripes"].interpolates;
		}
		else
		{
			stripes = sampleSet.results["patterns"].interpolates;
			patterns = sampleSet.results["stripes"].interpolates;
		}

		int localX = x - column[0].pos.x;	
		int localZ = z - column[0].pos.z;

		int terrainHeight = GetNoise3D(new Vector3(x, 0, z), NoiseConfig.terrain, NoiseConfig.terrainType);

		NoiseConfig.mountain.scale = 64 - terrainHeight;
		int mountainHeight = mountainBase + GetNoise3D(new Vector3(x, 0, z), NoiseConfig.mountain, NoiseConfig.mountainType);

		for(int i = 0; i < column.Length; i++)
		{
			Chunk chunk = column[i];
			bool air = true;

			for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.Size; y++)
			{
				int localY = y - chunk.pos.y;

				if (y <= floor)
				{
					chunk.SetBlock(localX, localY, localZ, new Block(16));
					air = false;
					continue;
				}

				int caveValue = caves[localX, (i * Chunk.Size) + localY, localZ];
				int stripeValue = stripes[localX, (i * Chunk.Size) + localY, localZ];
				int glassValue = patterns[localX, (i * Chunk.Size) + localY, localZ];

				// Taper clouds towards ceiling
				int cloudChance = cloudBreakPoint;
				if (y >= 5)
				{
					cloudChance += Mathf.FloorToInt(((NoiseConfig.cave.scale - cloudBreakPoint) / 11f) * (y - 4f));
				}

				// Slope gently out towards open space at bottom
				int adjustedCaveChance = GetAdjustedCaveValue(caveBreakPoint, y);
				int colorIndex;
				int modIndex;

				// mountains
				if (y <= mountainHeight && adjustedCaveChance < caveValue)
				{
					// glass or rock?
					if (glassRockBreakPoint < glassValue) 
					{
						// rock stripes
						if (stripeValue > stripeColorBreakPoint) 
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - stripeColorBreakPoint)));
							chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
							air = false;
						} 
						else 
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
							chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
							air = false;
						}
					}
					else
					{
						// glass sections
						// have rock stripes
						if (glassy2)
						{
							if (glassStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
								chunk.SetBlock (localX, localY, localZ, new BlockGlass(colorIndex));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeColorBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
									air = false;
								}

							}
						}
						else
						{
							if (glassStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
								chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeColorBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, new BlockGlass(colorIndex));
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, new BlockGlass(colorIndex));
									air = false;
								}

							}
						}

					}
				}

				// formations
				else if (caveValue > cloudChance && caveValue < cloudChance + ((NoiseConfig.cave.scale - cloudChance) * hollow) && glassValue > cloudChance - ((NoiseConfig.pattern.scale - cloudChance) * hollow))
				{
					// two colors
					if (stripeValue > stripeColorBreakPoint - glassIncrease1) 
					{
						// repeating or smooth patterns
                        modIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (float)(glassValue % (stripeValue % 16 + 16f)) / (float)(stripeValue % 16 + 16f)));
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - stripeColorBreakPoint - glassIncrease1) / (float)(NoiseConfig.stripe.scale - stripeColorBreakPoint - glassIncrease1)));

						// pattern with glass
						if (caveValue > glassValue)
						{
							if (solid)
							{
								chunk.SetBlock(localX, localY, localZ, new Block(candyStriper ? modIndex : colorIndex));
							}
							else if (patterned)
							{
								if (caveValue * patternAmount > glassValue)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, new BlockGlass((candyStriper ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, new Block((candyStriper ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, new Block((candyStriper2 ? modIndex : colorIndex)));
								}
							}
							else if (striped)
							{
								if (stripeValue > stripeColorBreakPoint)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, new BlockGlass((candyStriper ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, new Block((candyStriper ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, new Block(candyStriper2 ? modIndex : colorIndex));
								}
							}

						}
						else
						{
							chunk.SetBlock(localX, localY, localZ, new BlockGlass(candyStriper ? modIndex : colorIndex));
						}

						air = false;
					} 
					else 
					{
						// repeating or smooth patterns
						modIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, (float)(glassValue % (stripeValue % 16 + 16f)) / (float)(stripeValue % 16 + 16f)));
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, (stripeValue - stripeColorBreakPoint - glassIncrease1) / (float)(NoiseConfig.stripe.scale - stripeColorBreakPoint - glassIncrease1)));

						// pattern with glass
						if (caveValue > glassValue)
						{
							// stripe
							if (glassStripeColorBreakPoint > stripeValue)
							{
								if (solid)
								{
									chunk.SetBlock(localX, localY, localZ, new Block(candyStriper4 ? modIndex : colorIndex));
								}
								else if (patterned)
								{
									if (caveValue * patternAmount > glassValue)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, new BlockGlass((candyStriper4 ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, new Block((candyStriper4 ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, new Block(candyStriper3 ? modIndex : colorIndex));
									}
								}
								else if (striped)
								{
									if (stripeValue > stripeColorBreakPoint)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, new BlockGlass((candyStriper4 ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, new Block((candyStriper4 ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, new Block((candyStriper3 ? modIndex : colorIndex)));
									}
								}
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, new BlockGlass((candyStriper4 ? modIndex : colorIndex)));
							}

						}
						else
						{
							// other stripes
							if (stripeColorBreakPoint > stripeValue)
							{
								chunk.SetBlock(localX, localY, localZ, new BlockGlass(colorIndex));
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, new Block(colorIndex));
							}
						}

						air = false;
					}

				}
				// islands
				else if (caveValue < lowerCloudBreakPoint)
				{
					// rocks
					if (glassStripeColorBreakPoint > stripeValue - glassIncrease2) 
					{
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - glassIncrease2) / (float)glassStripeColorBreakPoint));
						chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
						air = false;
					} 
					// or glass
					else 
					{
						// in stripes with rock
						if (stripeValue > stripeColorBreakPoint)
						{
							if (candyStriper2)
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, ((caveValue - cloudChance) % ((stripeValue % 16) + 1)) / ((stripeValue % 16) + 1f)));
							}
							else
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - stripeColorBreakPoint) / (float)(NoiseConfig.stripe.scale - stripeColorBreakPoint)));
							}

							chunk.SetBlock (localX, localY, localZ, new BlockGlass(colorIndex));
							air = false;
						}
						else
						{
							if (candyStriper2)
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, ((caveValue - cloudChance) % ((stripeValue % 16) + 1)) / ((stripeValue % 16) + 1f)));
							}
							else
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)stripeColorBreakPoint));
							}

							chunk.SetBlock (localX, localY, localZ, new Block(colorIndex));
							air = false;
						}
					}

				}
				else
				{
					chunk.SetBlock (localX, localY, localZ, new BlockAir());
					if (!air)
					{
						sampleSet.spawnMap.height[localX, localZ] = y;
					}
					air = true;
				}
				if (y == 15 && sampleSet.spawnMap.height[localX, localZ] == Chunk.NoSpawn)
				{
					sampleSet.spawnMap.height[localX, localZ] = y + 1;
				}
			}

		}
	}


	void PopulateSpawns(SampleSet sampleSet, WorldPosition pos)
	{
		for (int x = pos.x; x < pos.x + Chunk.Size; x++)
		{
			for (int z = pos.z; z < pos.z + Chunk.Size; z++)
			{
				// Value controls the type of item (if any) that can spawn at this location
				sampleSet.spawnMap.value[x - pos.x, z - pos.z] = //Chunk.NoSpawn;
					GetNoise2D(new Vector3(pos.x + x, pos.z + z, 0), NoiseConfig.spawnTypes, NoiseType.SimplexValue);

				// Frequency is a base control on how many of the item will spawn
				int frequency = GetNoise2D(new Vector3(pos.x + x, pos.z + z, 0), NoiseConfig.spawnFrequency, NoiseType.SimplexValue);

				// And intensity controls how 'intense' the spawning action is at this location
				int intensity = GetNoise2D(
						new Vector3(pos.x + x, pos.z + z, 0), 
						NoiseConfig.spawnIntensity, 
						NoiseType.SimplexValue
					);

				sampleSet.spawnMap.intensity[x - pos.x, z - pos.z] = intensity;

				if (frequency < 50)
				{
					sampleSet.spawnMap.frequency[x - pos.x, z - pos.z] = 1;
				}
				else if (frequency < 70)
				{
					sampleSet.spawnMap.frequency[x - pos.x, z - pos.z] = 2;
				}
				else if (frequency < 75)
				{
					sampleSet.spawnMap.frequency[x - pos.x, z - pos.z] = 3;
				}
				else if (frequency < 80)
				{
					sampleSet.spawnMap.frequency[x - pos.x, z - pos.z] = 4;
				}
				else if (frequency < 85)
				{
					sampleSet.spawnMap.frequency[x - pos.x,z - pos.z] = 5;
				}
				else if (frequency < 90)
				{
					sampleSet.spawnMap.frequency[x - pos.x,z - pos.z] = 6;
				}
				else if (frequency < 95)
				{
					sampleSet.spawnMap.frequency[x - pos.x,z - pos.z] = 7;
				}
				else if (frequency < 98)
				{
					sampleSet.spawnMap.frequency[x - pos.x,z - pos.z] = 8;
				} 
				else
				{
					sampleSet.spawnMap.frequency[x - pos.x,z - pos.z] = 9;
				}

				sampleSet.spawnMap.height[x - pos.x,z - pos.z] = Chunk.NoSpawn;

			}
		}
	}

	public static int GetNoise3D(Vector3 point, NoiseOptions options, NoiseType method)
	{
		return Mathf.FloorToInt(
			(NoiseGenerator.Sum(
				NoiseGenerator.noiseMethods[(int)method][2], 
				new Vector3(point.x, point.y, point.z), 
				options.frequency, options.octaves, 
				options.lacunarity, 
				options.persistance) + 1f).value * (options.scale / 2f));
	}

	public static int GetNoise2D(Vector3 point, NoiseOptions options, NoiseType method)
	{
		return Mathf.FloorToInt(
			(NoiseGenerator.Sum(
				NoiseGenerator.noiseMethods[(int)method][1], 
				new Vector3(point.x, point.y, 0), 
				options.frequency, options.octaves, 
				options.lacunarity, 
				options.persistance) + 1f).value * (options.scale / 2f));
	}

	public static int GetNoise1D(Vector3 point, NoiseOptions options, NoiseType method)
	{
		return Mathf.FloorToInt(
			(NoiseGenerator.Sum(
				NoiseGenerator.noiseMethods[(int)method][0], 
				new Vector3(point.x, 0, 0), 
				options.frequency, 
				options.octaves, 
				options.lacunarity, 
				options.persistance) + 1f).value * (options.scale / 2f));
	}

	static int GetAdjustedCaveValue(int caveValue, int y)
	{
		if (y == -47) caveValue -= 172;
		if (y == -46) caveValue -= 152;
		if (y == -45) caveValue -= 132;
		if (y == -44) caveValue -= 112;
		if (y == -43) caveValue -= 92;
		if (y == -42) caveValue -= 72;
		if (y == -41) caveValue -= 52;
		if (y == -40) caveValue -= 32;
		if (y == -39) caveValue -= 22;
		if (y == -38) caveValue -= 17;
		if (y == -37) caveValue -= 12;
		if (y == -36) caveValue -= 7;
		if (y == -35) caveValue -= 2;

		return caveValue;
	}

	void SetupFlags()
	{
		caveBreakPoint = Mathf.FloorToInt(Mathf.Lerp(384, 640, GameUtils.Variance));

		glassRockBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 192, GameUtils.Variance));
		stripeColorBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 1024, GameUtils.Variance));
		glassStripeColorBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 1024, GameUtils.Variance));

		cloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(512, 768, GameUtils.Variance));
		lowerCloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 256, GameUtils.Variance));

		glassIncrease1 = Mathf.FloorToInt(Mathf.Lerp(0, 512, GameUtils.Variance));
		glassIncrease2 = Mathf.FloorToInt(Mathf.Lerp(0, 512, GameUtils.Variance));

		poleFlip = GameUtils.Variance > 0.95f ? true : false;

		stretchFactor = Mathf.Lerp(0, 100, GameUtils.Variance);
		squishFactor = Mathf.Lerp(0, 10, GameUtils.Variance);

		candyStriper = GameUtils.Variance > 0.95f ? true : false;
		candyStriper2 = GameUtils.Variance > 0.95f ? true : false;
		candyStriper3 = GameUtils.Variance > 0.95f ? true : false;
		candyStriper4 = GameUtils.Variance > 0.95f ? true : false;

		glassy1 = GameUtils.Variance > 0.85f ? true : false;
		glassy2 = GameUtils.Variance > 0.85f ? true : false;

		freakyFriday = GameUtils.Variance > 0.9f ? true : false;

		hollow = GameUtils.Variance;

		float stripedChance = GameUtils.Variance;
		float patternedChance = GameUtils.Variance / 2f;
		float solidChance = GameUtils.Variance / 3f;

		if (solidChance > patternedChance && solidChance > stripedChance)
		{
			solid = true;
		} 
		else if (patternedChance > solidChance && patternedChance > stripedChance)
		{
			patterned = true;
		}
		else
		{
			striped = true;
		}

		patternAmount = GameUtils.Variance;
	}
}
