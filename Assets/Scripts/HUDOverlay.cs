using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUDOverlay : MonoBehaviour {

	public GameObject canvas;
	public GameObject[] contents;

	private const float SLIDE_TIME = 0.5f;
	private const float HEIGHT_OFFSCREEN = 250;
	private const float HOLD_TIME = 3.0f;

	private RectTransform[] contentRects;
	private float[] contentGoalsY;

	private bool showing = true;

	private void Start()
	{
		contentRects = new RectTransform[contents.Length];
		contentGoalsY = new float[contents.Length];
		for (int i = 0; i < contents.Length; i++)
		{
			contentRects[i] = contents[i].GetComponent<RectTransform>();
			contentGoalsY[i] = contentRects[i].anchoredPosition.y;
		}
	}

	public void SetStars(Color[] starColors, int[] starCounts)
	{
		foreach (GameObject star in contents)
		{
			star.SetActive(false);
		}

		for (int i = 0; i < starCounts.Length; i++)
		{
			contents[i].SetActive(true);
			contents[i].GetComponentInChildren<Image>().color = starColors[i];
			contents[i].GetComponentInChildren<Text>().text = "" + starCounts[i];
		}
	}

	public void Hold()
	{
		showing = true;
		StartCoroutine(HoldContents());
	}

	public IEnumerator HoldContents()
	{
		yield return new WaitForSeconds(HOLD_TIME);
		StartCoroutine(SlideContents(false));
	}

	public void SlideIn()
	{
		if (!showing)
		{
			showing = true;
			StartCoroutine(SlideContents(true));
		}
	}

	private IEnumerator SlideContents(bool isIn)
	{
		for (float t = 0; t < SLIDE_TIME; t += Time.deltaTime)
		{
			for (int i = 0; i < contentRects.Length; i++)
			{
				RectTransform contentRect = contentRects[i];
				float newY = getInY(contentGoalsY[i], t, isIn);
				contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, newY);
			}
			yield return new WaitForEndOfFrame();
		}
		if (isIn)
		{
			Hold();
		}
		else
		{
			showing = false;
		}
	}

	private float getInY(float goalY, float t, bool isIn)
	{
		float tScale = t / SLIDE_TIME;
		if (isIn)
		{
			tScale = 1 - tScale; //reverse the numbers
		}
		float oneMinusTScale = 1 - tScale;
		
		//return Mathf.Pow(timeRemaining, 2) * goalY + HEIGHT_OFFSCREEN; //TODO: quadtratic slide
		return tScale * HEIGHT_OFFSCREEN + oneMinusTScale * goalY;
	}

}
