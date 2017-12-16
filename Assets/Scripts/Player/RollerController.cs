using UnityEngine;
using System.Collections;

public class RollerController : MonoBehaviour
{
	public GameObject boostEffect;
	public GameObject robotForm;
	public Light boostLight;
	public CameraOperator camOp;
	private Roller roller; 

	// the world-relative desired move direction, calculated from the camForward and user input.
	private Vector3 move = Vector3.zero;
    private Vector3 move90 = Vector3.zero;
	private Transform cam; 
	private Vector3 camForward;

	// holding down boost, or not landed jump
	private bool jumping;
    private bool boosting;
	private bool create;
	private bool pound;

    private World.Direction lastDirection;
    private World.Direction prevDirection;

	Rigidbody _rigidbody;
	Collider _collider;
	Vector3 lastPosition;

	private void Awake()
	{
		roller = GetComponent<Roller>();
		_rigidbody = GetComponent<Rigidbody>();
		_collider = GetComponent<Collider>();
		cam = Camera.main.transform;
		roller.camOp = camOp;

		// Make sure we don't fall off
		StartCoroutine(CheckOutOfBounds());
	}


	void Update()
	{
		if (!Game.Active)
		{
			return;
		}

		if (camOp.FirstPerson)
		{
			if (!robotForm.activeSelf)
			{
				robotForm.SetActive(true);
			}
		}
		else if (robotForm.activeSelf)
		{
			robotForm.SetActive(false);
			boostEffect.SetActive(true); // strange bug makes the ball roll funny if this isn't toggled!
		}

		// Get input force
		Vector2 input =  GameInput.Movement;

		// Get flags for current actions
		boosting = GameInput.Boosting;
		jumping = GameInput.Jumping;
		create = GameInput.CreateBlock;
		pound = GameInput.GroundPound;

		// calculate camera relative direction to move:
		camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraY = input.y * camForward;
        Vector3 cameraX = input.x * cam.right;

        move = (cameraY + cameraX).normalized;
        Vector3 worldMove = cam.TransformDirection(move);

        //Game.Log("X: " + worldMove.x + "  Y: " + worldMove.z);

        Debug.DrawRay(transform.position, move, Color.cyan, 0.1f);

        // stop bumping up against blocks and jiggling around due to physics
        if (camOp.FirstPerson)
        {
            World3 blockPos = World.GetBlockPosition(Game.Player.transform.position + move);
            ushort block = World.GetBlock(blockPos);

            if (block != Block.Air)
            {
                World.Direction direction = World.GetDirection(World.GetBlockPosition(Game.Player.transform.position), blockPos);
                if (direction != lastDirection)
                {
                    prevDirection = lastDirection;
                }
                lastDirection = direction;
                Game.Log(direction.ToString());

                Vector3 projectRight = Vector3.Project(move, Vector3.right);
                Vector3 projectForward = Vector3.Project(move, Vector3.forward);

                Debug.DrawRay(transform.position, projectRight.normalized, Color.red, 0.1f);
                Debug.DrawRay(transform.position, projectForward.normalized, Color.yellow, 0.1f);

                //if (worldMove.z > 0)
                if
                (
                    worldMove.z > 0 ||
                    direction == World.Direction.north || 
                    direction == World.Direction.south
                )
                {
                    block = World.GetBlock(World.GetBlockPosition(Game.Player.transform.position + projectRight.normalized));
                    if (block == Block.Air)
                    {
                        move = projectRight;
                    }
                    else
                    {
                        move = Vector3.zero;
                    }
                }
                else if
                (
                    worldMove.z < 0 ||
                    direction == World.Direction.east ||
                    direction == World.Direction.west
                )
                {
                    block = World.GetBlock(World.GetBlockPosition(Game.Player.transform.position + projectForward.normalized));
                    if (block == Block.Air)
                    {
                        move = projectForward;
                    }
                    else
                    {
                        move = Vector3.zero;
                    }
                }
                else
                {
                    move = Vector3.zero;
                }
            }
        }


		if (create)
		{
    		roller.CreateBlocks();
		}

		// TODO: Centralize all input handling
		if (Input.GetKey(KeyCode.Y))
		{
			roller.CreateSphere();
		}

		if (GameInput.SwapInputs)
		{
			Config.SwapInputs = !Config.SwapInputs;
		}

		bool boostOn = roller.GetAfterburnerState();
		if (!camOp.FirstPerson && camOp.Distance > 2f)
		{	
			boostEffect.SetActive(boostOn);
		}
		else
		{
			boostEffect.SetActive(false);
		}
		if ((int)Config.QualityLevel >= 2)
		{
			if (boostOn && !camOp.FirstPerson)
			{
				boostLight.intensity = 1f;
			}
			else
			{
				boostLight.intensity = 0f;
			}
		}

		// bash blocks
		if (!create && (boosting || pound))
		{
			Vector3 planePos = new Vector3(transform.position.x, 0, transform.position.z);
			Vector3 planeLastPos = new Vector3(lastPosition.x, 0, lastPosition.z);
			Vector3 forwardNormal = Vector3.Normalize(planePos - planeLastPos);
			float speed = Mathf.Abs(Vector3.Distance(transform.position, lastPosition));

			roller.BashBlocks(forwardNormal, speed, boosting, jumping, camOp.FirstPerson);
		}

	}


	void FixedUpdate()
	{
        if (!Game.Active)
        {
            return;
        }

		if (camOp.FirstPerson)
		{
			robotForm.transform.rotation = Quaternion.identity;
			robotForm.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
		}

		// move ball
		roller.Move(move, jumping, boosting, pound);
	}

	void LateUpdate()
	{
		lastPosition = transform.position;
	}

    IEnumerator CheckOutOfBounds()
	{
		for(;;) 
		{
			if (gameObject.transform.position.y < -65)
			{
				transform.position = Game.LastGoodPosition;
				_rigidbody.velocity = Vector3.zero;
			}

			yield return new WaitForSeconds(1f);
		}
	}
}

