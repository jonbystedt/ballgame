using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ColorFactory
{
    // Size should probably be divisible by two :)
    public static void GeneratePalette()
    {
        float valueCap = UnityEngine.Random.Range(0.75f, 1.0f);
        float saturationCap = UnityEngine.Random.Range(0.75f, 1.0f);

        // Randomized on hue only
        Color seedColor = UnityEngine.Random.ColorHSV(0f, 1f, saturationCap, saturationCap, valueCap, valueCap);

        // SingleColor: a color with a smooth tone gradient
        // WarmCool: a color and its complimentary opposites
        // DarkLight: one color with dark and light tones
        // ValueColor: a combination of the two
        GradientType type = (GradientType)UnityEngine.Random.Range(0, 4);

        GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(Tile.Colors, 0);

        valueCap = UnityEngine.Random.Range(0.65f, 0.9f);
        saturationCap = UnityEngine.Random.Range(0.5f, 1.0f);
        type = (GradientType)UnityEngine.Random.Range(0, 4);
        seedColor = Tile.Inverse(Tile.Colors[16]);

        GenerateGradients(type, seedColor, saturationCap, valueCap, 16).ToArray().CopyTo(Tile.Colors, 32);

        //lightColor = Color.Lerp(Tile.colors[30], Color.white, 0.85f);
        //darkColor = Color.Lerp(Tile.colors[1], Color.black, 0.85f);

        // for (int i = 0; i < 64; i++)
        // {
        // 	Tile.colors[64 + i] = Inverse(Tile.colors[i]);
        // }
    }

    static List<Color> GenerateGradients(
        GradientType type,
        Color seedColor,
        float saturationCap,
        float valueCap,
        int size
        )
    {
        List<Color> gradients = new List<Color>();

        float valueStart = 0.0f;
        float valueEnd = valueCap;
        float saturationStart = 0.0f;
        float saturationEnd = saturationCap;

        int swaps = Mathf.FloorToInt(Mathf.Lerp(0, 17, Mathf.Pow(UnityEngine.Random.value, 3)));

        if (type == GradientType.WarmCool || type == GradientType.SingleColor)
        {
            valueStart = UnityEngine.Random.Range(0.5f, 0.75f);
            valueEnd = valueCap;
            saturationStart = valueStart;
            saturationEnd = saturationCap;
        }
        else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
        {
            valueStart = UnityEngine.Random.Range(0.1f, 0.4f);
            valueEnd = UnityEngine.Random.Range(0.5f, 0.9f);
            saturationStart = valueStart - 0.1f;
            saturationEnd = saturationCap;
        }

        float h;
        float s;
        float v;

        Color.RGBToHSV(seedColor, out h, out s, out v);

        //float hueRange = Random.Range(0f,0.25f);
        float hueRange = Mathf.Lerp(0.1f, 0.5f, Mathf.Pow(UnityEngine.Random.value, 3));
        float hue;
        float darkest = UnityEngine.Random.value * 0.5f;
        float lightest = UnityEngine.Random.value;
        float desaturateMax = UnityEngine.Random.value;
        float dark;
        float light;
        float desat;
        float coin;

        Color color = new Color();

        // Fill in the first range
        for (int i = 0; i < size; i++)
        {
            if (type == GradientType.SingleColor)
            {
                hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)((size * 2f) - 1f));
            }
            else
            {
                hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)(size - 1f));
            }

            if (hue > 1) hue -= 1;
            if (hue < 0) hue += 1;

            coin = UnityEngine.Random.value;
            dark = Mathf.Lerp(0, darkest, UnityEngine.Random.value);
            light = Mathf.Lerp(0, lightest, UnityEngine.Random.value);

            if (type == GradientType.WarmCool)
            {
                color = Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueEnd, valueStart, i / (float)(size - 1)), v),
                        Color.HSVToRGB(hue, s, Mathf.Lerp(saturationStart, saturationEnd, i / (float)(size - 1))), 0.5f);

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }
            else if (type == GradientType.SingleColor)
            {
                color = Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)));

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }
            else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
            {
                color = Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v);

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }

            // desaturate world
            if (UnityEngine.Random.value < 0.25f)
            {
                desat = Mathf.Lerp(0, desaturateMax, UnityEngine.Random.value);
                color = Tile.Desaturate(color, desat);
            }

            if (type == GradientType.SingleColor || UnityEngine.Random.value < 0.05f)
            {
                color = Tile.Inverse(color);
            }

            gradients.Add(color);
        }

        // Flip the color if needed, and apply a small skew
        if (type == GradientType.WarmCool || type == GradientType.ValueColor)
        {
            h = h + 0.5f;
            h = h + UnityEngine.Random.Range(0, 0.25f) - 0.125f;
            if (h > 1) h -= 1;
        }

        // Fill in the second range
        for (int i = 0; i < size; i++)
        {
            if (type == GradientType.SingleColor)
            {
                hue = Mathf.Lerp(h + hueRange, h - hueRange, i + size / (float)((size * 2f) - 1f));
            }
            else
            {
                hue = Mathf.Lerp(h + hueRange, h - hueRange, i / (float)(size - 1f));
            }

            if (hue > 1) hue -= 1;
            if (hue < 0) hue += 1;

            coin = UnityEngine.Random.value;
            dark = Mathf.Lerp(0, darkest, UnityEngine.Random.value);
            light = Mathf.Lerp(0, lightest, UnityEngine.Random.value);

            if (type == GradientType.WarmCool)
            {
                // Use the range to vary the value, then flip it and vary the saturation
                color = Color.Lerp(Color.HSVToRGB(hue, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)), v),
                    Color.HSVToRGB(hue, s, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1))), 0.5f);

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }
            else if (type == GradientType.SingleColor)
            {
                color = Color.HSVToRGB(hue, Mathf.Lerp(saturationEnd, saturationStart, i / (float)(size - 1f)), v);

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }
            else if (type == GradientType.DarkLight || type == GradientType.ValueColor)
            {
                color = Color.HSVToRGB(hue, s, Mathf.Lerp(valueStart, valueEnd, i / (float)(size - 1)));

                if (coin < 0.25f)
                {
                    color = Tile.Darken(color, dark);
                }
                else if (coin < 0.5f)
                {
                    color = Tile.Brighten(color, dark);
                }
                else if (coin < 0.75f)
                {
                    color = Tile.Lighten(color, light);
                }
            }

            // desaturate world
            if (UnityEngine.Random.value < 0.5f)
            {
                desat = Mathf.Lerp(0, desaturateMax, UnityEngine.Random.value);
                color = Tile.Desaturate(color, desat);
            }

            if (type == GradientType.SingleColor || UnityEngine.Random.value < 0.05f)
            {
                color = Tile.Inverse(color);
            }

            gradients.Add(color);

        }

        for (int i = 0; i < swaps; i++)
        {
            int i1 = UnityEngine.Random.Range(0, gradients.Count);
            int i2 = UnityEngine.Random.Range(0, gradients.Count - 1);
            if (i2 >= i1)
            {
                i2++;
            }
            Color temp = gradients[i1];
            gradients[i1] = gradients[i2];
            gradients[i2] = temp;
        }

        return gradients;
    }
}

