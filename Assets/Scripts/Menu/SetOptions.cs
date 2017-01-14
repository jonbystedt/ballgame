using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class SetOptions : MonoBehaviour {

	public AudioMixer mainMixer;
	public Text graphicsModeText;
	public Text worldSizeText;
	public Slider graphicsModeSlider;
	public Slider worldSizeSlider;
	public Dropdown resolutionDropdown;

	void Start()
	{
		graphicsModeText.text = Config.GraphicsMode.ToString();
		graphicsModeSlider.value = (float)Config.GraphicsMode;

		worldSizeText.text = Config.WorldSize.ToString();
		worldSizeSlider.value = Mathf.Floor((Config.WorldSize / 2f) - 3f);

		List<string> resolutions = new List<string>();
		int selected = 0;
		int selectIndex = 0;
		for(int i = 0; i < Screen.resolutions.Length; i++)
		{
			var res = Screen.resolutions[i];
			string resString = res.width.ToString() + "x" + res.height.ToString();
			if (!resolutions.Contains(resString))
			{
				if (res.height == Screen.height && res.width == Screen.width)
				{
					selected = selectIndex;
				}
				selectIndex++;
				resolutions.Add(resString);
			}	
		}
		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(resolutions);
		resolutionDropdown.value = selected;
	}

	public void SetMusicLevel(float musicLevel)
	{
		if (musicLevel == 0)
		{
			mainMixer.SetFloat("musicVol", -80f);
		}
		else
		{
			mainMixer.SetFloat("musicVol", Mathf.Lerp(-48f, 0f, musicLevel));
		}

		Config.MusicVolume = Mathf.FloorToInt(musicLevel * 100f);
	}
		
	public void SetSfxLevel(float sfxLevel)
	{
		if (sfxLevel == 0)
		{
			mainMixer.SetFloat("sfxVol", -80f);
		}
		else
		{
			mainMixer.SetFloat("sfxVol", Mathf.Lerp(-48f, 0f, sfxLevel));
		}
		
		Config.SfxVolume = Mathf.FloorToInt(sfxLevel * 100f);
	}

	public void SetGraphicsLevel(float value)
	{
		Config.GraphicsMode = (GraphicsMode)(int)value;
		graphicsModeText.text = Config.GraphicsMode.ToString();

		// TODO: This would be a problem if it were possible to use the UI with a controller
		// if (Input.GetMouseButton(0))
		// {
		// 	Config.WorldSize = 8 + ((int)value * 4);
		// 	worldSizeSlider.value = Mathf.Floor((Config.WorldSize / 2f) - 3f);
		// 	worldSizeText.text = Config.WorldSize.ToString();
		// }
	}

	public void SetWorldSize(float worldSize)
	{
		Config.WorldSize = Mathf.FloorToInt(6f + (worldSize * 2f));
		worldSizeText.text = Config.WorldSize.ToString();
	}

	public void SetResolution(int index)
	{
		string resolution = resolutionDropdown.options[index].text;
		string[] axes = resolution.Split('x');
		int width = Int32.Parse(axes[0]);
		int height = Int32.Parse(axes[1]);

		Screen.SetResolution(width, height, true);
		Config.Resolution = resolution;
	}

	public void SetResolution(string resolution)
	{
		string[] axes = resolution.Split('x');
		int width = Int32.Parse(axes[0]);
		int height = Int32.Parse(axes[1]);

		Screen.SetResolution(width, height, true);
		Config.Resolution = resolution;
	}

}
