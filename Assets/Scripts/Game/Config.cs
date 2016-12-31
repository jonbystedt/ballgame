using UnityEngine;

public enum GraphicsMode
{
	Low,
	Medium,
	High,
	Ultra
}
	

public class Config : MonoBehaviour 
{
	int worldSize = 8;
	int mountainBase = 6;
	int chunkDeleteRadius;
	int chunkLoadRadius;
	int despawnRadius;
	int maxItemSpawns;
	float chunkDeleteTiming = 0.1f;
	bool swapInputs = false;

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

	private int coroutineTiming = 12000;

	public GraphicsMode graphicsMode;

	public static int WorldHeight = 4;

	public static int WorldSize
	{
		get 
		{ 
			if (_instance != null) 
			{
				return _instance.worldSize; 
			}
			else
			{
				return 8;
			}
		}
		set 
		{
			if (_instance != null)
			{
				// Load chunks out in a radius of 3 world sizes
				_instance.chunkLoadRadius = value + Mathf.FloorToInt(value / 2f);

				// Delete the chunks that fall outside this
				_instance.chunkDeleteRadius = value * 10;

				// Despawn radius is 2/3 the world size. Spawn radius is always 1.
				_instance.despawnRadius = value - Mathf.CeilToInt(value / 3f);
				_instance.spawnRadius = 1;

				// The maximum render distance is the world size * chunk size
				_instance.maxRenderDistance = value * Chunk.Size;

				_instance.maxOutlineDistance = _instance.maxRenderDistance - Mathf.FloorToInt(Chunk.Size * value * 0.25f);
			}
		}
	}

	public static int MountainBase
	{
		get { return _instance.mountainBase; }
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
		get { return _instance.swapInputs; }
		set { _instance.swapInputs = value; }
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

	public static bool ColoredOutlines
	{
		get { return _instance.coloredOutlines; }
		set { _instance.coloredOutlines = value; }
	}

	public static bool AtmosphericScattering
	{
		get { return _instance.atmosphericScattering;}
		set { _instance.atmosphericScattering = value; }
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
		get { return _instance.coroutineTiming; }
		set { _instance.coroutineTiming = value; }
	}

	public static GraphicsMode GraphicsMode
	{
		get { if (_instance != null) return _instance.graphicsMode; else return GraphicsMode.Medium; }
		set 
		{ 
			if (_instance != null)
			{
				_instance.graphicsMode = value; 

				if (value == GraphicsMode.Low)
				{
					MaxItemSpawns = 100;
					BlockSpawnChance = 0.05f;
					ContactShadows = false;
					GlobalFogEnabled = false;
					FogScale = 1.1f;
					AtmosphericScattering = false;
					ShadowsEnabled = false;
					Outlines = false;
					ColoredOutlines = false;
					if (QualitySettings.GetQualityLevel() != 0)
					{
						QualitySettings.SetQualityLevel(0);
					}
					WorldSize = 10;
				}

				if (value == GraphicsMode.Medium)
				{
					MaxItemSpawns = 100;
					BlockSpawnChance = 0.05f;
					FogScale = 1.0f;
					ContactShadows = false;
					GlobalFogEnabled = true;
					AtmosphericScattering = false;
					ShadowsEnabled = false;
					Outlines = false;
					ColoredOutlines = false;
					if (QualitySettings.GetQualityLevel() != 1)
					{
						QualitySettings.SetQualityLevel(1);
					}
					WorldSize = 12;
				}

				if (value == GraphicsMode.High)
				{
					MaxItemSpawns = 100;
					BlockSpawnChance = 0.05f;
					FogScale = 0.9f;
					ContactShadows = false;
					GlobalFogEnabled = true;
					AtmosphericScattering = true;
					ShadowsEnabled = true;
					Outlines = false;
					ColoredOutlines = false;
					if (QualitySettings.GetQualityLevel() != 2)
					{
						QualitySettings.SetQualityLevel(2);
					}
					WorldSize = 14;
				}

				if (value == GraphicsMode.Ultra)
				{
					MaxItemSpawns = 100;
					BlockSpawnChance = 0.05f;
					FogScale = 0.8f;
					ContactShadows = false;
					GlobalFogEnabled = true;
					AtmosphericScattering = true;
					ShadowsEnabled = true;
					Outlines = false;
					ColoredOutlines = false;
					if (QualitySettings.GetQualityLevel() != 3)
					{
						QualitySettings.SetQualityLevel(3);
					}
					WorldSize = 16;
				}
			}

		}
	}

	public static Config _instance;

	void Start()
	{
		DontDestroyOnLoad(gameObject);

		if (_instance == null)
		{
			_instance = this;
		}
	}
}
