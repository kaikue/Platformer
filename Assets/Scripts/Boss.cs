using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Boss : MonoBehaviour
{

	public GameObject Phase1Tiles;
	public GameObject Phase12Tiles;
	public GameObject BridgeLocations;
	public GameObject[] starsRequiredPrefabs; //required types for each phase
	public GameObject[] StarSpotSets;
	public GameObject leftHand;
	public GameObject rightHand;
	public Tile bridgeTile;
	public int phase = 0;

	private const int STAR_GROUP_SIZE = 3;

	private GameManager gm;
	private List<StarSpot> currentStarSpots;
	private List<StarSpot> queuedStarSpots;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		ActivateStarSpots();
	}

	private void Update()
	{
		bool allFilled = true;
		foreach (StarSpot starSpot in currentStarSpots)
		{
			if (!starSpot.filled && starSpot.gameObject.activeSelf)
			{
				allFilled = false;
			}
		}

		if (allFilled)
		{
			if (StarsRemaining() == 0)
			{
				NextPhase();
			}
			else
			{
				ActivateStarGroup();
			}
		}
		else
		{
			UpdateHUD();
		}
	}

	private void NextPhase()
	{
		switch (phase)
		{
			case 0:
				EndPhase0();
				break;
			case 1:
				EndPhase1();
				break;
			default:
				EndPhase2();
				break;
		}
	}

	private void EndPhase0()
	{
		phase++;
		ActivateStarSpots();
		Phase1Tiles.SetActive(false);
		BridgeLocations.SetActive(true);
	}

	private void EndPhase1()
	{
		phase++;
		ActivateStarSpots();
		//TODO: enable projectiles?
		Phase12Tiles.SetActive(false);
		FillSlimeBridges2();
	}

	private void EndPhase2()
	{
		//TODO: end game
		print("You win!");

		gm.hudOverlay.gameObject.SetActive(false);
	}

	private void ActivateStarSpots()
	{
		currentStarSpots = new List<StarSpot>();
		Transform t = StarSpotSets[phase].transform;
		foreach (Transform tc in t)
		{
			currentStarSpots.Add(tc.gameObject.GetComponent<StarSpot>());
		}
		queuedStarSpots = new List<StarSpot>(currentStarSpots);

		ActivateStarGroup();
	}

	private void ActivateStarGroup()
	{
		int numRemaining = StarsRemaining();
		int starsToPlace = Mathf.Min(numRemaining, STAR_GROUP_SIZE);
		for (int i = 0; i < starsToPlace; i++)
		{
			//activate random inactive, unfilled star
			int r = Random.Range(0, queuedStarSpots.Count);
			StarSpot spot = queuedStarSpots[r];
			spot.gameObject.SetActive(true);
			queuedStarSpots.Remove(spot);
			numRemaining--;
		}
	}

	private Star GetCurrentStar()
	{
		return starsRequiredPrefabs[phase].GetComponent<Star>();
	}

	private int StarsRemaining()
	{
		Star currentStar = GetCurrentStar();
		int numRemaining = gm.starsCollected[(int)currentStar.starType];
		return numRemaining;
	}

	private Color CurrentColor()
	{
		Star currentStar = GetCurrentStar();
		return currentStar.GetColor();
	}

	private void UpdateHUD()
	{
		gm.hudOverlay.gameObject.SetActive(false);
		/*Vector2 pos = gm.hudOverlay.contents[0].GetComponent<RectTransform>().anchoredPosition;
		pos.x = -300;
		gm.hudOverlay.contents[0].GetComponent<RectTransform>().anchoredPosition = pos;
		Color[] starColors = { CurrentColor() };
		int[] starCounts = { StarsRemaining() };
		gm.hudOverlay.SetStars(starColors, starCounts);*/
	}

	private void FillSlimeBridges2()
	{
		int[] xs = { -17, -16, -15, 14, 15, 16 };
		foreach (int x in xs)
		{
			Vector3Int pos = new Vector3Int(x, 2, 0);
			BridgeLocations.GetComponent<Tilemap>().SetTile(pos, bridgeTile);
		}
	}
}
