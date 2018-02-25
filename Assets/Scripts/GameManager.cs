using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject pausePrefab;
	public GameObject starCollectOverlay;

	public bool paused = false;
	private bool overlayActive = false;

	private GameObject player;
	private GameObject pauseOverlay;

	private int[] starsCollected;

	private void Start()
	{
		player = GameObject.Find("Player");
		LoadStars();
	}

	private void LoadStars()
	{
		int numTypes = Enum.GetValues(typeof(Star.StarType)).Length;
		starsCollected = new int[numTypes];
		//TODO: load from file
	}
	
	private void TogglePause()
	{
		paused = !paused;
		if (paused)
		{
			Time.timeScale = 0;
		}
		else
		{
			Time.timeScale = 1;
		}
		//player.GetComponent<AudioListener>().enabled = !paused;
	}

	public void TogglePauseMenu()
	{
		if (overlayActive) return;

		TogglePause();
		if (paused)
		{
			pauseOverlay = Instantiate(pausePrefab);
		}
		else
		{
			Destroy(pauseOverlay);
		}
	}

	public void CollectStar(Star star)
	{
		if (!star.WasCollected())
		{
			//TODO: save to file
			TogglePause();
			overlayActive = true;
			GameObject o = Instantiate(starCollectOverlay);
			o.GetComponent<StarCollectOverlay>().SetStarName(star.starText);
			starsCollected[(int)star.starType]++;
		}
		else
		{
			//TODO: play sound
		}
	}

	public void FinishOverlay()
	{
		TogglePause();
		overlayActive = false;
	}
}
