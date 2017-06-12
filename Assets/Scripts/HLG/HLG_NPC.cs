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
	public float runSpeed = 10.0f;

	public float followDistance = 1.0f;

	Rigidbody myRigidbody;

	Vector3 movementDirection;

	SpriteRenderer mySprite;

	public Sprite explodedSprite;

	public enum State
	{
		WANDER, FOLLOW, EXPLODED, RUN
	}

	State currentState = State.WANDER;

	void Awake()
	{
		exploder = GetComponentInChildren<HLG_Exploder> ();

		exploder.callback += Explode;

		myRigidbody = GetComponent<Rigidbody> ();

		float randomX = Random.Range (-1.0f, 1.0f);
		float randomZ = Random.Range (-1.0f, 1.0f);

		movementDirection = new Vector3 (randomX, 0.0f, randomZ).normalized;

		myRigidbody.velocity = movementDirection * wanderSpeed;

		mySprite = GetComponentInChildren<SpriteRenderer> ();
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
		case State.RUN:
			WanderUpdate ();
			break;
		}
	

	}

	void WanderUpdate()
	{
		mySprite.flipX = myRigidbody.velocity.x < 0.0f;
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

		if(Mathf.Abs(vecTo.x) >= .05)
			mySprite.flipX = vecTo.x < 0.0f;
	}

	void Explode(GameObject trigger)
	{
		HLG_GameManager.instance.ScorePoints (pointValue);
	
		//TODO: death animation? Fly up into the air?
		myRigidbody.velocity = Vector3.zero;

		Vector3 vecTo = trigger.transform.position - transform.position;

		vecTo.y = 0.0f;
		vecTo.Normalize ();

		Vector3 force = vecTo * 500.0f;

		force += Camera.main.transform.up * 500.0f;

		myRigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionX;

		myRigidbody.AddForce (force);

		float torque = Random.Range (-100.0f, 100.0f);

		myRigidbody.AddTorque (new Vector3 (0.0f, 0.0f, torque));

		currentState = State.EXPLODED;

		mySprite.sprite = explodedSprite;
	}

	public void StartRunning()
	{
		if (currentState == State.FOLLOW) 
		{
			Vector3 vecTo = transform.position - HLG_GameManager.instance.player.transform.position;

			movementDirection = vecTo.normalized;
			myRigidbody.velocity = movementDirection * runSpeed;

			currentState = State.RUN;
		}
		else if (currentState != State.EXPLODED) 
		{
			myRigidbody.velocity = movementDirection * runSpeed;
			currentState = State.RUN;
		}
	}


	public void SetToFollower()
	{
		currentState = State.FOLLOW;
	}

	public void StopFollowing()
	{
		if (currentState != State.EXPLODED) 
		{
			currentState = State.WANDER;

			Vector3 vecTo = transform.position - HLG_GameManager.instance.player.transform.position;

			movementDirection = vecTo.normalized;
			myRigidbody.velocity = movementDirection * wanderSpeed;
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("Wall")) 
		{
			Vector3 reflect = Vector3.Reflect (movementDirection, col.gameObject.transform.forward).normalized;

//			reflect.x += Random.Range (-.1f, .1f);
//			reflect.z += Random.Range (-.1f, .1f);

			reflect.y = 0.0f;
			reflect.Normalize ();

			movementDirection = reflect;

			if (currentState == State.WANDER)
				myRigidbody.velocity = movementDirection * wanderSpeed;
			else if (currentState == State.RUN)
				myRigidbody.velocity = movementDirection * runSpeed;
		}
	}
}
