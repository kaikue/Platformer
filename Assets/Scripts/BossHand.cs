using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHand : MonoBehaviour {

	public enum AttackState
	{
		IDLE,
		POUND,
		SWEEP,
		DESTROY
	}

	public int SideMultiplier;
	public AttackState state;

	private const float WIDTH = 6.0f;

	private const float POUND_TOP = 5.0f;
	private const float POUND_BOTTOM = -3.0f;

	public const float IDLE_TIME = 5.0f;
	public const float COLLIDER_REACTIVATE_TIME = 1.0f;

	//times are cumulative
	private const float POUND_PHASE1_TIME = 1.0f;
	private const float POUND_PHASE2_TIME = 2.0f;
	public const float POUND_PHASE3_TIME = 2.5f;
	public const float POUND_PHASE4_TIME = 3.5f;

	private const float SWEEP_PHASE1_TIME = 1.0f;
	private const float SWEEP_PHASE2_TIME = 2.5f;
	private const float SWEEP_PHASE3_TIME = 4.0f;
	public const float SWEEP_PHASE4_TIME = 5.0f;

	private float time;
	private Boss boss;

	private void Start()
	{
		boss = GameObject.Find("Boss").GetComponent<Boss>();
		Idle();
	}
	
	public void Idle()
	{
		time = 0;
		state = AttackState.IDLE;
	}

	public void PoundAttack()
	{
		time = 0;
		state = AttackState.POUND;
	}

	public void SweepAttack()
	{
		time = 0;
		state = AttackState.SWEEP;
	}

	public void DestroyAttack()
	{
		time = 0;
		state = AttackState.DESTROY;
	}

	/*private bool PhaseComplete()
	{
		switch (state)
		{
			case AttackState.IDLE:
				return false;
			case AttackState.POUND:
				return time >= POUND_PHASE4_TIME;
			case AttackState.SWEEP:
				return time >= SWEEP_PHASE4_TIME;
			case AttackState.DESTROY:
				return time >= POUND_PHASE4_TIME;
			default:
				return true;
		}
	}*/

	private Vector2 GetPosition()
	{
		switch(state)
		{
			case AttackState.IDLE:
				return GetIdlePosition();
			case AttackState.POUND:
				return GetPoundPosition();
			case AttackState.SWEEP:
				return GetSweepPosition();
			case AttackState.DESTROY:
				return GetDestroyPosition();
			default:
				return GetIdlePosition();
		}
	}
	
	private Vector2 GetIdlePosition()
	{
		float y = Mathf.Sin(Time.time * 2f);
		return new Vector2(SideMultiplier * WIDTH, y);
	}

	private Vector2 GetPoundPosition()
	{
		if (time < POUND_PHASE1_TIME)
		{
			//first phase: lift
			float y = Mathf.Lerp(0, POUND_TOP, time / POUND_PHASE1_TIME);
			return new Vector2(SideMultiplier * WIDTH, y);
		}
		else if (time < POUND_PHASE2_TIME)
		{
			//second phase: hold at top
			return new Vector2(SideMultiplier * WIDTH, POUND_TOP);
		}
		else if (time < POUND_PHASE3_TIME)
		{
			//third phase: slam
			float t = (time - POUND_PHASE2_TIME) / (POUND_PHASE3_TIME - POUND_PHASE2_TIME);
			float y = Mathf.Lerp(POUND_TOP, POUND_BOTTOM, t);
			return new Vector2(SideMultiplier * WIDTH, y);
		}
		else
		{
			//fourth phase: hold at bottom
			return new Vector2(SideMultiplier * WIDTH, POUND_BOTTOM);
		}
	}

	private Vector2 GetSweepPosition()
	{
		//first phase: spline between start pos, point under player, wall
		//second phase: hold
		//third phase: lerp between wall and mid at same height
		//fourth phase: hold
		float y = Mathf.Sin(Time.time * 2f);
		return new Vector2(SideMultiplier * 6, y);
	}

	private Vector2 GetDestroyPosition()
	{
		return GetPoundPosition();
	}
	
	private void FixedUpdate()
	{
		if (state == AttackState.IDLE)
		{
			transform.position = Vector2.Lerp(transform.position, GetPosition(), 2 * Time.fixedDeltaTime);
		}
		else
		{
			transform.position = GetPosition();
		}

		/*Vector2 oldPos = transform.position;
		Vector2 newPos = GetPosition();
		if ((newPos - oldPos).magnitude < 0.3f)
		{
			transform.position = newPos;
		}
		else
		{
			transform.position = Vector2.Lerp(transform.position, GetPosition(), 0.1f);
		}*/
		//transform.position = Vector2.Lerp(transform.position, GetPosition(), 1);// Time.fixedDeltaTime);

		if (state == AttackState.POUND && time > POUND_PHASE2_TIME) //also for destroy?
		{
			SetAllColliders(false);
		}

		time += Time.fixedDeltaTime;

		/*if (PhaseComplete())
		{
			if (state == AttackState.DESTROY)
			{
				boss.NextPhase(); //but only once
			}
			else if (state == AttackState.POUND)
			{
				//pound, but only once
			}
			Idle();
		}*/
	}

	private void SetAllColliders(bool enabled)
	{
		foreach (BoxCollider2D bc in gameObject.GetComponents<BoxCollider2D>()) {
			bc.enabled = enabled;
		}
	}

	public void ReactivateColliders()
	{
		StartCoroutine(ReactivateCollidersDelay());
	}

	private IEnumerator ReactivateCollidersDelay()
	{
		yield return new WaitForSeconds(COLLIDER_REACTIVATE_TIME);
		SetAllColliders(true);
	}
}
