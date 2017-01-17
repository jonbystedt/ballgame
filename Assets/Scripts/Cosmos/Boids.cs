using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralToolkit;
using ProceduralToolkit.Examples;

public class Boids : MonoBehaviour {

	public MeshFilter meshFilter;
	private BoidController controller;
	private bool simulate = false;
	private List<ColorHSV> targetPalette = new List<ColorHSV>();
	private List<ColorHSV> currentPalette = new List<ColorHSV>();
	public Transform boidsTransform;

	private void Update()
	{
		if (simulate)
		{
			controller.Update();         
		}
	}

	public void StartBoids()
	{
		Generate();
		currentPalette.AddRange(targetPalette);
		simulate = true;
		StartCoroutine(Simulate());
	}

	public void StopBoids()
	{
		simulate = false;
		StopCoroutine(Simulate());
	}

	private void Generate()
	{
		targetPalette = RandomE.TetradicPalette(0.25f, 0.75f);
		targetPalette.Add(ColorHSV.Lerp(targetPalette[2], targetPalette[3], 0.5f));

		controller = new BoidController();
		controller.maxWorldSphere = 50f + (10f * (Config.WorldSize - 6f) * 0.5f);
		controller.swarmCount = Mathf.FloorToInt(Mathf.Lerp(200f + (200f * (Config.WorldSize - 6f) * 0.5f), 500f + (500f * (Config.WorldSize - 6f) * 0.5f), Mathf.Pow(Random.value, 2)));
		Game.Log(controller.swarmCount.ToString());

		var mesh = controller.Generate(targetPalette[0].WithSV(1, 1).ToColor(),
			targetPalette[1].WithSV(0.8f, 0.8f).ToColor());
		if (meshFilter != null)
		{
			meshFilter.mesh = mesh;
			meshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 500f);
		}
		
	}

	private IEnumerator Simulate()
	{
		while (true)
		{
			yield return StartCoroutine(controller.CalculateVelocities(boidsTransform));
		}
	}
}
