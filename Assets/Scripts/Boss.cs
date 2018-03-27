using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Boss : MonoBehaviour
{
	public GameObject face;
	public Sprite normalFace;
	public Sprite angryFace;
	public GameObject Phase1Tiles;
	public GameObject Phase12Tiles;
	public GameObject BridgeLocations;
	public GameObject[] starsRequiredPrefabs; //required types for each phase
	public GameObject[] StarSpotSets;
	public GameObject leftHand;
	public GameObject rightHand;
	public Tile bridgeTile;
	public GameObject fade;
	public int phase = 0;

	private const int STAR_GROUP_SIZE = 3;

	private const float SCREEN_SHAKE_TIME = 0.8f;
	private const float SCREEN_SHAKE_AMOUNT = 0.35f;
	
	private const float POUND_CHANCE = 0.5f;

	private const float FADE_TIME = 3.0f;

	private GameManager gm;
	private SpriteRenderer sr;
	private GameObject player;
	private List<StarSpot> currentStarSpots;
	private List<StarSpot> queuedStarSpots;
	private BossHand left;
	private BossHand right;
	private bool destroying = false;
	private bool won = false;

	private void Start()
	{
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		sr = face.GetComponent<SpriteRenderer>();
		player = GameObject.FindGameObjectWithTag("Player");
		left = leftHand.GetComponent<BossHand>();
		right = rightHand.GetComponent<BossHand>();
		ActivateStarSpots();
		StartCoroutine(WaitAttack());
	}

	private void Update()
	{
		bool allFilled = true;
		foreach (StarSpot starSpot in currentStarSpots)
		{
			if (!starSpot.filled && starSpot.gameObject.activeSelf)
			{
				allFilled = false;
			}
		}

		if (allFilled)
		{
			if (StarsRemaining() == 0)
			{
				if (phase == 2)
				{
					Win();
				}
				else if (!destroying)
				{
					destroying = true;
					StopAllCoroutines();
					StartCoroutine(DestroyAttack());
				}
			}
			else
			{
				ActivateStarGroup();
			}
		}
		else
		{
			UpdateHUD();
		}
	}

	private void NextPhase()
	{
		switch (phase)
		{
			case 0:
				EndPhase0();
				break;
			case 1:
				EndPhase1();
				break;
			default:
				break;
		}
	}

	private void EndPhase0()
	{
		phase++;
		ActivateStarSpots();
		Phase1Tiles.SetActive(false);
		BridgeLocations.SetActive(true);
	}

	private void EndPhase1()
	{
		phase++;
		ActivateStarSpots();
		//TODO: enable projectiles?
		Phase12Tiles.SetActive(false);
		FillSlimeBridges2();
	}

	private void Win()
	{
		if (won) return;

		won = true;
		gm.hudOverlay.gameObject.SetActive(false);
		StartCoroutine(FadeOut());
	}

	private void ActivateStarSpots()
	{
		currentStarSpots = new List<StarSpot>();
		Transform t = StarSpotSets[phase].transform;
		foreach (Transform tc in t)
		{
			currentStarSpots.Add(tc.gameObject.GetComponent<StarSpot>());
		}
		queuedStarSpots = new List<StarSpot>(currentStarSpots);

		ActivateStarGroup();
	}

	private void ActivateStarGroup()
	{
		int numRemaining = StarsRemaining();
		int starsToPlace = Mathf.Min(numRemaining, STAR_GROUP_SIZE);
		for (int i = 0; i < starsToPlace; i++)
		{
			//activate random inactive, unfilled star
			int r = Random.Range(0, queuedStarSpots.Count);
			StarSpot spot = queuedStarSpots[r];
			spot.gameObject.SetActive(true);
			queuedStarSpots.Remove(spot);
			numRemaining--;
		}
	}

	private Star GetCurrentStar()
	{
		return starsRequiredPrefabs[phase].GetComponent<Star>();
	}

	private int StarsRemaining()
	{
		Star currentStar = GetCurrentStar();
		int numRemaining = gm.starsCollected[(int)currentStar.starType];
		return numRemaining;
	}

	private Color CurrentColor()
	{
		Star currentStar = GetCurrentStar();
		return currentStar.GetColor();
	}

	private void UpdateHUD()
	{
		gm.hudOverlay.gameObject.SetActive(false);
		/*Vector2 pos = gm.hudOverlay.contents[0].GetComponent<RectTransform>().anchoredPosition;
		pos.x = -300;
		gm.hudOverlay.contents[0].GetComponent<RectTransform>().anchoredPosition = pos;
		Color[] starColors = { CurrentColor() };
		int[] starCounts = { StarsRemaining() };
		gm.hudOverlay.SetStars(starColors, starCounts);*/
	}

	private void FillSlimeBridges2()
	{
		int[] xs = { -17, -16, -15, 14, 15, 16 };
		foreach (int x in xs)
		{
			Vector3Int pos = new Vector3Int(x, 2, 0);
			BridgeLocations.GetComponent<Tilemap>().SetTile(pos, bridgeTile);
		}
	}

	private void Attack()
	{
		float rand = Random.Range(0f, 1f);
		if (rand < POUND_CHANCE)
		{
			StartCoroutine(PoundAttack());
		}
		else
		{
			StartCoroutine(SweepAttack());
		}
	}

	private IEnumerator PoundAttack()
	{
		left.PoundAttack();
		right.PoundAttack();

		yield return new WaitForSeconds(BossHand.POUND_PHASE2_TIME);
		sr.sprite = angryFace;
		yield return new WaitForSeconds(BossHand.POUND_PHASE3_TIME - BossHand.POUND_PHASE2_TIME);
		PoundSlam();
		yield return new WaitForSeconds(BossHand.POUND_PHASE4_TIME - BossHand.POUND_PHASE3_TIME);

		left.ReactivateColliders();
		right.ReactivateColliders();
		StartCoroutine(WaitAttack());
	}

	private void PoundSlam()
	{
		StartCoroutine(ScreenShake(true));
		player.GetComponent<PlayerDemo>().Shove();
		//TODO: play sound
		print("WHAM");
	}

	private IEnumerator SweepAttack()
	{
		float playerX = player.transform.position.x;
		if (playerX < 0)
		{
			left.SweepAttack();
		}
		else
		{
			right.SweepAttack();
		}

		yield return new WaitForSeconds(BossHand.SWEEP_PHASE2_TIME);
		sr.sprite = angryFace;
		yield return new WaitForSeconds(BossHand.SWEEP_PHASE4_TIME - BossHand.SWEEP_PHASE2_TIME);

		StartCoroutine(WaitAttack());
	}

	private IEnumerator WaitAttack()
	{
		sr.sprite = normalFace;
		left.Idle();
		right.Idle();
		yield return new WaitForSeconds(BossHand.IDLE_TIME);

		Attack();
	}

	private IEnumerator DestroyAttack()
	{
		left.StopAllCoroutines();
		right.StopAllCoroutines();
		left.SetAllColliders(true);
		right.SetAllColliders(true);
		
		left.PoundAttack();
		right.PoundAttack();

		yield return new WaitForSeconds(BossHand.POUND_PHASE2_TIME);
		sr.sprite = angryFace;
		yield return new WaitForSeconds(BossHand.POUND_PHASE3_TIME - BossHand.POUND_PHASE2_TIME);
		DestroySlam();
		yield return new WaitForSeconds(BossHand.POUND_PHASE4_TIME - BossHand.POUND_PHASE3_TIME);

		left.ReactivateColliders();
		right.ReactivateColliders();
		StartCoroutine(WaitAttack());
		destroying = false;
	}

	private void DestroySlam()
	{
		PoundSlam();
		NextPhase();
	}

	private IEnumerator ScreenShake(bool end)
	{
		Vector3 basePos = Camera.main.transform.localPosition;
		float shakeTime = 0;
		while (shakeTime < SCREEN_SHAKE_TIME || !end)
		{
			Vector2 pos2d = (Vector2)basePos + Random.insideUnitCircle * SCREEN_SHAKE_AMOUNT;
			Camera.main.transform.localPosition = new Vector3(pos2d.x, pos2d.y, basePos.z);

			shakeTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		Camera.main.transform.localPosition = basePos;
	}

	private IEnumerator FadeOut()
	{
		StartCoroutine(ScreenShake(false));
		SpriteRenderer fadeSprite = fade.GetComponent<SpriteRenderer>();
		Color transparent = new Color(1, 1, 1, 0);
		Color white = new Color(1, 1, 1, 1);
		float t = 0;
		while (t < FADE_TIME)
		{
			Color c = Color.Lerp(transparent, white, t / FADE_TIME);
			fadeSprite.color = c;
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(FADE_TIME / 2);
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(sceneIndex + 1);
	}
}
