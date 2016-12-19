using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour {

	public float speed;
	public float jumpSpeed;
	public float boost;
	public float boostDuration;

	public float groundPoundForce = -50f;
	public float slamForce = 50f;
	public float assistMinDistance = 0.01f;
	public float gravityAssist = -7.0f;
	public float boostForce = 10f;
	public float drag = 0.5f;
	public float scaleFactor = 3f;
	public float maxVelocity = 18f;

	public GameObject followCamera;
	public GameObject moon;

	bool jumping = false;
	bool airborne = true;
	bool hovering = false;
	bool superboost = false;
	bool slam = false;

	Rigidbody _rigidbody;
	Vector3 lastPosition;
	Vector3 lastContact;

	List<PooledObject> blockSpawns = new List<PooledObject>();

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody>();
		lastPosition = transform.position;
		lastContact = transform.position;
		StartCoroutine("CheckOutOfBounds");
	}

	void Update()
	{
		if (Input.GetButton("Fire1"))
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

						Block block = World.GetBlock(editBlock);

						if (block is BlockAir)
						{
							VoxelEditor.SetBlock(editBlock, new BlockGlass(0));
						}
					}
				}
			}
		}
	}

	void LateUpdate()
	{
		moon.transform.position = new Vector3(transform.position.x, Mathf.Lerp(moon.transform.position.y , transform.position.y, 0.1f), transform.position.z);
		lastPosition = transform.position;
	}

	void FixedUpdate () 
	{
		Vector3 force = GetPlayerForce();
		float velocity = _rigidbody.velocity.x + _rigidbody.velocity.z;

		if (velocity < maxVelocity || superboost)
		{
			_rigidbody.AddForce(force * speed * 100, ForceMode.Acceleration);
		}

		_rigidbody.AddTorque(new Vector3(force.z * 1000f, 0, -force.x * 1000f) * speed, ForceMode.Impulse);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Block") == true)
		{
			float impact = (_rigidbody.velocity.x + _rigidbody.velocity.y + _rigidbody.velocity.z) * _rigidbody.mass;
			impact *= Mathf.Sign(impact);
			impact /= 1000;

			if (superboost && impact > 0.1f)
			{
				//Game.Log("Hit: " + impact.ToString());
				if (slam)
				{
					SlamBlocks(collision);
				}
				else
				{
					BashBlocks(collision);
				}
			}

			CheckIfAirborne(collision);
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.CompareTag("Block") == true) 
		{
			Invoke("activateAirborne", 0.05f);
			lastContact = transform.position;
		}
	}

	void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.CompareTag("Block")) 
		{
			CheckIfAirborne(collision);
			BashBlocks(collision);
		}
	}

	void CheckIfAirborne(Collision collision)
	{
		WorldPosition colBlock = World.GetBlockPosition(collision.contacts[0].point - collision.contacts[0].normal / 2f);
		WorldPosition playerPos = World.GetBlockPosition(transform.position);
		Direction dir = WorldUtils.GetDirection(playerPos, colBlock);

		if (dir.vertical == RelativeHeight.down && dir.direction == RelativeDirection.none)
		{
			jumping = false;
			slam = false;
			CancelInvoke("activateAirborne");
			airborne = false;
		}
	}

	void BashBlocks(Collision collision)
	{
		Vector3 impulse = collision.impulse / Time.fixedDeltaTime;
		float force = impulse.x + impulse.y + impulse.z;
		force *= Mathf.Sign(force);
		force /= 1000f; 

		float impact = (_rigidbody.velocity.x + _rigidbody.velocity.y + _rigidbody.velocity.z) * _rigidbody.mass;
		impact *= Mathf.Sign(impact);
		impact /= 1000f;

		//Game.Log("Impact: " + impact.ToString("N3") + " Force: " + force.ToString("N3"));

		foreach(ContactPoint cp in collision.contacts)
		{
			WorldPosition bashBlock = World.GetBlockPosition(cp.point - cp.normal / 2f);
			WorldPosition playerPos = World.GetBlockPosition(transform.position);

			if (bashBlock.y > -48 && ((bashBlock.y == playerPos.y && superboost && force > 10f && impact > 0.1f) || bashBlock.y > playerPos.y))
			{
				Block block = World.GetBlock(bashBlock);

				if (!(block is BlockAir))
				{
					if (block is BlockGlass)
					{
						BreakGlass(bashBlock, playerPos);
					}

					VoxelEditor.SetBlock(bashBlock, new BlockAir(), true);

					Color color = new Color(block.color.r, block.color.g, block.color.b);
					SpawnPickupsFromBlock(bashBlock, color);
				}
			}
		}
	}

	void SlamBlocks(Collision collision)
	{
		foreach(ContactPoint cp in collision.contacts)
		{
			WorldPosition bashBlock = World.GetBlockPosition(cp.point - cp.normal / 2f);
			WorldPosition playerPos = World.GetBlockPosition(transform.position);

			//RelativeLocation rLoc = WorldUtils.GetDirection(playerPos, bashBlock);

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					for (int z = -1; z <= 1; z++)
					{
						WorldPosition pos = new WorldPosition(bashBlock.x + x, bashBlock.y + y, bashBlock.z + z);
						Block block = World.GetBlock(pos);

						if (!(block is BlockAir) && bashBlock.y > -48)
						{
							if (block is BlockGlass)
							{
								BreakGlass(bashBlock, playerPos);
							}

							VoxelEditor.SetBlock(pos, new BlockAir(), true);

							Color color = new Color(block.color.r, block.color.g, block.color.b);
							SpawnPickupsFromBlock(bashBlock, color);

						}
					}
					//WorldPosition pos = WorldUtils.PositionOnPlane(bashBlock, rLoc, x, y);

				}
			}

		}
	}

	void BreakGlass(WorldPosition blockPos, WorldPosition playerPos)
	{
		Direction dir = WorldUtils.GetDirection(playerPos, blockPos);

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
				{
					continue;
				}

				WorldPosition breakPos = WorldUtils.PositionOnPlane(blockPos, dir, x, y);
				Block block = World.GetBlock(breakPos);

				if (block is BlockGlass)
				{
					VoxelEditor.SetBlock(breakPos, new BlockAir(), true);
				}
			}
		}
	}

	Vector3 GetPlayerForce()
	{
		Vector3 motion = Vector3.zero;
		Vector3 force = Vector3.zero;

		bool jump = Input.GetButtonDown("Jump");
		bool hold = Input.GetButton("Jump");
		superboost = Input.GetButton("Fire2");

		float verticalInput = Input.GetAxis("Vertical");
		float horizontalInput = Input.GetAxis("Horizontal");

		motion = (verticalInput * followCamera.transform.forward) + (horizontalInput * followCamera.transform.right);
		//motion = (verticalInput * Vector3.forward) + (horizontalInput * Vector3.right);
			motion.y = 0.0f;
			
		// Ground pound
		if (superboost && airborne) {
			if (motion == Vector3.zero)
			{
				hovering = false;
				force = new Vector3(0, groundPoundForce, 0);
				slam = true;
			}
			else
			{
				if (!slam)
				{
					force = new Vector3 (motion.x * slamForce, 0.0f, motion.z * slamForce);
					slam = true;
				}
			}
		}
		// Jump
		else if (!jumping && jump) 
		{
			force = new Vector3 (motion.x, jumpSpeed, motion.z);
			jumping = true;
			hovering = true;
			Invoke("cancelBoost", boostDuration / 100f);
		}
		// Hover
		else if (hovering && hold) 
		{
			force = new Vector3 (motion.x / 10f, boost, motion.z / 10f);
		}
		// Falling
		else if (airborne)
		{
			RaycastHit hit;
			if (!Physics.Raycast(transform.position, Vector3.down, out hit, assistMinDistance))
			{
				force = new Vector3 (motion.x / 10f, gravityAssist, motion.z / 10f);
			}
		}
		// Roll Boost
		else if (superboost)
		{
			force = new Vector3 (motion.x * boostForce, 0.0f, motion.z * boostForce);
		}
		// Rolling
		else  
		{
			force = new Vector3(motion.x * scaleFactor, drag, motion.z * scaleFactor);
		}

		return force * Time.fixedDeltaTime;
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
						break;
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
		StartCoroutine( 
			Wait(2f, () => {
				for (int i = blockSpawns.Count - 1; i >= 0; i--)
				{
					PooledObject spawn = blockSpawns[i];
					if (spawn != null)
					{
						if (UnityEngine.Random.value < 0.95f)
						{
							Pickup pickup = spawn.GetComponent<Pickup>();
							pickup.Fireworks(1f);
							blockSpawns.RemoveAt(i);

							StartCoroutine(Wait(0.3f, () => {
								pickup.ReturnToPool();
							}));
						}
						else
						{
							blockSpawns.RemoveAt(i);
							Column column = World.GetColumn(bashBlock);	
							column.spawns.Add(spawn);
						}
					}
				}
			}
		));
	}

	void cancelBoost()
	{
		hovering = false;
	}
	
	void activateAirborne()
	{
		airborne = true;
	}

	IEnumerator CheckOutOfBounds()
	{
		for(;;) 
		{
			if (gameObject.transform.position.y < -65)
			{
				Vector3 force = -lastPosition;
				transform.position = lastContact;
				_rigidbody.AddForce(force * speed * 1000 * Time.smoothDeltaTime, ForceMode.Impulse);
			}
			
			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator Wait(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback();
	}
}
