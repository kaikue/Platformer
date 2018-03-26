using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public GameObject player;

    public float Y_WINDOW;
    public float FALLING_VELOCITY;
    public float FALLING_SHIFT_RATE;

    private float lastPlayerY;

	// Use this for initialization
	void Start () {
        lastPlayerY = player.transform.position.y;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        float currentY = transform.position.y;
        float playerY = player.transform.position.y;
        if (player.GetComponent<Rigidbody2D>().velocity.y < -1.0 * FALLING_VELOCITY) {
            currentY += (playerY - lastPlayerY) - FALLING_SHIFT_RATE;
        } 

        float newY = Mathf.Clamp(currentY, player.transform.position.y - Y_WINDOW, player.transform.position.y + Y_WINDOW);
        transform.position = new Vector3(player.transform.position.x, newY, -10.0f);

        lastPlayerY = playerY;
	}
}
