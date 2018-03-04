using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairInteraction : MonoBehaviour {

    private Stairs currentStairs;

    private Rigidbody2D rb;
    private Collider2D col;
    private PlayableCharacter pc;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        pc = GetComponent<PlayableCharacter>();
    }

    private void FixedUpdate()
    {
        UpdateYPosition();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs"))
        {
            Stairs stairs = collision.GetComponent<Stairs>();
            if (EnteredFromAbove(stairs))
            {
                currentStairs = collision.GetComponent<Stairs>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs"))
        {
            if (currentStairs == collision.GetComponent<Stairs>())
            {
                currentStairs = null;
            }
        }
    }

    private void UpdateYPosition()
    {
        if (currentStairs != null)
        {
            float leftCornerY = currentStairs.GetYPosition(CharacterLeftCorner().x);
            float rightCornerY = currentStairs.GetYPosition(CharacterRightCorner().x);
            float maxCornerY = Mathf.Max(leftCornerY, rightCornerY);

            float characterY = CharacterLeftCorner().y;

            if (maxCornerY > characterY)
            {
                rb.position = new Vector2(rb.position.x, maxCornerY + (rb.position.y - characterY));
                rb.velocity = new Vector2(rb.velocity.x, currentStairs.GetSlope() * rb.velocity.x);
                if (pc.stateVertical == PlayableCharacter.StateVertical.FALLING)
                {
                    pc.ChangeVerticalState(PlayableCharacter.StateVertical.GROUNDED);
                }
            } else if (maxCornerY + 0.1f < characterY)
            {
                if (pc.stateVertical == PlayableCharacter.StateVertical.GROUNDED)
                {
                    pc.ChangeVerticalState(PlayableCharacter.StateVertical.FALLING);
                }
            }
        }
    }

    private bool EnteredFromAbove(Stairs stairs)
    {
        Vector2 rightCorner = CharacterRightCorner();
        Vector2 leftCorner = CharacterLeftCorner();

        return rightCorner.y >= stairs.GetYPosition(rightCorner.x) - 0.2f || leftCorner.y >= stairs.GetYPosition(leftCorner.x) - 0.2f;
    }

    private Vector2 CharacterRightCorner()
    {
        return new Vector2(col.bounds.max.x, col.bounds.min.y);
    }

    private Vector2 CharacterLeftCorner()
    {
        return new Vector2(col.bounds.min.x, col.bounds.min.y);
    }

}
