using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarCollectOverlay : MonoBehaviour {

	public GameObject canvas;
	public GameObject contents;
	public GameObject left;
	public GameObject right;
	public GameObject starName;

	private const float SLIDE_TIME = 4.0f;
	private const float SPEED_SCALE = 60;
	private const float SPEED_COMPRESSION_IN = 3.5f;
	private const float SPEED_COMPRESSION_OUT = 5;
	private const float TEXT_MOVE_TIME = 0.3f;
	private const float TIME_IN = SLIDE_TIME * TEXT_MOVE_TIME;
	private const float TIME_OUT = SLIDE_TIME * (1 - TEXT_MOVE_TIME);
	private const float CENTER_OFFSET = 200;

	private float time = 0;
	private RectTransform leftRect;
	private RectTransform rightRect;

	private void Start()
	{
		float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
		leftRect = left.GetComponent<RectTransform>();
		rightRect = right.GetComponent<RectTransform>();

		leftRect.anchoredPosition = new Vector2(-CENTER_OFFSET - canvasWidth / 2, leftRect.anchoredPosition.y);
		rightRect.anchoredPosition = new Vector2(CENTER_OFFSET + canvasWidth / 2, rightRect.anchoredPosition.y);

		StartCoroutine(Animate());
	}
	
	private IEnumerator Animate()
	{
		//fade in overlay

		//slide up contents from bottom

		//slide in left and right, then slow, then out
		while (time < SLIDE_TIME)
		{
			float offset = getTextMovement();
			leftRect.anchoredPosition = new Vector2(leftRect.anchoredPosition.x + offset, leftRect.anchoredPosition.y);
			rightRect.anchoredPosition = new Vector2(rightRect.anchoredPosition.x - offset, rightRect.anchoredPosition.y);
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		//slide up contents

		//fade out overlay

		Destroy(gameObject);
	}

	private float getTextMovement()
	{
		float compression = 0;
		float segTime = 0;
		if (time < TIME_IN)
		{
			compression = SPEED_COMPRESSION_IN;
			segTime = TIME_IN;
		}
		else if (time > TIME_OUT)
		{
			compression = SPEED_COMPRESSION_OUT;
			segTime = TIME_OUT;
		}
		else
		{
			return 0;
		}
		//= (time > inTime) ? SPEED_COMPRESSION_OUT : SPEED_COMPRESSION_IN;
		float speed = Mathf.Pow((time - segTime) * compression, 2) * SPEED_SCALE;
		return speed * Time.deltaTime;
	}

	public void SetStarName(string name)
	{
		starName.GetComponent<Text>().text = name;
	}
}
