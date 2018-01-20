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

	private bool onGround;
	
	void Start () {
		rb = gameObject.GetComponent<Rigidbody2D>();
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

		if (onGround && velocity.y <= 0)
		{
			velocity.y = 0;
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
			{
				velocity.y += JUMP_VEL;
				//print("jump~");
				//print(velocity);
			}
		}
		else if (!rb.IsSleeping())
		{
			velocity.y += GRAVITY_ACCEL;
			print("offground " + rb.IsSleeping());
		}
		
		rb.velocity = velocity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		if (rb.IsSleeping())
		{
			rb.WakeUp();
		}
		onGround = false;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//print("collision enter " + Time.fixedTime);
		CheckGround(collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		//print("collision stay " + Time.fixedTime);
		CheckGround(collision);
	}

	private void CheckGround(Collision2D collision)
	{
		//print("Points colliding: " + collision.contacts.Length);
		Vector2 normal = collision.contacts[0].normal;
		if (Vector2.Dot(normal, GRAVITY_VEC) < 0)
		{
			onGround = true;
		}
	}
}
