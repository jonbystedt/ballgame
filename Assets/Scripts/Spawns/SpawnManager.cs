using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public enum Spawns 
{
	Pickup,
	SilverPickup,
	BlackPickup,
	BouncyBall,
	SuperBouncyBall,
	MysteryEgg,
	Moon,
	DarkStar
}

public struct SpawnMap
{
	public int[,] value;
	public int[,] height;
	public int[,] frequency;
	public int[,] intensity;

	public SpawnMap(int size)
	{
		value = new int[size, size];
		height = new int[size, size];
		frequency = new int[size, size];
		intensity = new int[size, size];
	}
}

public class SpawnManager : MonoBehaviour {

	public static SpawnManager Spawn = null;

	public PooledObject pickup;
	public PooledObject bouncyBall;
	public PooledObject superBouncyBall;
	public PooledObject mysteryEgg;
	public PooledObject darkStar;
	public PooledObject silverPickup;
	public PooledObject moon;
	public PooledObject blackPickup;

	public Dictionary<int, ParticleSystem.MinMaxGradient> fireworksGradients = new Dictionary<int, ParticleSystem.MinMaxGradient>();

	private Dictionary<Spawns,PooledObject> objects = new Dictionary<Spawns,PooledObject>();

	public List<Pickup> Pickups = new List<Pickup>();

	public static List<Pickup> SleptPickups = new List<Pickup>();
	public static List<BouncyBall> SleptBalls = new List<BouncyBall>();

	Vector3 rotation = new Vector3 (15, 30, 45);

	void Awake()
	{
		if (Spawn == null)
		{
			Spawn = this;
		}

		objects.Add (Spawns.Pickup, pickup);
		objects.Add (Spawns.BouncyBall, bouncyBall);
		objects.Add (Spawns.SuperBouncyBall, superBouncyBall);
		objects.Add (Spawns.MysteryEgg, mysteryEgg);
		objects.Add (Spawns.SilverPickup, silverPickup);
		objects.Add (Spawns.Moon, moon);
		objects.Add (Spawns.BlackPickup, blackPickup);
		objects.Add (Spawns.DarkStar, darkStar);
	}

	public void Initialize()
	{
		PooledObject obj;
		for (int i = 0; i < Config.MaxSmallObjectCount; i++)
		{
            pickup.SetPoolSize(Config.MaxSmallObjectCount);
			obj = pickup.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            silverPickup.SetPoolSize(Config.MaxSmallObjectCount);
			obj = silverPickup.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            blackPickup.SetPoolSize(Config.MaxSmallObjectCount);
			obj = blackPickup.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            bouncyBall.SetPoolSize(Config.MaxSmallObjectCount);
			obj = bouncyBall.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();
		}

		for (int i = 0; i < Config.MaxLargeObjectCount; i++)
		{
            superBouncyBall.SetPoolSize(Config.MaxLargeObjectCount);
			obj = superBouncyBall.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            mysteryEgg.SetPoolSize(Config.MaxLargeObjectCount);
			obj = mysteryEgg.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            moon.SetPoolSize(Config.MaxLargeObjectCount);
			obj = moon.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();

            darkStar.SetPoolSize(Config.MaxLargeObjectCount);
			obj = darkStar.GetPooledInstance<PooledObject>();
			obj.ReturnToPool();
		}
	}

	void Update()
	{
		RotatePickups();
	}

	SpawnedObject SpawnPickup(PooledObject prefab, Color color)
	{
		Pickup pickup = prefab.GetPooledInstance<Pickup>();
		if (pickup == null && SleptPickups.Count > 0)
		{
			Pickup sleeper = SleptPickups[0];
			SleptPickups.RemoveAt(0);
			if (sleeper != null)
			{
				sleeper.ReturnToPool();
				pickup = prefab.GetPooledInstance<Pickup>();
			}
		}
		if (pickup == null)
		{
			return null;
		}

		pickup.type = ((Pickup)prefab).type;
		pickup.isLive = true;
		pickup.inRange = true;
		Pickups.Add(pickup);

		if (pickup.type == PickupType.Basic)
		{
			pickup.color = Color.Lerp(Tile.Brighten(color, 0.7f), Color.white, 0.1f);
			pickup.size = 0.5f;
			pickup.rotationSpeed = 2f;
			pickup.baseScore = 10;
			pickup.driftIntensity = 1000f;
			pickup.drift = true;
			pickup.baseColor = color;
		}
		if (pickup.type == PickupType.Silver)
		{
			pickup.color = Color.Lerp(Tile.Brighten(color,0.3f), Color.white, 0.5f);
			pickup.size = 1f;
			pickup.baseScore = 50;
			pickup.driftIntensity = 20000f;
			pickup.drift = true;
			pickup.rotationSpeed = 8f;
			pickup.baseColor = color;
		}
		if (pickup.type == PickupType.Black)
		{
			pickup.color = Color.Lerp(Tile.Brighten(color,0.3f), Color.black, 0.25f);
			pickup.size = 1f;
			pickup.baseScore = 25;
			pickup.driftIntensity = 10000f;
			pickup.drift = true;
			pickup.rotationSpeed = 4f;
			pickup.baseColor = color;
		}

		return (SpawnedObject)pickup;
	}

	SpawnedObject SpawnBall(PooledObject prefab, Color color)
	{
		BouncyBall ball = prefab.GetPooledInstance<BouncyBall>();
		if (ball == null && SleptBalls.Count > 0)
		{
			BouncyBall sleeper = SleptBalls[0];
			SleptBalls.RemoveAt(0);
			if (sleeper != null)
			{
				sleeper.ReturnToPool();
				ball = prefab.GetPooledInstance<BouncyBall>();
			}
		}
		if (ball == null)
		{
			return null;
		}

		ball.exploding = false;
		ball.inRange = true;
		ball.type = ((BouncyBall)prefab).type;
		ball.color = color;

		if (ball.type == BallType.Basic)
		{
			ball.size = 0.95f;
			ball.scoreModifier = 1;
			ball.growthRate = 0;
			ball.shrinkRate = 0;
			ball.massIncrease = 0;
			ball.massDecrease = 0;
			ball.maxSize = 0.95f;
			ball.minSize = 0.95f;
			ball.explodeAtMax = false;
			ball.explodeAtMin = false;
		}
		if (ball.type == BallType.Imploding)
		{
			ball.size = 1.8f;
			ball.scoreModifier = 3;
			ball.growthRate = 1.1f;
			ball.shrinkRate = 0.8f;
			ball.massIncrease = 1.075f;
			ball.massDecrease = 0.98f;
			ball.maxSize = 6f;
			ball.minSize = 0.85f;
			ball.explodeAtMax = false;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BlackPickup;
			ball.SpawnIncrement = 4f;
			ball.SpawnValue = 16f;
		}
		if (ball.type == BallType.Exploding)
		{
			ball.size = 4f;
			ball.scoreModifier = 5;
			ball.growthRate = 1.25f;
			ball.shrinkRate = 0.95f;
			ball.massIncrease = 1.01f;
			ball.massDecrease = 0.8f;
			ball.maxSize = 14f;
			ball.minSize = 1.6f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = false;
			ball.SpawnObject = Spawns.SilverPickup;
			ball.SpawnIncrement = 4f;
			ball.SpawnValue = 16f;
		}
		if (ball.type == BallType.Moon)
		{
			ball.size = 7f;
			ball.scoreModifier = 9;
			ball.growthRate = 1.2f;
			ball.shrinkRate = 0.9f;
			ball.massIncrease = 1.01f;
			ball.massDecrease = 1.05f; //!
			ball.maxSize = 14f;
			ball.minSize = 2f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BouncyBall;
			ball.SpawnIncrement = 10f;
			ball.SpawnValue = 30f;

			ball.emission = color;
		}
		if (ball.type == BallType.DarkStar)
		{
			ball.size = 7f;
			ball.scoreModifier = 7;
			ball.growthRate = 1.1f;
			ball.shrinkRate = 0.85f;
			ball.massIncrease = 1.1f;
			ball.massDecrease = 1.01f; //!
			ball.maxSize = 14f;
			ball.minSize = 2f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BouncyBall;
			ball.SpawnIncrement = 10f;
			ball.SpawnValue = 30f;

			ball.emission = color;
		}

		return (SpawnedObject)ball;
	}

	public PooledObject Object(Spawns o, Color color, float mass, Vector3 pos)
	{
		PooledObject prefab;
		SpawnedObject spawnedObject = null;
		if (objects.TryGetValue (o, out prefab))
		{
			if (o == Spawns.Pickup || o == Spawns.SilverPickup || o == Spawns.BlackPickup)
			{
				spawnedObject = SpawnPickup(prefab, color);
			}
			else
			{
				spawnedObject = SpawnBall(prefab, color);
			}
			if (spawnedObject == null)
			{
				return null;
			}

			spawnedObject.transform.position = pos;
			spawnedObject.mass = mass;
			spawnedObject.StartSlowUpdate();
		}

		return spawnedObject.GetComponent<PooledObject>();
	}

	public PooledObject Object(Spawns o, Color color, Vector3 pos)
	{
		return Object(o, color, 0f, pos);
	}
		
	// Spawn with delay, add to list, set color, set mass
	public void Objects(Spawns o, Color color, float mass, Vector3 pos, int count, float delay, List<PooledObject> spawns)
	{
		StartCoroutine(Wait(delay, () => {
			StartCoroutine(Repeat(count, Config.SpawnDelay, () => {
				PooledObject obj = Object(o, color, mass, pos);
				if (obj != null)
				{
					spawns.Add(obj);
				}
			}));
		}));
	}

	// Spawn with delay, add to list, set color
	public void Objects(Spawns o, Color color, Vector3 pos, int count, float delay)
	{
		Column column = World.GetColumn(new WorldPosition(pos));
		if (column != null && column.spawns != null)
		{
			Objects(o, color, 0f, pos, count, delay, column.spawns);
		}	
	}
	public void Objects(Spawns o, Color color, Vector3 pos, int count, float delay, List<PooledObject> spawns)
	{
		Objects(o, color, 0f, pos, count, delay, spawns);
	}

	public void Objects(Spawns o, Color color, WorldPosition pos, float height, int count, float delay, List<PooledObject> spawns)
	{
		Objects(o, color, new Vector3(pos.x, pos.y + height, pos.z), count, delay, spawns);
	}

	public void Objects(Spawns o, Color color, float mass, WorldPosition pos, float height, int count, float delay, List<PooledObject> spawns)
	{
		Objects(o, color, mass, new Vector3(pos.x, pos.y + height, pos.z), count, delay, spawns);
	}

	public void SpawnColumn(WorldPosition pos, Region region, List<PooledObject> spawns)
	{
		StartCoroutine(SpawnRoutine(pos, region, spawns));
	}

	IEnumerator SpawnRoutine(WorldPosition pos, Region region, List<PooledObject> spawns)
	{
		SampleSet sampleSet = InterpolatedNoise.Results[region];

		int totalRange = NoiseConfig.spawnTypes.scale;
		int floor = Mathf.FloorToInt(Mathf.Lerp(0f, totalRange / 5f, Config.SpawnIntensity / 100f));
		int exclusion = 0;
		int range;
		int upper;
		int lower;
        float weight;
        float intensity;
		float power = Mathf.Lerp(1f, 7f, 1f - (Config.SpawnIntensity / 100f));

		for (int x = 0; x < Chunk.Size; x++)
		{
			for (int z = 0; z < Chunk.Size; z++)
			{
				int spawnValue = sampleSet.spawnMap.value[x, z];

				if (spawnValue != Chunk.NoSpawn)
				{

                    intensity = Mathf.Pow(sampleSet.spawnMap.intensity[x, z] / (float)NoiseConfig.spawnIntensity.scale, power);
					exclusion = Mathf.FloorToInt(Mathf.Lerp(totalRange - floor, 0, intensity));
					range = totalRange - exclusion;

					// Pickups
					upper = Mathf.FloorToInt(range * 0.006f);
					lower = 0;
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
                        weight = ((float)(spawnValue - (exclusion + lower)) / (float)(upper - lower)) * (float)Config.PickupMaxWeight;

						Spawn.Objects(
							Spawns.Pickup, 
							Tile.Colors[17 + (spawnValue % 32)],
							weight,
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							0.5f, 
							sampleSet.spawnMap.frequency[x, z] * 3,
							1f,
							spawns
						);

						continue;
					}

					// Bouncy Balls
					upper = Mathf.FloorToInt(range * 0.1075f);
					lower = Mathf.FloorToInt(range * 0.1f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
                        weight = ((float)(spawnValue - (exclusion + lower)) / (float)(upper - lower)) * Config.BallMaxWeight;

						Spawn.Objects(
							Spawns.BouncyBall, 
							Tile.Colors[32 + (spawnValue % 32)],
							weight,
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							1.5f,
							sampleSet.spawnMap.frequency[x, z] * 3,
							1f,
							spawns
						);

						continue;
					}

					// Exploding Balls
					upper = Mathf.FloorToInt(range * 0.302f);
					lower = Mathf.FloorToInt(range * 0.3f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.SuperBouncyBall, 
							Color.Lerp(
								Tile.Lighten(Tile.Brighten(Tile.Colors[63],0.9f), 0.45f),
								Tile.Colors[spawnValue % 64],
								0.35f
							),
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							5f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns
						);

						continue;
					}

					// Imploding Balls
					upper = Mathf.FloorToInt(range * 0.402f);
					lower = Mathf.FloorToInt(range * 0.4f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.MysteryEgg,
							Color.Lerp(
								Tile.Darken(Tile.Brighten(Tile.Colors[33], 1f), 0.45f),
								Tile.Colors[spawnValue % 64],
								0.35f
							), 
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns
						);

						continue;
					}

					// Moons
					upper = Mathf.FloorToInt(range * 0.502f);
					lower = Mathf.FloorToInt(range * 0.5f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.Moon, 
							Color.white,
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns
						);

						continue;
					}
					upper = Mathf.FloorToInt(range * 0.602f);
					lower = Mathf.FloorToInt(range * 0.6f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.DarkStar, 
							Color.black,
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns
						);

						continue;
					}

					yield return new WaitForSeconds(Config.SpawnTiming);
				}
			}
		}
	}

	void RotatePickups()
	{
		for(int i = Pickups.Count - 1; i >= 0; i--)
		{
			Pickup pickup = Pickups[i];

			if (!pickup.inRange)
			{
				continue;
			}

			if (pickup != null && pickup.isActive)
			{
				pickup.Rotate(rotation);
			}
			else
			{
				Pickups.Remove(pickup);
			}
		}
	}

	IEnumerator Repeat(int count, float delay, Action callback)
	{
		for (int i = 0; i < count; i++)
		{
			callback();
			yield return new WaitForSeconds(delay);
		}
	}

	IEnumerator Wait(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback();
	}
}
