using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeManager : MonoBehaviour {

    public int MAX_SLIMES = 3;
    public GameObject slimeRetrieverPrefab;

    private GameObject slimeRetriever;
    private SlimeObject selectedBridge;
    private SlimeObject selectedStairs;
    private bool canRetrieveSlime;

    private readonly HashSet<SlimeObject> placedSlimeObjects = new HashSet<SlimeObject>();

	void Start () {
		
	}
	
	void Update () {
	    if (Input.GetKeyDown(KeyCode.B))
        {
            if (selectedBridge != null  && placedSlimeObjects.Count < MAX_SLIMES)
            {
                selectedBridge.Activate();
                placedSlimeObjects.Add(selectedBridge);

                if (placedSlimeObjects.Count == 1)
                {
                    slimeRetriever = Instantiate(slimeRetrieverPrefab);
                    slimeRetriever.transform.position = transform.position;
                    print("Created slime retriever");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (selectedStairs != null && placedSlimeObjects.Count < MAX_SLIMES)
            {
                selectedStairs.Activate();
                placedSlimeObjects.Add(selectedStairs);

                if (placedSlimeObjects.Count == 1)
                {
                    slimeRetriever = Instantiate(slimeRetrieverPrefab);
                    slimeRetriever.transform.position = transform.position;
                    print("Created slime retriever");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (canRetrieveSlime) 
            {
                foreach (SlimeObject slime in placedSlimeObjects)
                {
                    slime.Deactivate();
                }

                placedSlimeObjects.Clear();
                Destroy(slimeRetriever);
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BridgeCollider"))
        {
            SlimeObject bridge = collision.GetComponentInParent<SlimeObject>();
            if (!bridge.activated && placedSlimeObjects.Count < MAX_SLIMES)
            {
                bridge.Hint();
                selectedBridge = bridge;
            }
        } 

        if (collision.CompareTag("StairsCollider"))
        {
            SlimeObject stairs = collision.GetComponentInParent<SlimeObject>();
            if (!stairs.activated && placedSlimeObjects.Count < MAX_SLIMES)
            {
                stairs.Hint();
                selectedStairs = stairs;
            }
        } 

        if (collision.CompareTag("SlimeRetriever"))
        {
            canRetrieveSlime = true; 
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

        if (collision.CompareTag("SlimeRetriever"))
        {
            canRetrieveSlime = false; 
        }
    }
}
