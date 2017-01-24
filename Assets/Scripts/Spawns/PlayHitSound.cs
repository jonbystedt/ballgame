
using UnityEngine;
using System.Collections.Generic;

public class PlayHitSound : MonoBehaviour
{
	public float minImpactVelocity = 0.25f;
	public float maxImpactVelocity = 1.0f;
	public float impactPow = 10f;
	public float surfaceReduction = 0.25f;
	public float maxVol = 1f;
	public AudioClip hitSound;
	public AudioClip selfHitSound;
	public static Dictionary<int,float> clipTimers = new Dictionary<int,float>();
	AudioSource audioSource;

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
	private float _sqrDistanceFromPlayer = 0.0f;
	private float _maxSqrDistance = 0.0f;
	private float _lastMinImpactVelocity = 0.0f;

	private float clipTiming = 0.1f;
	private int clipHash;
	private int selfClipHash;


	public float sqrMaxImpactVelocity
	{
		get
		{
			if (_lastMaxImpactVelocity != maxImpactVelocity)
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

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		_maxSqrDistance = Mathf.Pow(Chunk.Size * 2f, 2f);
	}

	void Update()
	{
		_sqrDistanceFromPlayer = (transform.position - Game.Player.transform.position).sqrMagnitude;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (_sqrDistanceFromPlayer > _maxSqrDistance)
		{
			return;
		}

		float sqrImpactVelocity = collision.relativeVelocity.sqrMagnitude;

		if(sqrImpactVelocity > sqrMinImpactVelocity)
		{
			if (collision.gameObject.CompareTag("Pickup") || collision.gameObject.CompareTag("Ball") || collision.gameObject.CompareTag("Player"))
			{
				if (!clipTimers.ContainsKey(clipHash))
				{
					selfClipHash = selfHitSound.GetHashCode();
					clipTimers.Add(selfClipHash, Time.time + clipTiming);
				}

				if (clipTimers[selfClipHash] > Time.time)
				{
					return;
				}
				else
				{
					clipTimers[selfClipHash] = Time.time + clipTiming;
				}

				float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

				audioSource.clip = selfHitSound;
				audioSource.volume = Mathf.Lerp(0.0f, maxVol, impact);
				audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, impact);
				audioSource.Play();
			}
			else if (surfaceReduction > 0.0f)
			{
				if (!clipTimers.ContainsKey(clipHash))
				{
					clipHash = hitSound.GetHashCode();
					clipTimers.Add(clipHash, Time.time + clipTiming);
				}

				if (clipTimers[clipHash] > Time.time)
				{
					return;
				}
				else
				{
					clipTimers[clipHash] = Time.time + clipTiming;
				}

				float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

				audioSource.clip = hitSound;
				audioSource.volume = Mathf.Lerp(0.0f, surfaceReduction, impact);
				audioSource.Play();
			}
		}
	}
}