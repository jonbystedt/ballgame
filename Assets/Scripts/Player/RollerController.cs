using UnityEngine;
using System.Collections;

public class RollerController : MonoBehaviour
{
	public GameObject boostEffect;
	public CameraOperator camOp;
	private Roller roller; 

	// the world-relative desired move direction, calculated from the camForward and user input.
	private Vector3 move;
	private Transform cam; 
	private Vector3 camForward;

	// holding down boost, or not landed jump
	private bool jumping; 
	private bool jumpEnd;
	private bool boosting;
	private bool boostEnd;
	private bool create;
	private bool pound;

	Rigidbody _rigidbody;
	Vector3 lastPosition;

	private void Awake()
	{
		roller = GetComponent<Roller>();
		_rigidbody = GetComponent<Rigidbody>();
		cam = Camera.main.transform;

		// Make sure we don't fall off
		StartCoroutine(CheckOutOfBounds());
	}


	void Update()
	{
		if (!Game.PlayerActive)
		{
			return;
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
		move = (input.y * camForward + input.x * cam.right).normalized;

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

		if (boosting && !camOp.FirstPerson && camOp.Distance > 2f)
		{
			boostEffect.SetActive(true);
		}
		else
		{
			boostEffect.SetActive(false);
		}
	}


	void FixedUpdate()
	{
        if (!Game.PlayerActive)
        {
            return;
        }

		// move ball
		roller.Move(move, jumping, boosting, pound);

		// bash blocks
		if (!create && (boosting || jumping))
		{
			ExecuteBashBlocks();
		}
	}

	void LateUpdate()
	{
		lastPosition = transform.position;
	}

	void ExecuteBashBlocks()
	{
		Vector3 planePos = new Vector3(transform.position.x, 0, transform.position.z);
		Vector3 planeLastPos = new Vector3(lastPosition.x, 0, lastPosition.z);
		Vector3 forwardNormal = Vector3.Normalize(planePos - planeLastPos);
		float speed = Mathf.Abs(Vector3.Distance(transform.position, lastPosition));

		roller.BashBlocks(forwardNormal, speed, boosting);	
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

