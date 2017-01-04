using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseOptions : MonoBehaviour {

	public AudioMixer mainMixer;

	public void SetOutline(bool enabled)
	{
		//Config.Outlines = enabled;
	}

	public void SetMusicLevel(float musicLevel)
	{
		mainMixer.SetFloat("musicVol", Mathf.Lerp(-80f, 0f, musicLevel));
		Config.MusicVolume = Mathf.FloorToInt(musicLevel * 100f);
	}
		
	public void SetSfxLevel(float sfxLevel)
	{
		mainMixer.SetFloat("sfxVol", Mathf.Lerp(-80f, 0f, sfxLevel));
		Config.SfxVolume = Mathf.FloorToInt(sfxLevel * 100f);
	}
}
