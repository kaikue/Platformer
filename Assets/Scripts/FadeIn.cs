using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour {

	public GameObject fade;

	private const float FADE_TIME = 1.5f;

	private Image fadeImage;

	private void Start()
	{
		fadeImage = fade.GetComponent<Image>();
		StartCoroutine(Fade());
	}
	
	private IEnumerator Fade()
	{
		yield return new WaitForSeconds(0.5f);
		Color transparent = new Color(1, 1, 1, 0);
		Color white = new Color(1, 1, 1, 1);
		//AudioSource music = gm.GetComponent<AudioSource>();
		//float baseVolume = music.volume;
		float fadeTime = 0;
		while (fadeTime < FADE_TIME)
		{
			float t = fadeTime / FADE_TIME;
			Color c = Color.Lerp(white, transparent, t);
			fadeImage.color = c;

			//float volume = Mathf.Lerp(baseVolume, 0, t);
			//music.volume = volume;

			fadeTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		fade.SetActive(false);
	}
}
