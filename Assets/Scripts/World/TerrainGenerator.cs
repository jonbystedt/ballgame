using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CielaSpike;

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
	int patternBreakPoint;
	int stripeBreakPoint;
	int patternStripeColorBreakPoint;
	int modScale = 16;

	int caveBreakPoint = 512;
	int cloudBreakPoint;
	int islandBreakPoint;

	int glassIncrease1;
	int glassIncrease2;
	bool solid;
	bool striped;
	bool patterned;
	float patternAmount;

	float stretchFactor;
	float squishFactor;
	bool flipStripes;
	bool[] modPatterns = new bool[12];
	bool glassy1;
	bool glassy2;
	bool freakyFriday;
	bool reverseHollowTaper;
	float hollowFormation;
    float hollowMountains;
    float hollowGlass;
	float hollowPersistance;

	int floor;

	int WORLD_BLOCK_HEIGHT;

	public void Initialize()
	{	
		WORLD_BLOCK_HEIGHT = Config.WorldHeight * Chunk.Size;
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

		if (Config.Multithreaded)
		{
			this.StartCoroutineAsync(AwaitSamplesAsync(sampleSet, column));
		}
		else
		{
			StartCoroutine(AwaitSamples(sampleSet, column));
		}

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

		if (!flipStripes)
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


	IEnumerator AwaitSamplesAsync(SampleSet sampleSet, Chunk[] column)
	{
		for(;;)
		{	
			if (sampleSet.results.Values.All(x => x.complete))
			{
				// loop through the x and z axis. The GenerateColumn coroutine will build a column of blocks at this position.
				for (int x = column[0].pos.x; x < column[0].pos.x + Chunk.Size; x++)
				{
					for (int z = column[0].pos.z; z < column[0].pos.z + Chunk.Size; z++)
					{
						GenerateColumn(x, z, sampleSet, column);
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

		int oldScale = NoiseConfig.mountain.scale;
		NoiseConfig.mountain.scale = NoiseConfig.mountain.scale - terrainHeight;
		int mountainHeight = mountainBase + GetNoise3D(new Vector3(x, 0, z), NoiseConfig.mountain, NoiseConfig.mountainType);
		NoiseConfig.mountain.scale = oldScale;

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

				bool beach = false;
				if (y <= beachHeight)
				{
					beach = true;
				}

				// mountains if less than or equal to the height of a 2D noisemap, and not in the 'cave' negative space
				if (y <= mountainHeight && caveChance < caveValue)
				{
					// glass or rock? if the value of the 3D 'glass' noisemap is greater than the breakpoint this is potentially rock
                    if (patternBreakPoint < glassValue) 
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
						else if (stripeValue > stripeBreakPoint  && (caveChance < caveValue || beach)) 
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - stripeBreakPoint)));

							if (modPatterns[5])
							{
								colorIndex = GetModIndex(colorIndex, caveValue, stripeValue, 32);
							}
							
							chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
							air = false;
						} 
						else if (caveChance < caveValue || beach)
						{
							colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeBreakPoint));

							if (modPatterns[6])
							{
								colorIndex = GetModIndex(colorIndex, glassValue, stripeValue, 16);
							}
							
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
							chunk.SetBlock(localX, localY, localZ, Block.Air);
							if (!air)
							{
								sampleSet.spawnMap.height[localX, localZ] = y;
							}
							air = true;
						}
						// glass sections
						// have rock stripes
						// *** special glass section ***
						else if (glassy2 && (caveChance < caveValue || beach))
						{
							if (patternStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeBreakPoint));

								if (modPatterns[7])
								{
									colorIndex = GetModIndex(colorIndex, glassValue, stripeValue, 16);
								}
								if (modScale % (Mathf.Abs(y) + 1) < colorIndex) 
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Glass(colorIndex));
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
								}
								
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - patternStripeColorBreakPoint)));

									if (modPatterns[8])
									{
										colorIndex = GetModIndex(colorIndex, caveValue, stripeValue, 16);
									}
									
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - patternStripeColorBreakPoint)));

									if (modPatterns[9])
									{
										colorIndex = GetModIndex(colorIndex, caveValue, stripeValue, 32);
									}

									chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
									air = false;
								}

							}
						}
						// *** end special glass section ***
						else if (caveChance < caveValue || beach)
						{
							if (patternStripeColorBreakPoint > stripeValue) 
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)stripeBreakPoint));

								if (modPatterns[10])
								{
									colorIndex = GetModIndex(colorIndex, caveValue, stripeValue, 16);
								}
								
								chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (stripeValue > stripeBreakPoint)
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, stripeValue / (float)(NoiseConfig.stripe.scale - patternStripeColorBreakPoint)));

									if (modPatterns[11])
									{
										colorIndex = GetModIndex(colorIndex, glassValue, stripeValue, 16);
									}
									if (modScale % (Mathf.Abs(y) + 1) < colorIndex) 
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass(colorIndex));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
									}
									
									air = false;
								}
								else
								{
									colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)(NoiseConfig.stripe.scale - patternStripeColorBreakPoint)));

									if (modPatterns[0])
									{
										colorIndex = GetModIndex(colorIndex, glassValue, stripeValue, 32);
									}
									
									chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
									air = false;
								}

							}
						}

					}
				}
				// End of mountains

				// formations
				else if (caveValue > cloudChance && caveValue < cloudChance + ((NoiseConfig.cave.scale - cloudChance) * hollowFormation) 
					&& glassValue > cloudChance - ((NoiseConfig.pattern.scale - cloudChance)))
				{
					// two colors
					if (stripeValue > stripeBreakPoint - glassIncrease1) 
					{
						// repeating or smooth patterns
						modIndex = GetModIndex(0, glassValue, stripeValue, 16);
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - stripeBreakPoint - glassIncrease1) / (float)(NoiseConfig.stripe.scale - stripeBreakPoint - glassIncrease1)));

						// pattern with glass
						if (caveValue > glassValue)
						{
							if (solid)
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(modPatterns[1] ? modIndex : colorIndex));
							}
							else if (patterned)
							{
								if (caveValue * patternAmount > glassValue)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass((modPatterns[1] ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[1] ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[2] ? modIndex : colorIndex)));
								}
							}
							else if (striped)
							{
								if (stripeValue > stripeBreakPoint)
								{
									if (glassy1)
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass((modPatterns[1] ? modIndex : colorIndex) + 16));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[1] ? modIndex : colorIndex) + 16));
									}
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(modPatterns[2] ? modIndex : colorIndex));
								}
							}

						}
						else
						{
							if (modScale % (Mathf.Abs(y) + 1) < colorIndex) 
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Glass(modPatterns[1] ? modIndex : colorIndex));
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(modPatterns[1] ? modIndex : colorIndex));
							}
						}

						air = false;
					} 
					else 
					{
						// repeating or smooth patterns
						modIndex = GetModIndex(17, glassValue, stripeValue, 32);
						//modIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, (float)(glassValue % ((stripeValue % modScale) + 2)) /  ((float)(stripeValue % modScale) + 2f) ));
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, (stripeValue - stripeBreakPoint - glassIncrease1) / (float)(NoiseConfig.stripe.scale - stripeBreakPoint - glassIncrease1)));

						// pattern with glass
						if (caveValue > glassValue)
						{
							// stripe
							if (patternStripeColorBreakPoint > stripeValue)
							{
								if (solid)
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(modPatterns[4] ? modIndex : colorIndex));
								}
								else if (patterned)
								{
									if (caveValue * patternAmount > glassValue)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Glass((modPatterns[4] ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[4] ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock(modPatterns[3] ? modIndex : colorIndex));
									}
								}
								else if (striped)
								{
									if (stripeValue > stripeBreakPoint)
									{
										if (glassy1)
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Glass((modPatterns[4] ? modIndex : colorIndex) - 16));
										}
										else
										{
											chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[4] ? modIndex : colorIndex) - 16));
										}
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[3] ? modIndex : colorIndex)));
									}
								}
							}
							else
							{
								if (modScale % (Mathf.Abs(y) + 1) < colorIndex) 
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Glass((modPatterns[4] ? modIndex : colorIndex)));
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock((modPatterns[4] ? modIndex : colorIndex)));
								}
								
							}

						}
						else
						{
							// other stripes
							if (stripeBreakPoint > stripeValue)
							{
								if (modScale % y < colorIndex) 
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Glass(colorIndex));
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(colorIndex));
								}
								
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
				else if (caveValue < islandBreakPoint)
				{
					// rocks
					if (patternStripeColorBreakPoint > stripeValue - glassIncrease2) 
					{
						colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - glassIncrease2) / (float)patternStripeColorBreakPoint));
						chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
						air = false;
					} 
					// or glass
					else 
					{
						// in stripes with rock
						if (stripeValue > stripeBreakPoint)
						{
							if (modPatterns[2])
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, ((caveValue - cloudChance) % ((stripeValue % 16) + 1)) / ((stripeValue % 16) + 1f)));
							}
							else
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(0, 16, (stripeValue - stripeBreakPoint) / (float)(NoiseConfig.stripe.scale - stripeBreakPoint)));
							}
							if (modScale % (Mathf.Abs(y) + 1) < colorIndex) 
							{
								chunk.SetBlock (localX, localY, localZ, Blocks.Glass(colorIndex));
							}
							else
							{
								chunk.SetBlock (localX, localY, localZ, Blocks.Rock(colorIndex));
							}
							
							air = false;
						}
						else
						{
							if (modPatterns[2])
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, ((caveValue - cloudChance) % ((stripeValue % 16) + 1)) / ((stripeValue % 16) + 1f)));
							}
							else
							{
								colorIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, stripeValue / (float)stripeBreakPoint));
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
		// encourage the floor to slope out by lessening the cave chance along a bilinear curve below beachHeight
		if (y < floor + beachHeight + 1)
		{
			caveChance -= Mathf.FloorToInt(caveChance * beachPersistance 
						* bilinear.Evaluate((float)(floor - y + beachHeight + 1) / (float)beachHeight));
		}

		return caveChance;
	}

	// returns a value to use as the breakpoint between cloud and no cloud
	int GetCloudChance(int cloudChance, int y)
	{
		// taper formations using a log curve at the top of the world
		// the height of the taper is controlled by cloudEasing
		if (y >= Chunk.Size - cloudEasing)
		{
			int heightFromBreak = ToWorldHeight(y) - WORLD_BLOCK_HEIGHT + cloudEasing;
			cloudChance += Mathf.FloorToInt((NoiseConfig.cave.scale - cloudChance) 
							* log.Evaluate((float)heightFromBreak / (float)cloudEasing));
		}

		return cloudChance;
	}

	// returns a value that moderates the chance of the 'pattern' sample carving holes in the mountains
	float GetHollowValue(float hollowValue, int y)
	{
		float persistance = hollowPersistance;

		// persistance is the amount of the hollow value not affected by the linear fade below
		// at values below beachHeight this value is reduced on a log curve to promote beaches
		if (y < beachHeight - (Chunk.Size * (Config.WorldHeight - 1)))
		{
			persistance = persistance * log.Evaluate((float)(y + (Chunk.Size * (Config.WorldHeight - 1))) / (float)(beachHeight));
		}

		// the portion which does not persist varies from 0-max with world height
		hollowValue = hollowValue * persistance 
						+ Mathf.Lerp(0, hollowValue * (1f / persistance), (float)ToWorldHeight(y) / (float)WORLD_BLOCK_HEIGHT);

		return reverseHollowTaper ? 1f - hollowValue : hollowValue;
	}

	int GetModIndex(int colorIndex, int patternValue, int stripeValue, int max)
	{
		colorIndex += Mathf.FloorToInt(
			Mathf.Lerp
			(
				0, 
				16, 
				patternValue % ((stripeValue % modScale) + 2f) 
					/  (float)(Mathf.Abs(stripeValue % modScale) + 2f) 
			)
		);

		if (colorIndex > max)
		{
			colorIndex -= 16;
		}

		return colorIndex;
	}

	int ToWorldHeight(int y)
	{
		return y + (Chunk.Size * (Config.WorldHeight - 1));
	}

	void SetupFlags()
	{
		floor = ((Config.WorldHeight - 1) * -Chunk.Size);
		mountainBase = floor + 64 - NoiseConfig.mountain.scale;

		beachHeight = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.beachHeight.low, Config.Noise.beachHeight.high, GameUtils.Seed));
		beachPersistance = 0.5f + (GameUtils.Seed * 0.5f);
		cloudEasing = 16 + Mathf.FloorToInt(Mathf.Lerp(Config.Noise.cloudEasing.low, Config.Noise.cloudEasing.high, Mathf.Pow(GameUtils.Seed, 2)));
		hollowPersistance = Mathf.Pow(GameUtils.Seed, 10);

		caveBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.caveBreak.low, Config.Noise.caveBreak.high, GameUtils.Seed));
		patternBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.patternBreak.low, Config.Noise.patternBreak.high, GameUtils.Seed));
		stripeBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.stripeBreak.low, Config.Noise.stripeBreak.high, GameUtils.Seed));
		patternStripeColorBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.patternStripeBreak.low, Config.Noise.patternStripeBreak.high, GameUtils.Seed));

		cloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.cloudBreak.low, Config.Noise.cloudBreak.high, GameUtils.Seed));
		islandBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.islandBreak.low, Config.Noise.islandBreak.high, Mathf.Pow(GameUtils.Seed, 2)));

		glassIncrease1 = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.glass1.low, Config.Noise.glass1.high, Mathf.Pow(GameUtils.Seed, 10)));
		glassIncrease2 = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.glass2.low, Config.Noise.glass2.high, Mathf.Pow(GameUtils.Seed, 10)));

		flipStripes = GameUtils.Seed > 0.98f ? true : false;

		modScale = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.modScale.low, Config.Noise.modScale.high, Mathf.Pow(GameUtils.Seed,2)));

		stretchFactor = Mathf.Lerp(Config.Noise.stretch.low, Config.Noise.stretch.high, Mathf.Pow(GameUtils.Seed,2));
		squishFactor = Mathf.Lerp(Config.Noise.squish.low, Config.Noise.squish.high, Mathf.Pow(GameUtils.Seed,2));

		for (int i = 0; i < modPatterns.Length; i++)
		{
			modPatterns[i] = GameUtils.Seed > 0.9f ? true : false;
		}

		glassy1 = GameUtils.Seed > 0.98f ? true : false;
		glassy2 = GameUtils.Seed > 0.98f ? true : false;

		freakyFriday = GameUtils.Seed > 0.98f ? true : false;

		reverseHollowTaper = GameUtils.Seed > 0.95 ? true : false;

        hollowFormation = GameUtils.Seed;
		hollowMountains = Mathf.Pow(GameUtils.Seed * 0.1f, 14f);
		hollowGlass = Mathf.Pow(GameUtils.Seed * 0.1f, 14f);

		float stripedChance = GameUtils.Seed;
		float patternedChance = GameUtils.Seed / 2f;
		float solidChance = GameUtils.Seed / 3f;

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

		patternAmount = GameUtils.Seed;
	}
}
