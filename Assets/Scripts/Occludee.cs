using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Occludee : MonoBehaviour {

	private bool _hidden;
	public bool hidden 
	{ 
		get
		{
			return _hidden;
		}
		set
		{
			_renderer.enabled = !value;
			_glassRenderer.enabled = !value;
			_hidden = value;
		}
	}

	bool _occlusionActive = false;
	public bool OcclusionActive
	{
		get
		{
			return _occlusionActive;
		}
		set
		{
			if (!value)
			{
				hidden = false;
			}
			_occlusionActive = value;
		}
	}
		
	private Chunk chunk;

	private Renderer _renderer;
	private Renderer _glassRenderer;
	private RaycastHit _hit;
	private Vector3 p;
	private Ray testRay;
	private Vector3 hitPoint;
	private Vector3[] chunkPoints;
	private int counter;
	private int frameInterval;

	private float distanceFromCam;

	void Awake () 
	{
		chunk = transform.GetComponent<Chunk>();

		_hit = new RaycastHit();
		chunkPoints = new Vector3[9];

		this.enabled = true;
	}

	void Start () 
	{
		_renderer = GetComponent<Renderer>();
		_glassRenderer = chunk.transparentChunk.GetComponent<Renderer>();

		Show();
	}

	// Called by World.cs in chunk update loop
	public void OcclusionUpdate() 
	{
		if (chunk.outofrange || !OcclusionActive)
		{
			return;
		}

		frameInterval = Time.frameCount % 4; // update every 4th frame

		if (frameInterval == 0)
		{
			Game.Log("Running...");

			// Center point
			chunkPoints[0] = new Vector3(
				transform.position.x + Chunk.Size / 2f,
				transform.position.y + Chunk.Size / 2f,
				transform.position.z + Chunk.Size / 2f
			);

			// Top four corners
			chunkPoints[1] = new Vector3(
				transform.position.x + Chunk.Size,
				transform.position.y + Chunk.Size,
				transform.position.z + Chunk.Size
			);
			chunkPoints[2] = new Vector3(
				transform.position.x + Chunk.Size,
				transform.position.y + Chunk.Size,
				transform.position.z
			);
			chunkPoints[3] = new Vector3(
				transform.position.x,
				transform.position.y + Chunk.Size,
				transform.position.z
			);
			chunkPoints[4] = new Vector3(
				transform.position.x,
				transform.position.y + Chunk.Size,
				transform.position.z + Chunk.Size
			);

			// Bottom four corners
			chunkPoints[5] = new Vector3(
				transform.position.x + Chunk.Size,
				transform.position.y,
				transform.position.z + Chunk.Size
			);
			chunkPoints[6] = new Vector3(
				transform.position.x + Chunk.Size,
				transform.position.y,
				transform.position.z
			);
			chunkPoints[7] = new Vector3(
				transform.position.x,
				transform.position.y,
				transform.position.z
			);
			chunkPoints[8] = new Vector3(
				transform.position.x,
				transform.position.y,
				transform.position.z + Chunk.Size
			);

			bool outOfSight = true;
			for (int i = 0; i < 9; i++)
			{
				float targetDistance = Vector3.Distance(Game.CameraPosition, chunkPoints[i]);
				testRay = new Ray(chunkPoints[i], Game.CameraPosition - chunkPoints[i]);

				if (Physics.Raycast(testRay, out _hit, Game.MainCamera.farClipPlane))
				{
					float hitDistance = Vector3.Distance(_hit.point, chunkPoints[i]);
					if (hitDistance >= targetDistance)
					{
						outOfSight = false;
						break;
					}
				}
//				else
// 				{
//					outOfSight = false;
//					break;
//				}
			}

			if (outOfSight && Time.frameCount - counter > 250)
			{
				Game.Log("Chunk Occluded @ " + chunk.pos.x.ToString() + ", " + chunk.pos.y.ToString() + ", " + chunk.pos.z.ToString());
				Hide();
			}
			else
			{
				Show();

				// Track time until the chunk can hide again
				counter = Time.frameCount;
			}
		}
	}

	public void Show()
	{
		if (hidden)
		{
			hidden = false;
		}
	}

	public void Hide()
	{
		if (!hidden)
		{
			hidden = true;
		}
	}
}
