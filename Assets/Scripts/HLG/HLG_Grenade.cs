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
		explosionTimer -= Time.deltaTime;

		if (explosionTimer <= 0.0f) 
		{
			Explode ();
		}
	}

	public void Drop(HLG_Player o)
	{
		owner = o;
	
		explosionTimer = explosionDelay;
	}

	//TODO: explode
	void Explode()
	{
		Debug.Log ("Boom!");

		GameObject.Instantiate (explosionPrefab, transform.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f)); 

		owner.GrenadeExploded ();

		GameObject.Destroy (gameObject);	

		AudioSource.PlayClipAtPoint (HLG_SoundManager.instance.explosionMedium, transform.position);
	}
}
