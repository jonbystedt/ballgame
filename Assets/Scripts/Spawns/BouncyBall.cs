using UnityEngine;
using System.Collections;

public enum BallType
{
	Basic,
	Exploding,
	Imploding,
	Moon,
	DarkStar
}

public class BouncyBall : SpawnedObject 
{
	public int scoreModifier = 1;
	public float growthRate = 2f;
	public float shrinkRate = 0.5f;
	public float massIncrease = 0f;
	public float massDecrease = 0f;
	public float maxSize = 10f;
	public float minSize = 1f;

	public bool explodeAtMax = false;
	public bool explodeAtMin = false;

	public Spawns SpawnObject =  Spawns.SilverPickup;
	public float SpawnIncrement = 0.1f;
	public float SpawnValue = 0.1f;
	public int SpawnCount
	{
		get { return Mathf.FloorToInt(SpawnValue); }
	}
	public BallType type = BallType.Basic;

	public bool exploding = false;

	public override void Reset()
	{
		base.Reset();
		exploding = false;
	}

	protected override void SlowUpdate() 
	{
		base.SlowUpdate();

		if (inRange && !_renderer.enabled)
		{
			_renderer.enabled = true;
		}
	}

	protected override void Sleep()
	{
		SpawnManager.SleptBalls.Add(this);
	}

	protected override void Wake()
	{
		SpawnManager.SleptBalls.Remove(this);
	}

	public void Grow(float velocity)
	{
		if (transform.localScale.x < maxSize)
		{
			transform.localScale *= growthRate;
			GetComponent<Rigidbody>().mass *= massDecrease;
		}

		if (SpawnCount < Config.MaxItemSpawns)
		{
			SpawnValue += SpawnIncrement;
		}

		if (transform.localScale.x > maxSize && explodeAtMax && !exploding)
		{
			//Game.Log(velocity.ToString("f2"));
			if (velocity < 10f)
			{
				Explode();
				StartCoroutine(Expand());
			}
			else
			{
				Split(velocity);
				StartCoroutine(Expand());
			}
		}
	}

	public void Shrink()
	{
		if (transform.localScale.x > minSize)
		{
			transform.localScale *= shrinkRate;
			GetComponent<Rigidbody>().mass *= massIncrease;
		}

		if (SpawnCount < Config.MaxItemSpawns)
		{
			SpawnValue += SpawnIncrement;
		}

		if (transform.localScale.x < minSize && explodeAtMin && !exploding)
		{
			Explode();
			StartCoroutine(Implode());
		}
	}

	void Explode()
	{
		exploding = true;

		WorldPosition chunkPos = World.GetChunkPosition(transform.position);

		// Add the spawns to the local column's spawn list so that they can be managed.
		Column column = World.GetColumn(chunkPos);	

		Color color = Tile.Colors[SpawnCount % 64];
		if (type == BallType.Moon)
		{
			color = Tile.Lighten(color, 0.3f);
		}
		if (type == BallType.DarkStar)
		{
			color = Tile.Darken(color, 0.3f);
		}

		if (SpawnCount > 0)
		{
			World.Spawn.Objects(SpawnObject, Tile.Colors[SpawnCount % 64], transform.position, SpawnCount, 0f, column.spawns);
		}

		StartCoroutine(Wait(1f, () => {
			isActive = false;
			ReturnToPool();
		}));
	}

	void Split(float velocity)
	{
		Column column = World.GetColumn(World.GetChunkPosition(transform.position));
		if (column != null)
		{
			Spawns spawn;
			Color spawnColor = color;
			if (type == BallType.Imploding)
			{
				spawn = Spawns.MysteryEgg;
			} 
			else if (type == BallType.Exploding)
			{
				spawn = Spawns.SuperBouncyBall;
			}
			else if (type == BallType.Moon)
			{
				spawn = Spawns.Moon;
				spawnColor = Color.white;
			}
			else
			{
				spawn = Spawns.DarkStar;
				spawnColor = Color.black;	
			}

			Vector3 scale = transform.localScale * 0.5f;
			float newSpawnValue = SpawnValue * 0.25f;
			int splits = 2;
			if (velocity > 15f)
			{
				splits = 3;
			}
			if (velocity > 20f)
			{
				splits = 5;
			}

			StartCoroutine(Repeat(splits, 0.1f, () => {
				PooledObject obj = World.Spawn.Object(spawn, color, mass, transform.position);
				if (obj != null)
				{
					obj.transform.localScale = scale;

					BouncyBall ball = obj.GetComponent<BouncyBall>();

					// Prevent moon explosions
					if (ball.type == BallType.Moon)
					{
						ball.growthRate = 1.05f;
					}

					ball.Activate(0.5f);
					ball.SpawnValue = newSpawnValue;

					column.spawns.Add(obj);
				}
			}));

			// Boom
			//SpawnValue /= 2f;
			Explode();
		}
	}

	public void Activate(float time)
	{
		exploding = true;
		StartCoroutine(Wait(time, () => {
			exploding = false;
		}));
	}

	IEnumerator Expand()
	{
		for (;;)
		{
			if (!isActive)
			{
				break;
			}

			if (!Game.PlayerActive)
			{
				yield return null;
			}

			transform.localScale *= 1.01f;

			yield return null;
		}
	}

	IEnumerator Implode()
	{
		for (;;)
		{
			if (!isActive)
			{
				break;
			}

			if (!Game.PlayerActive)
			{
				yield return null;
			}

			transform.localScale /= 1.01f;

			yield return null;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (exploding)
		{
			return;
		}

		if (other.gameObject.CompareTag("Pickup")) 
		{
			Pickup pickup = other.gameObject.GetComponent<Pickup>();
			if (pickup.isActive && pickup.isLive)
			{
				// Score points!
				Game.UpdateScore(pickup.baseScore * scoreModifier);

				if ((type == BallType.DarkStar && pickup.type == PickupType.Black) || (type == BallType.Moon && pickup.type == PickupType.Silver))
				{
					if (other.transform.localScale.x < 6f)
					{
						other.transform.localScale *= 1.2f;
						pickup.rotationSpeed *= 0.8f;
					}

				}
				else
				{
					// Explode if embiggened or return object to pool
					if (pickup.transform.localScale.x > 1f)
					{
						pickup.Explode();
					}
					else
					{
						pickup.isLive = false;
						pickup.RemoveIn(0.3f);
					}
				}
			}

			Shrink();
		}

		if (other.gameObject.CompareTag("Ball"))
		{
			BouncyBall ball = other.gameObject.GetComponent<BouncyBall>();
			if (!ball.exploding && transform.localScale.x <= 2f)
			{
				float velocity = Mathf.Abs(other.attachedRigidbody.velocity.x + other.attachedRigidbody.velocity.y + other.attachedRigidbody.velocity.z);
				float scaleRatio = ball.transform.localScale.x - transform.localScale.x;

				if (scaleRatio > 0 && Random.value < scaleRatio)
				{
					ball.Grow(velocity);
					ball.SpawnValue = ball.SpawnValue + SpawnValue;

					// Return to pool
					exploding = true;
					StartCoroutine(Wait(0.1f, () => {
						if (isActive)
						{
							ReturnToPool();
						}
					}));
				}
			}
		}
	}
}
