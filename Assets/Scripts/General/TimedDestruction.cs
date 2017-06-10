using UnityEngine;
using System.Collections;

public class TimedDestruction : MonoBehaviour 
{
	public float delayTime;
	public float timeScale = 1.0f;
	
	// Update is called once per frame
	void Update () 
	{
		delayTime -= Time.deltaTime * timeScale;
		
		if(delayTime <= 0.0f)
			GameObject.Destroy(gameObject);
	}
}
