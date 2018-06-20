using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBridgeAnimation : MonoBehaviour {

	public const int NUM_FRAMES = 12;

	private const float FRAME_TIME = 0.02f; //time in seconds per frame of animation
	
	private SpriteRenderer sr;
	private SlimeManager sm;
	public Sprite[] sprites;
	public GameObject parent;
	
	private void Start()
	{
		sr = gameObject.GetComponent<SpriteRenderer>();
		sm = GameObject.Find("SlimeManager").GetComponent<SlimeManager>();
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
			if (frame == NUM_FRAMES / 2)
			{
				if (sm.CanActivateBridge())
				{
					parent.GetComponent<BoxCollider2D>().enabled = true;
				}
				else
				{
					sm.DestroyBridge();
				}
			}
			yield return new WaitForSeconds(FRAME_TIME);
		}
	}
}
