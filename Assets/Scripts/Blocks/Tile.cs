using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tile
{
	public static Color[] colors = new Color[64];
	static Color darkColor = Color.black;
	static Color lightColor = Color.white;
	public static Color DarkColor 
	{
		get { return darkColor; }
		set	{ darkColor = value; }
	 }
	public static Color LightColor 
	{
		get { return darkColor; }
		set	{ darkColor = value; }
	 }

	public static Color Darken(Color color, float amount)
	{
		float a = color.a;
		color = Color.Lerp(color, darkColor, amount);
		color.a = a;
		return color;
	}

	public static Color Lighten(Color color, float amount)
	{
		float a = color.a;
		color = Color.Lerp(color, lightColor, amount);
		color.a = a;
		return color;
	}

	public static Color Brighten(Color color, float amount)
	{
		float h;
		float s;
		float v;
		float a = color.a;

		Color.RGBToHSV(color, out h, out s, out v);

		s = Mathf.Lerp(s, 1.0f, amount);
		v = Mathf.Lerp(v, 1.0f, amount);
		color = Color.HSVToRGB(h, s, v);
		color.a = a;

		return color;
	}

	public static Color Desaturate(Color color, float amount)
	{
		float h;
		float s;
		float v;
		float a = color.a;

		Color.RGBToHSV(color, out h, out s, out v);
		s = Mathf.Lerp(s, 0.0f, amount);
		color = Color.HSVToRGB(h, s, v);
		color.a = a;

		return color;
	}

	public static Color Inverse(Color color)
	{
		float h;
		float s;
		float v;

		Color.RGBToHSV(color, out h, out s, out v);

		h = h + 0.5f;
		h = h + Random.Range(0,0.2f) - 0.1f;
		if (h > 1) h -= 1;

		return Color.HSVToRGB(h, s, v);
	}
}
