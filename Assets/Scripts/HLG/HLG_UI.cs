using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HLG_UI : MonoBehaviour 
{
	public GameObject introLogo;

	public TextMeshProUGUI scoreLabel;

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

	public static HLG_UI instance;

	void Awake()
	{
		instance = this;
	}


	// Use this for initialization
	void Start () 
	{
		playerWordBubble.SetActive (false);	
		followerWordBubble.SetActive (false);	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (lerpTimer > 0.0f) 
		{
			lerpDelay -= Time.deltaTime;

			awkwardMeterFill.fillAmount = Mathf.Lerp (fillTarget, prevFill, lerpTimer / lerpDelay);
		}
	}

	public void SetAwkwardMeterFill(float fill)
	{
		prevFill = awkwardMeterFill.fillAmount;			
		fillTarget = fill;
		lerpTimer = lerpDelay;
		
	}

	public void ShowPlayerTextBubble(string text)
	{
		playerWordBubble.SetActive (true);
		playerText.text = text;
	}

	public void ShowFollowerTextBubble(string text)
	{
		followerWordBubble.SetActive (true);
		followerText.text = text;
	}

	public void HidePlayerTextBubble()
	{
		playerWordBubble.SetActive (false);
	}

	public void HideFollowerTextBubble()
	{
		followerWordBubble.SetActive (false);
	}
}
