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
			NoiseConfig.terrain.frequency, 
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
					PooledObject obj = World.Spawn.Object(spawn, spawnColor, mass, pos);
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

	public void Fireworks(float impactForce)
	{
		if (transform.localScale.x > 1f)
		{
			return;
		}

		explosion.Stop();
		explosion.Clear();

		var ma = explosion.main;
		if (type == PickupType.Silver || type == PickupType.Black)
		{
			ma.simulationSpeed = Mathf.Lerp(1f, 10f, Mathf.Pow(impactForce, 2));

			var col = explosion.colorOverLifetime;
			col.enabled = true;

			var gradient = GetFireworksGradient(color.GetHashCode());
			col.color = gradient;

			ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
			p.startLifetime = Mathf.Lerp(2f, 12f, impactForce);
			p.startColor = gradient.Evaluate(0f);

			if (type == PickupType.Silver)
			{
				p.startSize = 0.45f;
				explosion.Emit(p, Mathf.FloorToInt(Mathf.Lerp(10f, 100f, impactForce * impactForce)));
				World.Spawn.Objects(Spawns.Pickup, Tile.Inverse(color), transform.position, 4, Config.SpawnDelay, 0f);
			}
			else
			{
				p.startSize = 0.5f;
				explosion.Emit(p, Mathf.FloorToInt(Mathf.Lerp(8f, 80f, impactForce * impactForce)));
				World.Spawn.Objects(Spawns.Pickup, Tile.Inverse(color), transform.position, 4, Config.SpawnDelay, 0f);
			}

		}
		else
		{
			ma.simulationSpeed = Mathf.Lerp(1f, 7f, impactForce);

			ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
			p.startLifetime = Mathf.Lerp(1f, 10f, impactForce);
			p.startSize = 0.5f;
			p.startColor = color;

			explosion.Emit(p, Mathf.FloorToInt(Mathf.Lerp(8f, 50f, impactForce * impactForce)));
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Ball"))
		{
			Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
			Rigidbody c_rigidbody = other.transform.GetComponent<Rigidbody>();

			float impactForce = GameUtils.GetImpactForce(rigidbody, c_rigidbody);

			Fireworks(impactForce);
		}
	}

	ParticleSystem.MinMaxGradient GetFireworksGradient(int key)
	{
		ParticleSystem.MinMaxGradient gradient;
		if (World.Spawn.fireworksGradients.TryGetValue(key, out gradient))
		{
			return gradient;
		}

		Gradient grad = new Gradient();
		Color startColor;
		if (type == PickupType.Black)
		{
			startColor = Tile.Brighten(Tile.Lighten(color, 0.25f), 1f);
		}
		else
		{
			startColor = Tile.Lighten(color, 0.75f);
		}
		grad.SetKeys(
			new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(Tile.Lighten(color, 0.3f), 0.2f), new GradientColorKey(Tile.Brighten(color, 1f), 1.0f)},
			new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)}
		);


		gradient = new ParticleSystem.MinMaxGradient(grad);

		World.Spawn.fireworksGradients.Add(key, gradient);

		return gradient;
	}
}
