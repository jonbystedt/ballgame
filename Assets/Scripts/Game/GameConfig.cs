﻿using System;

[Serializable]
public struct GameConfig 
{
	public int musicVol { get; set; }
	public int sfxVol { get; set; }
	public int graphicsQuality { get; set; }
	public int worldSize { get; set; }
	public int spawnIntensity { get; set; }
	public bool swapInputs { get; set; }
	public string resolution { get; set; }
}
