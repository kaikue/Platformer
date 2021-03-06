﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHand : MonoBehaviour {

	public enum AttackState
	{
		IDLE,
		POUND,
		SWEEP
	}

	public int SideMultiplier;
	public AttackState state;

	private const float WIDTH = 6.0f;

	private const float POUND_TOP = 5.0f;
	private const float POUND_BOTTOM = -3.0f;

	private const float SWEEP_UNDER_HEIGHT = -10.0f;
	private const float SWEEP_WALL_X = 18.5f;

	public const float IDLE_TIME = 5.0f;
	public const float COLLIDER_REACTIVATE_TIME = 1.0f;

	//times are cumulative
	public const float POUND_PHASE1_TIME = 1.0f;
	public const float POUND_PHASE2_TIME = 2.0f;
	public const float POUND_PHASE3_TIME = 2.5f;
	public const float POUND_PHASE4_TIME = 3.5f;

	public const float SWEEP_PHASE1_TIME = 1.0f;
	public const float SWEEP_PHASE2_TIME = 2.5f;
	public const float SWEEP_PHASE3_TIME = 4.0f;
	public const float SWEEP_PHASE4_TIME = 5.0f;

	private float time;
	private GameObject player;
	private Vector2 splineStart;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
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
		splineStart = transform.position;
	}
	
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
		if (time < SWEEP_PHASE1_TIME)
		{
			//first phase: spline between start pos, point under player, wall
			Vector2 start = splineStart;
			Vector2 mid = new Vector2(player.transform.position.x, player.transform.position.y + SWEEP_UNDER_HEIGHT);
			Vector2 end = new Vector2(SideMultiplier * SWEEP_WALL_X, player.transform.position.y);
			float t = time / SWEEP_PHASE1_TIME;

			return Vector2.Lerp(Vector2.Lerp(start, mid, t), Vector2.Lerp(mid, end, t), t);
		}
		else if (time < SWEEP_PHASE2_TIME)
		{
			//second phase: hold
			return new Vector2(SideMultiplier * SWEEP_WALL_X, transform.position.y);
		}
		else if (time < SWEEP_PHASE3_TIME)
		{
			//third phase: lerp between wall and mid at same height
			float t = (time - SWEEP_PHASE2_TIME) / (SWEEP_PHASE3_TIME - SWEEP_PHASE2_TIME);
			float x = Mathf.Lerp(SideMultiplier * SWEEP_WALL_X, SideMultiplier * WIDTH, t);
			return new Vector2(x, transform.position.y);
		}
		else
		{
			//fourth phase: hold
			return new Vector2(SideMultiplier * WIDTH, transform.position.y);
		}
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
			//transform.position = GetPosition();
			transform.position = Vector2.Lerp(transform.position, GetPosition(), 10 * Time.fixedDeltaTime);
		}
		
		if ((state == AttackState.POUND && time > POUND_PHASE2_TIME) ||
			(state == AttackState.SWEEP && time < SWEEP_PHASE1_TIME))
		{
			SetAllColliders(false);
		}

		if (state == AttackState.SWEEP && time >= SWEEP_PHASE1_TIME)
		{
			SetAllColliders(true);
		}

		time += Time.fixedDeltaTime;
	}

	public void SetAllColliders(bool enabled)
	{
		foreach (BoxCollider2D bc in gameObject.GetComponents<BoxCollider2D>())
		{
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
