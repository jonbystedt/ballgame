using UnityEngine;
using System.Collections;

public enum PickupType
{
	Basic,
	Black,
	Silver
}

public class Pickup : SpawnedObject 
{
	public int baseScore = 10;
	public float rotationSpeed = 2f;
	public float driftIntensity = 250f;
	public PickupType type = PickupType.Basic;

	float impactForce;

	public override void Reset()
	{
		base.Reset();
	}

	protected override void SlowUpdate() 
	{
		base.SlowUpdate();
	}

	protected override void AddToSleepList()
	{
		SpawnManager.SleptPickups.Add(this);
	}

	protected override void RemoveFromSleepList()
	{
		SpawnManager.SleptPickups.Remove(this);
	}

	public void Rotate(Vector3 direction) 
	{
		transform.Rotate(direction * Time.deltaTime * rotationSpeed);
	}

	public void Activate(float time)
	{
		isLive = false;
		Invoke("SetLive", time);
	}

	void SetLive()
	{
		isLive = true;
	}

	protected override void DoAction()
	{
		// Drift by noise deriviative
		Vector3 force = NoiseGenerator.SumWithDerivative(
			NoiseGenerator.Value3D, 
			World.GetBlockPosition(transform.position).ToVector3(), 
			NoiseConfig.terrain.frequency.value, 
			NoiseConfig.terrain.octaves, 
			NoiseConfig.terrain.lacunarity, 
			NoiseConfig.terrain.persistance
		);

		// Rotate force by hue
		force = Vector3.RotateTowards(force, -force, Mathf.Lerp(0f, 6.28319f, hsvColor.h), 0f);

		transform.GetComponent<Rigidbody>().AddForce(force * driftIntensity * 10);
	}

	public void Explode()
	{
		Column column = World.GetColumn(World.GetChunkPosition(transform.position));
		if (column != null)
		{
			StartCoroutine(ExplodeRoutine(column));
		}
	}

	public void Score()
	{
		GetComponent<PlayHitSound>().PlayScoreSound(impactForce);
	}

	IEnumerator ExplodeRoutine(Column column)
	{
		Color spawnColor;
		Vector3 pos;
		Spawns spawn;

		isLive = false;

		if (type == PickupType.Black)
		{
			spawn = Spawns.BlackPickup;
		}
		else
		{
			spawn = Spawns.SilverPickup;
		}

		float size = Mathf.Ceil(transform.localScale.x);

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					pos = new Vector3(
						transform.position.x - (transform.localScale.x * 0.5f) + ((1 / (size * 2)) * transform.localScale.x) + ((x / size) * transform.localScale.x),
						transform.position.y - (transform.localScale.y * 0.5f) + ((1 / (size * 2)) * transform.localScale.y) + ((x / size) * transform.localScale.x),
						transform.position.z - (transform.localScale.z * 0.5f) + ((1 / (size * 2)) * transform.localScale.z) + ((x / size) * transform.localScale.x)
					);
					//spawnColor = Tile.Colors[(x + y + z) % 64];
					spawnColor = Color.Lerp(color, Tile.Inverse(Tile.Brighten(color, 0.5f)), (x * y * z) / (size * size * size));
					PooledObject obj = World.Spawn.Object(spawn, spawnColor, mass, pos, 0f);
					if (obj != null)
					{
						column.spawns.Add(obj); 
						Pickup pickup = obj.GetComponent<Pickup>();
						pickup.Activate(0.4f);
						//SpawnManager.Pickups.Add(pickup);
					}
					_renderer.enabled = !_renderer.enabled;
					yield return new WaitForSeconds(Config.SpawnTiming);
				}
			}
		}
		_renderer.enabled = true;
		transform.localScale = new Vector3(1f, 1f, 1f);
		ReturnToPool();
	}

	public void Fireworks()
	{
		if (transform.localScale.x > 1f)
		{
			return;
		}

		explosion.Stop();
		explosion.Clear();

		float forcePow = impactForce * impactForce;

		var ma = explosion.main;
		if (type == PickupType.Silver || type == PickupType.Black)
		{
			ma.simulationSpeed = Mathf.Lerp(0.5f, 1f, forcePow);

			ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
			p.startLifetime = Mathf.Lerp(1f, 2f, impactForce);
			p.startSize = 0.25f;

			if (type == PickupType.Silver)
			{
				p.startColor = Tile.Brighten(Tile.Lighten(color, 0.75f), 1f);
			}
			else
			{
				p.startColor = Tile.Brighten(Tile.Lighten(color, 0.25f), 1f);
			}

			explosion.Emit(p, Mathf.FloorToInt(Mathf.Lerp(4f, 40f, forcePow)));
			World.Spawn.Objects(Spawns.Pickup, Tile.Inverse(color), transform.position, 4, Config.SpawnDelay, 0f);
		}
		else
		{
			ma.simulationSpeed = Mathf.Lerp(0.5f, 1f, forcePow);

			ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
			p.startLifetime = Mathf.Lerp(1f, 2f, forcePow);
			p.startSize = 0.25f;
			p.startColor = Tile.Brighten(color, 1f);

			explosion.Emit(p, Mathf.FloorToInt(Mathf.Lerp(4f, 24f, forcePow)));
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Ball"))
		{
			Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
			Rigidbody c_rigidbody = other.transform.GetComponent<Rigidbody>();

			impactForce = GameUtils.GetImpactForce(rigidbody, c_rigidbody);

			Fireworks();
		}
	}
}
