using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.IO;
using System.Collections.Generic;
using System;

public class PlaySound : MonoBehaviour 
{
	public AudioSource soundSource;	
	
	[HideInInspector]
	public bool initialized = false;

	Dictionary<string,AudioClip[]> SoundLibrary = new Dictionary<string,AudioClip[]>();	

	int libCount;

	void Awake () 
	{
		//Get a component reference to the AudioSource attached to the UI game object
		soundSource = GetComponent<AudioSource> ();
		soundSource.loop = false;

		//Get a list of mp3s in the StreamingAssets folder
		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
		DirectoryInfo startDir = dir.GetDirectories("sounds")[0];

		BuildSoundLibrary(startDir);
	}

	void BuildSoundLibrary(DirectoryInfo startDir)
	{
		DirectoryInfo[] sounds = startDir.GetDirectories();

		for (int i = 0; i < sounds.Length; i++)
		{
			FileInfo[] notes = sounds[i].GetFiles("*.ogg");
			Array.Sort(notes, delegate(FileInfo f1, FileInfo f2) {
				return f1.Name.CompareTo(f2.Name);
			});

			SoundLibrary.Add(sounds[i].Name, new AudioClip[notes.Length]);
			libCount += notes.Length;

			for (int j = 0; j < notes.Length; j++)
			{
				WWW request = new WWW("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "sounds") + "/" + sounds[i].Name + "/" + notes[j].Name);
				Game.Log(request.url);
				StartCoroutine(AwaitAudioClip(request, sounds[i].Name, j));
			}

		}
	}

	IEnumerator AwaitAudioClip(WWW request, string name, int noteIndex)
	{
		yield return request;

		AudioClip[] clips = SoundLibrary[name];
		clips[noteIndex] = request.GetAudioClip(false, true);

		soundSource.PlayOneShot(clips[noteIndex], 1f);
		Game.Log(name + " " + noteIndex.ToString());

		libCount--;

		if (libCount == 0)
		{
			initialized = true;
		}
	}	
}