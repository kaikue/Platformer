using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDemo : MonoBehaviour
{

    public GameObject SpriteObject;
    public GameObject HurtZone;
    public GameObject SlimeGuide;
    public GameObject Slime;
    public Portal portal;

    public AudioSource JumpSound;
    public AudioSource RollSound;
    public AudioSource SkidSound;
    public AudioSource BounceSound;
    public AudioSource StarCollectSound;
    public AudioSource DeathSound;

    private const float RUN_ACCEL = 0.4f; //acceleration of horizontal movement
    private const float MAX_RUN_VEL = 7.0f; //maximum speed of horizontal movement

    private const float GRAVITY_ACCEL = -0.6f; //acceleration of gravity
    private const float MAX_FALL_VEL = -40.0f; //maximum speed of fall
    private const float SLIDE_FACTOR = 0.5f; //multiplier for fall speed when sliding against wall
    private const float SNAP_DIST = 0.5f;

    private const float JUMP_VEL = 14.0f; //jump y speed
    private const float JUMP_RELEASE_FACTOR = 0.5f; //factor to reduce jump by when releasing
    private const float JUMP_GRACE_TIME = 0.1f; //time after leaving ground player can still jump
    private const float JUMP_BUFFER_TIME = 0.1f; //time before hitting ground a jump will still be queued

    private const float WALLJUMP_VEL = MAX_RUN_VEL; //speed applied at time of walljump
    private const float WALLJUMP_MIN_FACTOR = 0.75f; //amount of walljump kept at minimum if no input
    private const float WALLJUMP_TIME = 0.5f; //time it takes for walljump to wear off
    private const float WALLJUMP_GRACE_TIME = 0.2f; //time after leaving wall player can still walljump

    private const float ROLL_VEL = 2 * MAX_RUN_VEL; //speed of roll
    private const float ROLL_TIME = 1.0f; //time it takes for roll to wear off naturally
    private const float MAX_ROLL_TIME = 2.0f; //time it takes for roll to wear off at the bottom of a long slope
    private const float ROLL_MAX_ADDITION = 5.0f; //amount of roll added on high slopes
    private const float ROLLJUMP_VEL = JUMP_VEL * 2.0f / 3.0f; //roll cancel jump y speed
    private const float ROLL_HEIGHT = 0.5f; //scale factor of height when rolling
    private const float ROLL_FORCE_AMOUNT = 0.1f; //how much to push the player when they can't unroll

    private const float SLIME_BOUNCE_MULTIPLIER = 1.5f; // minimum bounce given by slime as a multiple of JUMP_VEL
    private const float MIN_SLIME_BOUNCE = SLIME_BOUNCE_MULTIPLIER * JUMP_VEL;

    private static Vector2 GRAVITY_NORMAL = new Vector2(0, GRAVITY_ACCEL).normalized;

    private const float HURTZONE_SIZE_FACTOR = 0.7f; //size of squish collider as factor of normal collider (should be between 0 and 1)

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

    private bool jumpQueued = false;
    private bool jumpReleaseQueued = false;
    private bool canJump = false; //used instead of onGround to allow grace time after leaving platform
    private bool canJumpRelease = false; //to only allow the first jump release to slow the jump
    private List<GameObject> grounds = new List<GameObject>();
    private List<GameObject> walls = new List<GameObject>();
    private int wallSide = 0; //1 for wall on left, 0 for none, -1 for wall on right (i.e. points away from wall in x)
    private int lastWallSide = 0;
    private float walljumpTime = 0; //counts down from WALLJUMP_TIME
    private bool walljumpPush = false; //if the player hasn't touched anything and the walljump should keep moving them
    private bool canMidairJump = false;

    private bool rollQueued = false;
    private float rollTime = 0;
    private bool canRoll = true;
    private int rollDir = 1; //-1 for left, 1 for right
    private float normalHeight;
    private bool rollingCollider = false;

    private BoxCollider2D hurtCollider;

    private Vector2 respawnPos;

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
        baseScaleX = transform.localScale.x;
        baseScaleY = transform.localScale.y;
        baseScaleZ = transform.localScale.z;

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        ec = GetComponent<EdgeCollider2D>();
        normalHeight = ec.points[1].y - ec.points[0].y;
        hurtCollider = HurtZone.GetComponent<BoxCollider2D>();
        RefreshHurtZone();

        respawnPos = transform.position;

        sr = SpriteObject.GetComponent<SpriteRenderer>();
        LoadSprites();

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
            StopCoroutine(CancelQueuedJump());
            jumpQueued = true;
            StartCoroutine(CancelQueuedJump());
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0))
        {
            jumpReleaseQueued = true;
        }

        bool triggerPressed = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
        bool shiftPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        if (shiftPressed || triggerPressed)
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
            MoveHorizontal(inputXVel, ref velocity);
        }

        bool onGround = grounds.Count > 0;
        /*if (!onGround && velocity.y == 0)
		{
			//not on ground but not moving up/down- try to snap to ground
			RaycastHit2D[] hits = BoxCast(GRAVITY_NORMAL, SNAP_DIST);
			if (hits.Length > 0)
			{
				RaycastHit2D hit = hits[0];
				float hitDist = hit.distance;
				//not quite the right distance (it's from center of bbox), but moveposition will handle that
				offset.y -= hitDist;
				onGround = true;
				grounds.Add(hit.transform.gameObject);
			}
		}*/

        if (onGround && velocity.y <= 0) //on the ground, didn't just jump
        {
            StayOnGround(ref velocity, ref offset);
        }
        else //in midair
        {
            if (jumpReleaseQueued)
            {
                jumpReleaseQueued = false;
                if (canJumpRelease && velocity.y > 0)
                {
                    velocity.y = velocity.y * JUMP_RELEASE_FACTOR;
                }
                canJumpRelease = false;
            }

            if (!onGround && jumpQueued && wallSide != 0 && !IsRolling())
            {
                WallJump(ref velocity);
            }

            Fall(ref velocity);
        }

        //continued moving past wall corner- clear wallslide
        if (wallSide != 0 && Math.Sign(velocity.x) != wallSide && walls.Count == 0)
        {
            wallSide = 0;
        }

        if (jumpQueued && ((canJump && velocity.y <= 0) || canMidairJump))
        {
            Jump(ref velocity);
        }

        if (walljumpTime > 0 || walljumpPush)
        {
            ApplyWalljumpPush(ref velocity);
        }

        if (velocity.y != 0 && !IsRolling())
        {
            if (wallSide != 0 && velocity.y < 0 && Math.Sign(velocity.x) != wallSide)
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
            rollQueued = false;
            if (canRoll)
            {
                Roll(ref velocity);
            }
        }

        if (IsRolling())
        {
            ApplyRoll(onGround, ref velocity, ref offset);

            if (IsRolling())
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
        transform.localScale = new Vector3(facing * baseScaleX, baseScaleY, baseScaleZ);

        rb.velocity = velocity;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime + offset);

        jumpReleaseQueued = false;
    }

    private void MoveHorizontal(float inputXVel, ref Vector2 velocity)
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

        //received horizontal input, so don't let player get pushed by natural walljump velocity
        walljumpPush = false;

        if (!IsRolling())
        {
            rollDir = Math.Sign(inputXVel);
        }
    }

    private void StayOnGround(ref Vector2 velocity, ref Vector2 offset)
    {
        velocity.y = 0; //TODO: remove?

        //apply moving platform velocity
        //use last where the player is more than half on it
        Rigidbody2D groundRB = grounds[grounds.Count - 1].GetComponent<Rigidbody2D>();
        if (groundRB != null)
        {
            offset += groundRB.velocity * Time.fixedDeltaTime;
        }

        /*if (groundAngle >= SLIDE_THRESHOLD)
        {
            print("slide");
            velocity.y += GRAVITY_ACCEL; //* slope (perp. to ground angle), * friction?
        }
        */
        ResetWalljump();

        if (!IsRolling() && !canRoll)
        {
            SetCanRoll();
        }
    }

    private void Fall(ref Vector2 velocity)
    {
        float gravAccel = GRAVITY_ACCEL;
        float maxFall = MAX_FALL_VEL;
        if (velocity.y < 0 && walls.Count > 0)
        {
            //slide down wall more slowly
            gravAccel *= SLIDE_FACTOR;
            maxFall *= SLIDE_FACTOR;
            //TODO: slide sound/particles?
        }

        velocity.y += gravAccel;
        velocity.y = Mathf.Max(velocity.y, maxFall); //max since they're negative
    }

    private void ApplyWalljumpPush(ref Vector2 velocity)
    {
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

    private void ApplyRoll(bool onGround, ref Vector2 velocity, ref Vector2 offset)
    {
        float timeFactor = rollTime / ROLL_TIME;
        float rollVel = rollDir * ROLL_VEL * timeFactor;

        bool shouldStop = false;
        if (Mathf.Abs(rollVel) < Mathf.Abs(velocity.x))
        {
            shouldStop = true;
        }

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
        Vector2 rollVec = rollVel * groundVec;
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

        if (!IsRolling() || shouldStop)
        {
            StopRoll();
        }
    }

    private void Jump(ref Vector2 velocity)
    {
        jumpQueued = false;
        StopCoroutine(CancelQueuedJump());
        canJump = false;
        canMidairJump = false;
        canJumpRelease = true;
        StopRoll();
        if (!IsRolling()) //don't jump if forced roll
        {
            velocity.y = JUMP_VEL;
            PlayJumpSound();
        }
    }

    private void WallJump(ref Vector2 velocity)
    {
        walljumpTime = WALLJUMP_TIME;
        lastWallSide = wallSide;
        velocity.y = JUMP_VEL;
        walljumpPush = true;
        jumpQueued = false;
        canJumpRelease = true;
        StopCoroutine(CancelQueuedJump());

        PlayJumpSound();
        //SkidSound.Stop();
    }

    private void Roll(ref Vector2 velocity)
    {
        canRoll = false;
        rollTime = ROLL_TIME;
        SetRollCollider();
        ResetWalljump();
        velocity.y = 0;

        RollSound.Play();
    }

    public void SetCanRoll()
    {
        canRoll = true;
        //TODO: fancy effects
    }

    public void SetCanMidairJump()
    {
        canMidairJump = true;
        //TODO: fancy glowy effects
    }

    private IEnumerator CancelQueuedJump()
    {
        yield return new WaitForSeconds(JUMP_BUFFER_TIME);
        jumpQueued = false;
    }

    private bool IsRolling()
    {
        return rollTime > 0;
    }

    public void Kill()
    {
        //TODO: make sure this sets all relevant properties
        StopRoll();
        grounds.Clear();
        canJump = false;
        canMidairJump = false;
        ResetWalljump();
        wallSide = 0;
        walls.Clear();
        rb.velocity = Vector3.zero;
        transform.position = respawnPos;

        DeathSound.Play();
    }

    private void SetRollCollider()
    {
        rollingCollider = true;
        float ecBottom = ec.points[0].y;
        float rollTop = ecBottom + normalHeight * ROLL_HEIGHT;
        SetColliderHeight(rollTop);
    }

    private void StopRoll()
    {
        //if it fits, otherwise keep anim state and rolling
        if (!rollingCollider) return;

        rollingCollider = false;
        float ecBottom = ec.points[0].y;
        float normalTop = ecBottom + normalHeight;
        SetColliderHeight(normalTop);

        RaycastHit2D[] hits = BoxCast(Vector2.zero, 0);
        if (hits.Length > 0) //collided with something when trying to unroll- force roll to continue
        {
            canRoll = false;
            rollTime = Mathf.Max(rollTime + Time.fixedDeltaTime, ROLL_FORCE_AMOUNT);
            SetRollCollider();
        }
        else
        {
            rollTime = 0;
        }
    }

    private void SetColliderHeight(float height)
    {
        Vector2[] points = ec.points;
        points[1].y = height;
        points[2].y = height;
        ec.points = points;
        RefreshHurtZone();
    }

    private void RefreshHurtZone()
    {
        Vector2[] points = ec.points;
        float left = points[1].x;
        float right = points[2].x;
        float bottom = points[0].y;
        float top = points[1].y;
        float ecWidth = right - left;
        float ecHeight = top - bottom;
        float offsetY = (bottom + top) / 2;
        float width = ecWidth * HURTZONE_SIZE_FACTOR;
        float height = ecHeight * HURTZONE_SIZE_FACTOR;
        Vector2 size = new Vector2(width, height);
        hurtCollider.offset = new Vector2(0, offsetY);
        hurtCollider.size = size;
    }

    private RaycastHit2D[] BoxCast(Vector2 direction, float distance)
    {
        Vector2 size = ec.points[2] - ec.points[0];
        return Physics2D.BoxCastAll(rb.position, size, 0, direction, distance, LayerMask.GetMask("LevelGeometry"));
    }

    private void ResetWalljump()
    {
        walljumpPush = false;
        walljumpTime = 0;

        //SkidSound.Stop();
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
        GameObject other = collision.gameObject;

        if (other.tag == "Slime")
        {
            if (other.transform.position.y < rb.position.y)
            {
                float prevXVel = rb.velocity.x;
                rb.velocity = Vector2.Reflect(rb.velocity, collision.contacts[0].normal);
                if (rb.velocity.y < MIN_SLIME_BOUNCE)
                {
                    rb.velocity = new Vector2(rb.velocity.x, MIN_SLIME_BOUNCE);
                }
                //rb.velocity = new Vector2(rb.velocity.x, SLIME_BOUNCE_MULTIPLIER * JUMP_VEL);

                //reverse if rolling into slime
                if (IsRolling() && Mathf.Sign(rb.velocity.x) != Mathf.Sign(prevXVel))
                {
                    rollDir *= -1;
                }

                BounceSound.Play();
            }
            else
            {
                Vector2 velocity = rb.velocity;
                velocity.y = 0;
                rb.velocity = velocity;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts.Length == 0) return; //not sure what happened

        if (collision.gameObject.layer != LayerMask.NameToLayer("LevelGeometry"))
        {
            return;
        }

        ContactPoint2D? groundPoint = GetGround(collision);
        if (groundPoint.HasValue)
        {
            if (!grounds.Contains(collision.gameObject))
            {
                grounds.Add(collision.gameObject);
            }
            groundNormal = groundPoint.Value.normal;
            HitGround();
        }
        else
        {
            RemoveContact(grounds, collision.gameObject, LeaveGround());
        }

        ContactPoint2D? ceilingPoint = GetCeiling(collision);
        if (ceilingPoint.HasValue)
        {
            HitCeiling();
        }

        ContactPoint2D? wallPoint = GetWall(collision);
        if (wallPoint.HasValue)
        {
            if (!walls.Contains(collision.gameObject))
            {
                walls.Add(collision.gameObject);
            }
            float x = Vector2.Dot(Vector2.right, wallPoint.Value.normal);
            int newWallSide = Mathf.RoundToInt(x);
            if (wallSide != newWallSide)
            {
                HitWall(newWallSide);
            }
        }
        else
        {
            RemoveContact(walls, collision.gameObject, LeaveWall());
        }
    }

    private void HitGround()
    {
        canJump = true;
        canMidairJump = false;
    }

    private void HitCeiling()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
    }

    private void HitWall(int newWallSide)
    {
        wallSide = newWallSide;
        StopCoroutine(LeaveWall());

        ResetWalljump();
        StopRoll();
        //if still rolling: bounce (stuck under ledge)
        if (IsRolling())
        {
            rollDir *= -1;
        }

        //SkidSound.PlayScheduled(0.1);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("LevelGeometry"))
        {
            return;
        }

        RemoveContact(grounds, collision.gameObject, LeaveGround());
        RemoveContact(walls, collision.gameObject, LeaveWall());
    }

    private float NormalDot(ContactPoint2D contact)
    {
        return Vector2.Dot(contact.normal, GRAVITY_NORMAL);
    }

    private ContactPoint2D? GetWithDotCheck(Collision2D collision, Func<float, bool> dotCheck)
    {
        if (collision == null) return null;

        int len = collision.contacts.Length;
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            float dot = NormalDot(collision.contacts[i]);
            if (dotCheck(dot))
            {
                return collision.contacts[i];
            }
        }
        return null;
    }

    private ContactPoint2D? GetGround(Collision2D collision)
    {
        return GetWithDotCheck(collision, d => d < 0);
    }

    private ContactPoint2D? GetCeiling(Collision2D collision)
    {
        return GetWithDotCheck(collision, d => d > 0);
    }

    private ContactPoint2D? GetWall(Collision2D collision)
    {
        return GetWithDotCheck(collision, d => Mathf.Abs(d) < 0.01f);
    }

    private void RemoveContact(List<GameObject> contacts, GameObject contact, IEnumerator ifEmpty)
    {
        bool removed = contacts.Remove(contact);
        if (removed && contacts.Count == 0)
        {
            StartCoroutine(ifEmpty);
        }
    }

    private IEnumerator LeaveGround()
    {
        yield return new WaitForSeconds(JUMP_GRACE_TIME);
        canJump = false;
    }

    private IEnumerator LeaveWall()
    {
        yield return new WaitForSeconds(WALLJUMP_GRACE_TIME);
        wallSide = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;

        if (collision.CompareTag("Respawn"))
        {
            respawnPos = transform.position;
        }

        Star star = other.GetComponent<Star>();
        if (star != null && !star.playerTouched)
        {
			star.playerTouched = true;
            gm.CollectStar(star);
            Destroy(other);
            StarCollectSound.Play();

            int starType = (int)gm.levelStarPrefab.GetComponent<Star>().starType;
            if (gm.starsCollected[starType] >= portal.starsRequired)
            {
                if (Slime.activeSelf)
                {
                    SlimeManager slimeManager = Slime.GetComponent<SlimeManager>();
                    slimeManager.DestroyBridge();
                    slimeManager.sprite.SetActive(false);
                }
                SlimeGuide.SetActive(true);
            }
        }

        Door door = other.GetComponent<Door>();
        if (door != null)
        {
            gm.ShowHUDDoorStars(door);
            door.TryOpen();
        }

        Portal portalCollided = other.GetComponent<Portal>();
        if (portalCollided != null)
        {
            int starType = (int)gm.levelStarPrefab.GetComponent<Star>().starType;
            if (gm.starsCollected[starType] >= portalCollided.starsRequired)
            {
                NextLevel();
            }
        }

        StarSpot starSpot = other.GetComponent<StarSpot>();
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

    public int GetFacing()
    {
        return facing;
    }
}
