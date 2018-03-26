using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour {

	public GameObject Phase1Tiles;
	public GameObject BridgeLocations;
	public GameObject[] starsRequiredPrefabs; //required types for each phase
	public GameObject[] StarSpotSets;
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
	}
	
	private void NextPhase()
	{
		switch(phase)
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
	}

	private void EndPhase2()
	{
		//TODO: end game
		print("You win!");
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
		/*StarSpotSets[phase].SetActive(true);
		Star currentStar = starsRequiredPrefabs[phase].GetComponent<Star>();
		int numCollected = gm.starsCollected[(int)currentStar.starType];
		StarSpot[] starSpots = StarSpotSets[phase].GetComponentsInChildren<StarSpot>();
		int numStarSpots = starSpots.Length;
		int numToRemove = numStarSpots - numCollected;
		for (int i = 0; i < numToRemove; i++)
		{
			//pick an active starspot and deactivate its gameobject
			GameObject starObj = null;
			while (starObj == null || starObj.activeSelf)
			{
				int r = Random.Range(0, numStarSpots);
				starObj = starSpots[r].gameObject;
			}
			starObj.SetActive(false);
		}*/

		int numRemaining = StarsRemaining();
		int starsToPlace = Mathf.Min(numRemaining, STAR_GROUP_SIZE);
		print("Placing " + starsToPlace);
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

	private int StarsRemaining()
	{
		Star currentStar = starsRequiredPrefabs[phase].GetComponent<Star>();
		int numRemaining = gm.starsCollected[(int)currentStar.starType];
		return numRemaining;
	}
}
