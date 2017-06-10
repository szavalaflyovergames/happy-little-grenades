using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLG_Dialog : MonoBehaviour 
{
	public HLG_Conversation[] introConversations;
	public HLG_Conversation[] followerConversations; 	

	public int currentLine;
	public int currentConversation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public string GetCurrentIntroLine()
	{
		return introConversations [currentConversation].lines [currentLine];
	}

	public string GetCurrentFollowerLine()
	{
		return followerConversations [currentConversation].lines [currentLine];
	}

	public void SelectFollowerConversation()
	{
		currentConversation = Random.Range (0, followerConversations.Length);
		currentLine = 0;
	}

	public void SelectIntro()
	{
		currentConversation = Random.Range (0, introConversations.Length);
	}

	public bool IntroDone()
	{
		return currentLine >= introConversations [currentConversation].lines.Length;
	}

	public bool ConversationDone()
	{
		return currentLine >= followerConversations [currentConversation].lines.Length;
	}
}

[System.Serializable]
public class HLG_Conversation
{
	[Multiline]
	public string[] lines; //For ease of prototyping, even lines are follower, odd are player (except for intros)
}
