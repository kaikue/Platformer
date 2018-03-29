using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {

    // Use this for initialization
    private void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity.y >= 0)
        {
            GetComponent<SpriteRenderer>().flipY = true;
        } else
        {
            GetComponent<SpriteRenderer>().flipY = false;
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
			GetComponent<BoxCollider2D>().enabled = false;
			GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject);
		}
	}
}
