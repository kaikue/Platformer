using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHand : MonoBehaviour {

	/*public enum Attack
	{
		IDLE,
		POUND,
		SWEEP
	}*/

	public int SideMultiplier;
	public bool idle;

	private void Start()
	{
		idle = true;
	}
	
	private void FixedUpdate()
	{
		if (idle)
		{
			Vector2 pos = transform.position;
			pos.y += Mathf.Sin(Time.time * 2f) * Time.fixedDeltaTime;
			transform.position = pos;
		}
	}
}
