using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {

    public float SCROLL_FACTOR_X = 0.5f;
    public float SCROLL_FACTOR_Y = 0.5f;
    public Vector3 Offset; 
    public GameObject cam;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    private void OnWillRenderObject()
    {
        transform.position = new Vector3(SCROLL_FACTOR_X * cam.transform.position.x, SCROLL_FACTOR_Y * cam.transform.position.y, transform.position.z) + Offset;
    }
}
