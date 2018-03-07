using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeObject : MonoBehaviour {

    public bool activated = false;

    private SpriteRenderer sr;
    private BoxCollider2D col;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
	}

    private void FixedUpdate()
    {
        col.enabled = activated;
    }

    // Update is called once per frame
    void Update () {
        if (activated)
        {
            sr.color = Color.white;
        } else
        {
            sr.color = Color.gray;
        }
	}
}
