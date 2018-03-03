using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    public PlayableCharacter child;
    //public PlayableCharacter slime;

	void Start () {
        child.enabled = true;
        //child.GetComponent<Camera>().enabled = true;
        //slime.enabled = false;
        //slime.GetComponent<Camera>().enabled = false;
	}
	
	void Update () {
	     	
	}

}
