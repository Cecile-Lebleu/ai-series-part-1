using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
	private PoolableObject Prefab;
	private List<PoolableObject> AvailableObjects;

	private ObjectPool(PoolableObject Prefab, int Size) {
		this.Prefab = Prefab;
		AvailableObjects = new List<PoolableObject>(Size);
	}

	public static ObjectPool CreateInstance(PoolableObject Prefab, int Size) {
		ObjectPool pool = new ObjectPool(Prefab, Size);

		GameObject poolObject = new GameObject(Prefab.name + " Pool");
		pool.CreateObjects(poolObject.transform, Size);

		return pool;
	}

	private void CreateObjects(Transform parent, int Size) {
		for (int i = 0; i < Size; i++) {
			PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
			poolableObject.Parent = this;
			poolableObject.gameObject.SetActive(false);
		}
	}

	public void ReturnObjectToPool(PoolableObject poolableObject) {
		AvailableObjects.Add(poolableObject);
	}

	public PoolableObject GetObject() {
		if (AvailableObjects.Count > 0) {
			PoolableObject instance = AvailableObjects[0];
			AvailableObjects.RemoveAt(0);

			instance.gameObject.SetActive(true);

			return instance;
		}
		else {
			// return null if you do not want to auto expand the size of the pool
			// or expand the size of the pool by creating a new object. Generates garbage.
			Debug.LogError($"Could not get an object from pool \"{Prefab.name}\" Pool. Probably a configuration issue.");
			return null;
		}
	}
}
