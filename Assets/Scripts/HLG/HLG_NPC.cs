using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the NPC characters for Happy Little Grenades
public class HLG_NPC : MonoBehaviour 
{
	HLG_Exploder exploder;

	public int pointValue = 100;

	public float wanderSpeed = 3.0f;
	public float followSpeed = 7.0f;

	public float followDistance = 1.0f;

	Rigidbody myRigidbody;

	public enum State
	{
		WANDER, FOLLOW, EXPLODED
	}

	State currentState = State.WANDER;

	void Awake()
	{
		exploder = GetComponentInChildren<HLG_Exploder> ();

		exploder.callback += Explode;

		myRigidbody = GetComponent<Rigidbody> ();
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
		Vector3 vecTo = transform.position - HLG_GameManager.instance.player.transform.position;

		Vector3 lerpPoint = HLG_GameManager.instance.player.transform.position + vecTo.normalized * followDistance;

		//I hate this kind of lerp, but it's late
		vecTo = lerpPoint - transform.position;

		if (vecTo.magnitude > .1f)
			myRigidbody.velocity = vecTo.normalized * followSpeed;
		else
			myRigidbody.velocity = Vector3.zero;
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
