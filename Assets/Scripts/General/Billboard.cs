using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour 
{
	public Camera m_Camera;
	
	public bool invert;
	
	public Vector3 upVector = Vector3.up;
	
	public bool localUp;
	
	//Autoassign the member camera.
	public bool autoAssign = false;
	
	void Awake()
	{
		if(autoAssign && m_Camera == null)
			m_Camera = Camera.main;
		
		if(m_Camera != null)
			UpdateFacing();
	}
	
    void LateUpdate()
    {
		if(m_Camera == null)
			return;
		
		UpdateFacing();
    }
	
	public void UpdateFacing()
	{
		if(localUp)
		{
			upVector = transform.up;
			
			transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward, upVector.normalized);
		}
		
		else
		{
			if(invert)
			{
				transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
	           		 m_Camera.transform.rotation * upVector.normalized);
			}
			
			else
			{
	       	 	transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.back,
	           		 m_Camera.transform.rotation * upVector.normalized);
			}
		}
	}
}
