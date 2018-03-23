using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TeamLogo : MonoBehaviour {

	bool started;

	// Use this for initialization
	void Start () {
		GetComponent<VideoPlayer> ().Play ();
	}
	
	// Update is called once per frame
	void Update () {
		if (GetComponent<VideoPlayer>().isPlaying) {
			started = true;
		} else if (started) {
			SceneManager.LoadScene("Title");
		}
		
	}
}
