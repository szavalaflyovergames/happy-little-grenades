using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script contains the core behavior for the game
public class HLG_GameManager : MonoBehaviour 
{
	public HLG_Player player;

	public GameObject npcPrefab;	//Prefab to instantiate for the NPCs

	public static HLG_GameManager instance;	//Use this static instance to interface with the game manager in all the other classes

	int points;

	HLG_Dialog dialog;

	public float lineDelay = 5.0f;
	float lineTimer;

	public HLG_NPC[] testNPCs;

	//List<HLG_NPC> npcs;

	State currentState;
	public enum State
	{
		PREGAME, INTRO, PLAYING, ENDGAME
	}

	void Awake()
	{
		instance = this;
	
		dialog = GetComponentInChildren<HLG_Dialog> ();
	}

	// Use this for initialization
	void Start () 
	{
		AssignIntroInputs ();

		points = 0;
	}

	void StartIntro(MonoBehaviour comp)
	{
		dialog.SelectIntro ();

		HLG_UI.instance.ShowPlayerTextBubble (dialog.GetCurrentIntroLine ());

		lineTimer = lineDelay;

		currentState = State.INTRO;
	}

	void StartGame()
	{
		InputControl.ClearControlMappings ();

		player.StartGame ();

		dialog.SelectFollowerConversation ();

		HLG_UI.instance.HideFollowerTextBubble ();
		HLG_UI.instance.HidePlayerTextBubble ();

		Invoke ("StartFollowerConversation", 2.0f);

		currentState = State.PLAYING;
	}

	void StartFollowerConversation()
	{
		SelectFollower ();

		dialog.currentLine = 0;

		HLG_UI.instance.ShowFollowerTextBubble (dialog.GetCurrentFollowerLine ());

		lineTimer = lineDelay;
	}

	void SelectFollower()
	{
		int index = Random.Range (0, testNPCs.Length);

		testNPCs [index].SetToFollower ();
	}

	// Update is called once per frame
	void Update () 
	{
		InputControl.Update ();	

		switch (currentState) 
		{
		case State.PREGAME:
			break;
		case State.INTRO:
			if (lineTimer > 0.0f) 
			{
				lineTimer -= Time.deltaTime;
				if (lineTimer <= 0.0f) {
					dialog.currentLine++;

					if (dialog.IntroDone ())
						StartGame ();
					else 
					{
						lineTimer = lineDelay;
						HLG_UI.instance.ShowPlayerTextBubble (dialog.GetCurrentIntroLine ());
					}
				}
			}
			break;
		case State.PLAYING:
			if (lineTimer > 0.0f) {
				lineTimer -= Time.deltaTime;
				if (lineTimer <= 0.0f) {
					dialog.currentLine++;

					if (dialog.ConversationDone ()) 
					{
						//TODO: figure out what to do here.
						HLG_UI.instance.HideFollowerTextBubble ();
					} else {
						lineTimer = lineDelay;

						if (dialog.currentLine % 2 == 0) {
							HLG_UI.instance.ShowFollowerTextBubble (dialog.GetCurrentFollowerLine ());
							HLG_UI.instance.HidePlayerTextBubble ();
						} else {
							HLG_UI.instance.ShowPlayerTextBubble (dialog.GetCurrentFollowerLine ());
							HLG_UI.instance.HideFollowerTextBubble ();
						}
					}
				}
			}
			break;
		case State.ENDGAME:
			break;
		}
	}

	//TODO: multiplier?
	public void ScorePoints(int p)
	{
		points += p;
	}

	void AssignIntroInputs()
	{
		InputControl.AddInputMapping (0, this);

		InputControl.AddInputHandler (InputControl.eDeviceType.Keyboard, "space", StartIntro, null, null, 0);

		for (int i = 1; i < 4; ++i) 
		{
			InputControl.AddInputMapping (i, this);
			InputControl.AddInputHandler (InputControl.eDeviceType.Gamepad, InputControl.Button.A, StartIntro, null, null, i);
		}
	}
}
