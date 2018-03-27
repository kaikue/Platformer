using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDemo : MonoBehaviour {

	public GameObject SpriteObject;
	public GameObject SlimeGuide;
	public GameObject Slime;
    public Portal portal;

    public AudioSource JumpSound;
    public AudioSource RollSound;
    public AudioSource SkidSound;
    public AudioSource BounceSound;
    public AudioSource StarCollectSound;
    public AudioSource DeathSound;

	private const float RUN_ACCEL = 0.4f;
	private const float GRAVITY_ACCEL = -0.6f;
	private const float MAX_RUN_VEL = 7.0f; //maximum speed of horizontal movement
	private const float SNAP_DIST = 0.5f;

	private const float JUMP_VEL = 14.0f; //jump y speed
	private const float WALLJUMP_VEL = 1.5f * MAX_RUN_VEL; //speed applied at time of walljump

	private const float WALLJUMP_MIN_FACTOR = 0.5f; //amount of walljump kept at minimum if no input
	private const float WALLJUMP_TIME = 0.5f; //time it takes for walljump to wear off
	private const float WALLJUMP_GRACE_TIME = 0.1f; //time after leaving wall player can still walljump

	private const float ROLL_VEL = 2 * MAX_RUN_VEL; //speed of roll
	private const float ROLL_TIME = 1.0f; //time it takes for roll to wear off naturally
	private const float MAX_ROLL_TIME = 2.0f; //time it takes for roll to wear off at the bottom of a long slope
	private const float ROLL_MAX_ADDITION = 5.0f; //amount of roll added on high slopes
	private const float ROLLJUMP_VEL = JUMP_VEL * 2 / 3; //roll cancel jump y speed
	private const float ROLL_HEIGHT = 0.5f; //scale factor of height when rolling
	private const float ROLL_FORCE_AMOUNT = 0.1f; //how much to push the player when they can't unroll

    private const float SLIME_BOUNCE_MULTIPLIER = 1.5f; // minimum bounce given by slime as a multiple of JUMP_VEL
	private const float MIN_SLIME_BOUNCE = SLIME_BOUNCE_MULTIPLIER * JUMP_VEL;

	private static float SLIDE_THRESHOLD;
	private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

	private const int NUM_RUN_FRAMES = 10;
	private const int NUM_ROLL_FRAMES = 4;

	private const float FRAME_TIME = 0.1f; //time in seconds per frame of animation

	private const float PITCH_VARIATION = 0.1f;

	private float baseScaleX;
	private float baseScaleY;
	private float baseScaleZ;

	private GameManager gm;
	private EdgeCollider2D ec;
	private Rigidbody2D rb;

	private Vector2 groundNormal;

	private Collision2D lastCollision;

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
	private float ecHeight;
	private bool rollingCollider = false;

    private Vector3 respawnPoint;

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
	private int facing = -1; //for animation: -1 for right, 1 for left (images face left)
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
		ec = gameObject.GetComponent<EdgeCollider2D>();
		ecHeight = ec.points[1].y - ec.points[0].y;

		SLIDE_THRESHOLD = -Mathf.Sqrt(2) / 2; //player will slide down 45 degree angle slopes

		sr = SpriteObject.GetComponent<SpriteRenderer>();
		LoadSprites();

        respawnPoint = transform.position;

        Slime.SetActive(!SceneManager.GetActiveScene().name.Equals("Hub"));
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

		if (Input.GetKeyDown(KeyCode.N))
		{
			NextLevel();
		}

		bool triggerPressed = Input.GetAxis("RTrigger") > 0;
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || triggerPressed)
		{
			rollQueued = true;
		}
		
		AdvanceAnim();
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
	
	private void FixedUpdate()
	{
		Vector2 velocity = rb.velocity; //for changing the player's actual velocity
		Vector2 offset = Vector2.zero; //for any additional movement nudges
		shouldStand = false;
		float inputXVel = Input.GetAxisRaw("Horizontal");
		if (inputXVel == 0)
		{
			velocity.x = 0;
			shouldStand = true;
		}
		else
		{
			if (velocity.x != 0 && Mathf.Sign(inputXVel) != Mathf.Sign(velocity.x)) //don't slide if switching directions on same frame
			{
				velocity.x = 0;
				shouldStand = true;
			}
			else
			{
				velocity.x += RUN_ACCEL * inputXVel;
				float speedCap = Mathf.Abs(inputXVel * MAX_RUN_VEL); //use input to clamp max speed so half tilted joystick is slower
				velocity.x = Mathf.Clamp(velocity.x, -speedCap, speedCap);
				SetAnimState(AnimState.RUN);
			}

			if (walljumpTime <= 0)
			{
				//received horizontal input, so don't let player get pushed by natural walljump velocity
				walljumpPush = false;
			}

			if (!isRolling())
			{
				rollDir = Math.Sign(inputXVel);
			}
		}
		
		bool onGround = grounds.Count > 0;
		if (!onGround && velocity.y == 0)
		{
			//not on ground but not moving up/down- try to snap to ground
			RaycastHit2D[] hits = BoxCast(GRAVITY_NORMAL, SNAP_DIST);
			if (hits.Length > 0)
			{
				float hitDist = hits[0].distance;
				//not quite the right distance (it's from center of bbox), but moveposition will handle that
				offset.y -= hitDist;
				onGround = true;
			}
		}

		if (onGround && velocity.y <= 0) //on the ground, didn't just jump
		{
			/*if (groundAngle >= SLIDE_THRESHOLD)
			{
				print("slide");
				velocity.y += GRAVITY_ACCEL; //* slope (perp. to ground angle), * friction?
			}
			else
			{*/
			ResetWalljump();
			
			if (!isRolling() && !canRoll)
			{
				canRoll = true;
			}
			
			velocity.y = 0;
			if (jumpQueued)
			{
				//regular jump
				StopRoll();
				velocity.y += JUMP_VEL;
                PlayJumpSound();
				jumpQueued = false;
			}

			//print(grounds.Count + " " + velocity.y + " " + onGround + " " + (velocity.y <= 0));

			//}
		}
		else
		{
			if (!onGround && jumpQueued && wallSide != 0 && !isRolling())
			{
				//walljump
				walljumpTime = WALLJUMP_TIME;
				lastWallSide = wallSide;
				velocity.y = JUMP_VEL;
				walljumpPush = true;
				jumpQueued = false;

				PlayJumpSound();
				SkidSound.Stop();
			}
			velocity.y += GRAVITY_ACCEL;
		}
		
		/*if (isRolling() && jumpQueued)
		{
			//midair roll cancel
			StopRoll();
			ResetWalljump();
			velocity.y = ROLLJUMP_VEL;
			jumpQueued = false;
		}*/

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

		if (velocity.y != 0 && !isRolling())
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

                RollSound.Play();
			}
		}

		if (isRolling())
		{
			//apply roll velocity
			float timeFactor = rollTime / ROLL_TIME;
			float rollVel = ROLL_VEL * timeFactor;
			
			//roll in direction of ground
			Vector2 groundVec;
			if (onGround)
			{
				groundVec = Vector3.Cross(groundNormal, Vector3.forward); //left handed
				groundVec.Normalize();
			}
			else
			{
				groundVec = Vector2.right;
			}
			Vector2 rollVec = rollDir * rollVel * groundVec;
			velocity.x += rollVec.x;
			float speedCapX = Mathf.Abs(rollVec.x);
			velocity.x = Mathf.Clamp(velocity.x, -speedCapX, speedCapX);
			offset.y += rollVec.y * Time.fixedDeltaTime; //do this with offset so it doesn't persist when rolling up

			//roll for longer on slope
			if (rollVec.y < 0)
			{
				float maxAddition = ROLL_MAX_ADDITION * Time.fixedDeltaTime;
				float groundAngle = Vector2.Dot(groundNormal.normalized, Vector2.right * rollDir);
				float rollTimeAddition = groundAngle * maxAddition;
				rollTime = Mathf.Min(rollTime + rollTimeAddition, ROLL_TIME);
			}

			rollTime -= Time.fixedDeltaTime;

			if (!isRolling())
			{
				StopRoll();
			}
			if (isRolling()) //both may be true if forced roll
			{
				SetAnimState(AnimState.ROLL);
			}
		}

		if (shouldStand)
		{
			SetAnimState(AnimState.STAND);
		}

		if (velocity.x != 0)
		{
			facing = -Math.Sign(velocity.x); //make this positive if sprites face right
			if (inputXVel == 0)
			{
				rollDir = Math.Sign(velocity.x);
			}
		}
		gameObject.transform.localScale = new Vector3(facing * baseScaleX, baseScaleY, baseScaleZ);

		rb.velocity = velocity;
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime + offset);
		
		jumpQueued = false;
		rollQueued = false;
	}

	private bool isRolling()
	{
		return rollTime > 0;
	}

	public void Kill()
    {
        transform.position = respawnPoint;
		StopRoll();
        lastCollision = null;
        grounds.Clear();
		ClearWall();
        rb.velocity = Vector3.zero;

        DeathSound.Play();
    }

    public int GetFacing()
    {
        return facing;
    }

	private void SetRollCollider()
	{
		rollingCollider = true;
		float ecBottom = ec.points[0].y;
		float rollTop = ecBottom + ecHeight * ROLL_HEIGHT;
		SetColliderHeight(rollTop);
	}

	private void SetNormalCollider()
	{
		//if it fits, otherwise keep anim state and rolling
		if (!rollingCollider) return;
		
		rollingCollider = false;
		float ecBottom = ec.points[0].y;
		float normalTop = ecBottom + ecHeight;
		SetColliderHeight(normalTop);

		RaycastHit2D[] hits = BoxCast(Vector2.zero, 0);
		if (hits.Length > 0) //collided with something else
		{
			canRoll = false;
			rollTime = ROLL_FORCE_AMOUNT;
			SetRollCollider();
		}
	}

	private void SetColliderHeight(float height)
	{
		Vector2[] points = ec.points;
		points[1].y = height;
		points[2].y = height;
		ec.points = points;
	}

	private RaycastHit2D[] BoxCast(Vector2 direction, float distance)
	{
		Vector2 size = ec.points[2] - ec.points[0];
		return Physics2D.BoxCastAll(gameObject.transform.position, size, 0, direction, distance, LayerMask.GetMask("LevelGeometry"));
	}
	
	private void ResetWalljump()
	{
		walljumpPush = false;
		walljumpTime = 0;

        SkidSound.Stop();
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

	private void AdvanceFrame(int numFrames)
	{
		if (animFrame >= numFrames)
		{
			animFrame = 0;
		}

		frameTime -= Time.deltaTime;
		if (frameTime <= 0)
		{
			frameTime = FRAME_TIME;
			animFrame = (animFrame + 1) % numFrames;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Slime")
		{
			float prevXVel = rb.velocity.x;
			rb.velocity = Vector2.Reflect(rb.velocity, collision.contacts[0].normal);
			if (rb.velocity.y < MIN_SLIME_BOUNCE)
			{
				rb.velocity = new Vector2(rb.velocity.x, MIN_SLIME_BOUNCE);
			}
			//rb.velocity = new Vector2(rb.velocity.x, SLIME_BOUNCE_MULTIPLIER * JUMP_VEL);

			//reverse if rolling into slime
			if (isRolling() && Mathf.Sign(rb.velocity.x) != Mathf.Sign(prevXVel))
			{
				rollDir *= -1;
			}

            BounceSound.Play();

			return;
		}
    }

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.contacts.Length == 0) return; //not sure what happened

		//TODO: don't assume it's land, check that first
		if (collision.gameObject.tag == "Slime" || collision.gameObject.tag == "Obstacle" || collision.gameObject.tag == "StarSpot")
        {
            return;
        }

		if (HasGround(collision) && !HasGround(lastCollision))
		{
			if (!grounds.Contains(collision.gameObject))
			{
				grounds.Add(collision.gameObject);
			}
			groundNormal = GetGround(collision).normal;
		}

		if (HasCeiling(collision) && !HasCeiling(lastCollision))
		{
			Vector2 velocity = rb.velocity;
			velocity.y = 0;
			rb.velocity = velocity;
		}

		if (HasWall(collision))
		{
			float x = Vector2.Dot(Vector2.right, GetWall(collision).normal);
			int newWallSide = Mathf.RoundToInt(x);
			if (!HasWall(lastCollision) || newWallSide != wallSide)
			{
				wallSide = newWallSide;
				StopCoroutine(LeaveWall());
				wall = collision.gameObject;
				
				ResetWalljump();
				StopRoll();
				//SkidSound.PlayScheduled(0.1);
			}
		}

		if (!HasGround(collision) && HasGround(lastCollision))
        {
		    grounds.Remove(collision.gameObject);
        }

        if (!HasWall(collision) && HasWall(lastCollision))
        {
			SkidSound.Stop();
			StartCoroutine(LeaveWall());
        }

        lastCollision = collision;

	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Slime") || collision.gameObject.tag == "Obstacle" || collision.gameObject.tag == "StarSpot")
		{
			return;
		}
		lastCollision = null;
		grounds.Remove(collision.gameObject);

		if (collision.gameObject == wall)
		{
			StartCoroutine(LeaveWall());
		}
	}

    private float NormalDot(ContactPoint2D contact)
    {
        Vector2 normal = contact.normal;
        return Vector2.Dot(normal, GRAVITY_NORMAL);
    }

	private float[] NormalDotList(Collision2D collision)
	{
        float[] dots = new float[collision.contacts.Length];
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            dots[i] = NormalDot(collision.contacts[i]);
        }

        return dots;
	}

	private bool HasGround(Collision2D collision)
	{
        if (collision == null) return false;
        float[] dots = NormalDotList(collision);
        foreach (float dot in dots)
        {
            if (dot < -.0001f)
            {
                return true;
            }
        }
        return false;
	}

    private ContactPoint2D GetGround(Collision2D collision)
    {
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            float dot = NormalDot(collision.contacts[i]);
            if (dot < -.0001f)
            {
                return collision.contacts[i];
            }
        }

        return collision.contacts[0]; // should never happen
    }

    private ContactPoint2D GetWall(Collision2D collision)
    {
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            float dot = NormalDot(collision.contacts[i]);
            if (Mathf.Abs(dot) < 0.001f)
            {
                return collision.contacts[i];
            }
        }

        return collision.contacts[0]; // should never happen
    }


	private bool HasCeiling(Collision2D collision)
	{
        if (collision == null) return false;
        float[] dots = NormalDotList(collision);
        foreach (float dot in dots)
        {
            if (dot > .0001f)
            {
                return true;
            }
        }
        return false;
	}

	private bool HasWall(Collision2D collision)
	{
        if (collision == null) return false;
        float[] dots = NormalDotList(collision);
        foreach (float dot in dots)
        {
            if (Mathf.Abs(dot) < 0.0001f)
            {
                return true;
            }           
        }
        return false;
	}

	private IEnumerator LeaveWall()
	{
		yield return new WaitForSeconds(WALLJUMP_GRACE_TIME);
		ClearWall();
	}

	private void ClearWall()
	{
		wall = null;
		wallSide = 0;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Respawn"))
        {
            respawnPoint = transform.position; 
        }
		
		Star star = collision.gameObject.GetComponent<Star>();
		if (star != null)
		{
			gm.CollectStar(star);
			Destroy(star.gameObject);
            StarCollectSound.Play();

			int starType = (int)gm.levelStarPrefab.GetComponent<Star>().starType;
			if (gm.starsCollected[starType] >= portal.starsRequired)
            {
                SlimeGuide.SetActive(true);
                Slime.SetActive(false);
            }

		}

		Door door = collision.gameObject.GetComponent<Door>();
		if (door != null)
		{
			gm.ShowHUDDoorStars(door);
			door.TryOpen();
		}

		Portal portalCollided = collision.gameObject.GetComponent<Portal>();
		if (portalCollided != null)
		{
			int starType = (int)gm.levelStarPrefab.GetComponent<Star>().starType;
			if (gm.starsCollected[starType] >= portalCollided.starsRequired)
			{
				NextLevel();
			}
		}

		StarSpot starSpot = collision.gameObject.GetComponent<StarSpot>();
		if (starSpot != null && !starSpot.touched)
		{
			starSpot.Fill();
		}
	}

	private void NextLevel()
	{
		gm.loadingOverlay.SetActive(true);
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(sceneIndex + 1);
	}

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Respawn"))
        {
            respawnPoint = transform.position; 
        }
    }

	private void PlayJumpSound()
	{
		JumpSound.pitch = UnityEngine.Random.Range(2 - PITCH_VARIATION, 2 + PITCH_VARIATION);
		JumpSound.Play();
	}

	public void Shove()
	{
		if (grounds.Count > 0)
		{
			//apply some velocity- use walljump code
			walljumpTime = WALLJUMP_TIME;
			lastWallSide = UnityEngine.Random.Range(0f, 1f) < 0.5f ? -1 : 1;
			Vector2 velocity = rb.velocity;
			velocity.y = JUMP_VEL / 2;
			rb.velocity = velocity;
			walljumpPush = true;
			DeathSound.Play();
		}
	}
}
