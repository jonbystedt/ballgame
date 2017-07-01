using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using CielaSpike;

public class InterpolatedNoise : MonoBehaviour 
{

	public static Dictionary<Region,SampleSet> Results = new Dictionary<Region,SampleSet>();

	public void Clear()
	{
		Results.Clear();
	}

	public void SampleNoise(SampleSet sampleSet)
	{
		if (!Results.ContainsKey(sampleSet.region))
		{
			Results.Add(sampleSet.region, sampleSet);

			foreach(KeyValuePair<string,SampleRegion> result in sampleSet.results)
			{
				if (Config.Multithreaded)
				{
					this.StartCoroutineAsync(GetSamplesAsync(result.Value));
					this.StartCoroutineAsync(MapExpandAsync(result.Value));
				}
				else
				{
					this.StartCoroutine(GetSamples(result.Value));
					this.StartCoroutine(MapExpand(result.Value));
				}
			}
		}
	}
		

	IEnumerator GetSamplesAsync(SampleRegion i)
	{
		int h_rate = 2;
		if (Config.Interpolation == InterpolationLevel.Off) 
		{
			h_rate = 1;
		}
		int sampleX = (i.region.sizeX / (i.sampleRate * h_rate)) + 1;
		int sampleY = (i.region.sizeY / i.sampleRate) + 1;
		int sampleZ = (i.region.sizeZ / (i.sampleRate * h_rate)) + 1;

		if (i.samples == null || i.samples.Length != sampleX * sampleY * sampleZ)
		{
			i.samples = new float[sampleX, sampleY, sampleZ];
		}

		for (int z = 0; z < sampleZ; z++)
		{
			for (int x = 0; x < sampleX; x++)
			{
				// distance calculation for drift
				Vector2 location = new Vector2(
					(x * i.sampleRate + (i.region.min.x / 2f)) * i.zoom.x,
					(z * i.sampleRate + (i.region.min.z / 2f)) * i.zoom.z
					);

				float driftMap = 0f;
				if (i.options.drift != 0f)
				{
					driftMap = NoiseGenerator.Sum(
						NoiseConfig.driftMapMethod,
						location,
						NoiseConfig.driftMap.frequency.value,
						NoiseConfig.driftMap.octaves,
						NoiseConfig.driftMap.lacunarity,
						NoiseConfig.driftMap.persistance);
				}
				 
				for (int y = 0; y < sampleY; y++)
				{
					// x and z are calculated above
					Vector3 position = new Vector3(
						location.x,
						(y * i.sampleRate + i.region.min.y) * i.zoom.y,
						location.y
						);

					// with drift
					i.samples[x, y, z] = NoiseGenerator.Sum(
						i.method, 
						position, 
						i.options.drift != 0f 
							? Mathf.Lerp(i.options.frequency.value, driftMap > 0f ? i.options.frequency.max : i.options.frequency.min, Mathf.Abs(driftMap))
							: i.options.frequency.value,
						i.options.octaves, 
						i.options.lacunarity, 
						i.options.persistance
					);
				}
			}
		}

		i.sampled = true;

		yield return null;
	}


	IEnumerator MapExpandAsync(SampleRegion i)
	{
		// Wait for the sampling coroutine to complete.
		for (;;)
		{
			if (!i.sampled)
			{
				yield return null;
			} 
			else
			{
				break;
			}
		}

		if (i.interpolates == null)
		{
			i.interpolates = new int[
				(i.region.sizeX / i.sampleRate) * i.sampleRate, 
				(i.region.sizeY / i.sampleRate) * i.sampleRate, 
				(i.region.sizeZ / i.sampleRate) * i.sampleRate
			];
		}

		int sampleX = i.region.sizeX / (i.sampleRate * 2) + 1;
		int sampleY = i.region.sizeY / i.sampleRate + 1;
		int sampleZ = i.region.sizeZ / (i.sampleRate * 2) + 1;

		for (int z = 0; z < sampleZ - 1; z++)
		{
			for (int y = 0; y < sampleY - 1; y++)
			{
				for (int x = 0; x < sampleX -1; x++)
				{
					float v000 = i.samples[x, y, z];
					float v100 = i.samples[x + 1, y, z];
					float v010 = i.samples[x, y + 1, z];
					float v110 = i.samples[x + 1, y + 1, z];
					float v001 = i.samples[x, y, z + 1];
					float v101 = i.samples[x + 1, y, z + 1];
					float v011 = i.samples[x, y + 1, z + 1];
					float v111 = i.samples[x + 1, y + 1, z + 1];

					for (int zi = 0; zi < i.sampleRate * 2; ++zi)
					{
						for (int yi = 0; yi < i.sampleRate; ++yi)
						{
							for (int xi = 0; xi < i.sampleRate * 2; ++xi)
							{
								float tx = (float)xi / (i.sampleRate * 2);
								float ty = (float)yi / i.sampleRate;
								float tz = (float)zi / (i.sampleRate * 2);

								i.interpolates[x * (i.sampleRate * 2) + xi, y * i.sampleRate + yi, z * (i.sampleRate * 2) + zi]
									= Mathf.FloorToInt(((
										GameUtils.TriLerp (v000, v100, v010, v110, v001, v101, v011, v111, tx, ty, tz)
									+ 1f) * (i.options.scale / 2f)));
							}
						}
					}
				}
			}
		}

		i.complete = true;

		yield return null;
	}

	IEnumerator GetSamples(SampleRegion i)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		int sampleX = (i.region.sizeX / (i.sampleRate * 2)) + 1;
		int sampleY = (i.region.sizeY / i.sampleRate) + 1;
		int sampleZ = (i.region.sizeZ / (i.sampleRate * 2)) + 1;

		if (i.samples == null || i.samples.Length != sampleX * sampleY * sampleZ)
		{
			i.samples = new float[sampleX, sampleY, sampleZ];
		}

		for (int z = 0; z < sampleZ; z++)
		{
			for (int x = 0; x < sampleX; x++)
			{
				// distance calculation for drift
				Vector2 location = new Vector2(
					(x * i.sampleRate + (i.region.min.x / 2f)) * i.zoom.x,
					(z * i.sampleRate + (i.region.min.z / 2f)) * i.zoom.z
					);
				//float distance = Mathf.Abs(Vector2.Distance(Vector2.zero, location));

				float driftMap = 0f;
				if (i.options.drift != 0f)
				{
					driftMap = NoiseGenerator.Sum(
						NoiseGenerator.noiseMethods[(int)NoiseType.Perlin][1],
						location,
						NoiseConfig.driftMap.frequency.value,
						NoiseConfig.driftMap.octaves,
						NoiseConfig.driftMap.lacunarity,
						NoiseConfig.driftMap.persistance);
				}
				 
				for (int y = 0; y < sampleY; y++)
				{
					// x and z are calculated above
					Vector3 position = new Vector3(
						location.x,
						(y * i.sampleRate + i.region.min.y) * i.zoom.y,
						location.y
						);

					// with drift
					//i.options.frequency + (i.options.drift * distance), 
					i.samples[x, y, z] = NoiseGenerator.Sum(
						i.method, 
						position, 
						i.options.drift != 0f 
							? Mathf.Lerp(i.options.frequency.value, driftMap > 0f ? i.options.frequency.max : i.options.frequency.min, Mathf.Abs(driftMap))
							: i.options.frequency.value,
						i.options.octaves, 
						i.options.lacunarity, 
						i.options.persistance
					);

					if (stopwatch.ElapsedTicks > Config.CoroutineTiming)
					{
						yield return null;
						stopwatch.Reset();
						stopwatch.Start();
					}
				}
			}
		}

		i.sampled = true;

		yield return null;
	}


	IEnumerator MapExpand(SampleRegion i)
	{
		// Wait for the sampling coroutine to complete.
		for (;;)
		{
			if (!i.sampled)
			{
				yield return null;
			} 
			else
			{
				break;
			}
		}

		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		if (i.interpolates == null)
		{
			i.interpolates = new int[
				(i.region.sizeX / i.sampleRate) * i.sampleRate, 
				(i.region.sizeY / i.sampleRate) * i.sampleRate, 
				(i.region.sizeZ / i.sampleRate) * i.sampleRate
			];
		}

		int sampleX = i.region.sizeX / (i.sampleRate * 2) + 1;
		int sampleY = i.region.sizeY / i.sampleRate + 1;
		int sampleZ = i.region.sizeZ / (i.sampleRate * 2) + 1;

		for (int z = 0; z < sampleZ - 1; z++)
		{
			for (int y = 0; y < sampleY - 1; y++)
			{
				for (int x = 0; x < sampleX -1; x++)
				{
					float v000 = i.samples[x, y, z];
					float v100 = i.samples[x + 1, y, z];
					float v010 = i.samples[x, y + 1, z];
					float v110 = i.samples[x + 1, y + 1, z];
					float v001 = i.samples[x, y, z + 1];
					float v101 = i.samples[x + 1, y, z + 1];
					float v011 = i.samples[x, y + 1, z + 1];
					float v111 = i.samples[x + 1, y + 1, z + 1];

					for (int zi = 0; zi < i.sampleRate * 2; ++zi)
					{
						for (int yi = 0; yi < i.sampleRate; ++yi)
						{
							for (int xi = 0; xi < i.sampleRate * 2; ++xi)
							{
								float tx = (float)xi / (i.sampleRate * 2);
								float ty = (float)yi / i.sampleRate;
								float tz = (float)zi / (i.sampleRate * 2);

								i.interpolates[x * (i.sampleRate * 2) + xi, y * i.sampleRate + yi, z * (i.sampleRate * 2) + zi]
									= Mathf.FloorToInt(((
										GameUtils.TriLerp (v000, v100, v010, v110, v001, v101, v011, v111, tx, ty, tz)
									+ 1f) * (i.options.scale / 2f)));

								if (stopwatch.ElapsedTicks > Config.CoroutineTiming)
								{
									yield return null;

									stopwatch.Reset();
									stopwatch.Start();
								}
							}
						}
					}
				}
			}
		}

		i.complete = true;

		yield return null;
	}

	int[,,] getSubset(int[,,] fullData, Region fullRegion, Region subRegion)
	{
		if (subRegion.sizeX != fullRegion.sizeX || subRegion.sizeY != fullRegion.sizeY || subRegion.sizeZ != fullRegion.sizeZ)
		{
			int[,,] subset = new int[subRegion.sizeX, subRegion.sizeY, subRegion.sizeZ];

			WorldPosition offset = new WorldPosition(
				subRegion.min.x - fullRegion.min.x, 
				subRegion.min.y - fullRegion.min.y, 
				subRegion.min.z - fullRegion.min.z
				);

			for (int z = 0; z < subRegion.sizeZ; ++z)
			{
				for (int y = 0; y < subRegion.sizeY; ++y)
				{
					Array.Copy(fullData, 
					           offset.x + fullRegion.sizeX * (y + offset.y + fullRegion.sizeY * (z + offset.z)),
					           subset,
					           subRegion.sizeX * (y + subRegion.sizeY * z),
					           subRegion.sizeX);
				}
			}

			return subset;
		}

		return fullData;
	}	
}
