using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Cosmos : MonoBehaviour {

	public Gradient nightDayColor;
	public Gradient nightDayFogColor;
	public AnimationCurve fogDensityCurve;

	public float fogScale = 1f;

	public float maxIntensity = 3f;
	public float minIntensity = 0f;
	public float minPoint = -0.2f;

	public float maxAmbient = 1f;
	public float minAmbient = 0f;
	public float minAmbientPoint = -0.2f;

	public float dayAtmosphereThickness = 0.4f;
	public float nightAtmosphereThickness = 0.87f;

	public Vector3 dayRotateSpeed;
	public Vector3 nightRotateSpeed;

	public GameObject orbit;
	public GameObject moon;
	public GameObject zodiac;
	public ParticleSystem stars;
	public DigitalRuby.RainMaker.RainScript rain;
	public DigitalRuby.ThunderAndLightning.LightningBoltPrefabScript lightningScript;
	public GameObject lightning;
	public GlobalFog fog;
	//public GlobalFogExtended scatterFog;
	public UnityStandardAssets.ImageEffects.EdgeDetectionColor edges;

	MeshRenderer moonrenderer;
	Light mainLight;
	Material sky;
	DigitalRuby.ThunderAndLightning.ThunderAndLightningScript lightningController;

	float skySpeed = 1;
	float skyDelta = 0.001f;
	bool daytime = true;
	bool starsout = false;

	public float days = 0;
	public float hours = 12;
	public float hours24 = 12;
	public float minutes = 0;
	public float am_pm = 0;
	public string ampm = "";
	public Color skyColor;
	public Moonlight moonlight;

	private Color greySkyColor = new Color(0.25f,0.25f,0.25f);
	private Color rainFogColor = new Color(0.15f,0.15f,0.15f);

	float dot;
	float tRange;
	float i;
	float newScale;
	float targetScale;
	float baseFog;
	float _ampm;
	Color starColor;


	void Start () 
	{
		mainLight = GetComponent<Light>();
		sky = RenderSettings.skybox;
		moonrenderer = moon.GetComponent<MeshRenderer>();
		lightningController = lightning.GetComponent<DigitalRuby.ThunderAndLightning.ThunderAndLightningScript>();

		stars.Clear();
		var em = stars.emission;
		em.enabled = false;

		rain.RainMistThreshold = 1f;
	}

	void Update () 
	{
		if (!Game.PlayerActive)
		{
			return;
		}

		HandleWeather();
		SetCosmos();
		HandleRotation();
		HandleCosmosControls();
	}

	void FixedUpdate()
	{
		// Move the cosmos
		orbit.transform.position = new Vector3(
			Game.Player.transform.position.x, 
			Mathf.Lerp(orbit.transform.position.y , Game.Player.transform.position.y, 0.1f), 
			Game.Player.transform.position.z
			);
	}

	void HandleWeather()
	{
		float currentTime = (days * (24 * 60)) + (hours24 * 60) + minutes;
		float rainChance = TerrainGenerator.GetNoise1D(new Vector3(currentTime,0,0), NoiseConfig.rainIntensity, NoiseType.Simplex);
		float lightningChance = TerrainGenerator.GetNoise1D(new Vector3(currentTime,0,0), NoiseConfig.lightningIntensity, NoiseType.Simplex);
		float percentChance;

		if (rainChance > NoiseConfig.rainBreakValue)
		{
			percentChance = (rainChance - NoiseConfig.rainBreakValue) / (NoiseConfig.rainIntensity.scale - NoiseConfig.rainBreakValue);
			rain.RainIntensity = Mathf.Lerp(rain.RainIntensity, percentChance, skyDelta);
		}
		else
		{
			rain.RainIntensity = Mathf.Lerp(rain.RainIntensity, 0, skyDelta);
		}

		if (lightningChance > NoiseConfig.lightningBreakValue)
		{
			percentChance 
				= (lightningChance - NoiseConfig.lightningBreakValue) / (NoiseConfig.lightningIntensity.scale - NoiseConfig.lightningBreakValue);

			lightningController.LightningIntervalTimeRange.Minimum
				= Mathf.Lerp(15, 1, percentChance);

			lightningController.LightningIntervalTimeRange.Maximum
				= Mathf.Lerp(60, 5, percentChance);

			lightning.SetActive(true);
		}
		else
		{
			lightning.SetActive(false);
		}

		//Game.Log("Rain: " + rainChance.ToString() + "  Lightning: " + lightningChance.ToString());
	}

	void SetCosmos()
	{
		// find light intensity
		tRange = 1 - minPoint;
		dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minPoint) / tRange);
		i = ((maxIntensity - minIntensity) * dot) + minIntensity;

		if (rain.RainIntensity == 0)
		{
			mainLight.intensity = i;
		}
		else
		{
			mainLight.intensity = Mathf.Lerp(i, i / 100, rain.RainIntensity);
		}

		// find ambient intensity
		tRange = 1 - minAmbientPoint;
		dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
		i = ((maxAmbient - minAmbient) * dot) + minAmbient;

		if (rain.RainIntensity == 0)
		{
			RenderSettings.ambientIntensity = i;
		}
		else 
		{
			RenderSettings.ambientIntensity = Mathf.Lerp(i, i / 10, rain.RainIntensity);
		}

		// set light color
		mainLight.color = nightDayColor.Evaluate(dot);
		RenderSettings.ambientLight = mainLight.color;

		// lightning color
		lightningScript.GlowTintColor = TileFactory.Brighten(TileFactory.Inverse(mainLight.color), 0.8f);
		lightningScript.LightningTintColor = TileFactory.Lighten(TileFactory.Inverse(mainLight.color), 0.8f);

		// Fog Color
		if (rain.RainIntensity == 0)
		{
			skyColor = nightDayFogColor.Evaluate(dot);
			RenderSettings.fogColor = skyColor;
		}
		else
		{
			skyColor = Color.Lerp(nightDayFogColor.Evaluate(dot), greySkyColor, rain.RainIntensity);
			RenderSettings.fogColor = Color.Lerp(nightDayFogColor.Evaluate(dot), rainFogColor, rain.RainIntensity);
		}

		// Outline Color
		if (Config.ColoredOutlines)
		{
			edges.edgesColor = TileFactory.Darken(Color.Lerp(skyColor, Color.gray, 0.5f), 0.2f);
		}

		// Star Color and Size
		starColor = Color.Lerp(TileFactory.colors[Mathf.FloorToInt((Random.value + dot) * 64) % 64],Color.black, Mathf.Clamp01(rain.RainIntensity * 3));
		starColor = TileFactory.Desaturate(starColor, 1 - Mathf.Pow(Random.value,2));
		starColor.a = Mathf.Lerp(1f, 0f, rain.RainIntensity);

		var ma = stars.main;
		ma.startColor = starColor;
		ma.startSize = Mathf.Lerp(0.1f, 0.4f, Mathf.Pow(Random.value, 3));
 
		// Fog Density
		if (rain.RainIntensity > 0)
		{
			baseFog = fogDensityCurve.Evaluate(dot) * Config.FogScale;

			if (daytime)
			{
				RenderSettings.fogDensity = Mathf.Lerp(Mathf.Lerp(
					baseFog,
					baseFog * 2.5f,
					rain.RainIntensity
				), RenderSettings.fogDensity, skyDelta);
			} 
			else 
			{
				RenderSettings.fogDensity = Mathf.Lerp(Mathf.Lerp(
					baseFog,
					baseFog * 5f,
					rain.RainIntensity
				), RenderSettings.fogDensity, skyDelta);
			}
		}
		else
		{
			RenderSettings.fogDensity = fogDensityCurve.Evaluate(dot) * Config.FogScale;
		}

		// if (Config.AtmosphericScattering)
		// {
		// 	scatterFog.Advanced.ScatteringSize = Mathf.Lerp(0.995f, 1f, Mathf.Clamp01(rain.RainIntensity * 2f));
		// 	scatterFog.Advanced.ScatteringIntensity = Mathf.Lerp(8f, 0f, Mathf.Clamp01(rain.RainIntensity * 2f));
		// }

		//Game.Log(RenderSettings.fogDensity.ToString());

		// Skybox Color
		sky.SetColor("_Tint", skyColor);
	}

	void HandleRotation()
	{
		// night begins and day breaks
		// slightly before and after the minimumn ambient point
		if (dot > 0.05f) 
		{
			if (!daytime)
			{
				daytime = true;
				Game.PlaySong();
			}

			_ampm = Rotate(dayRotateSpeed * Time.deltaTime * skySpeed);
		}
		else
		{
			if (daytime)
			{
				daytime = false;
				rain.RainMistThreshold = 0.1f;
			}

			_ampm = Rotate(nightRotateSpeed * Time.deltaTime * skySpeed);
		}

		if (daytime && dot >= 0.2f && _ampm < 0f && starsout)
		{
			var em = stars.emission;
			em.enabled = false;
			starsout = false;
		}

		if (daytime && dot >= 0.5f && _ampm < 0f && moonrenderer.enabled)
		{
			moonrenderer.enabled = false;
		}

		if (daytime && dot <= 0.3f && _ampm > 0f && !starsout)
		{
			var em = stars.emission;
			em.enabled = true;
			Game.PlaySong();
			starsout = true;
		}

		if (daytime && dot <= 0.5f && _ampm > 0f && !moonrenderer.enabled)
		{
			moonrenderer.enabled = true;
		}

		//Game.Log(dot.ToString("F2") + " " + ampm.ToString());
	}

	void HandleCosmosControls()
	{
		if (Input.GetKeyDown(KeyCode.Comma)) skySpeed *= 0.5f;
		if (Input.GetKeyDown(KeyCode.Period)) skySpeed *= 2f;
		if (Input.GetKeyDown(KeyCode.LeftBracket)) Config.Outlines = !Config.Outlines;
		if (Input.GetKeyDown(KeyCode.RightBracket)) Config.ColoredOutlines = !Config.ColoredOutlines;
	}
		
	float Rotate(Vector3 rotation)
	{
		orbit.transform.Rotate(rotation);

		// 360/24 = 15;
		hours24 += Mathf.Abs(rotation.x * (1f / 15f));
		// 360/24*60 = 0.25
		minutes += Mathf.Abs(rotation.x * 4f);

		if (hours24 >= 24f) 
		{
			hours24 -= 24f;
			days++;
		}

		hours = hours24 >= 13f ? hours24 -= 12f : hours24;

		if (minutes >= 60f)
			minutes -= 60f;
		
		am_pm = Vector3.Dot(mainLight.transform.forward, Vector3.forward);

		if (Game.ShowStats)
		{
			if (am_pm > 0)
				ampm = "PM";
			else
				ampm = "AM";

			//Game.UpdateClock(Mathf.FloorToInt(hours).ToString() + ":" + Mathf.FloorToInt(minutes).ToString("D2") + " " + ampm);
			Game.UpdateClock(Mathf.FloorToInt(days).ToString() + ":" + Mathf.FloorToInt(hours24).ToString() + ":" + Mathf.FloorToInt(minutes).ToString("D2"));
		}

		return am_pm;
	}

	public void CreateSky()
	{
		GradientColorKey[] gck;
		GradientAlphaKey[] gak;

		nightDayColor = new Gradient();
		gck = new GradientColorKey[5];
		gck[0].color = Color.Lerp(TileFactory.Brighten(TileFactory.colors[24], 0.8f), Color.black, 0.5f);
		gck[0].time = 0.295f;
		gck[1].color = Color.Lerp(TileFactory.Brighten(TileFactory.colors[24], 0.2f), Color.black, 0.25f);
		gck[1].time = 0.3f;
		gck[2].color = TileFactory.Lighten(TileFactory.Brighten(TileFactory.colors[32], 0.7f), 0.2f);
		gck[2].time = 0.34f;
		gck[3].color = TileFactory.Lighten(TileFactory.Brighten(TileFactory.colors[8], 0.9f), 0.9f);
		gck[3].time = 0.55f;
		gck[4].color = Color.Lerp(TileFactory.Brighten(TileFactory.colors[16], 0.9f), Color.white, 0.8f);
		gck[4].time = 0.80f;
		gak = new GradientAlphaKey[2];
		gak[0].alpha = 1.0f;
		gak[0].time = 0.0f;
		gak[1].alpha = 1.0f;
		gak[1].time = 1.0f;
		nightDayColor.SetKeys(gck, gak);

		nightDayFogColor = new Gradient();
		gck = new GradientColorKey[6];
		gck[0].color = TileFactory.Brighten(Color.Lerp(TileFactory.colors[53], Color.black, 0.8f), 0.05f);
		gck[0].time = 0.02f;
		gck[1].color = TileFactory.Brighten(Color.Lerp(TileFactory.colors[52], Color.black, 0.5f), 0.05f);
		gck[1].time = 0.055f;
		gck[2].color = TileFactory.Brighten(Color.Lerp(TileFactory.colors[51], Color.black, 0.05f), 0.1f);
		gck[2].time = 0.09f;
		gck[3].color = TileFactory.Brighten(TileFactory.colors[51], 0.4f);
		gck[3].time = 0.14f;
		gck[4].color = TileFactory.Lighten(TileFactory.Brighten(TileFactory.colors[52], 0.7f), 0.5f);
		gck[4].time = 0.2f;
		gck[5].color = Color.Lerp(TileFactory.Brighten(TileFactory.colors[21], 0.7f), Color.white, 0.65f);
		gck[5].time = 0.75f;
		nightDayFogColor.SetKeys(gck, gak);

		moonlight.nightDayColor = nightDayFogColor;
	}
}
