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
	int pBP; // patternBreakPoint
	int sBP; // stripeBreakPoint
	int psBP; // patternStripeBreakPoint
	int modScale = 16;
    int numModFlags = 16;
    int numGlassFlags = 4;
    int numIslandFlags = 4;

	int caveBreakPoint = 512;
	int cloudBreakPoint;
	int islandBreakPoint;

	int gI1; // glass increase 1
	int gI2; // glass increase 2

	float patternAmount;

	float stretchFactor;
	float squishFactor;

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
			new World3(column[0].pos.x, column.Min(chunk => chunk.pos.y), column[0].pos.z),
			new World3(
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
		SampleRegion caves = new SampleRegion
        (
            Config.WorldConfig.terrain.cave.id, 
            NoiseConfig.caveMethod, 
            Config.SampleRate,
            new Vector3(1,1,1)
        );
		SampleRegion patterns = new SampleRegion
        (
            Config.WorldConfig.terrain.pattern.id, 
            NoiseConfig.patternMethod, 
            Config.SampleRate, 
            new Vector3(1,1,1)
        );
		SampleRegion stripes;

		if (!Flags.Get(NoiseFlags.FlipStripes))
		{
			stripes= new SampleRegion
			(
				Config.WorldConfig.terrain.stripe.id, 
				NoiseConfig.stripeMethod, 
				Mathf.CeilToInt(Config.SampleRate / 2f), 
				new Vector3(1f / stretchFactor, squishFactor, 1f / stretchFactor)
			);
		}
		else
		{
			stripes = new SampleRegion
			(
                Config.WorldConfig.terrain.stripe.id, 
				NoiseConfig.stripeMethod, 
				Mathf.CeilToInt(Config.SampleRate / 2f), 
				new Vector3(squishFactor, 1f / stretchFactor, squishFactor)
			);
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

        int localX = x - column[0].pos.x;
        int localZ = z - column[0].pos.z;

        // Swap patterns for stripes if flag set
        if (!Flags.Get(NoiseFlags.FreakyFriday))
		{
			patterns = sampleSet.results["patterns"].interpolates;
			stripes = sampleSet.results["stripes"].interpolates;
		}
		else
		{
			stripes = sampleSet.results["patterns"].interpolates;
			patterns = sampleSet.results["stripes"].interpolates;
		}

        // Find height of terrain at this spot
		int terrainHeight = GetNoise3D
        (
            new Vector3(x, 0, z), 
            Config.WorldConfig.terrain.terrain, 
            NoiseConfig.terrainType
        );

        // Adjust mountain scale to terrain height to avoid overflow
		int oldScale = Config.WorldConfig.terrain.mountain.scale;

		if (Flags.Get(NoiseFlags.TigerStripes))
		{
            Config.WorldConfig.terrain.mountain.scale = Config.WorldConfig.terrain.mountain.scale - terrainHeight;
		}
		else if (Config.WorldConfig.terrain.mountain.scale + terrainHeight > Config.WorldHeight * Chunk.Size)
		{
            Config.WorldConfig.terrain.mountain.scale = (Config.WorldHeight * Chunk.Size) - terrainHeight;
		}

		int mountainHeight = mountainBase + GetNoise3D
        (
            new Vector3(x, 0, z), 
            Config.WorldConfig.terrain.mountain, 
            NoiseConfig.mountainType
        );

        Config.WorldConfig.terrain.mountain.scale = oldScale;

		int mesaHeight = WORLD_BLOCK_HEIGHT - Mathf.FloorToInt((float)terrainHeight * 0.25f + ((float)(mountainHeight - mountainBase) * 0.1f));

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

				int cV = caves[localX, (i * Chunk.Size) + localY, localZ];
				int sV = stripes[localX, (i * Chunk.Size) + localY, localZ];
				int gV = patterns[localX, (i * Chunk.Size) + localY, localZ];

				// Taper clouds towards ceiling				
				int clC = GetCloudChance(cloudBreakPoint, y);

				// Slope gently out towards open space at bottom
				int cvC = GetCaveChance(caveBreakPoint, y);

				// Mountains are more likely to become hollow towards the top
				float hMV = GetHollowValue(hollowMountains, y);
				float hGV = GetHollowValue(hollowGlass, y);

				int cI;
				int mI;

				bool beach = false;
				if (y <= beachHeight)
				{
					beach = true;
				}

				// mountains if less than or equal to the height of a 2D noisemap, and not in the 'cave' negative space
				if (y <= mountainHeight && cvC < cV)
				{
					// glass or rock? if the value of the 3D 'glass' noisemap is greater than the breakpoint this is potentially rock
                    if (pBP < gV) 
					{

						// but if the value of the 'glass' noisemap is greater than the 'hollow' cutoff this is air
						if (gV > Config.WorldConfig.terrain.pattern.scale - Mathf.FloorToInt((hMV * (float)Config.WorldConfig.terrain.pattern.scale)))
						{
							chunk.SetBlock(localX, localY, localZ, Block.Air);
							if (!air)
							{
								sampleSet.spawnMap.height[localX, localZ] = y;
							}
							air = true;
						}
						// two distinct rock stripes provided by the 3D noisemap 'stripes'
						else if (sV > sBP  && (cvC < cV || beach)) 
						{
							cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, sV / (float)(Config.WorldConfig.terrain.stripe.scale - sBP)));

							if (Flags.Get(NoiseFlags.ModPattern6))
							{
								cI = GetModIndex(cI, cV, sV, 32);
							}
							
							chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
							air = false;
						} 
						else if (cvC < cV || beach)
						{
							cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, sV / (float)sBP));

							if (Flags.Get(NoiseFlags.ModPattern7))
							{
								cI = GetModIndex(cI, gV, sV, 16);
							}
							
							chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
							air = false;
						}
					}
					// Okay, this could be glass
                    else 
					{
						// If we are less than the corresponding 'hollow' value this is air
						if (gV < Config.WorldConfig.terrain.pattern.scale * hGV) 
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
						else if (Flags.Get(NoiseFlags.Glass2) && (cvC < cV || beach))
						{
							if (psBP > sV) 
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, sV / (float)sBP));

								if (Flags.Get(NoiseFlags.ModPattern8))
								{
									cI = GetModIndex(cI, gV, sV, 16);
								}
								if (modScale % (Mathf.Abs(y) + 1) < cI) 
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Glass(cI));
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
								}
								
								air = false;
							} 
							else 
							{
								// glass stripes
								if (sV > sBP)
								{
									cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, sV / (float)(Config.WorldConfig.terrain.stripe.scale - psBP)));

									if (Flags.Get(NoiseFlags.ModPattern9))
									{
										cI = GetModIndex(cI, cV, sV, 16);
									}
									
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
									air = false;
								}
								else
								{
									cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, sV / (float)(Config.WorldConfig.terrain.stripe.scale - psBP)));

									if (Flags.Get(NoiseFlags.ModPattern10))
									{
										cI = GetModIndex(cI, cV, sV, 32);
									}

									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
									air = false;
								}

							}
						}
						// *** end special glass section ***
						else if (cvC < cV || beach)
						{
							if (psBP > sV) 
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, sV / (float)sBP));

								if (Flags.Get(NoiseFlags.ModPattern11))
								{
									cI = GetModIndex(cI, cV, sV, 16);
								}
								
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
								air = false;
							} 
							else 
							{
								// glass stripes
								if (sV > sBP)
								{
									cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, sV / (float)(Config.WorldConfig.terrain.stripe.scale - psBP)));

									if (Flags.Get(NoiseFlags.ModPattern12))
									{
										cI = GetModIndex(cI, gV, sV, 16);
									}
									if (modScale % (Mathf.Abs(y) + 1) < cI) 
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Glass(cI));
									}
									else
									{
										chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
									}
									
									air = false;
								}
								else
								{
									cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, sV / (float)(Config.WorldConfig.terrain.stripe.scale - psBP)));

									if (Flags.Get(NoiseFlags.ModPattern1))
									{
										cI = GetModIndex(cI, gV, sV, 32);
									}
									
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
									air = false;
								}

							}
						}

					}
				}
				// End of mountains

				// carve out the tops of the mesas
				else if (ToWorldHeight(y) > mesaHeight)
				{
					chunk.SetBlock(localX, localY, localZ, Block.Air);
					if (!air)
					{
						sampleSet.spawnMap.height[localX, localZ] = y;
					}
					air = true;
					continue;
				}

				// formations
				else if (cV > clC && cV < clC + ((Config.WorldConfig.terrain.cave.scale - clC) * hollowFormation) 
					&& gV > clC - ((Config.WorldConfig.terrain.pattern.scale - clC)))
				{
					// two colors
					if (sV > sBP - gI1) 
					{
						// repeating or smooth patterns
						mI = (GetModIndex(0, gV, sV, 16) + gV) % 48;

						cI = (Mathf.FloorToInt(Mathf.Lerp(0, 16, (sV - sBP - gI1) / (float)(Config.WorldConfig.terrain.stripe.scale - sBP - gI1))) + gV) % 48;
							
						// pattern with glass
						if (cV > gV)
						{
							if (Flags.Get(NoiseFlags.Solid))
							{
								chunk.SetBlock
                                (
                                    localX, 
                                    localY, 
                                    localZ, 
                                    Blocks.Rock(Flags.Get(NoiseFlags.ModPattern2) ? mI : cI)
                                );
							}
							else if (Flags.Get(NoiseFlags.Patterned))
							{
								if (cV * patternAmount > gV)
								{
									if (Flags.Get(NoiseFlags.Glass1))
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ, 
                                            Blocks.Glass((Flags.Get(NoiseFlags.ModPattern2) ? mI : cI) + 16)
                                        );
									}
									else
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ,
                                            Blocks.Rock((Flags.Get(NoiseFlags.ModPattern2) ? mI : cI) + 16)
                                        );
									}
								}
								else
								{
									chunk.SetBlock
                                    (
                                        localX, 
                                        localY, 
                                        localZ, 
                                        Blocks.Rock(Flags.Get(NoiseFlags.ModPattern3) ? mI : cI)
                                    );
								}
							}
							else if (Flags.Get(NoiseFlags.Striped))
							{
								if (sV > sBP)
								{
									if (Flags.Get(NoiseFlags.Glass1))
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ, 
                                            Blocks.Glass((Flags.Get(NoiseFlags.ModPattern2) ? mI : cI) + 16)
                                        );
									}
									else
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ, 
                                            Blocks.Rock((Flags.Get(NoiseFlags.ModPattern2) ? mI : cI) + 16)
                                        );
									}
								}
								else
								{
									chunk.SetBlock
                                    (
                                        localX, 
                                        localY, 
                                        localZ, 
                                        Blocks.Rock(Flags.Get(NoiseFlags.ModPattern3) ? mI : cI)
                                    );
								}
							}

						}
						else
						{
							if (modScale % (Mathf.Abs(y) + 1) < cI) 
							{
								chunk.SetBlock
                                (
                                    localX, 
                                    localY, 
                                    localZ, 
                                    Blocks.Glass(Flags.Get(NoiseFlags.ModPattern2) ? mI : cI)
                                );
							}
							else
							{
								chunk.SetBlock
                                (
                                    localX, 
                                    localY, 
                                    localZ, 
                                    Blocks.Rock(Flags.Get(NoiseFlags.ModPattern2) ? mI : cI)
                                );
							}
						}

						air = false;
					} 
					else 
					{
						// repeating or smooth patterns
						mI = GetModIndex(17, gV, sV, 32);
						//modIndex = Mathf.FloorToInt(Mathf.Lerp(17, 32, (float)(glassValue % ((stripeValue % modScale) + 2)) /  ((float)(stripeValue % modScale) + 2f) ));
						cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, (sV - sBP - gI1) / (float)(Config.WorldConfig.terrain.stripe.scale - sBP - gI1)));

						// pattern with glass
						if (cV > gV)
						{
							// stripe
							if (psBP > sV)
							{
								if (Flags.Get(NoiseFlags.Solid))
								{
									chunk.SetBlock
                                    (
                                        localX, 
                                        localY, 
                                        localZ, 
                                        Blocks.Rock(Flags.Get(NoiseFlags.ModPattern5) ? mI : cI)
                                    );
								}
								else if (Flags.Get(NoiseFlags.Patterned))
								{
									if (cV * patternAmount > gV)
									{
										if (Flags.Get(NoiseFlags.Glass1))
										{
											chunk.SetBlock
                                            (
                                                localX, 
                                                localY, 
                                                localZ, 
                                                Blocks.Glass((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI) - 16)
                                            );
										}
										else
										{
											chunk.SetBlock
                                            (
                                                localX, 
                                                localY, 
                                                localZ,
                                                Blocks.Rock((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI) - 16)
                                            );
										}
									}
									else
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ, 
                                            Blocks.Rock(Flags.Get(NoiseFlags.ModPattern4) ? mI : cI)
                                        );
									}
								}
								else if (Flags.Get(NoiseFlags.Striped))
								{
									if (sV > sBP)
									{
										if (Flags.Get(NoiseFlags.Glass1))
										{
											chunk.SetBlock
                                            (
                                                localX, 
                                                localY, 
                                                localZ, 
                                                Blocks.Glass((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI) - 16)
                                            );
										}
										else
										{
											chunk.SetBlock
                                            (
                                                localX, 
                                                localY, 
                                                localZ, 
                                                Blocks.Rock((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI) - 16)
                                            );
										}
									}
									else
									{
										chunk.SetBlock
                                        (
                                            localX, 
                                            localY, 
                                            localZ, 
                                            Blocks.Rock((Flags.Get(NoiseFlags.ModPattern4) ? mI : cI))
                                        );
									}
								}
							}
							else
							{
								if (modScale % (Mathf.Abs(y) + 1) < cI) 
								{
									chunk.SetBlock
                                    (
                                        localX, 
                                        localY, 
                                        localZ, 
                                        Blocks.Glass((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI))
                                    );
								}
								else
								{
									chunk.SetBlock
                                    (
                                        localX, 
                                        localY, 
                                        localZ, 
                                        Blocks.Rock((Flags.Get(NoiseFlags.ModPattern5) ? mI : cI))
                                    );
								}
								
							}

						}
						else
						{
							// other stripes
							if (sBP > sV)
							{
								if (modScale % y < cI) 
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Glass(cI));
								}
								else
								{
									chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
								}
								
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
							}
						}

						air = false;
					}

				}
				// islands
				else if ((Flags.Get(NoiseFlags.Islands1) && !Flags.Get(NoiseFlags.Islands2) && gV < islandBreakPoint) ||
					    (!Flags.Get(NoiseFlags.Islands1) && Flags.Get(NoiseFlags.Islands2) && gV > islandBreakPoint) ||
						(!Flags.Get(NoiseFlags.Islands1) && !Flags.Get(NoiseFlags.Islands2) && cV < islandBreakPoint))
				{
					// rocks
					if (psBP > sV - gI2) 
					{
						cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, (sV - gI2) / (float)psBP));
						chunk.SetBlock (localX, localY, localZ, Blocks.Rock(cI));
						air = false;
					} 
					// or glass
					else 
					{
						// in stripes with rock
						if (sV > sBP)
						{
							if (Flags.Get(NoiseFlags.ModPattern3))
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, ((cV - clC) % ((sV % 16) + 1)) / ((sV % 16) + 1f)));
							}
							else
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(0, 16, (sV - sBP) / (float)(Config.WorldConfig.terrain.stripe.scale - sBP)));
							}
							if (modScale % (Mathf.Abs(y) + 1) < cI) 
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Glass(cI));
							}
							else
							{
								chunk.SetBlock(localX, localY, localZ, Blocks.Rock(cI));
							}
							
							air = false;
						}
						else
						{
							if (Flags.Get(NoiseFlags.ModPattern3))
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, ((cV - clC) % ((sV % 16) + 1)) / ((sV % 16) + 1f)));
							}
							else
							{
								cI = Mathf.FloorToInt(Mathf.Lerp(17, 32, sV / (float)sBP));
							}

							chunk.SetBlock (localX, localY, localZ, Blocks.Rock(cI));
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


	void PopulateSpawns(SampleSet sampleSet, World3 pos)
	{
		for (int x = pos.x; x < pos.x + Chunk.Size; x++)
		{
			for (int z = pos.z; z < pos.z + Chunk.Size; z++)
			{
				// Value controls the type of item (if any) that can spawn at this location
				sampleSet.spawnMap.value[x - pos.x, z - pos.z] = //Chunk.NoSpawn;
					GetNoise2D(new Vector3(pos.x + x, pos.z + z, 0), Config.WorldConfig.spawns.type, NoiseType.SimplexValue);

				// Frequency is a base control on how many of the item will spawn
				int frequency = GetNoise2D(new Vector3(pos.x + x, pos.z + z, 0), Config.WorldConfig.spawns.frequency, NoiseType.SimplexValue);

				// And intensity controls how 'intense' the spawning action is at this location
				int intensity = GetNoise2D(
						new Vector3(pos.x + x, pos.z + z, 0),
                        Config.WorldConfig.spawns.intensity, 
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
				options.frequency.value, 
				options.octaves, 
				options.lacunarity, 
				options.persistance) + 1f).value * (options.scale / 2f));
	}

	public static int GetNoise2D(Vector3 point, NoiseOptions options, NoiseType method)
	{
		return Mathf.FloorToInt(
			(NoiseGenerator.Sum(
				NoiseGenerator.noiseMethods[(int)method][1], 
				new Vector3(point.x, point.y, 0), 
				options.frequency.value,
				options.octaves, 
				options.lacunarity, 
				options.persistance) + 1f).value * (options.scale / 2f));
	}

	public static int GetNoise1D(Vector3 point, NoiseOptions options, NoiseType method)
	{
		return Mathf.FloorToInt(
			(NoiseGenerator.Sum(
				NoiseGenerator.noiseMethods[(int)method][0], 
				new Vector3(point.x, 0, 0), 
				options.frequency.value, 
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
			cloudChance += Mathf.FloorToInt((Config.WorldConfig.terrain.cave.scale - cloudChance) 
							* log.Evaluate((float)heightFromBreak / (float)cloudEasing));
		}

		return cloudChance;
	}

	// returns a value that moderates the chance of the 'pattern' sample carving holes in the mountains
	float GetHollowValue(float hollowValue, int y)
	{
		return hollowValue;
		// float persistance = hollowPersistance;

		// // persistance is the amount of the hollow value not affected by the linear fade below
		// // at values below beachHeight this value is reduced on a log curve to promote beaches
		// if (y < beachHeight - (Chunk.Size * (Config.WorldHeight - 1)))
		// {
		// 	persistance = persistance * log.Evaluate((float)(y + (Chunk.Size * (Config.WorldHeight - 1))) / (float)(beachHeight));
		// }

		// // the portion which does not persist varies from 0-max with world height
		// hollowValue = hollowValue * persistance 
		// 				+ Mathf.Lerp(0, hollowValue * (1f / persistance), (float)ToWorldHeight(y) / (float)WORLD_BLOCK_HEIGHT);

		// return reverseHollowTaper ? 1f - hollowValue : hollowValue;
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
		mountainBase = floor + 64 - Config.WorldConfig.terrain.mountain.scale;

		beachHeight = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.beachHeight.low, Config.Noise.beachHeight.high, GameUtils.Seed));
		beachPersistance = 0.5f + (GameUtils.Seed * 0.5f);
		cloudEasing = 16 + Mathf.FloorToInt(Mathf.Lerp(Config.Noise.cloudEasing.low, Config.Noise.cloudEasing.high, Mathf.Pow(GameUtils.Seed, 2)));
		hollowPersistance = Mathf.Pow(GameUtils.Seed, 10);

		caveBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.caveBreak.low, Config.Noise.caveBreak.high, GameUtils.Seed));
		pBP = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.patternBreak.low, Config.Noise.patternBreak.high, GameUtils.Seed));
		sBP = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.stripeBreak.low, Config.Noise.stripeBreak.high, GameUtils.Seed));
		psBP = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.patternStripeBreak.low, Config.Noise.patternStripeBreak.high, GameUtils.Seed));

		cloudBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.cloudBreak.low, Config.Noise.cloudBreak.high, GameUtils.Seed));
		islandBreakPoint = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.islandBreak.low, Config.Noise.islandBreak.high, Mathf.Pow(GameUtils.Seed, 2)));

		gI1 = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.glass1.low, Config.Noise.glass1.high, Mathf.Pow(GameUtils.Seed, 10)));
		gI2 = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.glass2.low, Config.Noise.glass2.high, Mathf.Pow(GameUtils.Seed, 10)));

		Flags.Set(NoiseFlags.FlipStripes, GameUtils.Seed > 0.95f ? true : false);

		modScale = Mathf.FloorToInt(Mathf.Lerp(Config.Noise.modScale.low, Config.Noise.modScale.high, Mathf.Pow(GameUtils.Seed,2)));

		stretchFactor = Mathf.Lerp(Config.Noise.stretch.low, Config.Noise.stretch.high, Mathf.Pow(GameUtils.Seed,2));
		squishFactor = Mathf.Lerp(Config.Noise.squish.low, Config.Noise.squish.high, Mathf.Pow(GameUtils.Seed,2));

		for (int i = 1; i <= numModFlags; i++)
		{
			Flags.Set("ModPattern" + i.ToString(), GameUtils.Seed > 0.9f ? true : false);
		}

        for (int i = 1; i <= numGlassFlags; i++)
        {
            Flags.Set("Glass" + i.ToString(), GameUtils.Seed > 0.95f ? true : false);
        }

        for (int i = 1; i <= numIslandFlags; i++)
        {
            Flags.Set("Islands" + i.ToString(), GameUtils.Seed > 0.5f ? true : false);
        }

        Flags.Set(NoiseFlags.FreakyFriday, GameUtils.Seed > 0.8f ? true : false);

		Flags.Set(NoiseFlags.TigerStripes, GameUtils.Seed > 0.8f ? true : false);

		Flags.Set(NoiseFlags.ReverseHollow, GameUtils.Seed > 0.95 ? true : false);

        hollowFormation = GameUtils.Seed;
		hollowMountains = Mathf.Pow(GameUtils.Seed * 0.1f, 12f);
		hollowGlass = Mathf.Pow(GameUtils.Seed * 0.1f, 12f);

		float stripedChance = GameUtils.Seed;
		float patternedChance = GameUtils.Seed / 2f;
		float solidChance = GameUtils.Seed / 3f;

		if (solidChance > patternedChance && solidChance > stripedChance)
		{
			Flags.Set(NoiseFlags.Solid, true);
		} 
		else if (patternedChance > solidChance && patternedChance > stripedChance)
		{
			Flags.Set(NoiseFlags.Patterned, true);
		}
		else
		{
			Flags.Set(NoiseFlags.Striped, true);
		}

		patternAmount = GameUtils.Seed;

        Game.LogAppend(Flags.ToHex());
        Config.WorldConfig.terrain.flags = Flags.ToHex();
	}
}
