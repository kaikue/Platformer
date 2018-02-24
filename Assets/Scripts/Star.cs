using UnityEngine;

public class Star : MonoBehaviour {

	public Color color;
	public GameObject starImage;

	private const float STAR_DISTANCE = 0.1f;
	private const float STAR_SPEED = 3.0f;

	void Start()
	{
		gameObject.GetComponentInChildren<SpriteRenderer>().color = color;
		ParticleSystem.MainModule main = gameObject.GetComponentInChildren<ParticleSystem>().main;
		main.startColor = color;
	}

	void Update()
	{
		float y = (Mathf.Cos(Time.time * STAR_SPEED) - 0.5f) * STAR_DISTANCE;
		starImage.transform.position = transform.position + new Vector3(0, y, 0);
	}
}
