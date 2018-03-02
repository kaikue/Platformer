using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeManager : MonoBehaviour {

    public int MAX_SLIMES = 3;

    private SlimeObject selectedBridge;
    private SlimeObject selectedStairs;

    private readonly HashSet<SlimeObject> placedSlimeObjects = new HashSet<SlimeObject>();

	void Start () {
		
	}
	
	void Update () {
	    if (Input.GetKeyDown(KeyCode.B))
        {
            if (selectedBridge != null)
            {
                selectedBridge.Activate();
                placedSlimeObjects.Add(selectedBridge);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (selectedStairs != null)
            {
                selectedStairs.Activate();
                placedSlimeObjects.Add(selectedStairs);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (placedSlimeObjects.Count == MAX_SLIMES)
            {
                foreach (SlimeObject slime in placedSlimeObjects)
                {
                    slime.Deactivate();
                }

                placedSlimeObjects.Clear();
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BridgeCollider"))
        {
            SlimeObject bridge = collision.GetComponentInParent<SlimeObject>();
            if (!bridge.activated)
            {
                bridge.Hint();
                selectedBridge = bridge;
            }
        } 

        if (collision.CompareTag("StairsCollider"))
        {
            SlimeObject stairs = collision.GetComponentInParent<SlimeObject>();
            if (!stairs.activated)
            {
                stairs.Hint();
                selectedStairs = stairs;
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BridgeCollider"))
        {
            SlimeObject bridge = collision.GetComponentInParent<SlimeObject>();
            bridge.DeHint();
            selectedBridge = null;
        } 

        if (collision.CompareTag("StairsCollider"))
        {
            SlimeObject stairs = collision.GetComponentInParent<SlimeObject>();
            stairs.DeHint();
            selectedStairs = null;
        } 
    }
}
