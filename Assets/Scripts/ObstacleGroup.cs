using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGroup : MonoBehaviour {

    public Vector3 respawnPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            respawnPosition = collision.collider.gameObject.transform.position;   
        } 
    }
}
