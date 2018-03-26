using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {

    public float SCROLL_FACTOR = 0.5f;
    public GameObject cam;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    private void OnWillRenderObject()
    {
        transform.position = SCROLL_FACTOR * cam.transform.position;
    }
}
