
using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayHitSound : MonoBehaviour
{
	public float minImpactVelocity = 0.25f;
	public float maxImpactVelocity = 1.0f;
	public float impactPow = 10f;

	[Range(0f, 1000f)] 
	public float maxWorldVol = 0.5f;

	[Range(0f, 1000f)] 
	public float maxObjectVol = 0.9f;

	[Range(0f, 1000f)] 
	public float maxScoreVol = 0.5f;

	[HideInInspector]
	public AudioClip worldHitSound;

	[HideInInspector]
	public AudioClip objectHitSound;

	[HideInInspector]
	public AudioClip scoreSound;
	
	public static Dictionary<string,float> clipTimers = new Dictionary<string,float>();
	AudioSource worldHitSource;
	AudioSource objectHitSource;

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


	void Start()
	{
		var aSources = GetComponents<AudioSource>();
     	worldHitSource = aSources[0];
    	objectHitSource = aSources[1];

		_maxSqrDistance = Mathf.Pow(Chunk.Size * 2f, 2f);
	}

	void Update()
	{
		_sqrDistanceFromPlayer = (transform.position - Game.Player.transform.position).sqrMagnitude;
	}

	public void PlayScoreSound(float impactForce)
	{
		if (scoreSound != null)
		{
			worldHitSource.clip = scoreSound;
			worldHitSource.volume = Mathf.Lerp(0.0f, maxScoreVol * 0.001f, impactForce);
			worldHitSource.Play();
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
			if (objectHitSound != null && (collision.gameObject.CompareTag("Pickup") || collision.gameObject.CompareTag("Ball") || collision.gameObject.CompareTag("Player")))
			{
				ObjectHitSound(sqrImpactVelocity);
			}
			else if (worldHitSound != null)
			{
				WorldHitSound(sqrImpactVelocity);
			}
		}
	}

	void WorldHitSound(float sqrImpactVelocity)
	{
		if (String.IsNullOrEmpty(hitKey))
		{
			hitKey = worldHitSound.name + "_" + name;
		}
		if (!clipTimers.ContainsKey(hitKey))
		{
			clipTimers.Add(hitKey, Time.time + clipTiming);
		}

		if (clipTimers[hitKey] > Time.time || worldHitSource.isPlaying)
		{
			return;
		}
		else
		{
			clipTimers[hitKey] = Time.time + clipTiming;
		}

		float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

		worldHitSource.clip = worldHitSound;
		worldHitSource.volume = Mathf.Lerp(0.0f, maxWorldVol * 0.001f, impact);
		worldHitSource.Play();
	}

	void ObjectHitSound(float sqrImpactVelocity)
	{
		if (String.IsNullOrEmpty(selfKey))
		{
			selfKey = objectHitSound.name + "_self_" + name;
		}
		if (!clipTimers.ContainsKey(selfKey))
		{
			clipTimers.Add(selfKey, Time.time + clipTiming);
		}
		else if (clipTimers[selfKey] > Time.time || objectHitSource.isPlaying)
		{
			return;
		}
		else
		{
			clipTimers[selfKey] = Time.time + clipTiming;
		}

		float impact = Mathf.Pow((sqrImpactVelocity - sqrMinImpactVelocity)/sqrMaxImpactVelocity, impactPow);

		objectHitSource.clip = objectHitSound;
		objectHitSource.volume = Mathf.Lerp(0.0f, maxObjectVol * 0.001f, impact);
		objectHitSource.Play();
	}
}