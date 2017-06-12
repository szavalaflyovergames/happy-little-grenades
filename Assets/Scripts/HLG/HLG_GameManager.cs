using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//This script contains the core behavior for the game
public class HLG_GameManager : MonoBehaviour 
{
	public HLG_Player player;

	public GameObject npcPrefab;	//Prefab to instantiate for the NPCs

	public float maxAwkwardMulti = 4.0f;
	float awkwardLevel = 1.0f;

	int currentAwkwardStep;
	int numberOfSteps;		//Number of awkward level steps

	public static HLG_GameManager instance;	//Use this static instance to interface with the game manager in all the other classes

	int points;

	int objectsExploded;

	HLG_Dialog dialog;

	public float lineDelay = 5.0f;
	float lineTimer;

	public HLG_NPC[] testNPCs;

	float scoreTimer;
	public float scoreDelay = 5.0f;

	public AudioSource followerVoice;
	AudioSource crowdAudio;
	public AudioSource crowdPanicAudio;

	//List<HLG_NPC> npcs;

	HLG_NPC follower;

	State currentState;
	public enum State
	{
		PREGAME, INTRO, PLAYING, GRENADE, ENDGAME
	}

	void Awake()
	{
		instance = this;
	
		dialog = GetComponentInChildren<HLG_Dialog> ();

		crowdAudio = GetComponent<AudioSource> ();
	}

	// Use this for initialization
	void Start () 
	{
		InputControl.ClearControlMappings ();
		AssignIntroInputs ();

		points = 0;
	
		followerVoice.Pause ();
		player.GetComponent<AudioSource> ().Pause ();

	}

	void StartIntro(MonoBehaviour comp)
	{
		InputControl.ClearControlMappings ();

		dialog.SelectIntro ();

		HLG_UI.instance.HideIntro ();
		HLG_UI.instance.ShowPlayerTextBubble (dialog.GetCurrentIntroLine ());
		HLG_UI.instance.UpdateAwkwardMultiLabel (1.0f);
		HLG_UI.instance.SetAwkwardMeterFill (0.0f);

		lineTimer = lineDelay;

		currentState = State.INTRO;

		crowdAudio.PlayOneShot (HLG_SoundManager.instance.menuSound);

//		iTween.ShakeScale (HLG_UI.instance.awkwardMeter, iTween.Hash ("amount", new Vector3(.10f, .10f, 0.0f),
//			"islocal", true, "time", .25f, "looptype", iTween.LoopType.pingPong));

	}

	void RestartGame(MonoBehaviour comp)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene (0);

		crowdAudio.PlayOneShot (HLG_SoundManager.instance.menuSound, .5f);
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

	void EndGame()
	{
		currentState = State.ENDGAME;

		HLG_UI.instance.ShowEndGame (points);

		InputControl.ClearControlMappings ();

		AssignEndInputs ();
	}

	void StartFollowerConversation()
	{
		SelectFollower ();

		dialog.currentLine = 0;

		HLG_UI.instance.ShowFollowerTextBubble (dialog.GetCurrentFollowerLine ());

		lineTimer = lineDelay;
	
		numberOfSteps = dialog.followerConversations [dialog.currentConversation].lines.Length / 2;
		currentAwkwardStep = 1;

		UpdateAwkwardLevel ();

		player.GrenadesEnabled = true;
	}

	void SelectFollower()
	{
		int index = Random.Range (0, testNPCs.Length);

		follower = testNPCs [index];

		follower.SetToFollower ();
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
			if (lineTimer > 0.0f) 
			{
				lineTimer -= Time.deltaTime;
				if (lineTimer <= 0.0f) {
					dialog.currentLine++;

					if (dialog.ConversationDone ()) 
					{
						//TODO: figure out what to do here.
						HLG_UI.instance.HideFollowerTextBubble ();
					} 
					else 
					{
						lineTimer = lineDelay;

						if (dialog.currentLine % 2 == 0) 
						{
							HLG_UI.instance.ShowFollowerTextBubble (dialog.GetCurrentFollowerLine ());
							HLG_UI.instance.HidePlayerTextBubble ();

							//If it's the last line, end the awkward
							if (dialog.currentLine == dialog.followerConversations [dialog.currentConversation].lines.Length - 1) 
							{
								follower.StopFollowing ();

								awkwardLevel = 1.0f;

								HLG_UI.instance.SetAwkwardMeterFill (0.0f);
								HLG_UI.instance.UpdateAwkwardMultiLabel (awkwardLevel);

								DOTween.Kill (HLG_UI.instance.awkwardMeter.transform);
							}
							else 
							{
								currentAwkwardStep++;

								UpdateAwkwardLevel ();
							}						
						}
						else 
						{
							HLG_UI.instance.ShowPlayerTextBubble (dialog.GetCurrentFollowerLine ());
							HLG_UI.instance.HideFollowerTextBubble ();
						}
					}
				}
			}

			break;
		case State.GRENADE:
			if (scoreTimer > 0.0f) 
			{
				scoreTimer -= Time.deltaTime;

				if (scoreTimer <= 0.0f)
					EndGame ();
			}
			break;
		case State.ENDGAME:
			break;
		}
	}

	void UpdateAwkwardLevel()
	{
		float lerp = (float)currentAwkwardStep / (float)numberOfSteps;

		awkwardLevel = Mathf.Lerp (1.0f, maxAwkwardMulti, lerp);

		HLG_UI.instance.SetAwkwardMeterFill (lerp);

		HLG_UI.instance.UpdateAwkwardMultiLabel (awkwardLevel);

		if (lerp >= .99f) 
		{
			TweenParams tParms = new TweenParams ().SetLoops (-1).SetEase (Ease.OutElastic);

			HLG_UI.instance.awkwardMeter.transform.DOShakePosition (.2f, 10.0f, 50, 180).SetAs (tParms);
		}
		else 
		{
			DOTween.Kill (HLG_UI.instance.awkwardMeter.transform);
		}
	}

	//TODO: multiplier?
	public void ScorePoints(int p)
	{
		objectsExploded++;

		points += (int)((float)p * awkwardLevel);

		HLG_UI.instance.UpdateScore (points);

		scoreTimer = scoreDelay;
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

	void AssignEndInputs()
	{
		InputControl.AddInputMapping (0, this);

		InputControl.AddInputHandler (InputControl.eDeviceType.Keyboard, "space", RestartGame, null, null, 0);

		for (int i = 1; i < 4; ++i) 
		{
			InputControl.AddInputMapping (i, this);
			InputControl.AddInputHandler (InputControl.eDeviceType.Gamepad, InputControl.Button.A, RestartGame, null, null, i);
		}
	}

	public void GrenadeDropped()
	{
		HLG_UI.instance.HideFollowerTextBubble ();
		HLG_UI.instance.ShowPlayerTextBubble (player.GetEmergencyGrenadeLine());

		currentState = State.GRENADE;
	}

	public void GrenadeExploded()
	{
		HLG_UI.instance.HidePlayerTextBubble ();

		scoreTimer = scoreDelay;

		crowdAudio.Stop ();
		crowdPanicAudio.Play ();

		for (int i = 0; i < testNPCs.Length; ++i) 
		{
			testNPCs [i].StartRunning ();
		}
	}
}
