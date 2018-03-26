using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour {

	public GameObject Phase1Tiles;
	public GameObject BridgeLocations;
	public GameObject[] starsRequiredPrefabs; //required types for each phase
	public GameObject[] StarSpotSets;
	public int phase = 0;

	private GameManager gm;
	
	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		ActivateStarSpots();
    }

	private void Update()
	{
		bool allFilled = true;
		StarSpot[] currentStarSpots = StarSpotSets[phase].GetComponentsInChildren<StarSpot>();
        foreach (StarSpot starSpot in currentStarSpots)
		{
			if (!starSpot.filled)
			{
				allFilled = false;
			}
		}

		if (allFilled)
		{
			NextPhase();
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
		ActivateStarSpots();
        Phase1Tiles.SetActive(false);
		BridgeLocations.SetActive(true);
		phase++;
	}

	private void EndPhase1()
	{
		ActivateStarSpots();
		//TODO: enable projectiles?
		phase++;
	}

	private void EndPhase2()
	{
		//TODO: end game
	}

	private void ActivateStarSpots()
	{
		StarSpotSets[phase].SetActive(true);
		Star currentStar = starsRequiredPrefabs[phase].GetComponent<Star>();
		int numCollected = gm.starsCollected[(int)currentStar.starType];
		StarSpot[] starSpots = StarSpotSets[phase].GetComponentsInChildren<StarSpot>();
		int numStarSpots = starSpots.Length;
		int numToRemove = numStarSpots - numCollected;
		for (int i = 0; i < numToRemove; i++)
		{
			//TODO: pick an active starspot and deactivate its gameobject
		}
	}
}
