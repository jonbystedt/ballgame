using System.Collections;
using UnityEngine;
using ProceduralToolkit.Examples;

public class Boids : MonoBehaviour {

	public MeshFilter meshFilter;
	private BoidController controller;
	private bool simulate = false;
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
		controller = new BoidController();

		// Set swarm size based on world size with random variance
		controller.swarmCount = Mathf.FloorToInt(Mathf.Lerp(
			100f + (100f * (Config.WorldSize - 6f) * 0.5f), 
			300f + (300f * (Config.WorldSize - 6f) * 0.5f), 
			Mathf.Pow(Random.value, 2)));

		float size = Config.WorldSize;
		if (size < 12)
		{
			size = 12;
		}
		controller.worldSphere = size * 4f;
        controller.innerSphere = controller.worldSphere - size * 1.25f;

		// A single mesh is shared by all boids
		var mesh = controller.Generate(Tile.Brighten(Tile.Colors[0], 0.8f), Tile.Colors[32]);

		if (meshFilter != null)
		{
			meshFilter.mesh = mesh;

			// Update the mesh bounds so that it becomes visible to the player
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
