using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//It's happy and it's little!
public class HLG_Grenade : MonoBehaviour 
{
	public float explosionDelay = 2.0f;

	float explosionTimer;

	public GameObject explosionPrefab;

	HLG_Player owner;

	void Update()
	{
//		explosionTimer -= Time.deltaTime;
//
//		if (explosionTimer <= 0.0f) 
//		{
//			Explode ();
//		}
	}

	public void Drop(HLG_Player o)
	{
		owner = o;
	
		explosionTimer = explosionDelay;
	
		Rigidbody myRigidbody = GetComponent<Rigidbody> ();

		myRigidbody.AddForce (Vector3.up * 700.0f);

		float torque = Random.Range (-100.0f, 100.0f);

		myRigidbody.AddTorque (new Vector3 (0.0f, 0.0f, torque));
	}

	//TODO: explode
	void Explode()
	{
		Debug.Log ("Boom!");

		GameObject.Instantiate (explosionPrefab, transform.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f)); 

		owner.GrenadeExploded ();
		HLG_GameManager.instance.GrenadeExploded ();

		GameObject.Destroy (gameObject);	

		AudioSource.PlayClipAtPoint (HLG_SoundManager.instance.explosionMedium, transform.position);
	}

	void OnTriggerEnter(Collider floor)
	{
		if (floor.gameObject.layer == LayerMask.NameToLayer ("Floor")) 
		{
			Explode ();
		}
	}
}
