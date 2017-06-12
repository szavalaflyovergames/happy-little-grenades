﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HLG_UI : MonoBehaviour 
{
	public GameObject introLogo;
	public GameObject endGameScreen;

	public TextMeshProUGUI scoreLabel;
	public TextMeshProUGUI endGameScoreLabel;

	public GameObject awkwardMeter;
	public Image awkwardMeterFill;

	float fillTarget;
	float prevFill = 0.0f;

	public float lerpDelay = .25f;
	float lerpTimer;

	public GameObject playerWordBubble;
	public TextMeshProUGUI playerText;

	public GameObject followerWordBubble;
	public TextMeshProUGUI followerText;

	public TextMeshProUGUI highScoreText;
	public TextMeshProUGUI endGameHighScoreText;

	public TextMeshProUGUI multiplierValueText;

	public static HLG_UI instance;

	void Awake()
	{
		instance = this;

		awkwardMeterFill.fillAmount = 0.0f;
	}


	// Use this for initialization
	void Start () 
	{
		playerWordBubble.SetActive (false);	
		followerWordBubble.SetActive (false);	

		endGameScreen.SetActive (false);

		scoreLabel.gameObject.SetActive (false);

		awkwardMeter.SetActive (false);

		highScoreText.text = PlayerPrefs.GetInt ("HighScore", 0).ToString();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (lerpTimer > 0.0f) 
		{
			lerpTimer -= Time.deltaTime;

			awkwardMeterFill.fillAmount = Mathf.Lerp (fillTarget, prevFill, lerpTimer / lerpDelay);
		}
	}

	public void HideIntro()
	{
		introLogo.SetActive (false);
	}

	public void ShowEndGame(int points)
	{
		endGameScreen.SetActive (true);
		endGameScoreLabel.text = points.ToString ();

		scoreLabel.gameObject.SetActive (false);
		awkwardMeter.SetActive (false);

		if (points > PlayerPrefs.GetInt ("HighScore", 0))
		{	
			PlayerPrefs.SetInt ("HighScore", points);
		}

		endGameHighScoreText.text = PlayerPrefs.GetInt ("HighScore", 0).ToString ();
	}

	public void SetAwkwardMeterFill(float fill)
	{
		prevFill = awkwardMeterFill.fillAmount;			
		fillTarget = fill;
		lerpTimer = lerpDelay;
	}

	public void UpdateAwkwardMultiLabel(float multi)
	{
		multiplierValueText.text =	string.Format ("{0:N1}", multi) + "X";

		awkwardMeter.SetActive (true);
	}

	//It's not good to put the audio here, but the clock is ticking...
	public void ShowPlayerTextBubble(string text)
	{
		playerWordBubble.SetActive (true);
		playerText.text = text;

		HLG_GameManager.instance.player.GetComponent<AudioSource> ().UnPause ();
	}

	public void ShowFollowerTextBubble(string text)
	{
		followerWordBubble.SetActive (true);
		followerText.text = text;

		HLG_GameManager.instance.followerVoice.GetComponent<AudioSource> ().UnPause ();
	}

	public void HidePlayerTextBubble()
	{
		playerWordBubble.SetActive (false);

		HLG_GameManager.instance.player.GetComponent<AudioSource> ().Pause ();
	}

	public void HideFollowerTextBubble()
	{
		followerWordBubble.SetActive (false);

		HLG_GameManager.instance.followerVoice.GetComponent<AudioSource> ().Pause ();
	}

	public void UpdateScore(int points)
	{
		scoreLabel.gameObject.SetActive (true);

		scoreLabel.text = points.ToString ();
	}
}
