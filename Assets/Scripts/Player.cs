using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	/*
	 * https://eev.ee/blog/2017/10/13/coaxing-2d-platforming-out-of-unity/
	 * 
	 * TODO:
	 * slide down >= 45 degree slopes weirdness
	 * stick to slopes while walking down
	 * don't slow on walls
	 * wall jumps
	 * dive/roll, "attack", move combinations
	 */

	private const float RUN_ACCEL = 0.4f;
	private const float GRAVITY_ACCEL = -0.6f;
	private const float MAX_RUN_VEL = 7.0f; //maximum speed of horizontal movement
	private const float JUMP_VEL = 12.0f;

	private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private Rigidbody2D rb;
	private float groundAngle;
	private bool jumpQueued = false;
	private bool jumping = false;
	private List<GameObject> grounds = new List<GameObject>();
	
	void Start ()
	{
		rb = gameObject.GetComponent<Rigidbody2D>();
		SLIDE_THRESHOLD = -Mathf.Sqrt(2) / 2; //player will slide down 45 degree angle slopes
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
		Vector2 velocity = rb.velocity;
		float xVel = Input.GetAxisRaw("Horizontal");
		if (xVel == 0 || 
			(velocity.x != 0 && Mathf.Sign(xVel) != Mathf.Sign(velocity.x)))
		{
			velocity.x = 0;
		}
		else
		{
			velocity.x += RUN_ACCEL * xVel;
			float speedCap = Mathf.Abs(xVel * MAX_RUN_VEL); //use input to clamp max speed so half tilted joystick is slower
			velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
		}

		bool onGround = grounds.Count > 0;
		if (onGround && velocity.y <= 0)
		{
			/*if (groundAngle >= SLIDE_THRESHOLD)
			{
				print("slide");
				velocity.y += GRAVITY_ACCEL; //* slope (perp. to ground angle), * friction?
			}
			else
			{*/
			velocity.y = 0;
			jumping = false;
			if (jumpQueued)
			{
				velocity.y += JUMP_VEL;
				jumping = true;
			}
			//}
		}
		else
		{
			velocity.y += GRAVITY_ACCEL;
			print("offground " + jumping);
			if (!jumping)
			{
				//clamp to ground a bit
			}
		}
		
		rb.velocity = velocity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		jumpQueued = false;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsGround(collision))
		{
			grounds.Add(collision.gameObject);
			groundAngle = NormalDot(collision);
			//print(groundAngle);
		}
		else if (IsCeiling(collision))
		{
			Vector2 velocity = rb.velocity;
			velocity.y = 0;
			rb.velocity = velocity;
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		grounds.Remove(collision.gameObject);
	}
	
	private float NormalDot(Collision2D collision)
	{
		//   0 for horizontal (walls)
		// < 0 for floor
		// > 0 for ceiling
		Vector2 normal = collision.contacts[0].normal;
		return Vector2.Dot(normal, GRAVITY_NORMAL);
	}

	private bool IsGround(Collision2D collision)
	{
		return NormalDot(collision) < 0;
	}

	private bool IsCeiling(Collision2D collision)
	{
		return NormalDot(collision) > 0;
	}

	private bool IsWall(Collision2D collision)
	{
		return NormalDot(collision) == 0;
	}
}
