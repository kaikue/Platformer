using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBridgeAnimation : MonoBehaviour {

	public const int NUM_FRAMES = 12;

	private const float FRAME_TIME = 0.05f; //time in seconds per frame of animation
	
	private SpriteRenderer sr;
	public Sprite[] sprites;
	
	private void Start()
	{
		sr = gameObject.GetComponent<SpriteRenderer>();
		StartAnimation();
	}
	
	private void StartAnimation()
	{
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		for (int frame = 0; frame < NUM_FRAMES; frame++)
		{
			sr.sprite = sprites[frame];
			yield return new WaitForSeconds(FRAME_TIME);
		}
	}
}
