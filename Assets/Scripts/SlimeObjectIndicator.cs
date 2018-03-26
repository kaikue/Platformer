using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeObjectIndicator : MonoBehaviour {

    private SpriteRenderer sr;

    public bool hint = false;
    public bool canActivate = true;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {
        sr.enabled = hint;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            print("enter");
            canActivate = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            print("stay");
            canActivate = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            print("exit");
            canActivate = true;
        }
    }
}
