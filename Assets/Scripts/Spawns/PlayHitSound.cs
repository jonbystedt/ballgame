
using UnityEngine;
using System;
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
	public AudioClip scoreSound;
	public static Dictionary<string,float> clipTimers = new Dictionary<string,float>();
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

	private string hitKey;
	private string selfKey;

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

	public void Score()
	{
		if (scoreSound != null)
		{
			audioSource.clip = scoreSound;
			audioSource.volume = 0.4f;
			audioSource.pitch = 1f;
			audioSource.Play();
		}
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
			if (selfHitSound != null && (collision.gameObject.CompareTag("Pickup") || collision.gameObject.CompareTag("Ball") || collision.gameObject.CompareTag("Player")))
			{
				SelfHitSound(sqrImpactVelocity);
			}
			else if (hitSound != null)
			{
				HitSound(sqrImpactVelocity);
			}
		}
	}

	void HitSound(float sqrImpactVelocity)
	{
		if (String.IsNullOrEmpty(hitKey))
		{
			hitKey = hitSound.name + "_" + name;
		}
		if (!clipTimers.ContainsKey(hitKey))
		{
			clipTimers.Add(hitKey, Time.time + clipTiming);
		}

		if (clipTimers[hitKey] > Time.time || audioSource.isPlaying)
		{
			return;
		}
		else
		{
			clipTimers[hitKey] = Time.time + clipTiming;
		}

		float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

		audioSource.clip = hitSound;
		audioSource.volume = Mathf.Lerp(0.0f, surfaceReduction, impact);
		audioSource.Play();
	}

	void SelfHitSound(float sqrImpactVelocity)
	{
		if (String.IsNullOrEmpty(selfKey))
		{
			selfKey = selfHitSound.name + "_self_" + name;
		}
		if (!clipTimers.ContainsKey(selfKey))
		{
			clipTimers.Add(selfKey, Time.time + clipTiming);
		}
		else if (clipTimers[selfKey] > Time.time)
		{
			return;
		}
		else
		{
			clipTimers[selfKey] = Time.time + clipTiming;
		}

		float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

		audioSource.clip = selfHitSound;
		audioSource.volume = Mathf.Lerp(0.0f, maxVol, impact);
		audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, impact);
		audioSource.Play();
	}
}