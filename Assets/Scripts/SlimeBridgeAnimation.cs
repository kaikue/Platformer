using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBridgeAnimation : MonoBehaviour {

	public const int NUM_FRAMES = 12;

	private const float FRAME_TIME = 0.05f; //time in seconds per frame of animation
	
	private SpriteRenderer sr;
	public Sprite[] sprites;
	public GameObject parent;
	
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
			if (frame == NUM_FRAMES - 1)
			{
				parent.GetComponent<BoxCollider2D>().enabled = true;
			}
			yield return new WaitForSeconds(FRAME_TIME);
		}
	}
}
