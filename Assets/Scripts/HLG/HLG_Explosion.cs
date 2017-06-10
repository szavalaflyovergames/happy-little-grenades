using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLG_Explosion : MonoBehaviour 
{
	public GameObject effectPrefab;

	public Vector3 maxScale;
	Vector3 startScale;

	float lifeTime;

	GameObject effectInstance;

	TimedDestruction timedD;

	void Awake()
	{
		timedD = GetComponentInChildren<TimedDestruction> ();
	
		lifeTime = timedD.delayTime;
		startScale = transform.localScale;
	}

	// Use this for initialization
	void Start () 
	{
		effectInstance = GameObject.Instantiate (effectPrefab, transform.position, Quaternion.identity);	
	}

	void Update()
	{
		Vector3 lerpVec = Vector3.Lerp (maxScale, startScale, timedD.delayTime / lifeTime);

		transform.localScale = lerpVec;
	}

}
