using UnityEngine;

public class Star : MonoBehaviour {
	
	public enum StarType
	{
		YELLOW = 0,
		GREEN = 1,
		BLUE = 2,
		RED = 3,
		ORANGE = 4
	}

	public Color collectedColor;
	public GameObject starImage;
	public GameObject glow;
	public string starText;
	public StarType starType;
	public int starValue;

	public bool playerTouched = false;

	private const float STAR_DISTANCE = 0.1f;
	private const float STAR_SPEED = 3.0f;

	private bool wasCollected = false;
	private Color color;
	
	private void Start()
	{
		color = GetColor();

		ParticleSystem ps = gameObject.GetComponentInChildren<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		main.startColor = color;
		
		wasCollected = GameObject.Find("GameManager").GetComponent<GameManager>().WasStarCollected(this);

		if (wasCollected)
		{
			color = collectedColor;
			starImage.GetComponent<SpriteRenderer>().color = color;
			ps.gameObject.SetActive(false);
			gameObject.GetComponent<AudioSource>().enabled = false;
		}

		glow.GetComponent<SpriteRenderer>().material.color = color;
	}

	public Color GetColor()
	{
		return starImage.GetComponent<SpriteRenderer>().color;
	}

	private void Update()
	{
		float y = (Mathf.Sin(Time.time * STAR_SPEED) - 0.5f) * STAR_DISTANCE;
		starImage.transform.position = transform.position + new Vector3(0, y, 0);
	}

	public bool WasCollected()
	{
		return wasCollected;
	}
}
