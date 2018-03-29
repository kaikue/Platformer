using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpawner : MonoBehaviour {

    public float SPAWN_RATE;
    public float SPAWN_OFFSET;
    public float LAUNCH_VELOCITY;
    public float Y_OFFSET;
    public GameObject firePrefab;

    private float timeToSpawn;

	void Start () {
        timeToSpawn = SPAWN_OFFSET;
	}

    void FixedUpdate () {
        if (timeToSpawn <= 0.0f)
        {
            Spawn();
            timeToSpawn = 1.0f / SPAWN_RATE;
        } else if (timeToSpawn <= Mathf.Min((1.0f / SPAWN_RATE) * 2.0f / 3.0f, 1.0f))
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
        }


        timeToSpawn -= Time.fixedDeltaTime;
	}

    private void Spawn()
    {
        GameObject fire = Instantiate(firePrefab);
        fire.transform.position = new Vector3(transform.position.x, transform.position.y + Y_OFFSET, transform.position.z);
        fire.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, LAUNCH_VELOCITY);

        GetComponent<AudioSource>().Stop();
    }
}
