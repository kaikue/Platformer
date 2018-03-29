using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeObjectIndicator : MonoBehaviour {

	public GameObject sprite;
    private SpriteRenderer sr;

    public bool hint = false;
    public bool canActivate = true;
    public bool colliding = false;
	public bool activated = false;

	// Use this for initialization
	void Start () {
        sr = sprite.GetComponent<SpriteRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {
        sr.enabled = hint && !activated;
	}

    private void FixedUpdate()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (Mathf.Abs(rb.velocity.y) > 1.0f)
        {
            canActivate = false;
        } else if (!colliding)
        {
            canActivate = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canActivate = false;
            colliding = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canActivate = false;
            colliding = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canActivate = true;
            colliding = false;
        }
    }
}
