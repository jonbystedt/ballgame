using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class GameConfig 
{
	public int worldSize;
	public int graphicsQuality;
	public bool shadowsEnabled;
	public bool terrainCastsShadows;
	public int shadowDistance;
	public bool fogEnabled;
	public float fogScale;
	public bool terrainOutlines;
	public bool blockOutlines;
	public bool atmosphericScattering;
	public bool contactShadows;
	public bool godRays;
	public bool swapInputs;
	public int coroutineTiming;
}
