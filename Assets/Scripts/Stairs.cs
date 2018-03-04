using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour {

    public Vector2 bottomCorner;
    public Vector2 topCorner;

    public float GetYPosition(float xPosition)
    {
        float slope = GetSlope();

        float xMin = Mathf.Min(bottomCorner.x, topCorner.x);
        float xMax = Mathf.Max(bottomCorner.x, topCorner.x);

        return bottomCorner.y + (Mathf.Clamp(xPosition, xMin, xMax) - bottomCorner.x) * slope;
    }

    public float GetSlope()
    {
        return (topCorner.y - bottomCorner.y) / (topCorner.x - bottomCorner.x);
    }

    private void Reset()
    {
        Bounds bounds = GetComponent<BoxCollider2D>().bounds;
        bottomCorner = new Vector2(bounds.min.x, bounds.min.y + 1.1f);
        topCorner = new Vector2(bounds.max.x, bounds.max.y - 0.9f);
    }
}
