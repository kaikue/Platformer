using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour {

	public KeyCode continueKey;
	
	void Update ()
	{
		if (Input.GetKey (continueKey))
		{
			SceneManager.LoadScene("Hub");
		}
	}
}
