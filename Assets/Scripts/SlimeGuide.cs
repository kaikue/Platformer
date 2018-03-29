using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGuide : MonoBehaviour {

    public GameObject portal;
    public GameObject player;
    public Sprite[] sprites;

    public float distance = 2.0f;

    private SpriteRenderer sr;
    private int spriteIndex;
    private int spriteStaticFrames = 5;

    private IEnumerator animCoroutine;

	// Use this for initialization
	void Start () {
		
	}

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        spriteIndex = 0;
        sr.sprite = sprites[spriteIndex];
        animCoroutine = Animate();
        StartCoroutine(animCoroutine);    
    }

    private IEnumerator Animate()
    {
        while (spriteIndex < 5 + spriteStaticFrames)
        {
			sr.sprite = spriteIndex < spriteStaticFrames ? sprites[0] : sprites[spriteIndex - spriteStaticFrames];
			spriteIndex++;
			yield return new WaitForSecondsRealtime(0.16f);
        }
    }

     

    // Update is called once per frame
    void Update () {
        Vector2 playerToPortal = portal.transform.position - player.transform.position;

        float angleForRotation = Vector2.SignedAngle(Vector2.right, playerToPortal) - 90.0f;
        float angleForPosition = Vector2.SignedAngle(Vector2.right, playerToPortal);
        float angleForPositionRad = angleForPosition * Mathf.PI / 180.0f;

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, angleForRotation));
        transform.position = player.transform.position + 
            distance * (new Vector3(Mathf.Cos(angleForPositionRad), Mathf.Sin(angleForPositionRad), 0.0f));

	}
}
