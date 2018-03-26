using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour {

	public GameObject ContinueButton;
	public GameObject loadingOverlay;

	public string FirstLevelName = "Hub";

	private void Start()
	{
		if (!File.Exists(GameManager.SAVE_PATH))
		{
			ContinueButton.SetActive(false);
		}
	}

	public void Continue()
	{
		string[] lines = File.ReadAllLines(GameManager.SAVE_PATH);
		string levelName = lines[0];
		loadingOverlay.SetActive(true);
		SceneManager.LoadScene(levelName);
	}

	public void NewGame()
	{
		if (File.Exists(GameManager.SAVE_PATH))
		{
			//delete old save
			File.Delete(GameManager.SAVE_PATH);
		}

		using (StreamWriter sw = File.CreateText(GameManager.SAVE_PATH))
		{
			sw.WriteLine(FirstLevelName);
		}

		loadingOverlay.SetActive(true);
		//load intro
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
