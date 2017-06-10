using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the NPC characters for Happy Little Grenades
public class HLG_NPC : MonoBehaviour 
{
	HLG_Exploder exploder;

	public int pointValue = 100;

	public float wanderSpeed = 3.0f;

	public enum State
	{
		WANDER, FOLLOW, EXPLODED
	}

	State currentState = State.WANDER;

	void Awake()
	{
		exploder = GetComponentInChildren<HLG_Exploder> ();

		exploder.callback += Explode;
	}


	void Update()
	{
		switch (currentState) 
		{
		case State.WANDER:
			WanderUpdate ();
			break;
		case State.FOLLOW:
			FollowUpdate ();
			break;
		case State.EXPLODED:
			break;
		}
	}

	void WanderUpdate()
	{
	}

	void FollowUpdate()
	{
	}

	void Explode()
	{
		HLG_GameManager.instance.ScorePoints (pointValue);
	
		//TODO: death animation? Fly up into the air?
	}

	public void SetToFollower()
	{
		currentState = State.FOLLOW;
	}
}
