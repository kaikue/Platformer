﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	/*
	 * https://eev.ee/blog/2017/10/13/coaxing-2d-platforming-out-of-unity/
	 * 
	 * TODO:
	 * 
	 * Star pickup save in file
	 *	Render # stars of current level color in UI?
	 * 
	 * Spikes
	 *	Health
	 *	Hurt you, send you back to latest checkpoint
	 *	If you die you restart at beginning of level but keep stars
	 * 
	 * Doors
	 *	Star prefab
	 *		Get color and type
	 *	Number required
	 *	Slide open
	 * 
	 * Enemies
	 * Bosses
	 *	SuperStars
	 * Levels
	 *	left: Water? Forest? Temple?
	 *	up: Ice/mountain
	 *	down: Rock
	 *	right: Fire
	 * 
	 * Snap level objects to grid in editor?
	 *	Player can jump ~3 units
	 * 
	 * Rolling down slopes increases speed/time
	 * Rolling hurts enemies
	 * 
	 * Water
	 *	Decreases gravity
	 *	Decreases move speed
	 *	Allows offground jumps after swim animation is complete
	 *		plays full swim animation then goes to swim-stand
	 * 
	 * Pause screen buttons
	 *	resume, options?, quit
	 * 
	 * Fancy transitions for banner and overlay in star collect
	 * 
	 * Sounds
	 *	jump/walljump/roll cancel
	 *	roll
	 *	wall slide?
	 *	star collect (level up)
	 *	star twinkle
	 *	collect star again (bloop)
	 * 
	 * Art
	 *	Player animations
	 *	Star
	 *	Star collect image
	 *	Spikes
	 *	Door
	 *	Levels, backgrounds
	 * 
	 * Team logo (with sound)
	 * 
	 * slopes?
	 *  slide down >= 45 degree slopes weirdness
	 *  stick to slopes while walking down
	 * 
	 * pick up/throw?
	 * 
	 * Art notes:
	 *	Be sure to set NUM_RUN_FRAMES and NUM_ROLL_FRAMES to the correct values
	 *	Default sprite facing is left (remove - in facing calculation if not)
	 *	Wall slide sprite should face opposite of other sprites
	 *	Star should be white, colored by material
	 *	When changing player dimensions, update bounding box and fully retest level design
	 * 
	 * Cool moves:
	 *	Jump-roll for extra air distance
	 *	Jump-roll-jump for controlled distance
	 *	Jump-roll-jump (hold direction) for maximum distance
	 *	Walljump-roll backwards to climb over a lip
	 *	Roll-jump repeatedly to run fast?
	 */
	
	private const float RUN_ACCEL = 0.4f;
	private const float GRAVITY_ACCEL = -0.6f;
	private const float MAX_RUN_VEL = 7.0f; //maximum speed of horizontal movement

	private const float JUMP_VEL = 14.0f; //jump y speed
	private const float WALLJUMP_VEL = 1.5f * MAX_RUN_VEL; //speed applied at time of walljump
	private const float WALLJUMP_MIN_FACTOR = 0.5f; //amount of walljump kept at minimum if no input
	private const float WALLJUMP_TIME = 0.5f; //time it takes for walljump to wear off

	private const float ROLL_VEL = 2 * MAX_RUN_VEL; //speed of roll
	private const float ROLL_TIME = 0.8f; //time it takes for roll to wear off naturally
	private const float ROLLJUMP_VEL = JUMP_VEL * 2 / 3; //roll cancel jump y speed
	private const float ROLL_HEIGHT = 0.5f; //scale factor of height when rolling
	private const float ROLL_FORCE_AMOUNT = 0.1f; //how much to push the player when they can't unroll

	private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private const int NUM_RUN_FRAMES = 2;
	private const int NUM_ROLL_FRAMES = 2;

	private const float FRAME_TIME = 0.1f; //time per frame of animation

	private float baseScaleX;
	private float baseScaleY;
	private float baseScaleZ;

	private GameManager gm;
	private BoxCollider2D bc;
	private Rigidbody2D rb;

	private float groundAngle;

	private bool jumpQueued = false;
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
	private float bcHeight;
	private bool rollingCollider = false;
	
	enum AnimState
	{
		STAND,
		JUMP,
		WALLSLIDE,
		RUN,
		ROLL
	}
	private SpriteRenderer sr;
	private AnimState animState = AnimState.STAND;
	private int animFrame = 0;
	private float frameTime = FRAME_TIME;
	private int facing = 1; //for animation: -1 for left, 1 for right
	private bool shouldStand = false;

	private Sprite standSprite;
	private Sprite jumpSprite;
	private Sprite wallslideSprite;
	private Sprite[] runSprites;
	private Sprite[] rollSprites;

	private void Start()
	{
		baseScaleX = gameObject.transform.localScale.x;
		baseScaleY = gameObject.transform.localScale.y;
		baseScaleZ = gameObject.transform.localScale.z;

		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		rb = gameObject.GetComponent<Rigidbody2D>();
		bc = gameObject.GetComponent<BoxCollider2D>();
		bcHeight = bc.size.y;

		SLIDE_THRESHOLD = -Mathf.Sqrt(2) / 2; //player will slide down 45 degree angle slopes

		sr = gameObject.GetComponent<SpriteRenderer>();
		LoadSprites();
	}

	private void LoadSprites()
	{
		standSprite = LoadSprite("stand");
		jumpSprite = LoadSprite("jump");
		wallslideSprite = LoadSprite("wallslide");

		runSprites = new Sprite[NUM_RUN_FRAMES];
		LoadAnim("run", runSprites, NUM_RUN_FRAMES);
		
		rollSprites = new Sprite[NUM_ROLL_FRAMES];
		LoadAnim("roll", rollSprites, NUM_ROLL_FRAMES);
	}

	private Sprite LoadSprite(string name)
	{
		return Resources.Load<Sprite>("Images/player/" + name);
	}

	private void LoadAnim(string name, Sprite[] sprites, int numFrames)
	{
		for (int i = 0; i < numFrames; i++)
		{
			sprites[i] = LoadSprite(name + "/frame" + (i + 1));
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)) //Start
		{
			gm.TogglePauseMenu();
		}
		
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) //A
		{
			jumpQueued = true;
		}

		bool triggerPressed = Input.GetAxis("RTrigger") > 0;
		if (Input.GetKeyDown(KeyCode.LeftShift) || triggerPressed)
		{
			rollQueued = true;
		}

		sr.sprite = GetAnimSprite();
	}

	private Sprite GetAnimSprite()
	{
		switch (animState)
		{
			case AnimState.STAND:
				return standSprite;
			case AnimState.JUMP:
				return jumpSprite;
			case AnimState.WALLSLIDE:
				return wallslideSprite;
			case AnimState.RUN:
				return runSprites[animFrame];
			case AnimState.ROLL:
				return rollSprites[animFrame];
		}
		return standSprite;
	}
	
	private void FixedUpdate() {
		/*foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(kcode))
				Debug.Log("KeyCode down: " + kcode);
		}*/
		
		Vector2 velocity = rb.velocity;
		shouldStand = false;
		float xVel = Input.GetAxisRaw("Horizontal");
		if (xVel == 0)
		{
			velocity.x = 0;
			shouldStand = true;
		}
		else
		{
			if (velocity.x != 0 && Mathf.Sign(xVel) != Mathf.Sign(velocity.x)) //don't slide if switching directions on same frame
			{
				velocity.x = 0;
				shouldStand = true;
			}
			else
			{
				velocity.x += RUN_ACCEL * xVel;
				float speedCap = Mathf.Abs(xVel * MAX_RUN_VEL); //use input to clamp max speed so half tilted joystick is slower
				velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
				SetAnimState(AnimState.RUN);
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
			
			if (rollTime <= 0 && !canRoll)
			{
				canRoll = true;
			}
			
			velocity.y = 0;
			if (jumpQueued)
			{
				//regular jump
				StopRoll();
				velocity.y += JUMP_VEL;
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

		if (velocity.y != 0 && rollTime <= 0)
		{
			if (wallSide != 0 && Math.Sign(velocity.x) != wallSide)
			{
				SetAnimState(AnimState.WALLSLIDE);
			}
			else
			{
				SetAnimState(AnimState.JUMP);
			}
		}

		if (rollQueued)
		{
			if (canRoll)
			{
				canRoll = false;
				rollTime = ROLL_TIME;
				SetRollCollider();
			}
		}

		if (rollTime > 0)
		{
			//apply roll velocity
			float timeFactor = rollTime / ROLL_TIME;
			float rollVel = ROLL_VEL * timeFactor;
			velocity.x = rollDir * rollVel;
			rollTime -= Time.fixedDeltaTime;
			if (rollTime <= 0)
			{
				StopRoll();
			}
			if (rollTime > 0) //both may be true if forced roll
			{
				SetAnimState(AnimState.ROLL);
			}
		}

		if (shouldStand)
		{
			SetAnimState(AnimState.STAND);
		}

		AdvanceAnim();

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

	private void SetRollCollider()
	{
		rollingCollider = true;
		float rollHeight = bcHeight * ROLL_HEIGHT;
		bc.size = new Vector2(bc.size.x, rollHeight);
		bc.offset = new Vector2(0, -rollHeight / 2);
	}

	private void SetNormalCollider()
	{
		if (!rollingCollider) return;
		
		rollingCollider = false;
		//if it fits, otherwise keep anim state and rolling
		bc.size = new Vector2(bc.size.x, bcHeight);
		bc.offset = new Vector2(0, 0);
		RaycastHit2D[] hits = Physics2D.BoxCastAll(gameObject.transform.position, bc.size, 0, Vector2.zero, 0, LayerMask.GetMask("LevelGeometry"));
		if (hits.Length > 0) //collided with something else
		{
			canRoll = false;
			rollTime = ROLL_FORCE_AMOUNT;
			SetRollCollider();
		}
	}

	private void ResetWalljump()
	{
		walljumpPush = false;
		walljumpTime = 0;
	}

	private void StopRoll()
	{
		rollTime = 0;
		SetNormalCollider();
	}

	private void SetAnimState(AnimState state)
	{
		animState = state;

		if (state != AnimState.STAND)
		{
			shouldStand = false;
		}
	}

	private void AdvanceAnim()
	{
		if (animState == AnimState.RUN)
		{
			AdvanceFrame(NUM_RUN_FRAMES);
		}
		else if (animState == AnimState.ROLL)
		{
			AdvanceFrame(NUM_ROLL_FRAMES);
		}
		else
		{
			animFrame = 0;
			frameTime = FRAME_TIME;
		}
	}

	private void AdvanceFrame(int maxFrames)
	{
		frameTime -= Time.fixedDeltaTime; //physics based animations but whatever
		if (frameTime <= 0)
		{
			frameTime = FRAME_TIME;
			animFrame = (animFrame + 1) % maxFrames;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//TODO: don't assume it's land, check that first

		if (collision.contacts.Length == 0) return; //not sure what happened

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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Star star = collision.gameObject.GetComponent<Star>();
		if (star != null)
		{
			gm.CollectStar(star);
			Destroy(star.gameObject);
		}
	}
}
