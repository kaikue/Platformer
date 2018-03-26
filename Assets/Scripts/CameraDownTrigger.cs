using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDownTrigger : MonoBehaviour {
    public readonly Camera cam;

    public readonly int camSizeAbove;
    public readonly float camYPositionAbove;

    public readonly int camSizeBelow;
    public readonly float camYPositionBelow;

    private bool enteredFromAbove;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	    	
	}

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            enteredFromAbove = IsPlayerAbove(collision.collider.gameObject);
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            bool exitingFromAbove = IsPlayerAbove(collision.collider.gameObject);

            if (exitingFromAbove && !enteredFromAbove)
            {
                cam.transform.position = new Vector3(cam.transform.position.x, camYPositionAbove, cam.transform.position.z);
                cam.orthographicSize = camSizeAbove;
            } else if (!exitingFromAbove && enteredFromAbove)
            {
                cam.transform.position = new Vector3(cam.transform.position.x, camYPositionBelow, cam.transform.position.z);    
                cam.orthographicSize = camSizeBelow;
            }
        }
    }

    public bool IsPlayerAbove(GameObject player)
    {
        return player.transform.position.y > transform.position.y;
    }
}
