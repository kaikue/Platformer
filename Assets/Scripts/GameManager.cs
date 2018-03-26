using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public GameObject pausePrefab;
	public GameObject starCollectOverlayPrefab;
	public GameObject hudOverlayPrefab;
	public GameObject levelStarPrefab;
	public GameObject loadingOverlay;
	public HUDOverlay hudOverlay;

	public bool paused = false;
	private bool overlayActive = false;

	public const string SAVE_PATH = ".save";

	private GameObject player;
	private GameObject pauseOverlay;
	private Star levelStar;

	public int[] starsCollected;
	private List<string> starCollectedNames;
	private List<string> doorsOpenedNames;

	private void Awake()
	{
		GameObject hudObject = Instantiate(hudOverlayPrefab);
		hudOverlay = hudObject.GetComponent<HUDOverlay>();
		player = GameObject.Find("Player");
		LoadSave();
	}
	
	private void LoadSave()
	{
		int numTypes = Enum.GetValues(typeof(Star.StarType)).Length;
		starsCollected = new int[numTypes];
		starCollectedNames = new List<string>();
		doorsOpenedNames = new List<string>();
		
		string[] lines = File.ReadAllLines(SAVE_PATH);
		for (int l = 1; l < lines.Length; l++) //skip first line (level index)
		{
			string line = lines[l];
			string[] split = line.Split('|');
			if (split[0] == "S")
			{
				starCollectedNames.Add(split[1]);
				int type = int.Parse(split[2]);
				int num = int.Parse(split[3]);
				starsCollected[type] += num;
			}
			else if (split[0] == "D")
			{
				doorsOpenedNames.Add(split[1]);
			}
		}

		lines[0] = SceneManager.GetActiveScene().name;
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

	public void ShowHUDDoorStars(Door door)
	{
		GameObject[] doorStars = door.starsRequiredPrefabs;
		Color[] starColors = new Color[doorStars.Length];
		int[] starCounts = new int[doorStars.Length];
		for (int i = 0; i < doorStars.Length; i++)
		{
			Star star = doorStars[i].GetComponent<Star>();
			starColors[i] = star.GetColor();
			starCounts[i] = starsCollected[(int)star.starType];
		}
		
		hudOverlay.SetStars(starColors, starCounts);
		hudOverlay.SlideIn();
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

	public void QuitToTitle()
	{
		TogglePauseMenu();
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(1);
	}

	public bool WasStarCollected(Star star)
	{
		return starCollectedNames.Contains(star.starText);
	}

	public void CollectStar(Star star)
	{
		if (!star.WasCollected())
		{
			AppendToSave("S|" + star.starText + "|" + ((int)star.starType) + "|" + star.starValue);
			starCollectedNames.Add(star.starText);
			TogglePause();
			overlayActive = true;
			GameObject o = Instantiate(starCollectOverlayPrefab);
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

	public void SaveDoor(string doorName)
	{
		doorsOpenedNames.Add(doorName);
		AppendToSave("D|" + doorName);
	}

	public bool WasDoorOpened(string doorName)
	{
		return doorsOpenedNames.Contains(doorName);
	}
}
