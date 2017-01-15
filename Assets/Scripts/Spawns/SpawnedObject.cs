using UnityEngine;
using System.Collections;

public class SpawnedObject : PooledObject {

	public ParticleSystem explosion;
	public Color _color = Color.gray;
	public Color _emission = Color.black;
	public bool isLive = true;
	public bool inRange = true;

	protected MeshRenderer _renderer;
	protected Vector3 saveVelocity;
	protected Vector3 saveAngularVelocity;

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			material.SetColor("_Color", value);
			material.SetColor("_SpecularColor", value);
			material.SetColor("_EmissionColor", Color.Lerp(value, Color.black, 0.85f));

			if (explosion != null)
			{
				explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				var ma = explosion.main;
				ma.startColor = value;
			}
		}
	}

	public Color emission
	{
		get
		{
			return _emission;
		}
		set
		{
			_emission = value;
			material.SetColor("_EmissionColor", value);
		}
	}

	public float _mass = 1f;
	public float mass
	{
		get
		{
			return _mass;
		}
		set
		{
			if (value > 0f)
			{
				_mass = value;

				if (_rigidbody == null)
				{
					Invoke("SetMass", 0.1f);
				}
				else
				{
					_rigidbody.mass = value;
				}
			}
		}
	}

	public float _size = 1f;
	public float size
	{
		get
		{
			return _size;
		}
		set
		{
			if (value > 0f)
			{
				_size = value;
				transform.localScale = new Vector3(size, size, size);
			}
		}
	}

	protected GameObject player;
	protected Material material;
	protected Rigidbody _rigidbody;

	void Awake()
	{
		material = transform.GetComponent<Renderer>().material;
		_rigidbody = transform.GetComponent<Rigidbody>();
		_renderer = gameObject.GetComponent<MeshRenderer>();
	}

	void Start() 
	{
		player = GameObject.FindWithTag("Player");
	}

	void SetMass()
	{
		_rigidbody.mass = mass;
	}

	public void StartSlowUpdate()
	{
		StartCoroutine(UpdateAfterDelay(1f));
	}

	protected override void SlowUpdate() 
	{
		float distance = Vector2.Distance(
			new Vector2(gameObject.transform.position.x, gameObject.transform.position.z), 
			new Vector2(Game.Player.transform.position.x, Game.Player.transform.position.z)
			);

		if (distance > Config.DespawnRadius * Chunk.Size && inRange)
		{
			saveVelocity = _rigidbody.velocity;
			saveAngularVelocity = _rigidbody.angularVelocity;
			_rigidbody.isKinematic = true;
			_rigidbody.Sleep();
			_renderer.enabled = false;

			Sleep();
			inRange = false;
		}
		if (distance < Config.DespawnRadius * Chunk.Size && !inRange)
		{
			_rigidbody.isKinematic = false;
			_rigidbody.velocity = saveVelocity;
			_rigidbody.angularVelocity = saveAngularVelocity;
			_rigidbody.WakeUp();

			_renderer.enabled = true;
			Wake();
			inRange = true;
		}
	}

	protected virtual void Sleep() {}

	protected virtual void Wake() {}

	public override void Reset() 
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.Euler(Vector3.zero);
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;

		isLive = true;
		inRange = true;
	}

	public void RemoveIn(float seconds)
	{
		StartCoroutine(Wait(0.3f, () => {
			if (isActive)
			{
				ReturnToPool();
			}	
		}));
	}

	protected IEnumerator UpdateAfterDelay(float delay)
	{
		for(;;) 
		{
			// Check for out of bounds
			if (gameObject.transform.position.y < -65)
			{
				ReturnToPool();
			}

			SlowUpdate();
			
			yield return new WaitForSeconds(delay);
		}
	}
}
