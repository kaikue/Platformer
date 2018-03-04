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
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Activate()
    {
        sr.color = Color.white;
        col.enabled = true;
        activated = true;
        
    }

    public void Hint()
    {
        sr.color = Color.gray;
    }

    public void DeHint()
    {
        if (!activated)
        {
            sr.color = Color.clear;
        }
    }

    public void Deactivate()
    {
        sr.color = Color.clear;
        col.enabled = false;
        activated = false;
    }
}
