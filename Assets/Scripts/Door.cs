using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour {

	public string doorName; //to identify in saving/loading
	public GameObject[] starsRequiredPrefabs; //required types to open door
	public int[] starsRequiredCounts; //required # to open door
	public GameObject requirementsBase;
	public GameObject[] requirements; //star image and # text on door
	public Color openColor; //text & image will turn this color
	public float openOffsetX; //position to open to
	public float openOffsetY;

	private const float START_DELAY = 0.5f;
	private const float MID_DELAY = 0.5f;
	private const float SLIDE_TIME = 2.0f;
	private const float REQ_HEIGHT = 0.7f;

	private GameManager gm;
	private Vector3 closedPos;
	private Vector3 openPos;
	private bool open = false;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();

		for (int i = 0; i < starsRequiredCounts.Length; i++)
		{
			requirements[i].GetComponentInChildren<SpriteRenderer>().color = starsRequiredPrefabs[i].GetComponent<Star>().GetColor();
			requirements[i].GetComponentInChildren<Text>().text = "" + starsRequiredCounts[i];
			//rotate to look normal
			requirements[i].GetComponentInChildren<SpriteRenderer>().gameObject.transform.rotation = Quaternion.identity;
			requirements[i].GetComponentInChildren<Text>().gameObject.transform.rotation = Quaternion.identity;
		}
		for (int i = starsRequiredCounts.Length; i < requirements.Length; i++)
		{
			requirements[i].SetActive(false);
		}

		int numMissing = requirements.Length - starsRequiredCounts.Length;
		requirementsBase.transform.localPosition += Vector3.down * numMissing * REQ_HEIGHT / 2;

		closedPos = gameObject.transform.position;
		openPos = new Vector3(closedPos.x + openOffsetX, closedPos.y + openOffsetY, closedPos.z);

		//destroy if was previously opened
		if (gm.WasDoorOpened(doorName))
		{
			Destroy(gameObject);
		}
	}
	
	private bool CanOpen()
	{
		if (open)
		{
			return false;
		}

		int[] starsCollected = gm.starsCollected;
		for (int i = 0; i < starsRequiredCounts.Length; i++)
		{
			int starType = (int)starsRequiredPrefabs[i].GetComponent<Star>().starType;
			if (starsRequiredCounts[i] > starsCollected[starType])
			{
				return false;
			}
		}
		return true;
	}

	public void TryOpen()
	{
		if (CanOpen())
		{
			open = true;
			StartCoroutine(LightUp());
		}
	}

	private IEnumerator LightUp()
	{
		yield return new WaitForSeconds(START_DELAY);
		for (int i = 0; i < starsRequiredCounts.Length; i++)
		{
			//TODO: play ascending tone
			requirements[i].GetComponentInChildren<SpriteRenderer>().color = openColor;
			requirements[i].GetComponentInChildren<Text>().color = openColor;
			yield return new WaitForSeconds(MID_DELAY);
		}

		StartCoroutine(SlideOpen());
	}

	private IEnumerator SlideOpen()
	{
		for (float t = 0; t < SLIDE_TIME; t += Time.deltaTime)
		{
			gameObject.transform.position = Vector3.Lerp(closedPos, openPos, t / SLIDE_TIME);
			yield return new WaitForEndOfFrame();
		}

		gm.SaveDoor(doorName);
	}
}
