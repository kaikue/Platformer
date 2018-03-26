using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGuide : MonoBehaviour {

    public GameObject portal;
    public GameObject player;
    public float distance = 2.0f;
    public float angleForPosition;
    public float angleForRotation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 playerToPortal = portal.transform.position - player.transform.position;

        angleForRotation = Vector2.SignedAngle(Vector2.right, playerToPortal) - 90.0f;
        angleForPosition = Vector2.SignedAngle(Vector2.right, playerToPortal);
        float angleForPositionRad = angleForPosition * Mathf.PI / 180.0f;

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, angleForRotation));
        transform.position = player.transform.position + 
            distance * (new Vector3(Mathf.Cos(angleForPositionRad), Mathf.Sin(angleForPositionRad), 0.0f));

	}
}
