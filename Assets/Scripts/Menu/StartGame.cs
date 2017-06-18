using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;


public class StartGame : MonoBehaviour {

	public InputField input;
	public RectTransform progressBar;
	public Text message;

	public int sceneToStart = 1;										//Index number in build settings of scene to load if changeScenes is true
	public bool changeScenes;											//If true, load a new scene when Start is pressed, if false, fade out UI and continue in single scene
	public bool changeMusicOnStart;										//Choose whether to continue playing menu music or start a new music clip
	//public int musicToChangeTo = 1;										//Array index in array MusicClips to change to if changeMusicOnStart is true.


	[HideInInspector] public bool inMainMenu = true;					//If true, pause button disabled in main menu (Cancel in input manager, default escape key)
	[HideInInspector] public Animator animColorFade; 					//Reference to animator which will fade to and from black when starting game.
	[HideInInspector] public Animator animMenuAlpha;					//Reference to animator that will fade out alpha of MenuPanel canvas group
	[HideInInspector] public AnimationClip fadeColorAnimationClip;		//Animation clip fading to color (black default) when changing scenes
	[HideInInspector] public AnimationClip fadeAlphaAnimationClip;		//Animation clip fading out UI elements alpha


	public PlayMusic playMusic;							
	//private float fastFadeIn = .01f;						
	private float slowFade = 2f;
	private ShowPanels showPanels;	
	public SetOptions setOptions;
	private LoadChunks loader;

	private RectTransform inputRect;

	int defaultCoroutineTiming = 20000;
	int maxCoroutineTiming = 100000;
	
	void Awake()
	{
		showPanels = GetComponent<ShowPanels>();
		playMusic = GetComponent<PlayMusic> ();

		loader = GameObject.FindWithTag("Player").GetComponent<LoadChunks>();

		inputRect = input.GetComponent<RectTransform>();
		progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);
	}

	public void Reactivate()
	{
		animMenuAlpha.SetTrigger("unfade");

		input.text = "";
		message.text = "enter world seed";
		progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);

		input.ActivateInputField();
		input.Select();	
	}

	void Start()
	{
		input.ActivateInputField();
		input.Select();	
		Cursor.visible = true;
		StartCoroutine(AwaitConfig());
	}

	void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Return)) 
		{
			StartButtonClicked();
		}
	}

	public IEnumerator AwaitConfig()
	{
		for (;;)
		{
			if (Config._instance != null)
			{
				if (!Serialization.ReadConfig())
				{
					ApplyDefaultConfig();
				}
				else
				{
					defaultCoroutineTiming = Config.Settings.coroutineTiming;
				}

				if (!Serialization.ReadNoiseConfig())
				{
					ApplyDefaultNoiseConfig();
				}

				setOptions.SetResolution(Config.Settings.resolution);
				Config.GraphicsMode = Config.GraphicsMode;
				Config.WorldSize = Config.Settings.worldSize;
				setOptions.SetMusicLevel(Config.MusicVolume * 0.01f);
				setOptions.SetSfxLevel(Config.SfxVolume * 0.01f);
				break;
			}
			else
			{
				yield return null;
			}
		}
	}
		
	public void StartButtonClicked()
	{
		string seed = Game.Initialize(input.text.ToUpper());
		input.text = seed;

		loader.loading = true;
		loader._start();
		Config.CoroutineTiming = maxCoroutineTiming;
		message.text = "Building World";
		StartCoroutine("WaitForChunkLoad");
	}

	public void ExecuteStartGame()
	{
		// Column start = World.Columns[new WorldPosition(0,0,0).GetHashCode()];
		// SampleSet results = InterpolatedNoise.Results[start.region];

		//Game.Player.transform.position = new Vector3(start.region.min.x, results.spawnMap.height[0,0] + 1f, start.region.min.z);

		loader.spawning = true;
		Config.CoroutineTiming = defaultCoroutineTiming;

		if (changeMusicOnStart) 
		{
			playMusic.FadeDown(fadeColorAnimationClip.length);
			Invoke ("PlayNewMusic", fadeAlphaAnimationClip.length);
		}
		if (changeScenes) 
		{
			Invoke ("LoadDelayed", fadeColorAnimationClip.length * .5f);
			animColorFade.SetTrigger("fade");
		} 
		else 
		{
			StartGameInScene();
		}
	}


	public void LoadDelayed()
	{
		inMainMenu = false;
		showPanels.HideMenu();
		SceneManager.LoadScene(sceneToStart);
	}


	public void StartGameInScene()
	{
		// Pause button now works if escape is pressed since we are no longer in Main menu.
		inMainMenu = false;

		if (changeMusicOnStart) 
		{
			Invoke ("PlayNewMusic", fadeAlphaAnimationClip.length);
		}
		animMenuAlpha.SetTrigger("fade");

		Invoke("HideDelayed", fadeAlphaAnimationClip.length);

		Game.Begin();
	}


	public void PlayNewMusic()
	{
		int musicToChangeTo = UnityEngine.Random.Range(0, playMusic.numberOfSongs);

		if (musicToChangeTo == playMusic.currentTrack) 
			musicToChangeTo++;

		playMusic.FadeUp(slowFade);
		playMusic.PlaySelectedMusic(musicToChangeTo);
	}

	public void HideDelayed()
	{
		showPanels.HideMenu();
	}

	IEnumerator WaitForChunkLoad()
	{
		float progress = 0f;
		int chunksToLoad = Config.StartChunksToLoad > ChunkData.LoadOrder.Count() ? ChunkData.LoadOrder.Count() : Config.StartChunksToLoad;

		for(;;)
		{
			if (Game.ChunksLoaded >= chunksToLoad)
			{
				progressBar.sizeDelta = new Vector2(inputRect.rect.width, progressBar.sizeDelta.y);
				ExecuteStartGame();
				break;
			}
			else
			{
				progress = Game.ChunksLoaded / (float)chunksToLoad;
				progressBar.sizeDelta = new Vector2(inputRect.rect.width * progress, progressBar.sizeDelta.y);
				yield return null;
			}
		}
	}

	void ApplyDefaultConfig()
	{
		Config.Settings.startTime = "6:00";
		Config.Settings.graphicsQuality = 1;
		Config.Settings.worldSize = 12;
		Config.Settings.spawnIntensity = 50;
		Config.Settings.musicVol = 80;
		Config.Settings.sfxVol = 80;
		Config.Settings.swapInputs = false;
		Config.Settings.resolution = Screen.width.ToString() + "x" + Screen.height.ToString();
		Config.Settings.multithreaded = true;
	#if UNITY_STANDALONE_OSX
		Config.Settings.multithreaded = false;
	#endif
		Config.Settings.coroutineTiming = defaultCoroutineTiming;
	}

	void ApplyDefaultNoiseConfig()
	{
		Config.Noise = new NoiseSettings();

		Range frequency = new Range(0.00005f,0.025f);
		Range octaves = new Range(1f, 3f);
		Range lacunarity = new Range(0f, 4f);
		Range persistance = new Range(0f, 0.5f);

		Config.Noise.terrain = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.0005f, 0.025f);
		octaves = new Range(1f, 3f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.mountain = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.008f, 0.02f);
		octaves = new Range(1f, 1f);
		lacunarity = new Range(0f, 0f);
		persistance = new Range(0f, 0f);

		Config.Noise.cave = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.01f, 0.04f);
		octaves = new Range(1f, 4f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 1f);

		Config.Noise.pattern = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		frequency = new Range(0.00001f, 0.1f);
		octaves = new Range(1f, 4f);
		lacunarity = new Range(0f, 4f);
		persistance = new Range(0f, 2f);

		Config.Noise.stripe = new NoiseSetting(frequency, octaves, lacunarity, persistance);

		Config.Noise.terrainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.mountainTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.caveTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.patternTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};
		Config.Noise.stripeTypes = new NoiseType[] {NoiseType.Perlin, NoiseType.Simplex, NoiseType.SimplexValue, NoiseType.Value};

		Config.Noise.terrainScale = new Range(1f, 33f);
		Config.Noise.beachHeight = new Range(4f, 32f);
		Config.Noise.cloudEasing = new Range(16f, 32f);

		Config.Noise.caveBreak = new Range(256f, 768f);
		Config.Noise.patternBreak = new Range(128f, 768f);
		Config.Noise.stripeBreak = new Range(64f, 940f);
		Config.Noise.patternStripeBreak = new Range(0f, 1024f);

		Config.Noise.cloudBreak = new Range(512f, 768f);
		Config.Noise.islandBreak = new Range(0f, 256f);

		Config.Noise.glass1 = new Range(0f, 128f);
		Config.Noise.glass2 = new Range(0f, 128f);

		Config.Noise.modScale = new Range(2f, 128f);

		Config.Noise.stretch = new Range(0f, 1000f);
		Config.Noise.squish = new Range(0f, 100f);
	}
}
