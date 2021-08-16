using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
	public Transform Player;
	public int NumberOfEnemiesToSpawn;
	public float SpawnDelay = 1f;
	public List<Enemy> EnemyPrefabs = new List<Enemy>();
	public SpawnMethod EnemySpawnMethod = SpawnMethod.RoundRobin;

	private UnityEngine.AI.NavMeshTriangulation Triangulation;

	private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();

	private void Awake() {
		for (int i = 0; i < EnemyPrefabs.Count; i++) {
			EnemyObjectPools.Add(i, ObjectPool.CreateInstance(EnemyPrefabs[i], NumberOfEnemiesToSpawn));
		}
	}

	private void Start() {
		Triangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
		StartCoroutine(SpawnEnemies());
	}

	private IEnumerator SpawnEnemies() {
		WaitForSeconds Wait = new WaitForSeconds(SpawnDelay);
		int SpawnedEnemies = 0;

		while (SpawnedEnemies < NumberOfEnemiesToSpawn) {
			if (EnemySpawnMethod == SpawnMethod.RoundRobin) {
				SpawnRoundRobinEnemy(SpawnedEnemies);
			}
			else if (EnemySpawnMethod == SpawnMethod.Random) {
				SpawnRandomEnemy();
			}
			SpawnedEnemies++;

			yield return Wait;
		}
	}

	private void SpawnRoundRobinEnemy(int SpawnedEnemies) {
		int SpawnIndex = SpawnedEnemies % EnemyPrefabs.Count;

		DoSpawnEnemy(SpawnIndex);
	}

	private void SpawnRandomEnemy() {
		DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count));
	}

	private void DoSpawnEnemy(int SpawnIndex) {
		PoolableObject poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

		if (poolableObject != null) {
			Enemy enemy = poolableObject.GetComponent<Enemy>();

			int VertexIndex = Random.Range(0, Triangulation.vertices.Length);

			UnityEngine.AI.NavMeshHit Hit;

			if(UnityEngine.AI.NavMesh.SamplePosition(Triangulation.vertices[VertexIndex], out Hit, 2f, -1)) { // -1 for all areas or 1 for walkable
				enemy.Agent.Warp(Hit.position);
				// enemy needs to get enabled and start chasing now.
				enemy.Movement.Player = Player;
				enemy.Agent.enabled = true;
				enemy.Movement.StartChasing();
			} else {
				Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {Triangulation.vertices[VertexIndex]}");
			}
		} else {
			Debug.LogError($"Unable to fetch enemy of type {SpawnIndex} from object pool. Out of objects?");
		}
	}

	public enum SpawnMethod {
		RoundRobin,
		Random
	}
}
