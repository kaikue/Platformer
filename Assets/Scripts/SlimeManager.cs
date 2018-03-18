using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeManager : MonoBehaviour {

    public Tilemap bridgeActivatorTiles;
    public GameObject slimeObjectPrefab;
    public GameObject player;
    public float followDistance;

    private SpriteRenderer sr;
    private BoxCollider2D bc;

    private readonly HashSet<SlimeObject> selectedBridge = new HashSet<SlimeObject>();

    private Vector3 queuedCollisionPoint;
    private bool colliding;
    private bool activated;
    private bool bridgeSwapQueued;

	void Start () {
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            bridgeSwapQueued = true;
        }
	}

    private void FixedUpdate()
    {
        bc.offset = player.transform.position - transform.position;
        Vector2 radialVelocity = GetRadialVelocity();
        transform.position += Time.fixedDeltaTime * (new Vector3(radialVelocity.x, radialVelocity.y, 0.0f));

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
    }

    private void UpdateBridgeState(bool colliding)
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


    private Vector2 GetRadialVelocity()
    {
        Vector2 parentPos = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);

        Vector2 playerVel = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 playerDirection = player.GetComponent<PlayerDemo>().GetFacing() * Vector2.left;
        Vector2 followPos = parentPos + -1.0f * followDistance * playerDirection;

        Vector2 targetVel = 3.0f * (followPos - currentPos) + 0.5f * playerVel;
        return targetVel;
    }

    private Vector2 GetRotatedPosition(float newAngle, Vector2 parentPos, Vector2 currentPos)
    {
        float dist = Vector2.Distance(parentPos, currentPos);
        return new Vector2(parentPos.x + dist * Mathf.Cos(newAngle), parentPos.y + dist * Mathf.Sin(newAngle));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("collision enter");
        colliding = true;
        queuedCollisionPoint = collision.contacts[0].point;
        if (!bridgeSwapQueued)
        {
            UpdateBridgeState(colliding);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        print("collision exit");
        colliding = false;
        if (!bridgeSwapQueued)
        {
            UpdateBridgeState(colliding);
        }
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
