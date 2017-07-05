using System;
using UnityEngine;

public class SampleRegion
{
	public int[,,] interpolates;
	public float[,,] samples;

	public int sampleRate;
	public Vector3 zoom;
	public int options;
	public NoiseMethod method;
	public Region region;

	public bool complete;
	public bool sampled;

	public SampleRegion(int o, NoiseMethod m, int sr, Vector3 z)
	{
		interpolates = null;
		samples = null;
		options = o;
		method = m;
		zoom = z;
		sampleRate = sr;
		complete = false;
		sampled = false;
	}
}

