using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Roller : MonoBehaviour
{
	[SerializeField] private float movePower = 1000; // The force added to the ball to move it.
	[SerializeField] private bool useTorque = true; // Whether or not to use torque to move the ball.
	[SerializeField] private float maxAngularVelocity = 25; // The maximum velocity the ball can rotate at.
	[SerializeField] private float jumpPower = 500; // The force added to the ball when it jumps.
	[SerializeField] private float maxHoverPower = 25;
	[SerializeField] private float boostPower = 75;
	[SerializeField] private float maxGravityAssist = 650;
	[SerializeField] private float airResistance = 0.01f;
	[SerializeField] private float boostLength = 0.1f;
	[SerializeField] private float hoverTime = 2f;

	public AnimationCurve hoverCurve;
	public AnimationCurve gravityCurve;
	float hoverPower;
	float gravityAssist;

	bool boostReady = true;
	[HideInInspector] public bool groundPound = false;
	[HideInInspector] public bool boostIsActive = false;
	[HideInInspector] public bool grounded = true;
	bool jumpStarted = false;
	bool jumping = false;
	bool boosting = false;
	bool jumpEnded = true;
	bool jumpGrounded = true;
	bool boostEnded = false;
	bool groundPoundFinished = false;
	bool freeFalling = false;
	bool groundPoundActive = true;

	bool gpOffInvoked = false;
	bool boostOffInvoked = false;
	bool resetGroundPound = false;

	int gravityAttenuation = 0;

	private const float groundRayLength = 0.5f; // The length of the ray to check if the ball is grounded.
	private Rigidbody _rigidbody;
	private Collider _collider;

	private Coroutine hoverRoutine;

	List<PooledObject> blockSpawns = new List<PooledObject>();

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_collider = _rigidbody.GetComponent<Collider>();

		// Set the maximum angular velocity.
		_rigidbody.maxAngularVelocity = maxAngularVelocity;

		gravityAssist = maxGravityAssist;
	}

	public void Move(Vector3 moveDirection, bool jump, bool boost, bool pound)
	{
		if (jumping && !jump)
		{
			jumpEnded = true;
		}
		if (boosting && !boost)
		{
			boostEnded = true;
		}

		// preserve value of jump for coroutines
		jumping = jump;
		boosting = boost;

		if (resetGroundPound)
		{
			resetGroundPound = false;
		}

		// get block below player
		ushort ground = World.GetBlock(World.GetBlockPosition(transform.position - Vector3.up * 0.5f));
		if (ground != Block.Null && ground != Block.Air && !jumpStarted)
		{
			grounded = true;
		} 
		else
		{
			grounded = false;
		}

		//String logString = "";

		// did we hit the ground?
		if (grounded)
		{
			jumpGrounded = true;

			//logString += "Grounded";

            gravityAssist = maxGravityAssist;
            gravityAttenuation = 0;

			StopAllCoroutines();

			if (groundPound)
			{
				if (!IsInvoking("DisableGroundPound"))
				{
					Invoke("DisableGroundPound", 0.1f);
				}
			}

			if (hoverRoutine != null)
			{
				hoverPower = maxHoverPower;
				StopCoroutine(hoverRoutine);

				//logString += ", Stop Hover";
			}

			if (freeFalling)
			{
				//logString += ", Stop Falling";
				freeFalling = false;
			}
		}

		// are we activating boost?
		if (boostReady && boost)
		{
			//logString += "   Boost";
			boostIsActive = true;
			boostReady = false;

			if (!boostOffInvoked)
			{
				//logString += ", Invoke Cancel Boost";
				Invoke("BoostOff", boostLength);
				boostOffInvoked = true;
			}

		}

		// enable ground pound again after boosting
		if (groundPoundFinished && boostEnded)
		{
			groundPoundFinished = false;
			boostEnded = false;
		}

		// ground pound
		if (pound)
		{
			if (!grounded && !groundPoundFinished)
			{
				moveDirection = Vector3.down * boostPower * 100f;

				if (!groundPound && !groundPoundFinished)
				{
					groundPound = true;
				}
			}
		}

		// handle boost
		if (boost && boostIsActive)
		{
			if (!grounded)
			{
				moveDirection *= boostPower;
			}
			else
			{
				moveDirection *= boostPower;
			}			
		}

		// are we ready for another jump?
		if (jumpStarted && jumpEnded)
		{
			//logString += "   Ready For Jump";
			jumpEnded = false;
			jumpStarted = false;
			freeFalling = true;
		}

		// handle jumping
		if (grounded && jumping && !jumpStarted && jumpEnded)
		{
			//logString += "   Jump Started";
			jumpStarted = true;
			jumpGrounded = false;

			// ...add force in upwards.
			_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * jumpPower*2f, ForceMode.Impulse);

			// ...hover for a limited time
			hoverRoutine = StartCoroutine(Hover(hoverTime));
		}

		// we are the air and jump is being held
		else if (jumping && !grounded && hoverPower > 0)
		{
			//logString += "   Jumping";
			_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * hoverPower, ForceMode.Impulse);
		}

		// using torque to rotate the ball...
		if (useTorque || !boostIsActive)
		{
			// ... add torque around the axis defined by the move direction.
			_rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * movePower * 0.25f);
			_rigidbody.AddForce(moveDirection * movePower);
		}
		else
		{
			//logString += "   Hovering";
			// Otherwise just add force in the move direction.
			_rigidbody.AddForce(moveDirection * movePower);
		}

		// gravity assist when in air
		if (!grounded)
		{
			if ((!jumping || jumpGrounded) && !boost && !groundPound)
			{ 
				//logString += "   Falling";
				if (gravityAssist == maxGravityAssist)
				{
					gravityAttenuation = 0;
				}
				gravityAssist = Mathf.Lerp(0f, maxGravityAssist - 1f, gravityAttenuation / (freeFalling ? 10f : 100f));
				gravityAttenuation++;

				StopAllCoroutines();
				
			}
			_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.down * gravityAssist, ForceMode.Impulse);
		}

		//logString += jumpGrounded ? "   JUMPGROUNDED" : "";
		//logString += jumpEnded ? "   JUMPENDED" : "";
		//logString += jumpStarted ? "   JUMPSTARTED" : "";
		//Game.Log(logString);
	}

	void BoostOff()
	{
		boostIsActive = false;
		//_rigidbody.mass -= 1000f;
		boostReady = true;
		boostOffInvoked = false;
	}

	void DisableGroundPound()
	{
		groundPound = false;
		resetGroundPound = true;
		//_rigidbody.mass -= 1000f;
		//_collider.material.bounciness = 0.01f;
	}

	public void CreateSphere()
	{
		VoxelEditor.SetSphere(World.GetBlockPosition(transform.position), Blocks.Glass(0), 50);
	}

	public void CreateBlocks()
	{
		for (int x = -1; x < 2; x++)
		{
			for (int y = -3; y < 0; y++)
			{
				for (int z = -1; z < 2; z++)
				{
					Vector3 editLocation = new Vector3(
						gameObject.transform.position.x + x, 
						gameObject.transform.position.y + y, 
						gameObject.transform.position.z + z);

					WorldPosition editBlock = World.GetBlockPosition(editLocation);

					ushort block = World.GetBlock(editBlock);

					if (block == Block.Air)
					{
						VoxelEditor.SetBlock(editBlock, Blocks.Glass(0));
					}
				}
			}
		}
	}

	public void BashBlocks(Vector3 forward, float speed, bool boosting)
	{
		//Game.Log("Speed: " + speed.ToString("N2") + " Contacts: " + collision.contacts.Length);
		bool groundPounded = false;
		List<Vector3> normals = new List<Vector3>();

		if (!groundPoundFinished && groundPound)
		{
			// Normals for a 3x3 square below the player
			normals.Add(Vector3.down);
			normals.Add(new Vector3(1,-1,0));
			normals.Add(new Vector3(0,-1,1));
			normals.Add(new Vector3(-1,-1,0));
			normals.Add(new Vector3(0,-1,-1));
			normals.Add(new Vector3(1,-1,1));
			normals.Add(new Vector3(-1,-1,1));
			normals.Add(new Vector3(1,-1,-1));
			normals.Add(new Vector3(-1,-1,-1));
		}
		else
		{
			// Try to hit with the forward normal, fall back to a radial search
			// TODO: be more selective with search angles
			if (jumping && !grounded)
			{
				// Blocks above the player
				normals.Add(Vector3.up);
				normals.Add(new Vector3(1,1,0));
				normals.Add(new Vector3(0,1,1));
				normals.Add(new Vector3(-1,1,0));
				normals.Add(new Vector3(0,1,-1));
				normals.Add(new Vector3(1,1,1));
				normals.Add(new Vector3(-1,1,1));
				normals.Add(new Vector3(1,1,-1));
				normals.Add(new Vector3(-1,1,-1));
			}
			else
			{
				normals.Add(forward);
				normals.Add(new Vector3(1,0,0));
				normals.Add(new Vector3(0,0,1));
				normals.Add(new Vector3(-1,0,0));
				normals.Add(new Vector3(0,0,-1));
				normals.Add(new Vector3(1,0,1));
				normals.Add(new Vector3(-1,0,1));
				normals.Add(new Vector3(1,0,-1));
				normals.Add(new Vector3(-1,0,-1));
			}
		}
			
		Vector3 pos = transform.position;

		foreach(Vector3 normal in normals)
		{
			float searchRadius = 1f;
			
			WorldPosition b_pos = new WorldPosition(
				Mathf.FloorToInt(pos.x + normal.x * searchRadius), 
				Mathf.FloorToInt(pos.y + normal.y * searchRadius), 
				Mathf.FloorToInt(pos.z + normal.z * searchRadius));

			bool bash = false;

			// Don't break the floor!
			if (b_pos.y > -48)
			{
				// same level and boost button held
				if (b_pos.y == Mathf.RoundToInt(pos.y) && boosting)
				{
					bash = true;
				}

				// 1 level below and ground pound active
				if (b_pos.y < Mathf.RoundToInt(pos.y) && b_pos.y > Mathf.FloorToInt(pos.y - 2) && !groundPoundFinished && groundPound)
				{
					bash = true;
				}

				// overhead and jumping
				if (b_pos.y > Mathf.FloorToInt(pos.y) && jumping)
				{
					bash = true;
				}
			}
			else
			{
				groundPoundFinished = true;
			}

			if (bash)
			{
				ushort bashBlock = World.GetBlock(b_pos);

				if (bashBlock != Block.Null && bashBlock != Block.Air)
				{
					VoxelEditor.SetBlock(b_pos, Block.Air, true);

					if (b_pos.y < Mathf.RoundToInt(pos.y))
					{
						groundPounded = true;
					}

					TileColor tileColor = Blocks.GetColor(bashBlock);
					Color color = new Color(tileColor.r, tileColor.g, tileColor.b);
			
					if (Game.CameraOp.Distance > 5f)
					{
						SpawnPickupsFromBlock(b_pos, color);
					}
				}
			}
		}

		if (groundPounded)
		{
			groundPoundFinished = true;
		}
	}

	void SpawnPickupsFromBlock(WorldPosition bashBlock, Color color)
	{
		// Spawn a cube of pickups to replace the block
		for (int x = 1; x < 4; x += 2)
		{
			for (int y = 1; y < 4; y += 2)
			{
				for (int z = 1; z < 4; z += 2)
				{
					if (UnityEngine.Random.value > Config.BlockSpawnChance)
					{
						continue;
					}

					PooledObject obj = World.Spawn.Object(
						Spawns.Pickup, 
						color, 
						new Vector3(bashBlock.x + 0.25f * x, bashBlock.y + 0.25f * y, bashBlock.z + 0.25f * z));

					if (obj != null)
					{
						blockSpawns.Add(obj);
					}
				}
			}
		}

		// Decimate them :)
		StartCoroutine(Wait(2f, () => {
			for(int i = blockSpawns.Count - 1; i >= 0; i--)
			{
				PooledObject spawn = blockSpawns[i];
				if (spawn != null)
				{
					if (UnityEngine.Random.value < 0.9f)
					{
						Pickup pickup = spawn.transform.GetComponent<Pickup>();
						pickup.Fireworks(0.5f);
						blockSpawns.RemoveAt(i);

						StartCoroutine(Wait(0.3f, () => {
							spawn.ReturnToPool();
						}));
					}
					else
					{
						blockSpawns.RemoveAt(i);
						Column column = World.GetColumn(bashBlock);	
						if (column != null)
						{
							column.AddSpawn(spawn);
						}
					}
				}
			}
		}));
	}

	IEnumerator Wait(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback();
	}

	IEnumerator Hover(float time)
	{
		float i = 0.0f;
		float j = 0.0f;
		float rate = 1.0f / time*0.5f;

		gravityAssist = 0f;
		hoverPower = maxHoverPower;

		while (i < 1.0f)
		{
			i += Time.deltaTime * rate;
			hoverPower = hoverCurve.Evaluate(i) * maxHoverPower;

			if (!jumping)
			{
				hoverPower = 0f;
				gravityAssist = maxGravityAssist;
				break;
			}

			yield return null;
		}

		while (j < 1.0f)
		{
			j += Time.deltaTime * rate;
			gravityAssist = gravityCurve.Evaluate(j) * maxGravityAssist;

			if (!jumping)
			{
				gravityAssist = maxGravityAssist;
				break;
			}

			yield return null;
		}
	}
}

