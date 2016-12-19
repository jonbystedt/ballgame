using UnityEngine;
using System.Collections;

public class SpawnedObject : PooledObject {

	public ParticleSystem explosion;
	public Color _color = Color.gray;
	public Color _emission = Color.black;
	public bool isLive = true;

	protected MeshRenderer _renderer;

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
				explosion.startColor = value;
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
		StartCoroutine(UpdateAfterDelay(1f));
	}

	void SetMass()
	{
		_rigidbody.mass = mass;
	}

	protected virtual void SlowUpdate() {}

	public override void Reset() 
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.Euler(Vector3.zero);
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;

		isLive = true;
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
