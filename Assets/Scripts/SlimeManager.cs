using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeManager : MonoBehaviour {

    public Tilemap bridgeActivatorTiles;
    public GameObject slimeObjectPrefab;

    private SpriteRenderer sr;

    private readonly HashSet<SlimeObject> selectedBridge = new HashSet<SlimeObject>();

    private Vector3 queuedCollisionPoint;
    private bool colliding;
    private bool activated;
    private bool bridgeSwapQueued;

	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            bridgeSwapQueued = true;
        }
	}

    private void FixedUpdate()
    {
		if (bridgeSwapQueued)
		{
			if (activated)
			{
				if (selectedBridge.Count > 0)
				{
					DestroySlimeTiles();
				}
			}
			else
			{
				if (selectedBridge.Count > 0)
				{
					ActivateSlimeTiles();
				}
			}
			bridgeSwapQueued = false;
		}
		else
        {
            if (colliding && selectedBridge.Count == 0)
            {
                //print("generating");
                GenerateSlimeTiles();
            } else if (!colliding && selectedBridge.Count > 0)
            {
                ////print("leaving");
                if (!activated)
                {
                    //print("leaving/destroying");
                    DestroySlimeTiles();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        colliding = true;
        queuedCollisionPoint = collision.contacts[0].point;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        colliding = false;
    }

    private void GenerateSlimeTiles()
    {
		print("generate");
        HashSet<Vector3> connectedTilePositions = GetConnectedTilePositions(queuedCollisionPoint);
        //print(connectedTilePositions.Count);
        foreach (Vector3 pos in connectedTilePositions)
        {
            GameObject newSlimeObject = Instantiate(slimeObjectPrefab);
            newSlimeObject.transform.position = pos;
            selectedBridge.Add(newSlimeObject.GetComponent<SlimeObject>()); 
        }

        print(connectedTilePositions.Count);
    }

    private void DestroySlimeTiles()
	{
		print("destroy");
		foreach (SlimeObject slimeObject in selectedBridge)
        {
            Destroy(slimeObject.gameObject);
        }
        selectedBridge.Clear();
        activated = false;
        sr.enabled = true;
    }

    private void ActivateSlimeTiles()
	{
		print("activate");
		foreach (SlimeObject slimeObject in selectedBridge)
        {
            slimeObject.activated = true;
        }
        activated = true;
        sr.enabled = false;
    }

    private HashSet<Vector3> GetConnectedTilePositions(Vector2 worldPosition)
    {
        HashSet<Vector3> connectedTilePositions = new HashSet<Vector3>();

        Vector3Int contactCellPosition = bridgeActivatorTiles.GetComponent<Tilemap>().WorldToCell(worldPosition);

        for (int yDist = -1; yDist <= 1; yDist++)
        {
            for (int xDist = -1; xDist <= 1; xDist++)
            {
                Vector3Int centerCellPosition = contactCellPosition + new Vector3Int(0, yDist, 0);
                if (bridgeActivatorTiles.HasTile(centerCellPosition))
                {
                    connectedTilePositions.Add(bridgeActivatorTiles.GetCellCenterWorld(centerCellPosition));
                }

                for (int xDistLeft = 1; true; xDistLeft++)
                {
                    Vector3Int cellPosition = centerCellPosition + new Vector3Int(-1 * xDistLeft, 0, 0);
                    if (!bridgeActivatorTiles.HasTile(cellPosition)) {
                        break;
                    }

                    connectedTilePositions.Add(bridgeActivatorTiles.GetCellCenterWorld(cellPosition));
                }

                for (int xDistRight = 1; true; xDistRight++)
                {
                    Vector3Int cellPosition = centerCellPosition + new Vector3Int(xDistRight, 0, 0);
                    if (!bridgeActivatorTiles.HasTile(cellPosition)) {
                        break;
                    }

                    connectedTilePositions.Add(bridgeActivatorTiles.GetCellCenterWorld(cellPosition));
                }
            }
        }

        return connectedTilePositions;
    }
}
