/************************************************************************************

Filename    :   InputControl.cs
Content     :   Cross-platform wrapper for Unity input.
Created     :   March 8, 2014
Authors     :   Jonathan E. Wright

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.


************************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-------------------------------------------------------------------------------------
// ***** InputControl
//
// This is a cross-platform wrapper for Unity Input. See OVRGamepadController for a 
// list of the base axis and button names.  Depending on joystick number and platform
// the base names will be pre-pended with "Platform:Joy #:" to look them up in the
// Unity Input table.  For instance: using an axis name of "Left_X_Axis" with GetJoystickAxis()
// will result in looking up the axis named "Win: Joy 1: Left_X_Axis" when running on
// Windows and "Android: Joy 1: Left_X_Axis" when running on Android.
//
// In addition to wrapping joystick input, this class allows the assignment of up, held 
// and down events for any key, mouse button, or joystick button via the AddInputHandler()
// method.
//
// Currently this class relies on enumerations defined in OVRGamepadController
// so that it remains compatible with existing Unity OVR projects.  When this class
// is included it overloads the default GPC_GetAxis() and GPC_GetButton() calls to
// to ReadAxis() and ReadButton() in this class.  
// Ideally this class would completely replace the OVRGamepadController class.  This 
// would involve changing the GPC_GetAxis() and GPC_GetButton() calls in a project 
// and removing references to OVRGamepadController in this file (and moving some of 
// the tables to InputControl).
//

public static class InputControl
{
	[SerializeField]
	// FIXME: this was originally settable on the behavior before this was a static class... maybe remove it.
	private	static bool	AllowKeyControls = true;	// true to allow keyboard input (will be set false on some platforms)
	
	public delegate void OnKeyUp( MonoBehaviour comp );
	public delegate void OnKeyDown( MonoBehaviour comp );
	public delegate void OnKeyHeld( MonoBehaviour comp );
	

	// types of devices we can handle input for
	public enum eDeviceType
	{
		None = -1,
		Keyboard = 0,	// a key
		Mouse,		// a mouse button
		Gamepad, 	// a gamepad button
		Axis		// a joystick axis (or trigger)
	};
	
	// mouse button definitions
	public enum eMouseButton
	{
		None = -1,
		Left = 0,
		Right = 2,
		Middle = 3,
		Fourth = 4,
		Fifth = 5
	};

	public enum Axis 
	{ 
		None = -1,
		LeftXAxis = 0, 
		LeftYAxis, 
		RightXAxis, 
		RightYAxis, 
		LeftTrigger, 
		RightTrigger,
		Max 
	};
	
	public enum Button 
	{ 
		None = -1,
		A = 0,
		B, 
		X, 
		Y, 
		Up, 
		Down, 
		LeftShoulder, 
		RightShoulder, 
		Start, 
		Back, 
		LStick, 
		RStick, 
		L1, 
		R1,
		Max
	};

	public static string[] DefaultAxisNames = new string[(int)Axis.Max] 
	{
		"Left_X_Axis", 
		"Left_Y_Axis", 
		"Right_X_Axis", 
		"Right_Y_Axis", 
		"LeftTrigger", 
		"RightTrigger"
	};
	
	public static string[] DefaultButtonNames = new string[(int)Button.Max] 
	{
		"Button A", 
		"Button B", 
		"Button X", 
		"Button Y", 
		"Up", 
		"Down", 
		"Left Shoulder", 
		"Right Shoulder", 
		"Start", 
		"Back", 
		"LStick", 
		"RStick", 
		"LeftShoulder", 
		"RightShoulder"
	};

	public static string[] AxisNames = null;
	public static string[] ButtonNames = null;

	const int numControllersToRead = 4;

	public static List<KeyInfo> blockedKeys = new List<KeyInfo>();

	public static int NumControllersToRead
	{
		get{return numControllersToRead;}
	}

	public const string CONTROLLER_ASSIGNMENT = "ControllerAssignment";

	//==============================================
	// KeyInfo
	//
	// Holds information about a single key event.
	//==============================================
	public class KeyInfo 
	{
		// key constructor
		public KeyInfo( eDeviceType inDeviceType, 
		               string inKeyName, 
		               OnKeyDown inDownHandler, 
		               OnKeyHeld inHeldHandler, 
		               OnKeyUp inUpHandler ) 
		{
			DeviceType = inDeviceType;
			KeyName = inKeyName;
			MouseButton = eMouseButton.None;
			JoystickButton = Button.None;
			JoystickAxis = Axis.None;
			Threshold = 1000.0f;
			WasDown = false;
			DownHandler = inDownHandler;
			HeldHandler = inHeldHandler;
			UpHandler = inUpHandler;
		}				
		// mouse button constructor
		public KeyInfo( eDeviceType inDeviceType, 
		               eMouseButton inMouseButton,
		               OnKeyDown inDownHandler, 
		               OnKeyHeld inHeldHandler, 
		               OnKeyUp inUpHandler ) 
		{
			DeviceType = inDeviceType;
			KeyName = "Mouse Button " + (int)inMouseButton;
			MouseButton = inMouseButton;
			JoystickButton = Button.None;
			JoystickAxis = Axis.None;
			Threshold = 1000.0f;
			WasDown = false;
			DownHandler = inDownHandler;
			HeldHandler = inHeldHandler;
			UpHandler = inUpHandler;
		}
		// joystick button constructor
		public KeyInfo( eDeviceType inDeviceType, 
		               Button inJoystickButton,
		               OnKeyDown inDownHandler, 
		               OnKeyHeld inHeldHandler, 
		               OnKeyUp inUpHandler ) 
		{
			DeviceType = inDeviceType;
			KeyName = ButtonNames[(int)inJoystickButton];
			MouseButton = eMouseButton.None;
			JoystickButton = inJoystickButton;
			JoystickAxis = Axis.None;
			Threshold = 1000.0f;
			WasDown = false;
			DownHandler = inDownHandler;
			HeldHandler = inHeldHandler;
			UpHandler = inUpHandler;
		}
		// joystick axis constructor
		public KeyInfo( eDeviceType inDeviceType, 
		               Axis inJoystickAxis,
		               OnKeyDown inDownHandler, 
		               OnKeyHeld inHeldHandler, 
		               OnKeyUp inUpHandler ) 
		{
			DeviceType = inDeviceType;
			KeyName = AxisNames[(int)inJoystickAxis];
			MouseButton = eMouseButton.None;
			JoystickButton = Button.None;
			JoystickAxis = inJoystickAxis;
			Threshold = 0.5f;
			WasDown = false;
			DownHandler = inDownHandler;
			HeldHandler = inHeldHandler;
			UpHandler = inUpHandler;
		}
		
		public 	eDeviceType					DeviceType		= eDeviceType.None;
		public 	string						KeyName			= "";
		public	eMouseButton				MouseButton		= eMouseButton.None;
		public	Button	JoystickButton	= Button.None;
		public	Axis	JoystickAxis	= Axis.None;
		public	float						Threshold		= 1000.0f; // threshold for triggers
		public 	bool						WasDown			= false;
		public	OnKeyDown					DownHandler;
		public	OnKeyHeld					HeldHandler;
		public 	OnKeyUp						UpHandler;
		public	bool						exclusive		= false;
	};
	
	//private static List< KeyInfo >	KeyInfos = new List< KeyInfo >();
	
	private static string	PlatformPrefix = "";
	
	//==============================================
	// InputMapping
	//
	// Maps joystick input to a component.
	//==============================================
	public class InputMapping
	{
		public InputMapping( MonoBehaviour comp, int joystickNumber )
		{
			Component = comp;
			JoystickNumber = joystickNumber;
		}

		public List< KeyInfo >	KeyInfos = new List< KeyInfo >();

		public MonoBehaviour	Component;		// the component input goes to
		public int				JoystickNumber;	// the joystick that controls the object
	};
	
	// list of mappings from joystick to component
	private static List< InputMapping > InputMap = new List< InputMapping >();

	public static List<InputMapping> GetInputMap {
		get {
			return InputMap;
		}
		set { InputMap = value; }
	}
	
	//======================
	// Init_Windows
	// Initializes the input system for Windows.
	//======================
	private static void Init_Windows()
	{
		Debug.Log( "Initializing input for Windows." );
		AllowKeyControls = true;
		//PlatformPrefix = "Win:";
		PlatformPrefix = "";
	}
	//======================
	// Init_Windows_Editor
	// Initializes the input system for Windows when running from the Unity editor.
	//======================	
	private static void Init_Windows_Editor()
	{
		Debug.Log( "Initializing input for Windows Editor." );
		AllowKeyControls = true;
		//PlatformPrefix = "Win:";
		PlatformPrefix = "";
	}	
	//======================
	// Init_Android
	// Initializes the input system for Android.
	//======================	
	private static void Init_Android()
	{
		Debug.Log( "Initializing input for Android." );
		AllowKeyControls = true;
		PlatformPrefix = "Android:";		
	}
	//======================
	// Init_OSX.
	// Initializes the input system for OSX.
	//======================
	private static void Init_OSX()
	{
		Debug.Log( "Initializing input for OSX." );
		AllowKeyControls = true;
		PlatformPrefix = "OSX:";
		//PlatformPrefix = "";
	}	
	//======================
	// Init_OSX
	// Initializes the input system for OSX when running from the Unity editor.
	//======================	
	private static void Init_OSX_Editor()
	{
		Debug.Log( "Initializing input for OSX Editor." );
		AllowKeyControls = true;
		PlatformPrefix = "OSX:";		
	}	
	//======================
	// Init_iPhone
	// Initializes the input system for iPhone.
	//======================	
	private static void Init_iPhone()
	{
		Debug.Log( "Initializing input for iPhone." );
		AllowKeyControls = false;
		PlatformPrefix = "iPhone:";		
	}

	private static void Init_WiiU()
	{
		Debug.Log("Initializing input for WiiU.");
		AllowKeyControls = false;
		PlatformPrefix = "WiiU:";
	}
	
	//======================
	// InputControl
	// Static contructor for the InputControl class.
	//======================	
	static InputControl()
	{
		#if ( UNITY_ANDROID && !UNITY_EDITOR )
		SetReadAxisDelegate( ReadJoystickAxis );
		SetReadButtonDelegate( ReadJoystickButton );
		#endif
		switch ( Application.platform ) {
		case RuntimePlatform.WindowsPlayer: 
			Init_Windows(); 
			break;
		case RuntimePlatform.WindowsEditor: 
			Init_Windows_Editor();
			break;
		case RuntimePlatform.Android: 
			Init_Android();
			break;
		case RuntimePlatform.OSXPlayer: 
			Init_OSX();
			break;
		case RuntimePlatform.OSXEditor: 
			Init_OSX_Editor();
			break;
		case RuntimePlatform.IPhonePlayer: 
			Init_iPhone();
			break;
		//case RuntimePlatform.WiiU:
		//	Init_WiiU();
		//	break;
		default:
			break;
		}
		
		string[] joystickNames = Input.GetJoystickNames();
		for ( int i = 0; i < joystickNames.Length; ++i )
		{
			Debug.Log( "Found joystick '" + joystickNames[i] + "'..." );
		}

		ButtonNames = DefaultButtonNames;
		AxisNames = DefaultAxisNames;
	}
	//======================
	// AddInputHandler
	// Adds a hander for key input
	//======================
	public static void AddInputHandler( eDeviceType dt, string keyName, 
	                                   OnKeyDown onDown, OnKeyHeld onHeld, OnKeyUp onUp, int joystickNum = 0, bool exc = false)
	{
		KeyInfo k = new KeyInfo( dt, keyName, onDown, onHeld, onUp );
		k.WasDown = Input.GetKey( keyName );
		k.exclusive = exc;

		InputMap[joystickNum].KeyInfos.Add(k);
	}

	//======================
	// AddInputHandler
	// Adds a hander for mouse button input
	//======================
	public static void AddInputHandler( eDeviceType dt, eMouseButton mouseButton, 
	                                   OnKeyDown onDown, OnKeyHeld onHeld, OnKeyUp onUp, int joystickNum = 0,  bool exc = false)
	{
		KeyInfo k = new KeyInfo( dt, mouseButton, onDown, onHeld, onUp  );
		k.WasDown = GetMouseButton(mouseButton);
		k.exclusive = exc;

		InputMap[joystickNum].KeyInfos.Add( k );
	
	}
	//======================
	// AddInputHandler
	// Adds a hander for joystick button input
	//======================
	public static void AddInputHandler( eDeviceType dt, Button joystickButton,
	                                   OnKeyDown onDown, OnKeyHeld onHeld, OnKeyUp onUp, int joystickNum = 0,  bool exc = false)
	{
		KeyInfo k = new KeyInfo( dt, joystickButton, onDown, onHeld, onUp );
		k.exclusive = exc;
	
		k.WasDown = GetJoystickButton(InputMap[joystickNum].JoystickNumber, joystickButton);

		InputMap[joystickNum].KeyInfos.Add( k );
	}

	public static void RemoveInputHandlerButton (OnKeyDown onDown, OnKeyHeld onHeld, OnKeyUp onUp, int joystickNum = 0)
	{
		if(InputMap.Count <= joystickNum)
			return;

		for(int i = InputMap[joystickNum].KeyInfos.Count - 1; i >= 0; --i)
		{
			KeyInfo k = InputMap[joystickNum].KeyInfos[i];

			if(k.DownHandler == onDown && k.HeldHandler == onHeld && k.UpHandler == onUp)
			{
				InputMap[joystickNum].KeyInfos.RemoveAt(i);
			}
		}
	}

	//======================
	// AddInputHandler
	// Adds a hander for joystick axis input
	//======================
	public static void AddInputHandler( eDeviceType dt, Axis axis,
	                                   OnKeyDown onDown, OnKeyHeld onHeld, OnKeyUp onUp, int joystickNum = 0 )
	{
		InputMap[joystickNum].KeyInfos.Add( new KeyInfo( dt, axis, onDown, onHeld, onUp ) );
	}
	
	//======================
	// GetJoystickAxis
	// Returns the current value of the joystick axis specified by the name parameter.
	// The name should partially match the name of an axis specified in the Unity
	// Edit -> Project Settings -> Input pane, minus the Platform: Joy #: qualifiers.
	// For instance, specify "Left_X_Axis" to select the appropriate axis for the
	// current platform.  This will be permuted into something like "Win:Joy 1:Left_X_Axis"
	// before it is queried.
	//======================
	public static float GetJoystickAxis( int joystickNumber, string name )
	{
		// TODO: except for the joystick prefix this could be a table lookup
		// with a table-per-joystick this could be a lookup.
		#if ( UNITY_ANDROID && !UNITY_EDITOR )
		// on the Samsung gamepad, the left and right triggers are actually buttons
		// so we map left and right triggers to the left and right shoulder buttons.
		if ( name == "LeftTrigger" )
		{
			return GetJoystickButton( joystickNumber, Button.LeftShoulder ) ? 1.0f : 0.0f;
		}			
		else if ( name == "RightTrigger" )
		{
			return GetJoystickButton( joystickNumber, Button.RightShoulder ) ? 1.0f : 0.0f;
		}
		#endif
		string platformName = PlatformPrefix + "Joy " + joystickNumber + ":" + name;
		return Input.GetAxis( platformName );
	}
	
	//======================
	// GetJoystickAxis
	// Returns the current value of the specified joystick axis
	//======================
	public static float GetJoystickAxis( int joystickNumber, Axis axis )
	{
		string platformName = PlatformPrefix + "Joy " + joystickNumber + ":" + AxisNames[(int)axis];
		return Input.GetAxis( platformName );
	}
	
	//======================
	// ReadJoystickAxis
	//======================
	public static float ReadJoystickAxis( Axis axis )
	{
		return GetJoystickAxis( 1, axis );
	}
	
	//======================
	// GetJoystickButton
	// Returns true if a joystick button is depressed.
	// The name should partially match the name of an axis specified in the Unity
	// Edit -> Project Settings -> Input pane, minus the Platform: Joy #: qualifiers.
	// For instance, specify "Button A" to select the appropriate axis for the
	// current platform.  This will be permuted into something like "Win:Joy 1:Button A"
	// before it is queried.	
	//======================
	public static bool GetJoystickButton( int joystickNumber, string name )
	{
		// TODO: except for the joystick prefix this could be a table lookup
		// with a table-per-joystick this could be a lookup.
		string fullName = PlatformPrefix + "Joy " + joystickNumber + ":" + name;
		return Input.GetButton( fullName );	
	}
	
	//======================
	// GetJoystickButton	
	// Returns true if the specified joystick button is pressed
	//======================
	public static bool GetJoystickButton( int joystickNumber, Button button )
	{
		string fullName = PlatformPrefix + "Joy " + joystickNumber + ":" + ButtonNames[(int)button];
		//Debug.Log( "Checking button " + fullName );
		return Input.GetButton( fullName );	
	}
	
	//======================
	// ReadJoystickButton
	//======================
	public static bool ReadJoystickButton( Button button )
	{
		return GetJoystickButton( 1, button );
	}
	
	//======================	
	// GetMouseButton
	// Returns true if the specified mouse button is pressed.
	//======================
	public static bool GetMouseButton( eMouseButton button )
	{
		return Input.GetMouseButton( (int)button );
	}
	
	//======================
	// ShowAxisValues
	// Outputs debug spam for any non-zero axis. 
	// This is only used for finding which axes are which with new controllers.
	//======================
	private static void ShowAxisValues()
	{
		for ( int i = 1; i <= 20; ++i )
		{
			string axisName = "Test Axis " + i;
			float v = Input.GetAxis( axisName );
			if ( Mathf.Abs( v ) > 0.2f )
			{
				Debug.Log( "Test Axis " + i + ": v = " + v );
			}
		}
	}
	
	//======================
	// ShowButtonValues
	// Outputs debug spam for any depressed button.
	// This is only used for finding which buttons are which with new controllers.
	//======================
	private static void ShowButtonValues()
	{
		for ( int i = 0; i < 6; ++i ) {
			string buttonName = "Test Button " + i;
			if ( Input.GetButton( buttonName ) ) 
			{
				Debug.Log( "Test Button " + i + " is down." );
			}
		}
	}
	
	//======================
	// AddInputMapping
	// Adds a mapping from a joystick to a behavior.
	//======================
	public static void AddInputMapping( int joystickNumber, MonoBehaviour comp )
	{
		for ( int i = 0; i < InputMap.Count; ++i )
		{
			InputMapping im = InputMap[i];
			if ( im.Component == comp && im.JoystickNumber == joystickNumber )
			{
				Debug.LogError( "Input mapping already exists!" );
				return;
			}

		}
		InputMap.Add( new InputMapping( comp, joystickNumber ) );
	}
	
	//======================
	// RemoveInputMapping
	// Removes a mapping from a joystick to a behavior.
	//======================
	public static void RemoveInputMapping( int joystickNumber, MonoBehaviour comp )
	{
		for ( int i = 0; i < InputMap.Count; ++i )
		{
			InputMapping im = InputMap[i];
			if ( im.Component == comp && im.JoystickNumber == joystickNumber )
			{
				InputMap.RemoveAt( i );
				return;
			}
		}		
	}
	
	//======================
	// ClearControlMappings
	// Removes all control mappings.
	//======================
	public static void ClearControlMappings()
	{

		InputMap.Clear();
	}
	
	//======================
	// Update
	// Updates the state of all input mappings.  This must be called from 
	// a single MonoBehaviour's Update() method for input to be read.
	//======================
	public static void Update()
	{
		// Enable these two lines if you have a new controller that you need to
		// set up for which you do not know the axes.
		//ShowAxisValues();
		//ShowButtonValues();
		
		for ( int i = 0; i < InputMap.Count; ++i )
		{
			UpdateInputMapping( InputMap[i]);
		}
	}
	
	//======================
	// UpdateInputMapping
	// Updates a single input mapping.
	//======================
	private static void UpdateInputMapping(InputMapping im)
	{
		blockedKeys.Clear();

		for ( int i = im.KeyInfos.Count - 1; i >= 0; --i ) 
		//for ( int i = 0; i < im.KeyInfos.Count; ++i )
		{
			bool keyDown = false;
			// query the correct device
			KeyInfo keyInfo = im.KeyInfos[i];
			if ( keyInfo.DeviceType == eDeviceType.Gamepad )
			{
				//Debug.Log( "Checking gamepad button " + keyInfo.KeyName );
				keyDown = GetJoystickButton( im.JoystickNumber, keyInfo.JoystickButton );
			}
			else if ( keyInfo.DeviceType == eDeviceType.Axis )
			{
				float axisValue = GetJoystickAxis( im.JoystickNumber, keyInfo.JoystickAxis );
				keyDown = ( axisValue >= keyInfo.Threshold );
			}
			else if ( AllowKeyControls )
			{
				if ( keyInfo.DeviceType == eDeviceType.Mouse )
				{
					keyDown = GetMouseButton( keyInfo.MouseButton );
				}
				else if ( keyInfo.DeviceType == eDeviceType.Keyboard )
				{
					keyDown = Input.GetKey( keyInfo.KeyName );
				}
			}
			
			// handle the event
			if ( keyDown == false )
			{
				if ( keyInfo.WasDown )
				{
					//Debug.Log(PlatformPrefix + "Joy " + im.JoystickNumber + ":" + ButtonNames[(int)keyInfo.JoystickButton]);

					// key was just released
					if(keyInfo.UpHandler != null)
					{
						if(!KeyBlocked(keyInfo))
						{
							keyInfo.UpHandler( im.Component );
						}
					}
				}
			} 
			else 
			{
				if ( keyInfo.WasDown == false )
				{
					// key was just pressed
					//Debug.Log( "Key or Button down: " + keyInfo.KeyName );
					if(keyInfo.DownHandler != null)
					{
						if(!KeyBlocked(keyInfo))
						{
							keyInfo.DownHandler( im.Component );
						}
					}
				}
				else
				{
					// key is held
					if(keyInfo.HeldHandler != null)
					{
						if(!KeyBlocked(keyInfo))
						{
							keyInfo.HeldHandler( im.Component );
						}
					}
				}
			}

			if(keyInfo.exclusive)
				blockedKeys.Add(keyInfo);

			// update the key info
			keyInfo.WasDown = keyDown;
		}
	}

	static bool KeyBlocked(KeyInfo info)
	{
		if(info.JoystickButton != Button.None)
		{
			for(int i = 0; i < blockedKeys.Count; ++i)
			{
				if(info.JoystickButton == blockedKeys[i].JoystickButton)
					return true;
			}
		}
		else if(!string.IsNullOrEmpty(info.KeyName))
		{
			for(int i = 0; i < blockedKeys.Count; ++i)
			{
				if(info.KeyName == blockedKeys[i].KeyName)
					return true;
			}
		}
		else if(info.MouseButton != eMouseButton.None)
		{
			for(int i = 0; i < blockedKeys.Count; ++i)
			{
				if(info.MouseButton == blockedKeys[i].MouseButton)
					return true;
			}
		}


        return false;
	}
};