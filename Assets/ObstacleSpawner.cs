using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {

    public int SPAWN_RATE;
    public GameObject obstaclePrefab;

    private float timeToSpawn;

	void Start () {
        timeToSpawn = 0.0f;
	}
	
	void FixedUpdate () {
        if (timeToSpawn <= 0.0f)
        {
            Spawn();
            timeToSpawn = 1.0f / SPAWN_RATE;
        }

        timeToSpawn -= Time.fixedDeltaTime;
	}

    private void Spawn()
    {
        GameObject obstacle = Instantiate(obstaclePrefab);
        obstacle.transform.position = transform.position;
    }
}
