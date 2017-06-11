using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a script for an object that will explode
public class HLG_Exploder : MonoBehaviour 
{
	public GameObject explosionPrefab;

	public delegate void ExplosionCallback(GameObject trigger);

	//This will be use by the npc characters
	public ExplosionCallback callback;

	public AudioClip explosionSound;

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("Explosion")) 
		{
			Explode ();
		}
	}

	void Explode()
	{
		if(explosionSound != null)
			AudioSource.PlayClipAtPoint (explosionSound, transform.position);

		//TODO: score points
		if (explosionPrefab != null) 
		{
			GameObject.Instantiate (explosionPrefab, transform.position, Quaternion.identity);
		}

		Component.Destroy (this);

		if (callback != null)
			callback (gameObject);
	}
}
