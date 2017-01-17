using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using ProceduralToolkit;
using ProceduralToolkit.Examples;
using Random = UnityEngine.Random;

public class Cosmos : MonoBehaviour {

	public static float CurrentTime {get; set;}
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
	DigitalRuby.ThunderAndLightning.ThunderAndLightningScript lightningController;

	float skySpeed = 1;
	float skyDelta = 0.001f;
	float moonDelta = 0.01f;
	public static bool Daytime = true;
	bool starsout = false;

	public float days = 1;
	public float hours = 12;
	public float hours24 = 12;
	public float minutes = 0;
	public float am_pm = 0;
	public string ampm = "";
	public Color skyColor;
	public Moonlight moonlight;
	public GameObject boids;
	private Color greySkyColor = new Color(0.25f,0.25f,0.25f);

	float dot;
	float tRange;
	float i;
	float newScale;
	float targetScale;
	float baseFog;
	float _ampm;
	float fullSizeMoon;
	Color starColor;


	void Start () 
	{
		mainLight = GetComponent<Light>();
		moonrenderer = moon.GetComponent<MeshRenderer>();
		fullSizeMoon = moonrenderer.transform.localScale.y;

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
		FlyBoids();
		SetCosmos();
		HandleRotation();
		HandleCosmosControls();
	}

	void FixedUpdate()
	{
		if (!Game.PlayerActive)
		{
			return;
		}

		// Move the cosmos
		orbit.transform.position = new Vector3(
			Game.Player.transform.position.x, 
			Mathf.Lerp(orbit.transform.position.y , Game.Player.transform.position.y, 0.1f), 
			Game.Player.transform.position.z
			);
	}

	void FlyBoids()
	{
		//boids.transform.position = Game.Player.transform.position;
		boids.transform.position = new Vector3(
			Mathf.Lerp(boids.transform.position.x, Game.Player.transform.position.x, skyDelta),
			boids.transform.position.y,
			Mathf.Lerp(boids.transform.position.z, Game.Player.transform.position.z, skyDelta)
			);
	} 

	void HandleWeather()
	{
		CurrentTime = (days * (24 * 60)) + (hours24 * 60) + minutes;
		float rainChance = TerrainGenerator.GetNoise1D(new Vector3(CurrentTime,0,0), NoiseConfig.rainIntensity, NoiseType.Simplex);
		float lightningChance = TerrainGenerator.GetNoise1D(new Vector3(CurrentTime,0,0), NoiseConfig.lightningIntensity, NoiseType.Simplex);
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

		if (Daytime)
		{
			RenderSettings.ambientLight = mainLight.color;
		}

		// lightning color
		lightningScript.GlowTintColor = Tile.Brighten(Tile.Inverse(mainLight.color), 0.8f);
		lightningScript.LightningTintColor = Tile.Lighten(Tile.Inverse(mainLight.color), 0.8f);

		// Fog Color
		if (rain.RainIntensity == 0)
		{
			skyColor = nightDayFogColor.Evaluate(dot);
			RenderSettings.fogColor = skyColor;
		}
		else
		{
			skyColor = Color.Lerp(nightDayFogColor.Evaluate(dot), greySkyColor, rain.RainIntensity);
			RenderSettings.fogColor = skyColor;
		}

		// Outline Color
		if (Config.ColoredOutlines)
		{
			edges.edgesColor = Tile.Darken(Color.Lerp(skyColor, Color.gray, 0.5f), 0.2f);
		}

		// Skybox Color
		RenderSettings.skybox.SetColor("_Tint", skyColor);
	
		// Star Color and Size
		starColor = Color.Lerp(Tile.Colors[Mathf.FloorToInt((Random.value + dot) * 64) % 64],Color.black, Mathf.Clamp01(rain.RainIntensity * 3));
		starColor = Tile.Desaturate(starColor, 1 - Mathf.Pow(Random.value,2));
		starColor.a = Mathf.Lerp(1f, 0f, rain.RainIntensity);

		var ma = stars.main;
		ma.startColor = starColor;
		ma.startSize = Mathf.Lerp(0.1f, 0.4f, Mathf.Pow(Random.value, 3));
 
		// Fog Density
		if (rain.RainIntensity > 0)
		{
			baseFog = fogDensityCurve.Evaluate(dot) * Config.FogScale;

			if (Daytime)
			{
				RenderSettings.fogDensity = Mathf.Lerp(Mathf.Lerp(
					baseFog,
					baseFog * 1.2f,
					rain.RainIntensity
				), RenderSettings.fogDensity, skyDelta);
			} 
			else 
			{
				RenderSettings.fogDensity = Mathf.Lerp(Mathf.Lerp(
					baseFog,
					baseFog * 1.6f,
					rain.RainIntensity
				), RenderSettings.fogDensity, skyDelta);
			}
		}
		else
		{
			RenderSettings.fogDensity = Mathf.Lerp(fogDensityCurve.Evaluate(dot) * Config.FogScale, RenderSettings.fogDensity, skyDelta);
		}

		if (Time.frameCount % 100 == 0)
		{
			DynamicGI.UpdateEnvironment();
		}

		// if (Config.AtmosphericScattering)
		// {
		// 	scatterFog.Advanced.ScatteringSize = Mathf.Lerp(0.995f, 1f, Mathf.Clamp01(rain.RainIntensity * 2f));
		// 	scatterFog.Advanced.ScatteringIntensity = Mathf.Lerp(8f, 0f, Mathf.Clamp01(rain.RainIntensity * 2f));
		// }

		//Game.Log(RenderSettings.fogDensity.ToString());
	}

	void HandleRotation()
	{
		// night begins and day breaks
		// slightly before and after the minimumn ambient point
		if (dot > 0.05f) 
		{
			if (!Daytime)
			{
				days++;
				Daytime = true;
				Game.PlaySong();
			}

			_ampm = Rotate(dayRotateSpeed * Time.deltaTime * skySpeed);
		}
		else
		{
			if (Daytime)
			{
				Daytime = false;
			}

			_ampm = Rotate(nightRotateSpeed * Time.deltaTime * skySpeed);
		}

		if (Daytime && dot >= 0.2f && _ampm < 0f && starsout)
		{
			var em = stars.emission;
			em.enabled = false;
			starsout = false;
		}

		if (Daytime && dot >= 0.3f && _ampm < 0f && moonrenderer.transform.localScale.y == fullSizeMoon)
		{
			StartCoroutine(ShrinkMoon());
		}

		if (Daytime && dot <= 0.3f && _ampm > 0f && !starsout)
		{
			var em = stars.emission;
			em.enabled = true;
			Game.PlaySong();
			starsout = true;
		}

		if (Daytime && dot <= 0.3f && _ampm > 0f && moonrenderer.transform.localScale.y < fullSizeMoon)
		{
			StartCoroutine(GrowMoon());
		}

		//Game.Log(dot.ToString("F2") + " " + ampm.ToString());
	}

	IEnumerator ShrinkMoon()
	{
		for (;;)
		{
			if (moonrenderer.transform.localScale.y > 0.001f)
			{
				moonrenderer.transform.localScale *= (1f - moonDelta);
				yield return null;
			}
			else
			{
				break;
			}
		}
	}

	IEnumerator GrowMoon()
	{
		for (;;)
		{
			if (moonrenderer.transform.localScale.y < fullSizeMoon)
			{
				moonrenderer.transform.localScale *= (1f + moonDelta);

				if (moonrenderer.transform.localScale.y > fullSizeMoon)
				{
					moonrenderer.transform.localScale = new Vector3(fullSizeMoon, fullSizeMoon, fullSizeMoon);
				}

				yield return null;
			}
			else
			{
				break;
			}
		}
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
		}

		hours = hours24 >= 13f ? hours24 -= 12f : hours24;

		if (minutes >= 60f)
		{
			minutes -= 60f;
		}

		am_pm = Vector3.Dot(mainLight.transform.forward, Vector3.forward);

		if (Game.ShowStats)
		{
			if (am_pm > 0)
			{
				ampm = "PM";
			}	
			else
			{
				ampm = "AM";
			}
			
			Game.UpdateDay("Day " + days.ToString()); 
			Game.UpdateClock(Mathf.FloorToInt(hours).ToString() + ":" + Mathf.FloorToInt(minutes).ToString("D2") + " " + ampm);
		}

		return am_pm;
	}

	public void CreateSky()
	{
		GradientColorKey[] gck;
		GradientAlphaKey[] gak;

		nightDayColor = new Gradient();
		gck = new GradientColorKey[5];
		gck[0].color = Color.Lerp(Tile.Brighten(Tile.Colors[24], 0.8f), Color.black, 0.5f);
		gck[0].time = 0.0f;
		gck[1].color = Color.Lerp(Tile.Brighten(Tile.Colors[24], 0.2f), Color.black, 0.25f);
		gck[1].time = 0.22f;
		gck[2].color = Tile.Lighten(Tile.Brighten(Tile.Colors[32], 0.7f), 0.2f);
		gck[2].time = 0.3f;
		gck[3].color = Tile.Lighten(Tile.Brighten(Tile.Colors[8], 0.9f), 0.9f);
		gck[3].time = 0.55f;
		gck[4].color = Color.Lerp(Tile.Brighten(Tile.Colors[16], 0.9f), Color.white, 0.8f);
		gck[4].time = 0.80f;
		gak = new GradientAlphaKey[2];
		gak[0].alpha = 1.0f;
		gak[0].time = 0.0f;
		gak[1].alpha = 1.0f;
		gak[1].time = 1.0f;
		nightDayColor.SetKeys(gck, gak);

		nightDayFogColor = new Gradient();
		gck = new GradientColorKey[6];
		gck[0].color = Tile.Brighten(Color.Lerp(Tile.Colors[53], Color.black, 0.8f), 0.05f);
		gck[0].time = 0.19f;
		gck[1].color = Tile.Brighten(Color.Lerp(Tile.Colors[52], Color.black, 0.5f), 0.05f);
		gck[1].time = 0.235f;
		gck[2].color = Tile.Brighten(Color.Lerp(Tile.Colors[51], Color.black, 0.05f), 0.1f);
		gck[2].time = 0.26f;
		gck[3].color = Tile.Brighten(Tile.Colors[51], 0.4f);
		gck[3].time = 0.31f;
		gck[4].color = Tile.Lighten(Tile.Brighten(Tile.Colors[52], 0.7f), 0.5f);
		gck[4].time = 0.37f;
		gck[5].color = Color.Lerp(Tile.Brighten(Tile.Colors[21], 0.7f), Color.white, 0.65f);
		gck[5].time = 0.75f;
		nightDayFogColor.SetKeys(gck, gak);

		moonlight.nightDayColor = nightDayFogColor;

		zodiac.transform.localScale = Vector3.one;
		zodiac.transform.localScale *= 2f + (Config.WorldSize - 6f) * 0.25f;
	}
}
