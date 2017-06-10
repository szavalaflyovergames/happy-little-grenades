using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the player character
public class HLG_Player : MonoBehaviour 
{
	public float walkingSpeed = 5.0f;
	public float runningSpeed = 10.0f;

	public GameObject grenadePrefab;

	Rigidbody myRigidbody;

	Vector2 movementInputVector;

	State currentState = State.WALKING;

	//Idle = before gameplay starts
	//Walking = after gameplay starts
	//Grenade = dropping grenade

	//Might not use the grenade state after all.
	public enum State
	{
		IDLE, WALKING, GRENADE, RUNNING
	}

	const string HORIZONTAL_AXIS = "Horizontal";
	const string HORIZONTAL_DPAD = "HorizontalDPad";
	const string VERTICAL_AXIS = "Vertical";
	const string VERTICAL_DPAD = "VerticalDPad";

	void Awake()
	{
		myRigidbody = GetComponentInChildren<Rigidbody> ();
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (currentState != State.IDLE) 
		{
			ReceiveInput ();

			//Sloppy, but it's a game jam so lets rock!!!! (SZ)
			if (currentState == State.WALKING)
				movementInputVector *= walkingSpeed;
			else if (currentState == State.RUNNING)
				movementInputVector *= runningSpeed;
			else
				movementInputVector = Vector2.zero;
		
//			Vector3 pos = transform.position;
//
//			pos.x += movementInputVector.x * Time.deltaTime;
//			pos.z += movementInputVector.y * Time.deltaTime;
//
//			transform.position = pos;

			myRigidbody.velocity = new Vector3 (movementInputVector.x, 0.0f, movementInputVector.y);
		}
	}

	void ReceiveInput()
	{
		float vertAxis;
		float horAxis;

		vertAxis = Input.GetAxis (VERTICAL_AXIS + "Keyboard");
		horAxis = Input.GetAxis (HORIZONTAL_AXIS + "Keyboard");

		if (vertAxis == 0.0f && horAxis == 0.0f) 
		{
			for (int i = 1; i < 4; ++i) 
			{
				vertAxis = InputControl.GetJoystickAxis(i, VERTICAL_AXIS);

				if (vertAxis == 0.0f)
					vertAxis = InputControl.GetJoystickAxis (i, VERTICAL_DPAD); 

				if (horAxis == 0.0f)
					horAxis = InputControl.GetJoystickAxis (i, HORIZONTAL_DPAD);
				
				horAxis = InputControl.GetJoystickAxis(i, HORIZONTAL_AXIS);
			
				if (horAxis != 0.0f || vertAxis != 0.0f)
					return;
			}
		}

		movementInputVector = new Vector2 (horAxis, vertAxis).normalized;
	
	}

	public void StartGame()
	{
		AssignInputs ();
	}

	void AssignInputs()
	{
		InputControl.AddInputMapping (0, this);

		InputControl.AddInputHandler (InputControl.eDeviceType.Keyboard, "space", DropGrenade, null, null, 0);

		for (int i = 1; i < 4; ++i) 
		{
			InputControl.AddInputMapping (i, this);
			InputControl.AddInputHandler (InputControl.eDeviceType.Gamepad, InputControl.Button.A, DropGrenade, null, null, i);
		}
	}

	void DropGrenade(MonoBehaviour comp)
	{
		if (currentState != State.WALKING)
			return;

		Debug.Log ("Dropping 'Nades!");

		GameObject grenade = GameObject.Instantiate (grenadePrefab, transform.position, Quaternion.identity);

		grenade.GetComponent<HLG_Grenade> ().Drop (this);

		currentState = State.RUNNING;
	}

	public void GrenadeExploded()
	{
		
	}
}
