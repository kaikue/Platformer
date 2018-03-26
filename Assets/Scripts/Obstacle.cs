using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	private AudioSource CrashSound;
	bool destroying;

	// Use this for initialization
	void Start() {
		CrashSound = GetComponent<AudioSource>();
        CrashSound.pitch = Random.Range(0.7f * CrashSound.pitch, 1.3f * CrashSound.pitch);
	}

	// Update is called once per frame
	void Update() {
		if (destroying && !CrashSound.isPlaying)
		{
			Destroy(gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			collision.collider.gameObject.GetComponent<PlayerDemo>().Kill();
		}

		if (!collision.collider.CompareTag("Obstacle"))
		{
			CrashSound.Play();
			GetComponent<BoxCollider2D>().enabled = false;
			GetComponent<SpriteRenderer>().enabled = false;
			destroying = true;
		}
	}
}
