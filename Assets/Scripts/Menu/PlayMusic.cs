using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.IO;

public class PlayMusic : MonoBehaviour {
	
	public AudioMixerSnapshot volumeDown;			//Reference to Audio mixer snapshot in which the master volume of main mixer is turned down
	public AudioMixerSnapshot volumeUp;				//Reference to Audio mixer snapshot in which the master volume of main mixer is turned up

	FileInfo[] songFiles;
	public int numberOfSongs;
	public int currentTrack;

	public AudioSource musicSource;				//Reference to the AudioSource which plays music

	public bool playing = false;

	void Awake () 
	{
		//Get a component reference to the AudioSource attached to the UI game object
		musicSource = GetComponent<AudioSource> ();
		musicSource.loop = false;

		//Get a list of mp3s in the StreamingAssets folder
		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
		songFiles = dir.GetFiles("*.ogg");

		numberOfSongs = songFiles.Length;
	}
	
	//Used if running the game in a single scene, takes an integer music source allowing you to choose a clip by number and play.
	public void PlaySelectedMusic(int musicChoice)
	{
		currentTrack = musicChoice;
		playing = true;

		//Play the music clip at the array index musicChoice
		musicChoice = musicChoice % songFiles.Length;
		WWW request = new WWW("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, songFiles[musicChoice].Name));

		StartCoroutine(AwaitAudioClip(request));
	}
		
	public void FadeUp(float fadeTime)
	{
		volumeUp.TransitionTo(fadeTime);
	}
		
	public void FadeDown(float fadeTime)
	{
		volumeDown.TransitionTo(fadeTime);
	}
		
	IEnumerator AwaitAudioClip(WWW request)
	{
		yield return request;

		musicSource.clip = request.GetAudioClip(false, true);
		musicSource.Play();

		Game.Log(songFiles[currentTrack].Name.Split('.')[0]);

		Invoke("FadeOut", musicSource.clip.length - 2f);

		// Make sure songs don't play too close to each other
		Invoke("AllowSong", musicSource.clip.length * 2f);
	}

	public void FadeOut()
	{
		FadeDown(2f);
	}

	public void StopPlaying()
	{
		musicSource.Stop();
		CancelInvoke("FadeOut");
	}

	void AllowSong()
	{
		playing = false;
	}
}
