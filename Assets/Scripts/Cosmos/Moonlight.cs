using UnityEngine;

public class Moonlight : MonoBehaviour {

	public Gradient nightDayColor;

	public float maxIntensity = 3f;
	public float minIntensity = 0f;
	public float minPoint = 0.2f;
	public Vector3 moonSpinDir = new Vector3 (15, 30, 45);
	public float moonSpinSpeed = 0.5f; 

	Light moonlight;
	public Light moonSpotlight;
	public GameObject moon;
	public GameObject innerMoon;
	public Cosmos cosmos;

	Material moonCheese;

	Material innerMoonCheese;

	void Start () 
	{
		moonlight = GetComponent<Light>();
		moonCheese = moon.GetComponent<Renderer>().material;
		moonCheese.EnableKeyword("_EMISSION");
		//innerMoonCheese = innerMoon.GetComponent<Renderer>().material;
		//innerMoonCheese.EnableKeyword("_EMISSION");
	}

	void Update () 
	{
		float tRange = 1 - minPoint;
		float dot = Mathf.Clamp01 ((Vector3.Dot (moonlight.transform.forward, Vector3.down) - minPoint) / tRange);
		float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

		moonlight.intensity = Mathf.Lerp(i, i * 0.5f, cosmos.rain.RainIntensity);
		moonSpotlight.intensity = Mathf.Lerp(i * 0.25f, i * 0.5f, cosmos.rain.RainIntensity);

		Color color = nightDayColor.Evaluate(dot);
		moonlight.color = Color.Lerp(color, Color.black, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.2f));
		moonSpotlight.color = Color.Lerp(color, Color.black, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.2f));

		if (!Cosmos.Daytime)
		{
			RenderSettings.ambientLight = color;
		}

		color = Tile.Lighten(color, 1f - dot);
		color = Color.Lerp(color, Color.black, cosmos.rain.RainIntensity);
		color.a = Mathf.Lerp(0.3f, 0f, Mathf.Clamp01(dot - 0.2f));

		moonCheese.SetColor("_Color", color);		
		moonCheese.SetColor("_EmissionColor", 
			Tile.Darken(color, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.2f)) * Mathf.Lerp(Mathf.Clamp01(dot - 0.25f), 0, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.5f)));

		// color = Tile.Lighten(nightDayColor.Evaluate(dot - 0.05f), dot);
		// color = Color.Lerp(color, Color.black, cosmos.rain.RainIntensity);
		//color.a = Mathf.Lerp(0f, 1f, Mathf.Clamp01(dot - 0.2f));

		float scale = Mathf.Lerp(0f, 1f, Mathf.Clamp01(dot + 0.4f));
		scale = Mathf.Lerp(scale, 0f, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.5f));
		innerMoon.GetComponent<Renderer>().transform.localScale = new Vector3(scale, scale, scale);

		// innerMoonCheese.SetColor("_Color", color);		
		// innerMoonCheese.SetColor("_EmissionColor", 
		// 	Tile.Darken(color, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.5f)) * Mathf.Lerp(Mathf.Clamp01(dot - 0.25f), 0, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.5f)));

		moon.transform.Rotate(moonSpinDir * Time.deltaTime * moonSpinSpeed);
		innerMoon.transform.Rotate(-moonSpinDir * 2 * Time.deltaTime * moonSpinSpeed);
	}
}
