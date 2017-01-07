using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetOptions : MonoBehaviour {

	public AudioMixer mainMixer;
	public Text graphicsModeText;
	public Text worldSizeText;
	public Slider graphicsModeSlider;
	public Slider worldSizeSlider;

	void Start()
	{
		graphicsModeText.text = Config.GraphicsMode.ToString();
		graphicsModeSlider.enabled = false;
		graphicsModeSlider.value = (float)Config.GraphicsMode;
		graphicsModeSlider.enabled = true;
		worldSizeSlider.value = Mathf.Floor((Config.WorldSize / 2f) - 3f);
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
		if (Input.GetMouseButton(0))
		{
			Config.WorldSize = 8 + ((int)value * 4);
			worldSizeSlider.value = Mathf.Floor((Config.WorldSize / 2f) - 3f);
			worldSizeText.text = Config.WorldSize.ToString();
		}
	}

	public void SetWorldSize(float worldSize)
	{
		Config.WorldSize = Mathf.FloorToInt(6f + (worldSize * 2f));
		worldSizeText.text = Config.WorldSize.ToString();
	}

}
