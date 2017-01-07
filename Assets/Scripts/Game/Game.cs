﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityStandardAssets.ImageEffects;

public class Game : MonoBehaviour 
{
	public float restartDelay = 5f;

	public GameObject player;
	public GameObject HUD;

	public World world;
	public LoadChunks loader;
	public ShowPanels showPanels;
	public Cosmos cosmos;

	public EdgeDetection edgeDetect;
	public EdgeDetectionColor edgeDetectColor;
	HBAO hbao;
	GlobalFog fog;
	GlobalFog distanceFog;
	//GlobalFogExtended scatterFog;
	SunShafts godRays;

	public Camera mainCamera;
	public Camera cosmosCamera;
	public Light sun;
	public Light moon;
	public Image menuGlow;
	public GameObject afterburner;

	private Vector3 lastGoodPosition;
	private Vector3 cameraPosition;

	private bool playerActive = false;
	private bool showStats = false;
	private bool firstRun = true;
	private int chunksLoaded = 0;
	
	private Text scoreText;
	private Text logMessage;
	private Text dayText;
	private Text clockText;
	private Text positionText;

	private StartGame startGame;
	private CameraOperator camOp;
	private int score = 0;
	private int logCount = 2;

	static Game _instance;

	public static int ChunksLoaded
	{
		get { return _instance.chunksLoaded; }
		set { _instance.chunksLoaded = value; }
	}

	public static bool PlayerActive
	{
		get { return _instance.playerActive; }
		set 
		{ 
			if (value)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;				
			}
			_instance.playerActive = value; 
		}
	}

	public static bool ShowStats
	{
		get { return _instance.showStats; }
	}

	public static GameObject Player
	{
		get { return _instance.player; }
		set { _instance.player = value; }
	}

	public static LoadChunks Loader
	{
		get { return _instance.loader; }
		set { _instance.loader = value; }
	}

	public static CameraOperator CameraOp
	{
		get { return _instance.camOp; }
		set { _instance.camOp = value; }
	}

	public static Vector3 LastGoodPosition
	{
		get { return _instance.lastGoodPosition; }
		set { _instance.lastGoodPosition = value; }
	}

	public static Vector3 CameraPosition
	{
		get { return _instance.cameraPosition; }
		set { _instance.cameraPosition = value; }
	}

	public static Camera MainCamera
	{
		get { return _instance.mainCamera; }
	}

	public void Reset()
	{
		loader.Reset();
		world.Reset();
		TileFactory.Clear();
		GameUtils.SeedValue = 0;
		Serialization.Reset();

		player.GetComponent<Rigidbody>().isKinematic = true;
		PlayerActive = false;
		player.transform.position = new Vector3(-0.5f, 256f, -0.5f);

		menuGlow.color = Tile.Brighten(RenderSettings.fogColor, 0.5f);
		RenderSettings.fogColor = Tile.Brighten(Color.Lerp(RenderSettings.fogColor, Color.black, 0.9f), 0.05f);
		RenderSettings.skybox.SetColor("_Tint", RenderSettings.fogColor);
		RenderSettings.fogDensity = 10f;
		RenderSettings.ambientIntensity = 0f;
		sun.intensity = 0f;
		afterburner.SetActive(true);

		clockText.text = "";
		positionText.text = "";

		startGame.playMusic.StopPlaying();

		showPanels.ShowMenu();
	}

	void Awake()
	{
		//DontDestroyOnLoad(gameObject);

		if (_instance == null)
		{
			_instance = this;
		}

		logMessage = HUD.transform.Find("Log").GetComponent<Text>();
		scoreText = HUD.transform.Find("Score").GetComponent<Text>();
		dayText = HUD.transform.Find("FPS").GetComponent<Text>();
		clockText = HUD.transform.Find("Clock").GetComponent<Text>();
		positionText = HUD.transform.Find("Position").GetComponent<Text>();

		startGame = HUD.transform.GetComponent<StartGame>();

		camOp = mainCamera.GetComponentInParent<CameraOperator>();

		player.GetComponent<Rigidbody>().isKinematic = true;
		playerActive = false;
		UpdateScore(0);

		RenderSettings.fogDensity = 10f;
		RenderSettings.skybox.SetColor("_Tint", RenderSettings.fogColor);

		// float resolution = 1E9f / Stopwatch.Frequency;
		//Game.Log(String.Format("The minimum measurable time on this system is: {0} nanoseconds", resolution.ToString()));
	}

	void Update() 
	{
		if (!playerActive)
		{
			return;
		}

		if (Input.GetKeyDown (KeyCode.Tab)) 
		{
			showStats = !showStats;

			if (!showStats)
			{
				dayText.text = "";
				clockText.text = "";
				positionText.text = "";
			}
			else
			{
				UpdatePosition(World.GetChunkPosition(player.transform.position));
			}
		}

		if (Config.Outlines && !edgeDetect.enabled)
		{
			edgeDetect.enabled = true;
		}
		if (!Config.Outlines && edgeDetect.enabled)
		{
			edgeDetect.enabled = false;
		}
		if (Config.ColoredOutlines && !edgeDetectColor.enabled)
		{
			edgeDetectColor.enabled = true;
		}
		if (!Config.ColoredOutlines && edgeDetectColor.enabled)
		{
			edgeDetectColor.enabled = false;
		}
	}

	public void _begin()
	{
		SetTime(6,0);

		if (ShowStats)
		{
			UpdatePosition(World.GetChunkPosition(player.transform.position));
		}

		StartCoroutine(Wait(1f, () => {
			player.GetComponent<Rigidbody>().isKinematic = false;
			player.transform.position = new Vector3(-0.5f, 32f, -0.5f);
			player.GetComponent<Roller>().asleep = true;
			PlayerActive = true;
			CameraOp.FirstPerson = true;
			PlaySong();
		}));
	}

	public static void Begin()
	{
		_instance._begin();
	}

	public string _initialize(string seed)
	{
		if (String.IsNullOrEmpty(seed))
		{
			seed = GameUtils.GenerateSeed(5).Trim();
		}
		else
		{
			seed = seed.ToUpper().Trim();
		}

		int n;
		if (int.TryParse(seed, out n))
		{
			UnityEngine.Random.InitState(n);
		}
		else
		{
			UnityEngine.Random.InitState(seed.GetHashCode());
		}
			
		GameUtils.SetHash();
		GameUtils.CreateVarianceTable();
		TileFactory.GenerateColorPalette();
		Blocks.Initialize();
		cosmos.CreateSky();

		World.Seed = seed;
		Serialization.Decompress();

		// These populate the object pools
		ChunkData.SetLoadOrder();
		if (firstRun)
		{
			World.Spawn.Initialize();
		}

		NoiseConfig.Initialize();

		// Initialize graphics
		edgeDetect.enabled = Config.Outlines;
		edgeDetectColor.enabled = Config.ColoredOutlines;

		hbao = mainCamera.GetComponent<HBAO>();
		hbao.enabled = Config.ContactShadows;

		RenderSettings.fog = true;//Config.GlobalFogEnabled;
		fog = mainCamera.GetComponent<GlobalFog>();
		fog.enabled = Config.GlobalFogEnabled;// && !Config.AtmosphericScattering;
		distanceFog = cosmosCamera.GetComponent<GlobalFog>();
		distanceFog.enabled = Config.GlobalFogEnabled;

		// scatterFog = mainCamera.GetComponent<GlobalFogExtended>();
		// scatterFog.Advanced.UseScattering = Config.AtmosphericScattering;
		// scatterFog.enabled = Config.GlobalFogEnabled && Config.AtmosphericScattering;

		if (Config.ShadowsEnabled)
		{
			sun.shadows = LightShadows.Soft;
			moon.shadows = LightShadows.Hard;
		}
		else
		{
			sun.shadows = LightShadows.None;
			moon.shadows = LightShadows.None;
		}

		RenderSettings.ambientIntensity = 0f;
		sun.intensity = 0f;

		firstRun = false;

		return seed;
	}

	public static string Initialize(string seed)
	{
		return _instance._initialize(seed);
	}

	public static string Initialize()
	{
		return _instance._initialize("");
	}

	public static void UpdateScore(int value) 
	{
		_instance.score += value;
		_instance.scoreText.text = _instance.score.ToString();
	}

	public static void UpdateClock(string time)
	{
		if (PlayerActive)
		{
			_instance.clockText.text = time;
		}
	}	

	public static void UpdateDay(string day)
	{
		if (PlayerActive && _instance.dayText.text != day)
		{
			_instance.dayText.text = day;
		}
	}

	public static void UpdatePosition(WorldPosition pos)
	{
		if (!PlayerActive)
		{
			return;
		}

		if (ShowStats)
		{			
			_instance.positionText.text = "X: " + pos.x.ToString() + ", Y: " + pos.y.ToString() + ", Z: " + pos.z.ToString();
		}

		// Track position for respawning
		Column column = World.GetColumn(pos);
		if (column != null && column.chunks[0] != null)
		{
			SampleSet results;
			InterpolatedNoise.Results.TryGetValue(column.region, out results);
			if (results != null)
			{
				LastGoodPosition = new Vector3(
					pos.x + Chunk.HalfSize + 0.5f,
					results.spawnMap.height[Chunk.HalfSize, Chunk.HalfSize],
					pos.z + Chunk.HalfSize + 0.5f
				);
			}

		}		
	}

	public static void SetTime(int hours, int minutes)
	{
		Vector3 rotation = Vector3.zero;
		rotation.x = (hours % 24) * (360 / 24f) + (minutes % 60) * (360 / (24f * 60f));

		_instance.cosmos.orbit.transform.rotation = Quaternion.identity;
		_instance.cosmos.orbit.transform.Rotate(rotation);
		_instance.cosmos.hours = hours % 12;
		_instance.cosmos.hours24 = hours % 24;
		_instance.cosmos.minutes = minutes;
	}

	public static void Log(string message)
	{
		if (_instance.logMessage.text.Contains(message))
		{
			message = message + " x" + _instance.logCount;
			_instance.logCount++;
		}
		else
		{
			_instance.logCount = 2;
		}
		 UnityEngine.Debug.Log(message);
		_instance.logMessage.text = message;
	}

	public static void PlaySong()
	{
		if (!_instance.startGame.playMusic.playing)
		{
			_instance.startGame.PlayNewMusic();
		}
	}

	IEnumerator Wait(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback();
	}
}
