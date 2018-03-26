using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSpot : MonoBehaviour {
	
	public AudioSource StarFillSound;
	public GameObject Image;
	public GameObject Glow;
	public bool touched = false;
	public bool filled = false;

	private const float SHRUNK_SCALE = 0.5f;
	private const float SHRINK_TIME = 1.0f;

	private const float PITCH_VARIATION = 0.2f;

	private GameManager gm;
	private Boss boss;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		boss = GameObject.Find("Boss").GetComponent<Boss>();
	}

	public void Fill()
	{
		touched = true;

		StarFillSound.pitch = Random.Range(1 - PITCH_VARIATION, 1 + PITCH_VARIATION);
		StarFillSound.Play();

		Star currentStar = boss.starsRequiredPrefabs[boss.phase].GetComponent<Star>();

		gm.starsCollected[(int)currentStar.starType]--;

		Color starColor = currentStar.GetColor();
		Image.GetComponent<SpriteRenderer>().color = starColor;
		Glow.GetComponent<SpriteRenderer>().material.color = starColor;
		
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
