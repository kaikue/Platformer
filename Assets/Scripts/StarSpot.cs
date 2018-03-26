using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSpot : MonoBehaviour {
	
	public AudioSource StarFillSound;
	public bool filled = false;

	private const float SHRUNK_SCALE = 0.5f;
	private const float SHRINK_TIME = 1.0f;

	private GameManager gm;
	private Boss boss;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		boss = GameObject.Find("Boss").GetComponent<Boss>();
	}

	public void Fill()
	{
		StarFillSound.Play();

		Star currentStar = boss.starsRequiredPrefabs[boss.phase].GetComponent<Star>();

		gm.starsCollected[(int)currentStar.starType]--;

		Color starColor = currentStar.GetColor();
		SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in srs)
		{
			sr.color = starColor;
		}

		StartCoroutine(Shrink());
	}

	private IEnumerator Shrink()
	{
		for (float t = 0; t < SHRINK_TIME; t += Time.deltaTime)
		{
			float scale = transform.localScale.x;
			float newScale = Mathf.Lerp(scale, SHRUNK_SCALE, t / SHRINK_TIME);
			transform.localScale = new Vector3(newScale, newScale, 1);
			yield return new WaitForEndOfFrame();
		}
		filled = true;
	}
}
