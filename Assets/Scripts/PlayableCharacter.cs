using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableCharacter : MonoBehaviour {

    enum Action
    {
        JUMP,
        MOVE_RIGHT,
        MOVE_LEFT,
    }

    enum CollisionType
    {
        GROUND,
        STAIRS,
        CEILING,
        WALL
    }

    enum StateHorizontal
    {
        MOVING_RIGHT,
        MOVING_RIGHT_DECELERATING,
        MOVING_RIGHT_ACCELERATING_RIGHT,
        MOVING_RIGHT_ACCELERATING_LEFT,
        STILL,
        MOVING_LEFT,
        MOVING_LEFT_DECELERATING,
        MOVING_LEFT_ACCELERATING_LEFT,
        MOVING_LEFT_ACCELERATING_RIGHT
    }

    enum StateVertical
    {
        JUMPING,
        FALLING,
        GROUNDED,
        STAIRS,
    }

    public Manager manager;

    public KeyCode KEYCODE_JUMP = KeyCode.Space;
    public KeyCode KEYCODE_MOVE_RIGHT = KeyCode.RightArrow;
    public KeyCode KEYCODE_MOVE_LEFT = KeyCode.LeftArrow;
    public KeyCode KEYCODE_SWITCH_CHARACTERS = KeyCode.S;

    public float JUMP_SPEED;
    public float RUN_SPEED;

    public Vector2 JUMP_FORCE;
    public Vector2 GRAVITY;
    public Vector2 MOVE_RIGHT_FORCE;
    public Vector2 DECELERATION_RIGHT_FORCE;
    public Vector2 MOVE_LEFT_FORCE;
    public Vector2 DECELERATION_LEFT_FORCE;

    private readonly HashSet<Action> queuedActions = new HashSet<Action>();
    private readonly Dictionary<Collider2D, CollisionType> activeCollisions = new Dictionary<Collider2D, CollisionType>();

    // State
    private StateHorizontal stateHorizontal;
    private StateVertical stateVertical;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer sr;

	void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        stateVertical = StateVertical.FALLING;
        stateHorizontal = StateHorizontal.STILL;
    } 

	void Update ()
    {
        ProcessInput();
        RenderState();
	}

    void FixedUpdate()
    {
        ProcessQueuedActions();
        ProcessCollisions();
        ApplyState();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionType type = GetCollisionType(collision);
        if (activeCollisions.ContainsKey(collision.collider))
        {
            activeCollisions.Remove(collision.collider);
        }
        activeCollisions.Add(collision.collider, type);
    }


    void OnCollisionExit2D(Collision2D collision)
    {
        activeCollisions.Remove(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs"))
        {
            activeCollisions.Add(collision, CollisionType.STAIRS); 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs"))
        {
            activeCollisions.Remove(collision);
        }
    }

    private void ProcessCollisions()
    {
        bool ground = activeCollisions.ContainsValue(CollisionType.GROUND);
        bool stairs = activeCollisions.ContainsValue(CollisionType.STAIRS);
        bool ceiling = activeCollisions.ContainsValue(CollisionType.CEILING);
        bool wall = activeCollisions.ContainsValue(CollisionType.WALL);

        if (wall)
        {
            stateHorizontal = StateHorizontal.STILL;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }

        if (stairs)
        {
            stateVertical = StateVertical.STAIRS;
            return;
        }

        if (ground && ceiling)
        {
            switch (stateVertical)
            {
                case StateVertical.JUMPING:
                case StateVertical.FALLING:
                case StateVertical.STAIRS:
                    stateVertical = StateVertical.GROUNDED;
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                    break;
                default:
                    break;
            }
        }
        else if (ground && !ceiling)
        {
            switch (stateVertical)
            {
                case StateVertical.FALLING:
                case StateVertical.STAIRS:
                    stateVertical = StateVertical.GROUNDED;
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                    break;
                default:
                    break;
            }
        }
        else if (!ground && ceiling)
        {
            switch (stateVertical)
            {
                case StateVertical.JUMPING:
                case StateVertical.GROUNDED:
                case StateVertical.STAIRS:
                    stateVertical = StateVertical.FALLING;
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                    break;
                default:
                    break;
            }
        }
        else if (!ground && !ceiling)
        {
            switch (stateVertical)
            {
                case StateVertical.GROUNDED:
                case StateVertical.STAIRS:
                    stateVertical = StateVertical.FALLING;
                    break;
                default:
                    break;
            }
        }

    }

    // Process input from current frame
    private void ProcessInput()
    {
        if (Input.GetKeyDown(KEYCODE_JUMP))
        {
            queuedActions.Add(Action.JUMP);
        }

        if (Input.GetKeyDown(KEYCODE_MOVE_RIGHT))
        {
            queuedActions.Add(Action.MOVE_RIGHT);
        }

        if (Input.GetKeyUp(KEYCODE_MOVE_RIGHT))
        {
            queuedActions.Remove(Action.MOVE_RIGHT);
        }

        if (Input.GetKeyDown(KEYCODE_MOVE_LEFT))
        {
            queuedActions.Add(Action.MOVE_LEFT);
        }

        if (Input.GetKeyUp(KEYCODE_MOVE_LEFT))
        {
            queuedActions.Remove(Action.MOVE_LEFT);
        }

        if (Input.GetKeyDown(KEYCODE_SWITCH_CHARACTERS))
        {
            //manager.SwitchCharacter();
        }
    }

    private void RenderState()
    {
    }

    // Process state changes due to inputs recieved on this frame. Clear input queue
    private void ProcessQueuedActions()
    {
        if (queuedActions.Contains(Action.JUMP))
        {
            if (stateVertical == StateVertical.GROUNDED)
            {
                stateVertical = StateVertical.JUMPING;
            }
            queuedActions.Remove(Action.JUMP);
        }

        if (queuedActions.Contains(Action.MOVE_RIGHT) && !queuedActions.Contains(Action.MOVE_LEFT))
        {
            switch (stateHorizontal)
            {
                case StateHorizontal.MOVING_RIGHT_DECELERATING:
                case StateHorizontal.MOVING_RIGHT_ACCELERATING_LEFT:
                case StateHorizontal.STILL:
                    stateHorizontal = StateHorizontal.MOVING_RIGHT_ACCELERATING_RIGHT;
                    break;
                case StateHorizontal.MOVING_LEFT:
                case StateHorizontal.MOVING_LEFT_DECELERATING:
                case StateHorizontal.MOVING_LEFT_ACCELERATING_LEFT:
                    stateHorizontal = StateHorizontal.MOVING_LEFT_ACCELERATING_RIGHT;
                    break;
                default:
                    break;
            }

        } else if (queuedActions.Contains(Action.MOVE_LEFT) && !queuedActions.Contains(Action.MOVE_RIGHT))
        {
            switch (stateHorizontal)
            {
                case StateHorizontal.MOVING_LEFT_DECELERATING:
                case StateHorizontal.MOVING_LEFT_ACCELERATING_RIGHT:
                case StateHorizontal.STILL:
                    stateHorizontal = StateHorizontal.MOVING_LEFT_ACCELERATING_LEFT;
                    break;
                case StateHorizontal.MOVING_RIGHT:
                case StateHorizontal.MOVING_RIGHT_DECELERATING:
                case StateHorizontal.MOVING_RIGHT_ACCELERATING_RIGHT:
                    stateHorizontal = StateHorizontal.MOVING_RIGHT_ACCELERATING_LEFT;
                    break;
                default:
                    break;
            }
        } else 
        {
            switch (stateHorizontal)
            {
                case StateHorizontal.MOVING_RIGHT:
                case StateHorizontal.MOVING_RIGHT_ACCELERATING_RIGHT:
                case StateHorizontal.MOVING_RIGHT_ACCELERATING_LEFT:
                    stateHorizontal = StateHorizontal.MOVING_RIGHT_DECELERATING;
                    break;
                case StateHorizontal.MOVING_LEFT:
                case StateHorizontal.MOVING_LEFT_ACCELERATING_LEFT:
                case StateHorizontal.MOVING_LEFT_ACCELERATING_RIGHT:
                    stateHorizontal = StateHorizontal.MOVING_LEFT_DECELERATING;
                    break;
                default:
                    break;
            }
        }
    }

    // Apply physics updates based on the current state. Update state if necessary
    private void ApplyState()
    {
        if (stateVertical == StateVertical.STAIRS)
        {
            rb.isKinematic = true;
            sr.color = Color.red;
        } else
        {
            rb.isKinematic = false;
            sr.color = Color.blue;
        }

        switch (stateHorizontal)
        {
            case StateHorizontal.MOVING_RIGHT_ACCELERATING_RIGHT:
                if (rb.velocity.x < RUN_SPEED)
                {
                    rb.velocity += MOVE_RIGHT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.MOVING_RIGHT;
                }
                break;
            case StateHorizontal.MOVING_LEFT_ACCELERATING_RIGHT:
                if (rb.velocity.x < 0.0f)
                {
                    rb.velocity += MOVE_RIGHT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.MOVING_RIGHT_ACCELERATING_RIGHT;
                }
                break;
            case StateHorizontal.MOVING_RIGHT_ACCELERATING_LEFT:
                if (rb.velocity.x > 0.0f)
                {
                    rb.velocity += MOVE_LEFT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.MOVING_LEFT_ACCELERATING_LEFT;
                }
                break;
            case StateHorizontal.MOVING_LEFT_ACCELERATING_LEFT:
                if (rb.velocity.x > -1.0f * RUN_SPEED)
                {
                    rb.velocity += MOVE_LEFT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.MOVING_LEFT;
                }
                break;
            case StateHorizontal.MOVING_RIGHT_DECELERATING:
                if (rb.velocity.x + DECELERATION_RIGHT_FORCE.x * Time.fixedDeltaTime > 0)
                {
                    rb.velocity += DECELERATION_RIGHT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.STILL;
                }
                break;
            case StateHorizontal.MOVING_LEFT_DECELERATING:
                if (rb.velocity.x + DECELERATION_LEFT_FORCE.x * Time.fixedDeltaTime < 0)
                {
                    rb.velocity += DECELERATION_LEFT_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateHorizontal = StateHorizontal.STILL;
                }
                break;
            default:
                break;
        }

        switch (stateVertical)
        {
            case StateVertical.JUMPING:
                if (rb.velocity.y < JUMP_SPEED)
                {
                    rb.velocity += JUMP_FORCE * Time.fixedDeltaTime;
                } else
                {
                    stateVertical = StateVertical.FALLING;
                }
                break;
            case StateVertical.FALLING:
                rb.velocity += GRAVITY * Time.fixedDeltaTime; 
                break;
            case StateVertical.STAIRS:
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.x * 3.0f / 5.0f);
                break;
            default:
                break;
        }
    }

    private CollisionType GetCollisionType(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        float normalAngle = Vector2.Angle(normal, GRAVITY);

        if (Mathf.Abs(normalAngle) < 15.0f)
        {
            return CollisionType.CEILING;
        } else if (Mathf.Abs(normalAngle - 180.0f) < 15.0f)
        {
            return CollisionType.GROUND;
        } else 
        {
            return CollisionType.WALL;
        }

    }
}
