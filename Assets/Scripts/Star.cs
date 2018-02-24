using UnityEngine;

public class Star : MonoBehaviour {
	
	public Color collectedColor;
	public GameObject starImage;
	public GameObject glow;
	public string starText;

	private const float STAR_DISTANCE = 0.1f;
	private const float STAR_SPEED = 3.0f;

	private bool wasCollected = false;
	private Color color;

	private void Start()
	{
		color = starImage.GetComponent<SpriteRenderer>().color;
		
		ParticleSystem ps = gameObject.GetComponentInChildren<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		main.startColor = color;
		//main.startColor = new Color(color.r, color.g, color.b, ;

		//TODO: set wasCollected (read from file)

		if (wasCollected)
		{
			color = collectedColor;
			starImage.GetComponent<SpriteRenderer>().color = color;
			ps.gameObject.SetActive(false); //maybe? or just set color
		}

		glow.GetComponent<SpriteRenderer>().material.color = color;
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
