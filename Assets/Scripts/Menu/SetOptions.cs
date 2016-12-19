using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetOptions : MonoBehaviour {

	public AudioMixer mainMixer;
	public Text graphicsModeText;
	public Text worldSizeText;
	public Slider graphicsModeSlider;
	//public Toggle outlineToggle;

	void Start()
	{
		graphicsModeText.text = Config.GraphicsMode.ToString();
		graphicsModeSlider.value = (float)Config.GraphicsMode;
	}

	public void SetMusicLevel(float musicLvl)
	{
		mainMixer.SetFloat("musicVol", musicLvl);
	}
		
	public void SetSfxLevel(float sfxLevel)
	{
		mainMixer.SetFloat("sfxVol", sfxLevel);
	}

	public void SetGraphicsLevel(float value)
	{
		Config.GraphicsMode = (GraphicsMode)(int)value;
		graphicsModeText.text = Config.GraphicsMode.ToString();
	}
}
