using UnityEngine;

public class CameraOperator : MonoBehaviour
{
	// time taken to move when avoiding clipping (low value = fast)
    public float clipMoveTime = 0.05f;  
	public float slowClipMoveTime = 0.1f;
	public float clipReturnTime = 0.4f;

	// time taken to move back towards desired position         
    public float returnTime = 0.4f; 

	 // the closest distance the camera can be from the target              
    public float closestDistance = 0.5f;            
	public float maxCameraHeight = 20f;
	public float targetAngle = -10f;
	public float playerDistance = 3f;
	private float cameraDistance;

	private bool firstPerson = false;

	public bool FirstPerson
	{
		get { return firstPerson; }
		set 
		{ 
			WorldPosition overhead = World.GetBlockPosition(
				new Vector3(
					Game.Player.transform.position.x, 
					Game.Player.transform.position.y + 1f, 
					Game.Player.transform.position.z)
					);

			ushort block = World.GetBlock(overhead);

			if (value && (block == Block.Air || block == Block.Null))
			{
				freeLookCamera.m_MoveSpeed = 100000f;
				_player.GetComponent<Roller>().SetFirstPerson(true);

				closestDistance = 0f;

				firstPerson = value;
			}
			else if (!value)
			{
				freeLookCamera.m_MoveSpeed = 10000f;
				_player.GetComponent<Roller>().SetFirstPerson(false);

				closestDistance = 1f;

				firstPerson = value;
			}
			 
		}
	}

	public float Distance
	{
		get { return cameraDistance; }
	}

	public MeshRenderer playerRenderer;
	public MeshRenderer outlineRenderer;
	public MeshRenderer solidRenderer;
	public AnimationCurve curve;
	//public Vector3 cameraPosition;

	public Camera _camera;                  // the transform of the camera
    Transform _pivot;                // the point at which the camera pivots around
	public Transform _player;

	float originalDistance;             // the original distance to the camera before any modification are made
	float moveVelocity;             // the velocity at which the camera moved
	float currentDistance;              // the current distance from the camera to the target
	float collisionDistance;
          
	float lastTargetDist;
	float targetDist;
	float moveTime;
	Vector3 cameraPosition;
	Vector3 cameraDirection;
	float spread;
	ushort cameraBlock;
	ushort cameraForwardBlock;

	ModifiedFreeLookCam freeLookCamera;

    void Start()
    {
        _pivot = _camera.transform.parent;
		freeLookCamera = GetComponent<ModifiedFreeLookCam>();

        originalDistance = _camera.transform.localPosition.magnitude;
        currentDistance = originalDistance;
		collisionDistance = originalDistance;

		lastTargetDist = playerDistance;
    }

	private void Update()
	{
		// TODO: Centralize all input handling
		if (Input.GetKeyDown (KeyCode.F) && Game.PlayerActive) 
		{
			FirstPerson = !firstPerson;
		}
	}
		

    private void LateUpdate()
	{
		if (!Game.PlayerActive)
		{
			_camera.transform.position = Game.CameraPosition;
			return;
		}

		cameraPosition = _camera.transform.position;

		// Normalized direction to the camera
		cameraDirection = (cameraPosition - Game.Player.transform.position).normalized;
		spread = 0.9f;

		cameraBlock = Block.Air;
		cameraForwardBlock = Block.Air;

		if (firstPerson)
		{
			targetDist = Mathf.Lerp(lastTargetDist, closestDistance, returnTime);
		}
		else
		{
			// Find the height of the camera
			float cameraHeight = (Mathf.Clamp(freeLookCamera.m_TiltAngle, 0, 360) + targetAngle) / (freeLookCamera.m_TiltMax + targetAngle);

			// Find target distance, given height
			targetDist = Mathf.Lerp(playerDistance, maxCameraHeight, cameraHeight);

			// Target camera position, constructed from angle and target distance.
			cameraPosition = Game.Player.transform.position + cameraDirection * targetDist;

			// block the camera is thought to be occupying
			cameraBlock = World.GetBlock(World.GetBlockPosition(cameraPosition));

			// if camera is above the bottom of the world
			if (cameraPosition.y >= -(Chunk.Size * (Config.WorldHeight - 1f)))
			{
				DoFreeBlockSearch(0f);
			}
			else
			{
				targetDist = closestDistance;
			}

			// Smooth movement towards the new target
			moveTime = currentDistance < targetDist 
				? (cameraBlock == Block.Air ? clipReturnTime : clipMoveTime)
				: (cameraBlock == Block.Air ? slowClipMoveTime : clipMoveTime);
			targetDist = Mathf.Lerp(lastTargetDist, targetDist, moveTime);
		}

		// Save the target distance
		lastTargetDist = targetDist;

		moveTime = currentDistance < targetDist 
						? (cameraForwardBlock == Block.Air ? returnTime: clipMoveTime)
						: (cameraForwardBlock == Block.Air ? slowClipMoveTime : clipMoveTime);
		// Smoothly move towards the target distance
		currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref moveVelocity, moveTime);

		// Clamp the distance between the closest allowed and the target distance
		currentDistance = Mathf.Clamp(currentDistance, closestDistance, targetDist);

		// Move the camera
		if (!float.IsNaN(currentDistance))
		{
			Vector3 localTranslation = -Vector3.forward * currentDistance; 
			_camera.transform.localPosition = localTranslation;

			cameraPosition = _camera.transform.position;

			if (freeLookCamera.m_TiltAngle < 0)
			{
				cameraPosition.y = Game.Player.transform.position.y;
			}
			if (FirstPerson)
			{
				cameraPosition.y += 1f;
			}
			_camera.transform.position = cameraPosition;
			Game.CameraPosition = cameraPosition;
		}

		// Record the actual distance from the camera now
		cameraDistance = Vector3.Distance(_camera.transform.position, _player.position);

		// player object renderer LOD
		if (firstPerson || cameraDistance <= 0.7f)
		{
			outlineRenderer.enabled = false;
			playerRenderer.enabled = false;
			solidRenderer.enabled = false;
		}
		else if (cameraDistance <= 2f)
		{
			playerRenderer.enabled = false;
			outlineRenderer.enabled = true;
			solidRenderer.enabled = false;
		}
		else if (cameraDistance <= 6f) {
			playerRenderer.enabled = true;
			outlineRenderer.enabled = true;
			solidRenderer.enabled = false;
		}
		else
		{
			playerRenderer.enabled = true;
			outlineRenderer.enabled = true;
			solidRenderer.enabled = true;
		}			
    }

	private void DoFreeBlockSearch(float startDistance)
	{
		float testDistance = startDistance;
		//bool lookingUp = (freeLookCamera.m_TiltAngle <= 0); 

		// Incremental search for a free location from the camera towards the player
		do 
		{
			// Position forward of target camera position
			Vector3 forwardPos = cameraPosition - cameraDirection * (testDistance + 1f);

			// Block at the forward position
			WorldPosition testBlockPosForward = World.GetBlockPosition(forwardPos);
			ushort tbForward = World.GetBlock(testBlockPosForward);
			cameraForwardBlock = tbForward;

			// If the forward position looks clear, perform further checks
			if (IsEmpty(tbForward))
			{
				// Block slightly above the forward position
				WorldPosition testSpreadUp = World.GetBlockPosition(forwardPos + _camera.transform.up * 
					Mathf.Lerp(spread/3f, spread, freeLookCamera.m_TiltAngle / 15f));

				// Block slightly below the forward position
				WorldPosition testSpreadDown = World.GetBlockPosition(forwardPos - _camera.transform.up * 
					Mathf.Lerp(spread/3f, spread, freeLookCamera.m_TiltAngle / 15f));

				// Block slightly left of the forward position
				WorldPosition testSpreadLeft= World.GetBlockPosition(forwardPos - _camera.transform.right * spread);

				// Block slightly right of the forward position
				WorldPosition testSpreadRight = World.GetBlockPosition(forwardPos + _camera.transform.right * spread);

				ushort tbUp = World.GetBlock(testSpreadUp);
				ushort tbDown = World.GetBlock(testSpreadDown);
				ushort tbLeft = World.GetBlock(testSpreadLeft);
				ushort tbRight = World.GetBlock(testSpreadRight);

				bool down = IsEmpty(tbDown);
				bool up  = IsEmpty(tbUp);
				bool left = IsEmpty(tbLeft);
				bool right = IsEmpty(tbRight);

				if (up && down && left && right)
				{
					if (Mathf.Abs(freeLookCamera.m_TiltAngle) <= 10)
					{
						break;
					}
					// Diagonals
					WorldPosition testSpreadUpLeft = World.GetBlockPosition(
						forwardPos + _camera.transform.up - _camera.transform.right * 
						Mathf.Lerp(spread/3f, spread/2f, freeLookCamera.m_TiltAngle / 15f)
						);
					WorldPosition testSpreadUpRight = World.GetBlockPosition(
						forwardPos + _camera.transform.up + _camera.transform.right * 
						Mathf.Lerp(spread/3f, spread/2f, freeLookCamera.m_TiltAngle / 15f)
						);
					WorldPosition testSpreadDownLeft = World.GetBlockPosition(
						forwardPos - _camera.transform.up - _camera.transform.right * 
						Mathf.Lerp(spread/3f, spread/2f, freeLookCamera.m_TiltAngle / 15f)
						);
					WorldPosition testSpreadDownRight = World.GetBlockPosition(
						forwardPos - _camera.transform.up + _camera.transform.right * 
						Mathf.Lerp(spread/3f, spread/2f, freeLookCamera.m_TiltAngle / 15f)
						);

					ushort tbUpLeft = World.GetBlock(testSpreadUpLeft);
					ushort tbUpRight = World.GetBlock(testSpreadUpRight);
					ushort tbDownLeft = World.GetBlock(testSpreadDownLeft);
					ushort tbDownRight = World.GetBlock(testSpreadDownRight);

					bool upLeft = IsEmpty(tbUpLeft);
					bool upRight = IsEmpty(tbUpRight);
					bool downLeft = IsEmpty(tbDownLeft);
					bool downRight = IsEmpty(tbDownRight);

					if (upLeft && upRight && downLeft && downRight)
					{
						break;
					}
				}
				
			}

			testDistance += 0.05f;
		} 
		while(testDistance <= targetDist); 

		// Adjust target distance
		targetDist = targetDist - testDistance - 1f;
	}

	bool IsEmpty(ushort block)
	{
		if (block == Block.Null || block == Block.Air)
		{
			return true;
		}
		return false;
	}
}

