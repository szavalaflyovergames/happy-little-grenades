using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EditorPopupEnterString : EditorWindow 
{
    public static EditorPopupEnterString m_Window   { get; private set; }
    public string m_Name                            { get; private set; }
    public bool m_DemandInput                       { get; set; }
    public event System.Action<string> OnConfirm = delegate { };

	private static void Init()
	{
        m_Window = new EditorPopupEnterString();
        m_Window.ShowPopup();
	}

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Enter Name");
        m_Name = EditorGUILayout.TextField(m_Name);
        if(GUILayout.Button("Confirm") || Event.current.keyCode == KeyCode.Return)
        {
            if (!m_DemandInput || m_DemandInput && !string.IsNullOrEmpty(m_Name))
            {
                OnConfirm(m_Name);
                Close();
            }
        }
    }
}
