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
	Collider _collider;
	Vector3 lastPosition;
	float nextBashFrame = 0f;
	float bashInterval = 3f;


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

			roller.BashBlocks(forwardNormal, speed, boosting, camOp.FirstPerson);
		}

	}


	void FixedUpdate()
	{
        if (!Game.PlayerActive)
        {
            return;
        }

		if (camOp.FirstPerson)
		{
			if (!robotForm.activeSelf)
			{
				robotForm.SetActive(true);
			}
			robotForm.transform.rotation = Quaternion.identity;
			robotForm.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
		}
		else if (robotForm.activeSelf)
		{
			robotForm.SetActive(false);
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

