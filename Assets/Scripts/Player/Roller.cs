using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Roller : MonoBehaviour
{
	[SerializeField] private float movePower = 1000f; 
	[SerializeField] private bool useTorque = true; 
	[SerializeField] private float maxAngularVelocity = 25f; 
	[SerializeField] private float jumpPower = 500f; 
	[SerializeField] private float maxHoverPower = 25f;
	[SerializeField] private float boostPower = 75f;
	[SerializeField] private float maxGravityAssist = 650f;
	[SerializeField] private float airResistance = 0.01f;
	[SerializeField] private float boostLength = 0.1f;
	[SerializeField] private float hoverTime = 2f;
	[SerializeField] private float lift = 25f;
	[SerializeField] private float stall = 50f;
	[SerializeField] private float torque = 0.1f;

	public bool asleep = true;
	public AnimationCurve hoverCurve;
	public AnimationCurve gravityCurve;
	float hoverPower;
	float gravityAssist;
	[HideInInspector] public CameraOperator camOp;

	bool boostReady = true;
	[HideInInspector] public bool groundPound = false;
	[HideInInspector] public bool boostIsActive = false;
	[HideInInspector] public bool grounded = true;
	bool jumpStarted = false;
	bool jumping = false;
	bool boosting = false;
	bool pounding = false;
	bool jumpEnded = true;
	bool jumpGrounded = true;
	bool boostEnded = true;
	bool groundPoundEnabled = true;
	bool freeFalling = false;
	bool groundPoundActive = true;

	bool gpOffInvoked = false;
	bool boostOffInvoked = false;
	bool resetGroundPound = false;

	int gravityAttenuation = 0;

	private Rigidbody _rigidbody;

	private Coroutine hoverRoutine;

	List<PooledObject> blockSpawns = new List<PooledObject>();

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();

		// Set the maximum angular velocity.
		_rigidbody.maxAngularVelocity = maxAngularVelocity;

		gravityAssist = maxGravityAssist;

		SetFirstPerson(camOp.FirstPerson);
	}

	public void SetFirstPerson(bool enabled)
	{
		if (enabled) 
		{
			 _rigidbody.mass = 1000f;
			_rigidbody.drag = 10f;
			_rigidbody.angularDrag = 2f;
			
			movePower = 100000f;
			jumpPower = 3000f;
			boostPower = 1.25f;	
		}
		else
		{
			_rigidbody.mass = 750f;
			_rigidbody.drag = 4f;
			_rigidbody.angularDrag = 2f;

			movePower = 70000f;
			jumpPower = 3500f;
			boostPower = 2f;
		}
		
	}

	public void Move(Vector3 moveDirection, bool jump, bool boost, bool pound)
	{
		// if (moveDirection == Vector3.zero)
		// {
		// 	_rigidbody.angularVelocity *= 0.8f;
		// }

		// *** SET FLAGS ***
		// player released jump
		if (jumping && !jump)
		{
			jumpEnded = true;
		}

		// player released boost
		if (boosting && !boost)
		{
			boostEnded = true;
		}

		// preserve value of jump for coroutines
		jumping = jump;
		boosting = boost;

		// check if player is on the ground
		ushort ground = World.GetBlock(World.GetBlockPosition(transform.position - Vector3.up * 0.75f));
		if (ground != Block.Null && ground != Block.Air && !jumpStarted)
		{
			grounded = true;
		} 
		else
		{
			grounded = false;
		}

		// player can only pound if they are not grounded
		if (!grounded && pound)
		{
			pounding = true;
		}
		else
		{
			pounding = false;
		}

		// are we ready for another jump? 
		if (jumpStarted && jumpEnded)
		{
			jumpEnded = false;
			jumpStarted = false;
			freeFalling = true;
		}

		// *** HANDLE HITTING THE GROUND ***
		if (grounded)
		{
			if (camOp.FirstPerson) 
			{
				if (!boosting && jumping)
				{
					HitGround();
				}
			}
			else
			{
				HitGround();
			}
			
		}

		// *** HANDLE BOOST ACTIVATION ***
		if (boostReady && boost)
		{
			ActivateBoost();
		}

		// *** HANDLE GROUND POUND ***
		if (pound && !grounded && groundPoundEnabled)
		{
			moveDirection = HandleGroundPound(moveDirection);
		}

		// *** HANDLE BOOST ***
		if (boost && boostIsActive)
		{
			moveDirection = HandleBoost(moveDirection);
		}

		// *** UPWARDS FORCE ***
		ApplyUpwardsForce(moveDirection);

		// *** TORQUE AND LATERAL FORCE ***
		ApplyLateralForce(moveDirection);

		// *** GRAVITY ***
		if (!grounded)
		{
			ApplyGravity(moveDirection);
		}
	}

	void HitGround()
	{
		// world entry animation
		if (asleep)
		{
			//Game.CameraOp.FirstPerson = false;
			asleep = false;
		}

		jumpGrounded = true;

		gravityAssist = maxGravityAssist;
		gravityAttenuation = 0;

		StopAllCoroutines();

		if (freeFalling)
		{
			freeFalling = false;
		}
	}

	void ActivateBoost()
	{
		boostEnded = false;
		boostIsActive = true;
		boostReady = false;

		if (!boostOffInvoked)
		{
			Invoke("BoostOff", boostLength);
			boostOffInvoked = true;
		}
	}

	Vector3 HandleBoost(Vector3 moveDirection)
	{
		if (grounded || !camOp.FirstPerson)
		{
			moveDirection *= boostPower;
		}

		return moveDirection;
	}

	void BoostOff()
	{
		boostIsActive = false;
		boostReady = true;
		boostOffInvoked = false;
	}

	Vector3 HandleGroundPound(Vector3 moveDirection)
	{
		moveDirection += Vector3.down * boostPower * 100f;

		if (!groundPound)
		{
			groundPound = true;
		}

		return moveDirection;
	}

	void DisableGroundPound()
	{
		groundPound = false;
		groundPoundEnabled = false;
	}

	void ApplyUpwardsForce(Vector3 moveDirection)
	{
		// *** first, handle jumping ***
		if (grounded && jumping && !jumpStarted && jumpEnded)
		{
			//logString += "   Jump Started";
			jumpStarted = true;
			jumpGrounded = false;

			groundPoundEnabled = true;

			// ...add force in upwards. boost jump
			if (boosting && !camOp.FirstPerson)
			{
				float multiplier = Mathf.Lerp(20f, 10f, Mathf.Clamp01((moveDirection.x + moveDirection.y) / 5f));
				_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * jumpPower*multiplier, ForceMode.Impulse);
			}
			// regular jump
			else
			{
				_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * jumpPower*2f, ForceMode.Impulse);
			}
		
			// ...hover for a limited time
			hoverRoutine = StartCoroutine(Hover(hoverTime));
		}

		// *** then the case when the player is in the air and jump is still being held ***
		else if (jumping && !grounded && hoverPower > 0)
		{
			_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * hoverPower, ForceMode.Impulse);
		}
	}

	void ApplyLateralForce(Vector3 moveDirection)
	{
		if (useTorque)
		{
			// ... add torque around the axis defined by the move direction.
			_rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * movePower * torque);
			_rigidbody.AddForce(moveDirection * movePower);
		}
		else
		{
			// Otherwise just add force in the move direction.
			_rigidbody.AddForce(moveDirection * movePower);
		}
	}

	void ApplyGravity(Vector3 moveDirection)
	{
		//Game.Log((jumping ? "JUMPING" : " - ") + (jumpGrounded ? "JUMPGROUNDED" : " - ") + (groundPound ? "GP" : " - "));
		// *** FALLING ***
		// in the air and not jumping is falling, whether we have leaped or fallen off a cliff
		if ((!jumping || jumpGrounded) && !groundPound)
		{ 
			//logString += "   Falling";
			// slowly increase the gravity assist to the maximum. 
			if (gravityAssist == maxGravityAssist)
			{
				gravityAttenuation = 0;
			}

			// freeFalling indicates that we have fallen off a cliff and should increase much faster.
			gravityAssist = Mathf.Lerp(0f, maxGravityAssist - 1f, gravityAttenuation / (freeFalling ? 8f : 100f));
			gravityAttenuation++;

			// stop the hover power calculations from running
			//Game.Log("Killing Hover");
			StopAllCoroutines();
			
		}
		// regular assist, always applied unless boosting. affected by calculation above when falling.
		if (camOp.FirstPerson || !boosting || (boosting && !jumpEnded))
		{
			_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.down * gravityAssist, ForceMode.Impulse);
		}
		// when boosting, releasing jump should slowly descend, pressing jump should ascend
		else
		{
			if (Game.Player.transform.position.y < 64f)
			{
				if (jumping)
				{
					_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * gravityAssist * lift, ForceMode.Impulse);
				}
				else
				{
					_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.down * gravityAssist * stall, ForceMode.Impulse);
				}
			}
		}	
	}

	public void CreateSphere()
	{
		VoxelEditor.SetSphere(World.GetBlockPosition(transform.position), Blocks.Glass(0), 50);
	}

	public void CreateBlocks()
	{
		if (camOp.FirstPerson)
		{
			Vector3 camForward = Vector3.Scale(Game.MainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
			Vector3 editLocation = transform.position + camForward;

			WorldPosition editBlock = World.GetBlockPosition(editLocation);

			ushort block = World.GetBlock(editBlock);

			if (block == Block.Air)
			{
				VoxelEditor.SetBlock(editBlock, Blocks.Glass(0));
			}
		}
		else
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
	}

	public bool BashBlocks(Vector3 forward, float speed, bool boosting, bool firstPerson)
	{
		//Game.Log("Speed: " + speed.ToString("N2") + " Contacts: " + collision.contacts.Length);
		bool groundPounded = false;
		bool hit = false;
		List<Vector3> normals = new List<Vector3>();

		if (groundPoundEnabled && groundPound)
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
			if (!grounded || firstPerson)
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

			if (firstPerson && !grounded)
			{
				// Blocks above the player
				normals.Add(new Vector3(0,2,0));
				normals.Add(new Vector3(1,2,0));
				normals.Add(new Vector3(0,2,1));
				normals.Add(new Vector3(-1,2,0));
				normals.Add(new Vector3(0,2,-1));
				normals.Add(new Vector3(1,2,1));
				normals.Add(new Vector3(-1,2,1));
				normals.Add(new Vector3(1,2,-1));
				normals.Add(new Vector3(-1,2,-1));
			}

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
				int blockPosY = Mathf.RoundToInt(pos.y);
				// same level and boost button held
				if (b_pos.y == blockPosY  && boosting)
				{
					bash = true;
				}

				// 1 level below and ground pound active
				if (b_pos.y < blockPosY && b_pos.y > blockPosY - 2 && groundPound)
				{
					bash = true;
				}

				// overhead and jumping
				if (b_pos.y > blockPosY && (jumping || firstPerson))
				{
					bash = true;
				}
			}
			else
			{
				groundPounded = true;
			}

			if (bash)
			{
				ushort bashBlock = World.GetBlock(b_pos);

				if (bashBlock != Block.Null && bashBlock != Block.Air)
				{
					VoxelEditor.SetBlock(b_pos, Block.Air, true);
					hit = true;

					if (b_pos.y < Mathf.RoundToInt(pos.y))
					{
						groundPounded = true;
					}

					TileColor tileColor = Blocks.GetColor(bashBlock);
					Color color = new Color(tileColor.r, tileColor.g, tileColor.b);
			
					//if (Game.CameraOp.Distance > 5f)
					//{
						SpawnPickupsFromBlock(b_pos, color);
					//}
				}
			}
		}

		if (groundPounded)
		{
			if (!IsInvoking("DisableGroundPound"))
			{
				Invoke("DisableGroundPound", 0.05f);
			}
		}

		return hit;
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

			//Game.Log("Boost Hover");

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

			//Game.Log("Float Hover");

			yield return null;
		}
	}

	public bool GetAfterburnerState()
	{
		if (boosting || pounding)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}

