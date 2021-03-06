﻿using UnityEngine;
using UnityEngine.PostProcessing.Utilities;

public enum Quality
{
	Low,
	Medium,
	High,
	Ultra
}

public enum InterpolationLevel
{
	Off,
	Low,
	Normal,
	High
}
	

public class Config : MonoBehaviour 
{
	int chunkDeleteRadius;
	int chunkLoadRadius;
	int despawnRadius;
	int maxItemSpawns;
	float chunkDeleteTiming = 0.1f;
	bool swapInputs = false;
	int interpolationFactor = 1;

	public Camera mainCamera;

	public int startChunksToLoad;
	public int spawnRadius;
	public float spawnTiming;
	public float spawnDelay;
	public float blockSpawnChance;
	public float fogScale;
	public float pickupMaxWeight;
	public float ballMaxWeight;
	public bool outlines;
	public bool coloredOutlines;
	public bool shadowsEnabled;
	public bool contactShadows;
	public bool globalFogEnabled;
	public bool atmosphericScattering;
	public int maxRenderDistance;
	public int maxOutlineDistance;
	public int maxSmallObjectCount = 1000;
	public int maxLargeObjectCount = 100;
	public int musicVolume = 100;
	public int sfxVolume = 100;

	private int coroutineTiming = 12000;

	public Quality graphicsMode;
	public InterpolationLevel interpolation;

    PostProcessingController post;

	public static int WorldHeight = 4;

	// Serializable game configuration object
	public static GameConfig Settings;
	public static NoiseSettings Noise;
    public static WorldSettings Instance;

	public static int WorldSize
	{
		get 
		{ 
			return Settings.worldSize;
		}
		set 
		{
			if (_instance != null)
			{
				// start chunks
				StartChunksToLoad = 128 * Mathf.FloorToInt((value - 2f) * 0.25f);
				// set fog scale
				float baseFog = 1.6f;

				Config.FogScale = baseFog - (value * 0.0333f);

				// Load chunks out in a radius of 3 world sizes
				_instance.chunkLoadRadius = 8;

				// Delete the chunks that fall outside this
				// TODO: detect memory usage to trigger deletion
				_instance.chunkDeleteRadius = value * 5;

				// Despawn radius is 3/4 the world size. Spawn radius is always 1.
				_instance.despawnRadius = value - Mathf.CeilToInt(value / 4f);
				_instance.spawnRadius = 1;

				// The maximum render distance is the world size * chunk size
				_instance.maxRenderDistance = value * Chunk.Size;

				_instance.maxOutlineDistance = _instance.maxRenderDistance - Mathf.FloorToInt(Chunk.Size * value * 0.25f);

				Settings.worldSize = value;
			}
		}
	}

	public static Quality QualityLevel
	{
		get 
		{ 
			return (Quality)Settings.quality; 
		}
		set 
		{ 
			if (_instance != null)
			{
				Settings.quality = (int)value; 

				if (value == Quality.Low)
				{
					MaxItemSpawns = 10;
					MaxLargeObjectCount = 12;
					MaxSmallObjectCount = 45;
					BlockSpawnChance = 0.05f;
					ContactShadows = false;
					GlobalFogEnabled = false;
					ShadowsEnabled = false;
					Outlines = false;
					World.ChunkShadows = false;

                    _instance.post.enableAntialiasing = false;
                    _instance.post.enableBloom = false;
                    _instance.post.enableDepthOfField = false;
                    _instance.post.enableEyeAdaptation = false;
                    _instance.post.enableMotionBlur = false;


                    if (QualitySettings.GetQualityLevel() != 0)
					{
						QualitySettings.SetQualityLevel(0);
					}
				}

				if (value == Quality.Medium)
				{
					MaxItemSpawns = 20;
					MaxLargeObjectCount = 15;
					MaxSmallObjectCount = 75;
					BlockSpawnChance = 0.05f;
					ContactShadows = true;
					GlobalFogEnabled = false;
					ShadowsEnabled = false;
					Outlines = false;
					World.ChunkShadows = false;

                    _instance.post.enableAntialiasing = true;
                    _instance.post.enableBloom = true;
                    _instance.post.enableDepthOfField = false;
                    _instance.post.enableEyeAdaptation = true;
                    _instance.post.enableMotionBlur = false;

                    if (QualitySettings.GetQualityLevel() != 1)
					{
						QualitySettings.SetQualityLevel(1);
					}
				}

				if (value == Quality.High)
				{
					MaxItemSpawns = 30;
					MaxLargeObjectCount = 20;
					MaxSmallObjectCount = 100;
					BlockSpawnChance = 0.05f;
					ContactShadows = true;
					GlobalFogEnabled = false;
					ShadowsEnabled = true;
					Outlines = false;
					World.ChunkShadows = true;

                    _instance.post.enableAntialiasing = true;
                    _instance.post.enableBloom = true;
                    _instance.post.enableDepthOfField = true;
                    _instance.post.enableEyeAdaptation = true;
                    _instance.post.enableMotionBlur = true;

                    Interpolation = InterpolationLevel.Low;
					if (QualitySettings.GetQualityLevel() != 2)
					{
						QualitySettings.SetQualityLevel(2);
					}
				}

				if (value == Quality.Ultra)
				{
					MaxItemSpawns = 50;
					MaxLargeObjectCount = 30;
					MaxSmallObjectCount = 150;
					BlockSpawnChance = 0.05f;
					ContactShadows = true;
					GlobalFogEnabled = false;
					ShadowsEnabled = true;
					Outlines = false;
					World.ChunkShadows = true;

                    _instance.post.enableAntialiasing = true;
                    _instance.post.enableBloom = true;
                    _instance.post.enableDepthOfField = true;
                    _instance.post.enableEyeAdaptation = true;
                    _instance.post.enableMotionBlur = true;

                    if (QualitySettings.GetQualityLevel() != 3)
					{
						QualitySettings.SetQualityLevel(3);
					}
				}
			}

		}
	}

	public static InterpolationLevel Interpolation
	{
		get { return (InterpolationLevel)Settings.interpolation; }
		set { Settings.interpolation = (int)value; }
	}

	public static int SampleRate
	{
		get 
		{
			if (Interpolation == InterpolationLevel.High)
			{
				return 4;
			}
			if (Interpolation == InterpolationLevel.Normal)
			{
				return 2;
			}
			else return 1;
		}
	}

	// World Size Settings
	public static int ChunkDeleteRadius
	{
		get  { return _instance.chunkDeleteRadius; }
	}

	public static int ChunkLoadRadius
	{
		get { return _instance.chunkLoadRadius; }
	}

	public static int DespawnRadius
	{
		get { return _instance.despawnRadius; }
	}

	public static int SpawnRadius
	{
		get { return _instance.spawnRadius; }
	}

	public static int MaxRenderDistance
	{
		get { return _instance.maxRenderDistance; }
	}

	// Spawn Settings
	public static int MaxItemSpawns
	{
		get { return _instance.maxItemSpawns; }
		set { _instance.maxItemSpawns = value; }
	}

	public static int MaxSmallObjectCount
	{
		get { return _instance.maxSmallObjectCount; }
		set { _instance.maxSmallObjectCount = value; }
	}

	public static int MaxLargeObjectCount
	{
		get { return _instance.maxLargeObjectCount; }
		set { _instance.maxLargeObjectCount = value; }
	}

	public static float SpawnTiming
	{
		get { return _instance.spawnTiming; }
		set { _instance.spawnTiming = value; }
	}

	public static float SpawnDelay
	{
		get { return _instance.spawnDelay; }
		set { _instance.spawnDelay = value; }
	}

	public static int SpawnIntensity
	{
		get { return Settings.spawnIntensity; }
		set { Settings.spawnIntensity = value; }
	}

	public static float BlockSpawnChance
	{
		get { return _instance.blockSpawnChance; }
		set { _instance.blockSpawnChance = value; }
	}

	// Chunk Loading Settings
	public static float ChunkDeleteTiming
	{
		get { return _instance.chunkDeleteTiming; }
	}

	public static int StartChunksToLoad
	{
		get { return _instance.startChunksToLoad;}
		private set { _instance.startChunksToLoad = value; }
	}

	// Controls
	public static bool SwapInputs
	{
		get { return Settings.swapInputs; }
		set { Settings.swapInputs = value; }
	}

	public static int MusicVolume
	{
		get { return Settings.musicVol; }
		set 
		{
			 Settings.musicVol = value; 
		}
	}

	public static int SfxVolume
	{
		get { return Settings.sfxVol; }
		set { Settings.sfxVol = value; }
	}

	// Graphics Settings
	public static float FogScale
	{
		get { return _instance.fogScale; }
		set { _instance.fogScale = value; }
	}

	public static bool ContactShadows
	{
		get { return _instance.contactShadows;}
		set { _instance.contactShadows = value; }
	}

	public static bool Outlines
	{
		get { return _instance.outlines; }
		set { _instance.outlines = value; }
	}

	public static bool GlobalFogEnabled
	{
		get { return _instance.globalFogEnabled;}
		set { _instance.globalFogEnabled = value; }
	}

	public static bool ShadowsEnabled
	{
		get { return _instance.shadowsEnabled;}
		set { _instance.shadowsEnabled = value; }
	}

	public static float PickupMaxWeight
	{
		get { return _instance.pickupMaxWeight; }
		set { _instance.pickupMaxWeight = value; }
	}

	public static float BallMaxWeight
	{
		get { return _instance.ballMaxWeight; }
		set { _instance.ballMaxWeight = value; }
	}

	public static int CoroutineTiming
	{
		get { return Settings.coroutineTiming; }
		set { Settings.coroutineTiming = value; }
	}

	public static bool Multithreaded
	{
		get { return Settings.multithreaded; }
	}

	public static string Resolution
	{
		get { return Settings.resolution; }
		set { Settings.resolution = value; }
	}

	public static string StartTime
	{
		get { return Settings.startTime; }
		set { Settings.startTime = value; }
	}

	public static Config _instance;

	void Start()
	{
        //DontDestroyOnLoad(gameObject);
        post = Game.MainCamera.GetComponent<PostProcessingController>();

		if (_instance == null)
		{
			_instance = this;
		}
	}
}
