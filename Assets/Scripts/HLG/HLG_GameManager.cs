using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script contains the core behavior for the game
public class HLG_GameManager : MonoBehaviour 
{

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		InputControl.Update ();	
	}
}
