using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	private const float RUN_ACCEL = 0.2f;
	private const float GRAVITY_ACCEL = -0.3f;
	private const float MAX_RUN_VEL = 5.0f;
	private const float JUMP_VEL = 10;

	private static Vector2 GRAVITY_VEC = new Vector2(0, GRAVITY_ACCEL);

	private Rigidbody2D rb;

	private bool jumpQueued = false;
	private List<GameObject> grounds = new List<GameObject>();
	
	void Start () {
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
		{
			jumpQueued = true;
		}
	}

	void FixedUpdate() {
		/*foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(kcode))
				Debug.Log("KeyCode down: " + kcode);
		}*/
		/*if (rb.IsSleeping()) {
			print("i sleep");
			rb.WakeUp();
		}*/
		//print("fixed update " + Time.fixedTime);
		Vector2 velocity = rb.velocity;
		float xVel = Input.GetAxisRaw("Horizontal");
		if (xVel == 0 || 
			(velocity.x != 0 && Mathf.Sign(xVel) != Mathf.Sign(velocity.x)))
		{
			velocity.x = 0; //TODO: slow to a stop
		}
		else
		{
			velocity.x += RUN_ACCEL * xVel;
			velocity.x = Mathf.Clamp(velocity.x, -MAX_RUN_VEL, MAX_RUN_VEL);
		}

		bool onGround = grounds.Count > 0;
		if (onGround && velocity.y <= 0)
		{
			velocity.y = 0;
			if (jumpQueued)
			{
				velocity.y += JUMP_VEL;
				//print("jump~");
				//print(velocity);
			}
		}
		else // if (!rb.IsSleeping())
		{
			velocity.y += GRAVITY_ACCEL;
			//print("offground " + rb.IsSleeping());
		}

		//print(grounds.Count);
		rb.velocity = velocity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		jumpQueued = false;

		/*if (rb.IsSleeping())
		{
			rb.WakeUp();
		}*/
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//print("collision enter " + Time.fixedTime);
		if (IsGround(collision))
		{
			grounds.Add(collision.gameObject);
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		//TODO: try switching this back to stay so i can use normal for sliding down slopes?

		//print("collision stay " + Time.fixedTime);
		grounds.Remove(collision.gameObject);
	}
	
	private bool IsGround(Collision2D collision)
	{
		//print("Points colliding: " + collision.contacts.Length);
		Vector2 normal = collision.contacts[0].normal;
		return Vector2.Dot(normal, GRAVITY_VEC) < 0;
	}
}
