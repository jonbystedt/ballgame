using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	public int PoolSize = 100;
	PooledObject prefab;
	List<PooledObject> availableObjects = new List<PooledObject>();
	int objCount;

	public static ObjectPool GetPool(PooledObject prefab) 
	{
		GameObject obj;
		ObjectPool pool;
		if (Application.isEditor) 
		{
			obj = GameObject.Find(prefab.name + " Pool");
			if (obj) 
			{
				pool = obj.GetComponent<ObjectPool>();
				if (pool) 
				{
					return pool;
				}
			}
		}
		obj = new GameObject(prefab.name + " Pool");
		//DontDestroyOnLoad(obj);
		pool = obj.AddComponent<ObjectPool>();
		pool.prefab = prefab;
		return pool;
	}

	public PooledObject GetObject() 
	{
		PooledObject obj;

		if (PoolSize != -1 && objCount >= PoolSize)
		{
			return null;
		}

		int lastAvailableIndex = availableObjects.Count - 1;
		if (lastAvailableIndex >= 0) 
		{
			obj = availableObjects[lastAvailableIndex];
			availableObjects.RemoveAt(lastAvailableIndex);
			obj.gameObject.SetActive(true);
			obj.isActive = true;
		}
		else 
		{
			obj = Instantiate<PooledObject>(prefab);
			obj.transform.SetParent(transform, false);
			obj.Pool = this;
			obj.isActive = true;
		}
		objCount++;
		return obj;
	}

	public void AddObject (PooledObject obj) 
	{
		obj.isActive = false;
		obj.gameObject.SetActive(false);
		obj.StopAllCoroutines();
		availableObjects.Add(obj);
		objCount--;
	}

	public int GetCount()
	{
		return availableObjects.Count;
	}

	public void ClearPool()
	{
		availableObjects = new List<PooledObject>();
		objCount = 0;
	}
}
