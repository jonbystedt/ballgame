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

	Material moonMaterial;
	Material innerMoonMat;

	void Start () 
	{
		moonlight = GetComponent<Light>();
		moonMaterial = moon.GetComponent<Renderer>().material;
		innerMoonMat = innerMoon.GetComponent<Renderer>().material;
	}

	void Update () 
	{
		float tRange = 1 - minPoint;
		float dot = Mathf.Clamp01 ((Vector3.Dot (moonlight.transform.forward, Vector3.down) - minPoint) / tRange);
		float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

		moonlight.intensity = Mathf.Lerp(i, i / 10, cosmos.rain.RainIntensity);
		moonSpotlight.intensity = Mathf.Lerp(i, i / 3, cosmos.rain.RainIntensity);

		//Debug.Log ("Moon Intensity: " + i.ToString());
		//Game.Log(dot.ToString("N2"));

		Color color = nightDayColor.Evaluate(dot);
		moonlight.color = Color.Lerp(color, Color.black, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.2f));
		moonSpotlight.color = Color.Lerp(color, Color.black, Mathf.Clamp01(cosmos.rain.RainIntensity + 0.2f));
		RenderSettings.ambientLight = color;

		color = Color.Lerp(color,Color.black,cosmos.rain.RainIntensity);
		color.a = Mathf.Lerp(0.0025f, 0f, cosmos.rain.RainIntensity);
		moonMaterial.SetColor("_EmissionColor", Tile.Darken(color, cosmos.rain.RainIntensity));
		moonMaterial.SetColor("_Color", color);

		color = nightDayColor.Evaluate(dot - 0.05f);
		color = Color.Lerp(color,Color.black,Mathf.Clamp01(cosmos.rain.RainIntensity + 0.5f));
		color.a = Mathf.Lerp(0.0025f, 0f, cosmos.rain.RainIntensity);
		innerMoonMat.SetColor("_EmissionColor", Tile.Darken(color, cosmos.rain.RainIntensity));
		innerMoonMat.SetColor("_Color", Tile.Brighten(color, 0.5f));

		moon.transform.Rotate(moonSpinDir * Time.deltaTime * moonSpinSpeed);
		innerMoon.transform.Rotate(-moonSpinDir * 2 * Time.deltaTime * moonSpinSpeed);
	}
}
