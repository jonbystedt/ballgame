
using UnityEngine;

public class PlayHitSound : MonoBehaviour
{
	public float minImpactVelocity = 0.1f;
	public float maxImpactVelocity = 1.0f;
	public AudioClip hitSound;
	AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public float sqrMinImpactVelocity
	{
		get
		{
			if(_lastMinImpactVelocity != minImpactVelocity)
			{
				_lastMinImpactVelocity = minImpactVelocity;
				_sqrMinImpactVelocity = minImpactVelocity * minImpactVelocity;
			}

			return _sqrMinImpactVelocity;
		}
	}

	private float _sqrMinImpactVelocity = 0.0f;
	private float _lastMinImpactVelocity = 0.0f;



	public float sqrMaxImpactVelocity
	{
		get
		{
			if(_lastMaxImpactVelocity != maxImpactVelocity)
			{
				_lastMaxImpactVelocity = maxImpactVelocity;
				_sqrMaxImpactVelocity = maxImpactVelocity * maxImpactVelocity;
			}
			return _sqrMaxImpactVelocity;
		}
	}

	private float _sqrMaxImpactVelocity = 0.0f;
	private float _lastMaxImpactVelocity = 0.0f;

	public float minPitch = 0.5f;
	public float maxPitch = 1.5f;

	void OnCollisionEnter(Collision collision)
	{
		float sqrImpactVelocity = collision.relativeVelocity.sqrMagnitude;

		if(sqrImpactVelocity > sqrMinImpactVelocity)
		{
			GetComponent<AudioSource>().volume = Mathf.Lerp(0.0f,1.0f,(sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity);
			GetComponent<AudioSource>().pitch = Mathf.Lerp(minPitch,maxPitch,(sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity);
			GetComponent<AudioSource>().Play();
		}
	}
}