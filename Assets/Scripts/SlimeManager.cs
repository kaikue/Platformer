using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeManager : MonoBehaviour {

    private SlimeObject selectedBridge;

	void Start () {
		
	}
	
	void Update () {
	    if (Input.GetKeyDown(KeyCode.B))
        {
            if (selectedBridge != null)
            {
                selectedBridge.Activate();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            selectedBridge.Deactivate();
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
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BridgeCollider"))
        {
            SlimeObject bridge = collision.GetComponentInParent<SlimeObject>();
            bridge.DeHint();
            selectedBridge = null;
        } 
    }
}
