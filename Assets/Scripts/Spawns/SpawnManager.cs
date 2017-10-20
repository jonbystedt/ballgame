using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ProceduralToolkit;

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

public enum Note
{
	C1, Cs1, D1, Eb1, E1, F1, Fs1, G1, Gs1, A1, Bb1, B1,
	C2, Cs2, D2, Eb2, E2, F2, Fs2, G2, Gs2, A2, Bb2, B2,
	C3, Cs3, D3, Eb3, E3, F3, Fs3, G3, Gs3, A3, Bb3, B3,
	C4, Cs4, D4, Eb4, E4, F4, Fs4, G4, Gs4, A4, Bb4, B4,
	C5, Cs5, D5, Eb5, E5, F5, Fs5, G5, Gs5, A5, Bb5, B5
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

	public static List<Pickup> Pickups = new List<Pickup>();
	public static List<BouncyBall> Balls = new List<BouncyBall>();

	public static List<Pickup> SleptPickups = new List<Pickup>();
	public static List<BouncyBall> SleptBalls = new List<BouncyBall>();

	public AudioClip[] pickupOctave;
	public AudioClip[] ballOctave;
	public AudioClip[] largeBallOctave;
	public AudioClip[] selfBallOctave;
	public AudioClip[] scoreOctave;
	public AudioClip[] score2Octave;
	public AudioClip[] moonOctave;
	public AudioClip[] bigPickupOctave;
	public AudioClip[] darkPickupOctave;
	public AudioClip[] largeDarkOctave;
	public AudioClip[] darkStarOctave;
	public AudioClip[] selfHits;

	Dictionary<int, Note[]> scales = new Dictionary<int, Note[]>();
	int key = -1;

	Vector3 rotation = new Vector3 (15, 30, 45);
	int bix = 0;

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

	public void Initialize(bool create)
	{
		Pickups = new List<Pickup>();
		Balls = new List<BouncyBall>();
		SleptPickups = new List<Pickup>();
		SleptBalls = new List<BouncyBall>();

		pickup.SetPoolSize(Config.MaxSmallObjectCount);
		silverPickup.SetPoolSize(Config.MaxSmallObjectCount);
		blackPickup.SetPoolSize(Config.MaxSmallObjectCount);
		bouncyBall.SetPoolSize(Config.MaxSmallObjectCount);

		superBouncyBall.SetPoolSize(Config.MaxLargeObjectCount);
		mysteryEgg.SetPoolSize(Config.MaxLargeObjectCount);		
		moon.SetPoolSize(Config.MaxLargeObjectCount);
		darkStar.SetPoolSize(Config.MaxLargeObjectCount);

		if (scales.Count == 0)
		{
			PopulateScale();
		}


		if (create)
		{
			PooledObject obj;
			for (int i = 0; i < Config.MaxSmallObjectCount; i++)
			{
				obj = pickup.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();

				obj = silverPickup.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();

				obj = blackPickup.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();

				obj = bouncyBall.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();
			}

			for (int i = 0; i < Config.MaxLargeObjectCount; i++)
			{
				obj = superBouncyBall.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();
				
				obj = mysteryEgg.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();

				obj = moon.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();
				
				obj = darkStar.GetPooledInstance<PooledObject>();
				obj.ReturnToPool();
			}
		}
		else
		{
			ObjectPool.GetPool(bouncyBall).Wipe();
			ObjectPool.GetPool(superBouncyBall).Wipe();
			ObjectPool.GetPool(mysteryEgg).Wipe();
			ObjectPool.GetPool(moon).Wipe();
			ObjectPool.GetPool(darkStar).Wipe();
		}
	}

	void Update()
	{
		HandlePickups();
		HandleBalls();
		key = -1;
	}

	SpawnedObject SpawnPickup(PooledObject prefab, Color color, int key, float corruption)
	{
		Pickup pickup = prefab.GetPooledInstance<Pickup>();
		if (pickup == null && SleptPickups.Count > 0)
		{
			Pickup sleeper = SleptPickups[0];
			Pickups.Remove(sleeper);
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
			pickup.driftIntensity = 2000f;
			pickup.hasAction = true;
			pickup.hsvColor = new ColorHSV(color);

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, pickup.hsvColor.h));
			//int scoreNote = Mathf.FloorToInt(Mathf.Lerp(0f, 23.999f, pickup.hsvColor.h));
			var playSound = pickup.GetComponent<PlayHitSound>();
			playSound.worldHitSound = pickupOctave[(int)(scales[key][note])];
			playSound.objectHitSound = pickupOctave[(int)(scales[key][note])];

			int scale = Mathf.FloorToInt(Mathf.Lerp(0f, 4.999f, pickup.hsvColor.h));
			playSound.scoreSound = score2Octave[key * 3 + scale];
		}
		if (pickup.type == PickupType.Silver)
		{
			pickup.color = Color.Lerp(Tile.Brighten(color,0.3f), Color.white, 0.5f);
			pickup.size = 1f;
			pickup.baseScore = 50;
			pickup.driftIntensity = 20000f;
			pickup.hasAction = true;
			pickup.rotationSpeed = 8f;
			pickup.hsvColor = new ColorHSV(color);

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, pickup.hsvColor.h));
			var playSound = pickup.GetComponent<PlayHitSound>();
			playSound.worldHitSound = pickupOctave[(int)scales[key][note]];
			playSound.objectHitSound = bigPickupOctave[(int)scales[key][note]];

			int scale = Mathf.FloorToInt(Mathf.Lerp(0f, 4.999f, pickup.hsvColor.h));
			playSound.scoreSound = scoreOctave[key * 5 + scale];
		}
		if (pickup.type == PickupType.Black)
		{
			pickup.color = Color.Lerp(Tile.Brighten(color,0.3f), Color.black, 0.35f);
			pickup.size = 1f;
			pickup.baseScore = 25;
			pickup.driftIntensity = 10000f;
			pickup.hasAction = true;
			pickup.rotationSpeed = 4f;
			pickup.hsvColor = new ColorHSV(color);

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, pickup.hsvColor.h));
			var playSound = pickup.GetComponent<PlayHitSound>();
			playSound.worldHitSound = pickupOctave[(int)scales[key][note]];
			playSound.objectHitSound = darkPickupOctave[(int)scales[key][note]];

			int scale = Mathf.FloorToInt(Mathf.Lerp(0f, 4.999f, pickup.hsvColor.h));
			playSound.scoreSound = scoreOctave[key * 5 + scale];
		}

		return (SpawnedObject)pickup;
	}

	SpawnedObject SpawnBall(PooledObject prefab, Color color, int key, float corruption)
	{
		BouncyBall ball = prefab.GetPooledInstance<BouncyBall>();
		if (ball == null && SleptBalls.Count > 0)
		{
			BouncyBall sleeper = SleptBalls[0];
			SleptBalls.RemoveAt(0);
			if (sleeper != null)
			{
				sleeper.ReturnToPool();
				Balls.Remove(sleeper);
				ball = prefab.GetPooledInstance<BouncyBall>();
			}
		}
		if (ball == null)
		{
			return null;
		}

		ball.exploding = false;
		ball.inRange = true;
		ball.hasAction = true;
		ball.type = ((BouncyBall)prefab).type;
		ball.hsvColor = new ColorHSV(color);
		Balls.Add(ball);

		if (ball.type == BallType.Basic)
		{
			ball.color = color;
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

			if (corruption > 0.1f)
			{
				ball.emission = Tile.Brighten(color, 1f);
			}

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, ball.hsvColor.h));
			int note2 = Mathf.FloorToInt(Mathf.Lerp(0f, 23.999f, ball.hsvColor.h));
			var playSound = ball.GetComponent<PlayHitSound>();
			playSound.worldHitSound = selfBallOctave[(int)scales[key][note]];
			playSound.objectHitSound = ballOctave[(int)scales[key][note2] + 11];
		}
		if (ball.type == BallType.Imploding)
		{
			ball.color = color;
			ball.size = 3f;
			ball.scoreModifier = 10;
			ball.growthRate = 1.1f;
			ball.shrinkRate = 0.8f;
			ball.massIncrease = 1.1f;
			ball.massDecrease = 0.98f;
			ball.maxSize = 6f;
			ball.minSize = 0.85f;
			ball.explodeAtMax = false;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BlackPickup;
			ball.SpawnIncrement = 1f;
			ball.SpawnValue = 4f;

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, ball.hsvColor.h));
			var playSound = ball.GetComponent<PlayHitSound>();
			playSound.worldHitSound = selfBallOctave[(int)scales[key][note]];
			playSound.objectHitSound = largeDarkOctave[(int)scales[key][note]];
		}
		if (ball.type == BallType.Exploding)
		{
			ball.color = color;
			ball.size = 4f;
			ball.scoreModifier = 5;
			ball.growthRate = 1.25f;
			ball.shrinkRate = 0.95f;
			ball.massIncrease = 1.01f;
			ball.massDecrease = 0.98f;
			ball.maxSize = 14f;
			ball.minSize = 1.6f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = false;
			ball.SpawnObject = Spawns.SilverPickup;
			ball.SpawnIncrement = 1f;
			ball.SpawnValue = 4f;

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, ball.hsvColor.h));
			var playSound = ball.GetComponent<PlayHitSound>();
			playSound.worldHitSound = selfHits[(int)scales[key][note] + 11];
			playSound.objectHitSound = ballOctave[(int)scales[key][note] + 47];
		}
		if (ball.type == BallType.Moon)
		{
			ball.color = Color.white;
			ball.size = 7f;
			ball.scoreModifier = 2;
			ball.growthRate = 1.2f;
			ball.shrinkRate = 0.9f;
			ball.massIncrease = 1.01f;
			ball.massDecrease = 1.02f; 
			ball.maxSize = 14f;
			ball.minSize = 2f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BouncyBall;
			ball.SpawnIncrement = 2f;
			ball.SpawnValue = 10f;

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, ball.hsvColor.h));
			var playSound = ball.GetComponent<PlayHitSound>();
			playSound.worldHitSound = selfHits[(int)scales[key][note] + 23]; 
			playSound.objectHitSound = moonOctave[(int)scales[key][note]];

			ball.emission = Color.white;
		}
		if (ball.type == BallType.DarkStar)
		{
			ball.color = Color.black;
			ball.size = 7f;
			ball.scoreModifier = 4;
			ball.growthRate = 1.1f;
			ball.shrinkRate = 0.85f;
			ball.massIncrease = 1.1f;
			ball.massDecrease = 1.01f; //!
			ball.maxSize = 14f;
			ball.minSize = 2f;
			ball.explodeAtMax = true;
			ball.explodeAtMin = true;
			ball.SpawnObject = Spawns.BouncyBall;
			ball.SpawnIncrement = 2f;
			ball.SpawnValue = 10f;

			int note = Mathf.FloorToInt(Mathf.Lerp(0f, 6.999f, ball.hsvColor.h));
			var playSound = ball.GetComponent<PlayHitSound>();
			playSound.worldHitSound = selfHits[(int)scales[key][note]];
			playSound.objectHitSound = darkStarOctave[(int)scales[key][note]];

			ball.emission = Color.black;
		}

		return (SpawnedObject)ball;
	}

	public PooledObject Object(Spawns o, Color color, float mass, Vector3 pos, float corruption)
	{
		PooledObject prefab;
		SpawnedObject spawnedObject = null;
		if (objects.TryGetValue (o, out prefab))
		{
			if (key == -1) 
			{
				key = TerrainGenerator.GetNoise2D(pos, NoiseConfig.worldKey, NoiseType.SimplexValue);
			}

			if (o == Spawns.Pickup || o == Spawns.SilverPickup || o == Spawns.BlackPickup)
			{
				spawnedObject = SpawnPickup(prefab, color, key, corruption);
			}
			else
			{
				spawnedObject = SpawnBall(prefab, color, key, corruption);
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

	public PooledObject Object(Spawns o, Color color, Vector3 pos, float corruption)
	{
		return Object(o, color, 0f, pos, corruption);
	}
		
	// Spawn with delay, add to list, set color, set mass
	public void Objects(Spawns o, Color color, float mass, Vector3 pos, int count, float delay, List<PooledObject> spawns, float corruption)
	{
		StartCoroutine(Wait(delay, () => 
		{
			StartCoroutine(Repeat(count, Config.SpawnDelay, () => 
			{
				PooledObject obj = Object(o, color, mass, pos, corruption);

				if (obj != null)
				{
					spawns.Add(obj);
				}
			}));
		}));
	}

	// Spawn with delay, add to list, set color
	public void Objects(Spawns o, Color color, Vector3 pos, int count, float delay, float corruption)
	{
		Column column = World.GetColumn(new WorldPosition(pos));
		if (column != null && column.spawns != null)
		{
			Objects(o, color, 0f, pos, count, delay, column.spawns, corruption);
		}	
	}
	public void Objects(Spawns o, Color color, Vector3 pos, int count, float delay, List<PooledObject> spawns, float corruption)
	{
		Objects(o, color, 0f, pos, count, delay, spawns, corruption);
	}

	public void Objects(Spawns o, Color color, WorldPosition pos, float height, int count, float delay, List<PooledObject> spawns, float corruption)
	{
		Objects(o, color, new Vector3(pos.x, pos.y + height, pos.z), count, delay, spawns, corruption);
	}

	public void Objects(Spawns o, Color color, float mass, WorldPosition pos, float height, int count, float delay, List<PooledObject> spawns, float corruption)
	{
		Objects(o, color, mass, new Vector3(pos.x, pos.y + height, pos.z), count, delay, spawns, corruption);
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
					upper = Mathf.FloorToInt(range * 0.007f);
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
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}

					// Bouncy Balls
					upper = Mathf.FloorToInt(range * 0.16f);
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
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}

					// Exploding Balls
					upper = Mathf.FloorToInt(range * 0.3025f);
					lower = Mathf.FloorToInt(range * 0.3f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.SuperBouncyBall, 
							Color.Lerp(
								Tile.Lighten(Tile.Brighten(Tile.Colors[63 - (spawnValue % 8)],0.9f), 0.45f),
								Tile.Colors[spawnValue % 64],
								0.35f
							),
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							5f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}

					// Imploding Balls
					upper = Mathf.FloorToInt(range * 0.4025f);
					lower = Mathf.FloorToInt(range * 0.4f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.MysteryEgg,
							Color.Lerp(
								Tile.Darken(Tile.Brighten(Tile.Colors[33 - (spawnValue % 8)], 1f), 0.45f),
								Tile.Colors[spawnValue % 64],
								0.35f
							), 
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}

					// Moons
					upper = Mathf.FloorToInt(range * 0.5025f);
					lower = Mathf.FloorToInt(range * 0.5f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.Moon, 
							Tile.Colors[16 + (spawnValue % 16)],
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}
					upper = Mathf.FloorToInt(range * 0.6025f);
					lower = Mathf.FloorToInt(range * 0.6f);
					if (spawnValue >= exclusion + lower && spawnValue < exclusion + upper)
					{
						Spawn.Objects(
							Spawns.DarkStar, 
							Tile.Colors[spawnValue % 16],
							new WorldPosition(pos.x + x, sampleSet.spawnMap.height[x, z], pos.z + z), 
							10f,
							sampleSet.spawnMap.frequency[x, z],
							1f,
							spawns,
							0f
						);

						continue;
					}

					if (Game.PlayerActive) 
					{
						yield return new WaitForSeconds(Config.SpawnTiming);
					}
					else
					{
						break;
					}
				}
			}
		}
	}

	void HandlePickups()
	{
		for (int i = Pickups.Count - 1; i >= 0; i--)
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

	void HandleBalls()
	{
		if (SpawnManager.Balls.Count == 0)
		{
			return;
		}

		// find a ball
		Vector3 d = Vector3.zero;
		float distance = Mathf.Infinity;
		BouncyBall ball = null;

		if (bix <= 0 || bix > SpawnManager.Balls.Count - 1)
		{
			bix = SpawnManager.Balls.Count - 1;
		}
		for (; bix >= 0 ; bix--)
		{
			ball = SpawnManager.Balls[bix];

			if (!ball.inRange)
			{
				continue;
			}

			if (ball == null || !ball.isActive)
			{
				SpawnManager.Balls.Remove(ball);
			}
			else
			{
				break;
			}
		}

		if (ball == null)
		{
			return;
		}

		// find closest ball to this ball
		for (int i = SpawnManager.Balls.Count - 1; i >= 0; i--)
		{
			if (i == bix)
			{
				continue;
			}

			BouncyBall b = SpawnManager.Balls[i];

			if (!b.inRange)
			{
				continue;
			}

			if (b == null || !b.isActive)
			{
				SpawnManager.Balls.Remove(b);
			}

			if (distance > 4f)
			{
				d = b.transform.position - ball.transform.position;
				if (d.sqrMagnitude < distance)
				{
					ball.closest = b;
					distance = d.sqrMagnitude;
				}
			}
		}

		bix--;
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

	void PopulateScale()
	{
		// G
		scales.Add(0, new Note[]
		{
			Note.C1, Note.D1, Note.E1, Note.Fs1, Note.G1, Note.A1, Note.B1,
			Note.C2, Note.D2, Note.E2, Note.Fs2, Note.G2, Note.A2, Note.B2,
			Note.C3, Note.D3, Note.E3, Note.Fs3, Note.G3, Note.A3, Note.B3,
			Note.C4, Note.D4, Note.E4, Note.Fs4, Note.G4, Note.A4, Note.B4,
			Note.C5, Note.D5, Note.E5, Note.Fs5, Note.G5, Note.A5, Note.B5
		});

		// D
		scales.Add(1, new Note[]
		{
			Note.Cs1, Note.D1, Note.E1, Note.Fs1, Note.G1, Note.A1, Note.B1,
			Note.Cs2, Note.D2, Note.E2, Note.Fs2, Note.G2, Note.A2, Note.B2,
			Note.Cs3, Note.D3, Note.E3, Note.Fs3, Note.G3, Note.A3, Note.B3,
			Note.Cs4, Note.D4, Note.E4, Note.Fs4, Note.G4, Note.A4, Note.B4,
			Note.Cs5, Note.D5, Note.E5, Note.Fs5, Note.G5, Note.A5, Note.B5				
		});
			
		// A
		scales.Add(2, new Note[]
		{
			Note.Cs1, Note.D1, Note.E1, Note.Fs1, Note.Gs1, Note.A1, Note.B1,
			Note.Cs2, Note.D2, Note.E2, Note.Fs2, Note.Gs2, Note.A2, Note.B2,
			Note.Cs3, Note.D3, Note.E3, Note.Fs3, Note.Gs3, Note.A3, Note.B3,
			Note.Cs4, Note.D4, Note.E4, Note.Fs4, Note.Gs4, Note.A4, Note.B4,
			Note.Cs5, Note.D5, Note.E5, Note.Fs5, Note.Gs5, Note.A5, Note.B5
		});

		// E
		scales.Add(3, new Note[]
		{
			Note.Cs1, Note.Eb1, Note.E1, Note.Fs1, Note.Gs1, Note.A1, Note.B1,
			Note.Cs2, Note.Eb2, Note.E2, Note.Fs2, Note.Gs2, Note.A2, Note.B2,
			Note.Cs3, Note.Eb3, Note.E3, Note.Fs3, Note.Gs3, Note.A3, Note.B3,
			Note.Cs4, Note.Eb4, Note.E4, Note.Fs4, Note.Gs4, Note.A4, Note.B4,
			Note.Cs5, Note.Eb5, Note.E5, Note.Fs5, Note.Gs5, Note.A5, Note.B5
		});

		// B
		scales.Add(4, new Note[]
		{
			Note.Cs1, Note.Eb1, Note.E1, Note.Fs1, Note.Gs1, Note.Bb1, Note.B1,
			Note.Cs2, Note.Eb2, Note.E2, Note.Fs2, Note.Gs2, Note.Bb2, Note.B2,
			Note.Cs3, Note.Eb3, Note.E3, Note.Fs3, Note.Gs3, Note.Bb3, Note.B3,
			Note.Cs4, Note.Eb4, Note.E4, Note.Fs4, Note.Gs4, Note.Bb4, Note.B4,
			Note.Cs5, Note.Eb5, Note.E5, Note.Fs5, Note.Gs5, Note.Bb5, Note.B5
		});

		// F#
		scales.Add(5, new Note[]
		{
			Note.Cs1, Note.Eb1, Note.F1, Note.Fs1, Note.Gs1, Note.Bb1, Note.B1,
			Note.Cs2, Note.Eb2, Note.F2, Note.Fs2, Note.Gs2, Note.Bb2, Note.B2,
			Note.Cs3, Note.Eb3, Note.F3, Note.Fs3, Note.Gs3, Note.Bb3, Note.B3,
			Note.Cs4, Note.Eb4, Note.F4, Note.Fs4, Note.Gs4, Note.Bb4, Note.B4,
			Note.Cs5, Note.Eb5, Note.F5, Note.Fs5, Note.Gs5, Note.Bb5, Note.B5
		});
			
		// C#
		scales.Add(6, new Note[]
		{
			Note.C1, Note.Cs1, Note.Eb1, Note.F1, Note.Fs1, Note.Gs1, Note.Bb1,
			Note.C2, Note.Cs2, Note.Eb2, Note.F2, Note.Fs2, Note.Gs2, Note.Bb2,
			Note.C3, Note.Cs3, Note.Eb3, Note.F3, Note.Fs3, Note.Gs3, Note.Bb3,
			Note.C4, Note.Cs4, Note.Eb4, Note.F4, Note.Fs4, Note.Gs4, Note.Bb4,
			Note.C5, Note.Cs5, Note.Eb5, Note.F5, Note.Fs5, Note.Gs5, Note.Bb5
		});

		// G#
		scales.Add(7, new Note[]
		{
			Note.C1, Note.Cs1, Note.Eb1, Note.F1, Note.G1, Note.Gs1, Note.Bb1,
			Note.C2, Note.Cs2, Note.Eb2, Note.F2, Note.G2, Note.Gs2, Note.Bb2,
			Note.C3, Note.Cs3, Note.Eb3, Note.F3, Note.G3, Note.Gs3, Note.Bb3,
			Note.C4, Note.Cs4, Note.Eb4, Note.F4, Note.G4, Note.Gs4, Note.Bb4,
			Note.C5, Note.Cs5, Note.Eb5, Note.F5, Note.G5, Note.Gs5, Note.Bb5
		});

		// Eb
		scales.Add(8, new Note[]
		{
			Note.C1, Note.D1, Note.Eb1, Note.F1, Note.G1, Note.Gs1, Note.Bb1,
			Note.C2, Note.D2, Note.Eb2, Note.F2, Note.G2, Note.Gs2, Note.Bb2,
			Note.C3, Note.D3, Note.Eb3, Note.F3, Note.G3, Note.Gs3, Note.Bb3,
			Note.C4, Note.D4, Note.Eb4, Note.F4, Note.G4, Note.Gs4, Note.Bb4,
			Note.C5, Note.D5, Note.Eb5, Note.F5, Note.G5, Note.Gs5, Note.Bb5
		});

		// Bb
		scales.Add(9, new Note[]
		{
			Note.C1, Note.D1, Note.Eb1, Note.F1, Note.G1, Note.A1, Note.Bb1,
			Note.C2, Note.D2, Note.Eb2, Note.F2, Note.G2, Note.A2, Note.Bb2,
			Note.C3, Note.D3, Note.Eb3, Note.F3, Note.G3, Note.A3, Note.Bb3,
			Note.C4, Note.D4, Note.Eb4, Note.F4, Note.G4, Note.A4, Note.Bb4,
			Note.C5, Note.D5, Note.Eb5, Note.F5, Note.G5, Note.A5, Note.Bb5
		});

		// F
		scales.Add(10, new Note[]
		{
			Note.C1, Note.D1, Note.E1, Note.F1, Note.G1, Note.A1, Note.Bb1,
			Note.C2, Note.D2, Note.E2, Note.F2, Note.G2, Note.A2, Note.Bb2,
			Note.C3, Note.D3, Note.E3, Note.F3, Note.G3, Note.A3, Note.Bb3,
			Note.C4, Note.D4, Note.E4, Note.F4, Note.G4, Note.A4, Note.Bb4,
			Note.C5, Note.D5, Note.E5, Note.F5, Note.G5, Note.A5, Note.Bb5
		});

		// C
		scales.Add(11, new Note[] 
		{
			Note.C1, Note.D1, Note.E1, Note.F1, Note.G1, Note.A1, Note.B1,
			Note.C2, Note.D2, Note.E2, Note.F2, Note.G2, Note.A2, Note.B2,
			Note.C3, Note.D3, Note.E3, Note.F3, Note.G3, Note.A3, Note.B3,
			Note.C4, Note.D4, Note.E4, Note.F4, Note.G4, Note.A4, Note.B4,
			Note.C5, Note.D5, Note.E5, Note.F5, Note.G5, Note.A5, Note.B5
		});
	}
}
