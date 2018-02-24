using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	/*
	 * https://eev.ee/blog/2017/10/13/coaxing-2d-platforming-out-of-unity/
	 * 
	 * TODO:
	 * 
	 * animate player
	 * 
	 * don't slow when jumping against walls? makes walljumping easier...
	 *		maybe just not when going up? somehow?
	 * 
	 * pick up/throw?
	 * 
	 * slopes?
	 *  slide down >= 45 degree slopes weirdness
	 *  stick to slopes while walking down
	 * 
	 * Cool moves:
	 *	Jump-roll for height & distance
	 *	Jump-roll-jump for controlled distance
	 *	Walljump-roll to climb over a lip
	 */

	private const float RUN_ACCEL = 0.4f;
	private const float GRAVITY_ACCEL = -0.6f;
	private const float MAX_RUN_VEL = 7.0f; //maximum speed of horizontal movement

	private const float JUMP_VEL = 12.0f; //jump y speed
	private const float WALLJUMP_VEL = MAX_RUN_VEL; //speed applied at time of walljump
	private const float WALLJUMP_MIN_FACTOR = 0.5f; //amount of walljump kept at minimum if no input
	private const float WALLJUMP_TIME = 0.4f; //time it takes for walljump to wear off

	private const float ROLL_VEL = 2 * MAX_RUN_VEL; //speed of roll
	private const float ROLL_TIME = 0.8f; //time it takes for roll to wear off naturally
	private const float ROLLJUMP_VEL = JUMP_VEL * 2 / 3; //roll cancel jump y speed
	private const float ROLL_COOLDOWN_TIME = 0.1f; //time on ground it takes after a completed roll before a new one

	private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private const int NUM_WALK_SPRITES = 2;
	private const int NUM_ROLL_SPRITES = 2;

	private float baseScaleX;
	private float baseScaleY;
	private float baseScaleZ;

	private Rigidbody2D rb;
	private float groundAngle;

	private bool jumpQueued = false;
	private bool jumping = false;
	private List<GameObject> grounds = new List<GameObject>();
	private GameObject wall = null;
	private int wallSide = 0; //1 for wall on left, 0 for none, -1 for wall on right (i.e. points away from wall in x)
	private int lastWallSide = 0;
	private float walljumpTime = 0; //counts down from WALLJUMP_TIME
	private bool walljumpPush = false; //if the player hasn't touched anything and the walljump should keep moving them
	
	private bool rollQueued = false;
	private float rollTime = 0;
	private bool canRoll = true;
	private int rollDir = 1; //-1 for left, 1 for right
	private float rollCooldown = 0;

	enum AnimState
	{
		STAND,
		JUMP,
		WALLSLIDE,
		WALK,
		ROLL
	}
	private AnimState animState = AnimState.STAND;
	private int animFrame = 0;
	private int facing = 1; //for animation: -1 for left, 1 for right

	private Sprite standSprite;
	private Sprite jumpSprite;
	private Sprite wallslideSprite;
	private Sprite[] walkSprites;
	private Sprite[] rollSprites;

	void Start()
	{
		baseScaleX = gameObject.transform.localScale.x;
		baseScaleY = gameObject.transform.localScale.y;
		baseScaleZ = gameObject.transform.localScale.z;

		rb = gameObject.GetComponent<Rigidbody2D>();

		SLIDE_THRESHOLD = -Mathf.Sqrt(2) / 2; //player will slide down 45 degree angle slopes

		LoadSprites();
	}

	private void LoadSprites()
	{
		standSprite = LoadSprite("walk");
		jumpSprite = LoadSprite("jump");
		wallslideSprite = LoadSprite("wallslide");

		walkSprites = new Sprite[NUM_WALK_SPRITES];
		for (int i = 0; i < NUM_WALK_SPRITES; i++)
		{
			walkSprites[i] = LoadSprite("walk/frame" + (i + 1));
		}

		rollSprites = new Sprite[NUM_ROLL_SPRITES];
		for (int i = 0; i < NUM_ROLL_SPRITES; i++)
		{
			rollSprites[i] = LoadSprite("roll/frame" + (i + 1));
		}
	}

	private Sprite LoadSprite(string name)
	{
		return Resources.Load<Sprite>("Images/player/" + name);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
		{
			jumpQueued = true;
		}

		bool triggerPressed = Input.GetAxis("RTrigger") > 0;
		if (Input.GetKeyDown(KeyCode.LeftShift) || triggerPressed)
		{
			rollQueued = true;
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
		if (xVel == 0)
		{
			velocity.x = 0;
		}
		else
		{
			if (velocity.x != 0 && Mathf.Sign(xVel) != Mathf.Sign(velocity.x)) //don't slide if switching directions on same frame
			{
				velocity.x = 0;
			}
			else
			{
				velocity.x += RUN_ACCEL * xVel;
				float speedCap = Mathf.Abs(xVel * MAX_RUN_VEL); //use input to clamp max speed so half tilted joystick is slower
				velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
			}

			if (walljumpTime <= 0)
			{
				//received horizontal input, so don't let player get pushed by natural walljump velocity
				walljumpPush = false;
			}

			if (rollTime <= 0)
			{
				rollDir = Math.Sign(xVel);
			}
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
			ResetWalljump();
			
			if (rollCooldown <= 0)
			{
				canRoll = true;
			}
			else if (rollTime <= 0)
			{
				rollCooldown -= Time.fixedDeltaTime;
			}
			
			velocity.y = 0;
			jumping = false;
			if (jumpQueued)
			{
				//regular jump
				StopRoll();
				velocity.y += JUMP_VEL;
				jumping = true;
				jumpQueued = false;
			}
			//}
		}
		else
		{
			if (!onGround && jumpQueued && wallSide != 0)
			{
				//walljump
				walljumpTime = WALLJUMP_TIME;
				lastWallSide = wallSide;
				velocity.y = JUMP_VEL;
				walljumpPush = true;
				jumping = true;
				jumpQueued = false;
			}

			velocity.y += GRAVITY_ACCEL;
			/*if (!jumping)
			{
				//clamp to ground a bit
			}*/
		}
		
		if (rollTime > 0 && jumpQueued)
		{
			//roll cancel
			StopRoll();
			ResetWalljump();
			velocity.y = ROLLJUMP_VEL;
			jumping = true;
			jumpQueued = false;
		}

		if (walljumpTime > 0 || walljumpPush)
		{
			//apply walljump velocity
			float timeFactor = walljumpTime / WALLJUMP_TIME;
			if (walljumpPush)
			{
				timeFactor = Mathf.Max(timeFactor, WALLJUMP_MIN_FACTOR);
			}
			float walljumpVel = WALLJUMP_VEL * lastWallSide * timeFactor;

			velocity.x += walljumpVel;
			velocity.x = Mathf.Clamp(velocity.x, -MAX_RUN_VEL, MAX_RUN_VEL);

			if (walljumpTime > 0)
			{
				walljumpTime -= Time.fixedDeltaTime;
			}
		}

		if (rollQueued)
		{
			if (canRoll)
			{
				canRoll = false;
				rollTime = ROLL_TIME;
				rollCooldown = ROLL_COOLDOWN_TIME;
			}
		}

		if (rollTime > 0)
		{
			//apply roll velocity
			float timeFactor = rollTime / ROLL_TIME;
			float rollVel = ROLL_VEL * timeFactor;
			velocity.x = rollDir * rollVel;
			rollTime -= Time.fixedDeltaTime;
		}

		if (velocity.x != 0)
		{
			facing = -Math.Sign(velocity.x); //make this positive if sprites face right
		}
		gameObject.transform.localScale = new Vector3(facing * baseScaleX, baseScaleY, baseScaleZ);

		rb.velocity = velocity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		jumpQueued = false;
		rollQueued = false;
	}

	private void ResetWalljump()
	{
		walljumpPush = false;
		walljumpTime = 0;
	}

	private void StopRoll()
	{
		rollTime = 0;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//TODO: don't assume it's land, check that first
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
		else if (IsWall(collision))
		{
			float x = Vector2.Dot(Vector2.right, collision.contacts[0].normal);
			wallSide = Mathf.RoundToInt(x);
			wall = collision.gameObject;

			ResetWalljump();
			StopRoll();
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		grounds.Remove(collision.gameObject);
		if (collision.gameObject == wall)
		{
			wall = null;
			wallSide = 0;
		}
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
