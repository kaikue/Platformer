using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public GameObject pausePrefab;
	public GameObject starCollectOverlay;
	public GameObject levelStarPrefab;

	public bool paused = false;
	private bool overlayActive = false;

	private const string SAVE_PATH = ".save";

	private GameObject player;
	private GameObject pauseOverlay;
	private HUDOverlay hudOverlay;
	private Star levelStar;

	private int[] starsCollected;
	private List<string> starCollectedNames;
	
	private void Awake()
	{
		hudOverlay = gameObject.GetComponentInChildren<HUDOverlay>();
		player = GameObject.Find("Player");
		LoadSave();
	}
	
	private void LoadSave()
	{
		int numTypes = Enum.GetValues(typeof(Star.StarType)).Length;
		starsCollected = new int[numTypes];
		starCollectedNames = new List<string>();

		//create file (temporary here, move to newgame later)
		if (!File.Exists(SAVE_PATH))
		{
			using (StreamWriter sw = File.CreateText(SAVE_PATH))
			{
				sw.WriteLine("0");
			}
		}
		
		string[] lines = File.ReadAllLines(SAVE_PATH);
		for (int l = 1; l < lines.Length; l++) //skip first line (level index)
		{
			string line = lines[l];
			string[] split = line.Split('|');
			starCollectedNames.Add(split[0]);
			int type = int.Parse(split[1]);
			int num = int.Parse(split[2]);
			starsCollected[type] += num;
		}

		lines[0] = "" + SceneManager.GetActiveScene().buildIndex;
		File.WriteAllLines(SAVE_PATH, lines);
	}

	private void AppendToSave(string line)
	{
		File.AppendAllText(SAVE_PATH, line + Environment.NewLine);
	}

	private void Start()
	{
		SetHUDLevelStar();
		hudOverlay.Hold();
	}

	private void SetHUDLevelStar()
	{
		levelStar = levelStarPrefab.GetComponent<Star>();
		Color[] starColors = new Color[] { levelStar.GetColor() };
		int[] starCounts = new int[] { starsCollected[(int)levelStar.starType] };
		hudOverlay.SetStars(starColors, starCounts);
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

	public bool WasStarCollected(Star star)
	{
		return starCollectedNames.Contains(star.starText);
	}

	public void CollectStar(Star star)
	{
		if (!star.WasCollected())
		{
			AppendToSave(star.starText + "|" + ((int)star.starType) + "|" + star.starValue);
			starCollectedNames.Add(star.starText);
			TogglePause();
			overlayActive = true;
			GameObject o = Instantiate(starCollectOverlay);
			o.GetComponent<StarCollectOverlay>().SetStarName(star.starText);
			starsCollected[(int)star.starType] += star.starValue;
			SetHUDLevelStar();
			hudOverlay.SlideIn();
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
