using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLG_Explosion : MonoBehaviour 
{
	public GameObject effectPrefab;

	GameObject effectInstance;

	// Use this for initialization
	void Start () 
	{
		effectInstance = GameObject.Instantiate (effectPrefab, transform.position, Quaternion.identity);	
	}

}
