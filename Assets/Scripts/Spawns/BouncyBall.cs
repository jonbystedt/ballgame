using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	public float corruption = 0.0f;
	public float maxBallSpeed = 25f;

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
	public bool actionEnabled = false;

	public BouncyBall closest;

	public WorldPosition lastBlockPosition;

	private int pickupCount;
	private int ballCount;
	private int maxPickup = 25;
	private int maxBall = 1000;

	public override void Reset()
	{
		base.Reset();
		exploding = false;
	}

	public override void Wipe()
	{
		closest = null;
		pickupCount = 0;
		ballCount = 0;

		if (Random.value - corruption > 0)
		{
			actionEnabled = false;
		}
	}

	protected override void SlowUpdate() 
	{
		base.SlowUpdate();

		if (inRange && !_renderer.enabled)
		{
			_renderer.enabled = true;
		}
	}

	protected override void AddToSleepList()
	{
		SpawnManager.SleptBalls.Add(this);
	}

	protected override void RemoveFromSleepList()
	{
		SpawnManager.SleptBalls.Remove(this);
	}

	protected override void DoAction()
	{
		if (!actionEnabled || closest == null || !closest.inRange || !closest.isActive)
		{
			return;
		}

		if (type == BallType.Basic)
		{
			if (pickupCount >= maxPickup)
			{
				actionEnabled = false;
				Split(100f * corruption);
				return;
			}

			if (ballCount >= maxBall)
			{
				actionEnabled = false;
				Explode();
				StartCoroutine(Expand());
				return;
			}

			if (transform.localScale.x < minSize)
			{
				actionEnabled = false;
				StartCoroutine(Wait(1f, () => {
					isActive = false;
					ReturnToPool();
				}));
				return;
			}
		}


		WorldPosition blockPosition = World.GetBlockPosition(transform.position);
		Vector3 d = closest.transform.position - transform.position;
		float difference = hsvColor.h - closest.hsvColor.h;
		if (difference > 0.5f)
		{
			difference = 1f - difference;
		}

		Vector3 force = Vector3.RotateTowards(-d, d, Mathf.Lerp(0f, 6.28319f, difference * 2f), 0f) * 10f;
		force = Vector3.ClampMagnitude(force, maxBallSpeed * _rigidbody.mass * 0.1f);

		if (blockPosition == lastBlockPosition)
		{
			force += Vector3.up * maxBallSpeed * _rigidbody.mass * 0.25f;
		}
		
		_rigidbody.AddForce(force, ForceMode.Impulse);

		lastBlockPosition = blockPosition;
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
			if (velocity < 15f)
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

		// Add the spawns to the local column spawn list so that they can be managed.
		Column column = World.GetColumn(chunkPos);	

		if (SpawnCount > 0)
		{
			StartCoroutine(Spawn());
		}
		else
		{
			StartCoroutine(Wait(0.5f, () => {
				exploding = false;
				isActive = false;
				ReturnToPool();
			}));
		}
	}

	IEnumerator Spawn()
	{
		int count = SpawnCount;

		if (type == BallType.Basic && count == 1)
		{
			count = 0;
		}

		while (count > 0)
		{
			Color color = Tile.Colors[TerrainGenerator.GetNoise3D(transform.position,NoiseConfig.pattern, NoiseType.SimplexValue) % 64];

			if (type == BallType.Moon)
			{
				color = Tile.Lighten(color, 0.2f);
			}
			if (type == BallType.DarkStar)
			{
				color = Tile.Darken(color, 0.2f);
			}

			World.Spawn.Object(SpawnObject, color, transform.position);
			count--;

			yield return null;
		}

		exploding = false;
		isActive = false;
		ReturnToPool();
	}

	void Split(float velocity)
	{
		Column column = World.GetColumn(World.GetChunkPosition(transform.position));
		if (column != null)
		{
			Spawns spawn;
			
			//Color spawnColor = hsvColor.ToColor();
			Vector3 scale = transform.localScale * 0.5f;
			float newSpawnValue = SpawnValue * 0.25f;

			int splits = 1;
			if (velocity > 20f)
			{
				splits = 2;
			}
			else if (velocity > 30f)
			{
				splits = 3;
			}
			else if (velocity > 50f)
			{
				splits = 4;
			}
			else if (velocity > 80f)
			{
				splits = 5;
			}

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
			}
			else if (type == BallType.DarkStar)
			{
				spawn = Spawns.DarkStar;
			}
			else 
			{
				spawn = Spawns.BouncyBall;
				newSpawnValue = 1f;
				scale = transform.localScale * 0.95f;
				splits *= 10;
			}

			List<ProceduralToolkit.ColorHSV> newColors = hsvColor.GetAnalogousPalette(splits);
			int count = 0;

			StartCoroutine(Repeat(splits, 0.1f, () => 
			{
				PooledObject obj = World.Spawn.Object(spawn, newColors[count].ToColor(), mass, transform.position);
				count++;

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
		if (gameObject.activeSelf)
		{
			exploding = true;
			StartCoroutine(Wait(time, () => {
				exploding = false;
			}));
		}
	}

	IEnumerator Expand()
	{
		for (;;)
		{
			if (!isActive || !exploding)
			{
				break;
			}

			if (!Game.PlayerActive)
			{
				yield return null;
			}

			if (transform.localScale.x < maxSize * 2f)
			{
				transform.localScale *= 1.02f;
			}
			else
			{
				break;
			}
			
			yield return null;
		}
	}

	IEnumerator Implode()
	{
		for (;;)
		{
			if (!isActive || !exploding)
			{
				break;
			}

			if (!Game.PlayerActive)
			{
				yield return null;
			}

			if (transform.localScale.x > minSize * 4f)
			{
				transform.localScale /= 1.02f;
			}
			else
			{
				break;
			}

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
				actionEnabled = true;
				corruption += 0.01f;
				pickupCount++;
				ballCount = 0;

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
						pickup.Score();
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
				corruption += 0.01f;
				actionEnabled = true;
				ballCount++;

				float velocity = Mathf.Abs(other.attachedRigidbody.velocity.x + other.attachedRigidbody.velocity.y + other.attachedRigidbody.velocity.z);
				float scaleRatio = ball.transform.localScale.x - transform.localScale.x;

				if (scaleRatio > 0 && UnityEngine.Random.value < scaleRatio)
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
