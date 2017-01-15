using UnityEngine;
using System.Collections;
using System;

public class PooledObject : MonoBehaviour {

	[System.NonSerialized]
	ObjectPool poolInstanceForPrefab;

	public bool isActive { get; set; }
	public int id { get; set; }
	public static int NextId = 0;

	protected virtual void SlowUpdate() {}

	public T GetPooledInstance<T>() where T : PooledObject
	{
		if (!poolInstanceForPrefab)
		{
			poolInstanceForPrefab = ObjectPool.GetPool(this);
		}
		var instance = (T)poolInstanceForPrefab.GetObject();
		if (instance != null)
		{
			instance.Reset();
		}

		return instance;
	}

	public void SetPoolSize(int poolSize)
	{
		if (!poolInstanceForPrefab)
		{
			poolInstanceForPrefab = ObjectPool.GetPool(this);
		}
		poolInstanceForPrefab.PoolSize = poolSize;
	}

	public ObjectPool Pool { get; set; }

	public void ReturnToPool() 
	{
		if (Pool)
		{
			Pool.AddObject(this);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public virtual void Reset() {}

	public IEnumerator Wait(float time, Action callback)
	{	
		yield return new WaitForSeconds(time);
		callback();
	}

	public IEnumerator Repeat(int count, float delay, Action callback)
	{
		for (int i = 0; i < count; i++)
		{
			callback();
			yield return new WaitForSeconds(delay);
		}
	}
}
