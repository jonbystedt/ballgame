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

	public void SetMusicLevel(float musicLvl)
	{
		mainMixer.SetFloat("musicVol", musicLvl);
	}

	public void SetSfxLevel(float sfxLevel)
	{
		mainMixer.SetFloat("sfxVol", sfxLevel);
	}
}
