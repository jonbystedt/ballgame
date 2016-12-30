using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class TerrainGenerator : MonoBehaviour
{
	public AnimationCurve linear;
	public AnimationCurve log;
	public AnimationCurve bilinear;
	public AnimationCurve trilinear;
	public bool initialized = false;
	public int mountainBase;
	public InterpolatedNoise generator;

	private List<SampleSet> SampleSetPool = new List<SampleSet>(); // TODO: Find expected size

	int beachHeight;
	int cloudEasing;
	float beachPersistance;
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
	float hollowFormation;
    float hollowMountains;
    float hollowGlass;
	float hollowPersistance;

	int floor;

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

		SampleRegion patterns = new SampleRegion(NoiseConfig.pattern, NoiseConfig.patternMethod, 2, new Vector3(1,1,1));

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

				// loop through the x and z axis. The GenerateColumn coroutine will build a column of blocks at this position.
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

		// loop through each chunk in the column
		for(int i = 0; i < column.Length; i++)
		{
			Chunk chunk = column[i];

			// track the topmost block of air in this column to use as the spawn location
			bool air = true;

			// building a single column of blocks on the y axis
			for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.Size; y++)
			{
				int localY = y - chunk.pos.y;

				// build a layer of blocks on the lowest level
				if (y <= floor)
				{
					chunk.SetBlock(localX, localY, localZ, Blocks.Rock(16));
					air = false;
					continue;
				}

				int caveValue = caves[localX, (i * Chunk.Size) + localY, localZ];
				int stripeValue = stripes[localX, (i * Chunk.Size) + localY, localZ];
				int glassValue = patterns[localX, (i * Chunk.Size) + localY, localZ];

				// Taper clouds towards ceiling				
				int cloudChance = GetCloudChance(cloudBreakPoint, y);

				// Slope gently out towards open space at bottom
				int caveChance = GetCaveChance(caveBreakPoint, y);

				// Mountains are more likely to become hollow towards the top
				float hollowMountainValue = GetHollowValue(hollowMountains, y);
				float hollowGlassValue = GetHollowValue(hollowGlass, y);

				int colorIndex;
				int modIndex;

				// mountains if less than or equal to the height of a 2D noisemap, and not in the 'cave' negative space
				if (y <= mountainHeight && caveChance < caveValue)
				{
					// glass or rock? if the value of the 3D 'glass' noisemap is greater than the breakpoint this is potentially rock
                    if (glassRockBreakPoint < glassValue) 
					{
						// but if the value of the 'glass' noisemap is greater than the 'hollow' cutoff this is air
						if (glassValue > NoiseConfig.pattern.scale - Mathf.FloorToInt((hollowMountainValue * (float)NoiseConfig.pattern.scale)))
						{
							chunk.SetBlock (localX, localY, localZ, Block.Air);
							if (!air)
							{
								sampleSet.spawnMap.height[localX, localZ] = y;
							}
							air = true;
						}
						// two distinct rock stripes provided by the 3D noisemap 'stripes'
						else if (stripeValue > stripeColorBreakPoint) 
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - stripeColorBreakPoint)));
							chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
							air = false;
						} 
						else 
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
							chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
							air = false;
						}
					}
					// Okay, this could be glass
                    else 
					{
						// If we are less than the corresponding 'hollow' value this is air
						if (glassValue < NoiseConfig.pattern.scale * hollowGlassValue) 
						{
							chunk.SetBlock (localX, localY, localZ, Block.Air);
							if (!air)
							{
								sampleSet.spawnMap.height[localX, localZ] = y;
							}
							air = true;
						}
						// glass sections
						// have rock stripes
						else if (glassy2)
						{
							if (glassStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
								chunk.SetBlock (localX, localY, localZ, Blocks.Glass(colorIndex));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeColorBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
									air = false;
								}

							}
						}
						else
						{
							if (glassStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeColorBreakPoint));
								chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeColorBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, Blocks.Glass(colorIndex));
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - glassStripeColorBreakPoint)));
									chunk.SetBlock (localX, localY, localZ, Blocks.Glass(colorIndex));
									air = false;
								}

							}
						}

					}
				}
				// End of mountains

				// formations
				else if (caveValue > cloudChance && caveValue < cloudChance + ((NoiseConfig.cave.scale - cloudChance) * hollowFormation) 
					&& glassValue > cloudChance - ((NoiseConfig.pattern.scale - cloudChance) * hollowFormation))
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
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(candyStriper ? modIndex : colorIndex));
							}
							else if (patterned)
							{
								if (caveValue * patternAmount > glassValue)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass((candyStriper ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper2 ? modIndex : colorIndex)));
								}
							}
							else if (striped)
							{
								if (stripeValue > stripeColorBreakPoint)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass((candyStriper ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(candyStriper2 ? modIndex : colorIndex));
								}
							}

						}
						else
						{
							chunk.SetBlock(localX, localY, localZ, Blocks.Glass(candyStriper ? modIndex : colorIndex));
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
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(candyStriper4 ? modIndex : colorIndex));
								}
								else if (patterned)
								{
									if (caveValue * patternAmount > glassValue)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Glass((candyStriper4 ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper4 ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock(candyStriper3 ? modIndex : colorIndex));
									}
								}
								else if (striped)
								{
									if (stripeValue > stripeColorBreakPoint)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Glass((candyStriper4 ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper4 ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((candyStriper3 ? modIndex : colorIndex)));
									}
								}
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Glass((candyStriper4 ? modIndex : colorIndex)));
							}

						}
						else
						{
							// other stripes
							if (stripeColorBreakPoint > stripeValue)
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Glass(colorIndex));
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
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
						chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
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

							chunk.SetBlock (localX, localY, localZ, Blocks.Glass(colorIndex));
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

							chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
							air = false;
						}
					}

				}
				else
				{
					chunk.SetBlock (localX, localY, localZ, Block.Air);
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

	// returns a value to use as the breakpoint between cave and no cave
	int GetCaveChance(int caveChance, int y)
	{
		if (y < floor + beachHeight + 1)
		{
			caveChance -= Mathf.FloorToInt(caveChance * beachPersistance 
						* bilinear.Evaluate(floor - y + beachHeight + 1 / beachHeight));
		}

		return caveChance;
	}

	// returns a value to use as the breakpoint between cloud and no cloud
	int GetCloudChance(int cloudChance, int y)
	{
		if (y >= Chunk.Size - cloudEasing)
		{
			cloudChance += Mathf.FloorToInt((NoiseConfig.cave.scale - cloudChance) 
							* log.Evaluate(Mathf.Abs(Chunk.Size - cloudEasing - y) + 1 / cloudEasing));
		}

		return cloudChance;
	}

	// returns a value that moderates the chance of the 'pattern' sample carving holes in the mountains
	float GetHollowValue(float hollowValue, int y)
	{
		float persistance = hollowPersistance;
		if (y < -(Chunk.Size * Config.WorldHeight - 1) + beachHeight)
		{
			persistance = persistance * (1f - log.Evaluate(Mathf.Abs(y + (Chunk.Size * Config.WorldHeight - 1) + beachHeight)  / Chunk.Size));
		}
		hollowValue = (hollowValue * (1f - persistance)) 
						+ Mathf.Lerp(
							0, 
							hollowValue * persistance, 
							y + 1 + (Chunk.Size * (Config.WorldHeight - 1)) / (Chunk.Size * Config.WorldHeight));

		return hollowValue;
	}

	void SetupFlags()
	{
		floor = ((Config.WorldHeight - 1) * -Chunk.Size);
		mountainBase = floor + Config.MountainBase;

		beachHeight = Mathf.FloorToInt(Mathf.Lerp(4, 32, GameUtils.SeedValue));
		beachPersistance = 0.5f + (GameUtils.SeedValue * 0.5f);
		cloudEasing = 16;
		hollowPersistance = 1f - Mathf.Pow(GameUtils.SeedValue, 2);

		caveBreakPoint = Mathf.FloorToInt(Mathf.Lerp(384, 640, GameUtils.SeedValue));

		glassRockBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 192, GameUtils.SeedValue));
		stripeColorBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 1024, GameUtils.SeedValue));
		glassStripeColorBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 1024, GameUtils.SeedValue));

		cloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(512, 768, GameUtils.SeedValue));
		lowerCloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(0, 256, GameUtils.SeedValue));

		glassIncrease1 = Mathf.FloorToInt(Mathf.Lerp(0, 512, GameUtils.SeedValue));
		glassIncrease2 = Mathf.FloorToInt(Mathf.Lerp(0, 512, GameUtils.SeedValue));

		poleFlip = GameUtils.SeedValue > 0.95f ? true : false;

		stretchFactor = Mathf.Lerp(0, 1000, Mathf.Pow(GameUtils.SeedValue,2));
		squishFactor = Mathf.Lerp(0, 100, Mathf.Pow(GameUtils.SeedValue,2));

		candyStriper = GameUtils.SeedValue > 0.95f ? true : false;
		candyStriper2 = GameUtils.SeedValue > 0.95f ? true : false;
		candyStriper3 = GameUtils.SeedValue > 0.95f ? true : false;
		candyStriper4 = GameUtils.SeedValue > 0.95f ? true : false;

		glassy1 = GameUtils.SeedValue > 0.85f ? true : false;
		glassy2 = GameUtils.SeedValue > 0.85f ? true : false;

		freakyFriday = GameUtils.SeedValue > 0.9f ? true : false;

        hollowFormation = GameUtils.SeedValue;
        hollowMountains = Mathf.Pow(GameUtils.SeedValue, 5);
        hollowGlass = Mathf.Pow(GameUtils.SeedValue, 5);

		float stripedChance = GameUtils.SeedValue;
		float patternedChance = GameUtils.SeedValue / 2f;
		float solidChance = GameUtils.SeedValue / 3f;

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

		patternAmount = GameUtils.SeedValue;
	}
}
