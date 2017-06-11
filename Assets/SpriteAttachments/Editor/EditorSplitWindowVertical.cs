using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class EditorSplitWindowVertical : EditorWindow
{
    public float SplitterWidth { get; set; }
    public GUIStyle RightViewGUIStyle { get; set; }
    public GUIStyle LeftViewGUIStyle { get; set; }
    public string LeftViewTitle { get; set; }
    public string RightViewTitle { get; set; }
    
    public Rect RightViewArea { get; private set; }
    public Rect LeftViewArea { get; private set; }
    public Vector2 ScrollPosLeft { get; private set; }
    public Vector2 ScrollPosRight { get; private set; }
    public Rect Splitter { get; private set; }
    public bool Dragging { get; private set; }
    public float SplitterX { get; private set; }
    
    protected virtual void Awake()
    {
        LeftViewTitle = "Left View";
        RightViewTitle = "Right View";
        LeftViewGUIStyle = GUIStyle.none;
        RightViewGUIStyle = GUIStyle.none;
        SplitterWidth = 7.0f;
        SplitterX = 200.0f;
    }

    protected virtual void OnGUI()
    {
        GUILayout.BeginHorizontal();

        ScrollPosLeft = GUILayout.BeginScrollView(ScrollPosLeft,
            GUILayout.Width(SplitterX),
            GUILayout.MaxWidth(SplitterX),
            GUILayout.MinWidth(SplitterX));
        LeftViewArea = new Rect(0, 0, SplitterX, Screen.height);
        OnGUILeftView();
        GUILayout.EndScrollView();

        GUILayout.Box("",
            GUILayout.Width(SplitterWidth),
            GUILayout.MaxWidth(SplitterWidth),
            GUILayout.MinWidth(SplitterWidth),
            GUILayout.ExpandHeight(true));
        Splitter = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(Splitter, MouseCursor.ResizeHorizontal);

        ScrollPosRight = GUILayout.BeginScrollView(ScrollPosRight,
            GUILayout.ExpandWidth(true));

        RightViewArea = new Rect(SplitterX, 0, Screen.width, Screen.height);
        OnGUIRightView();
        GUILayout.EndScrollView();

        GUILayout.EndHorizontal();

        if(Event.current != null)
        {
            if(Event.current.type == EventType.MouseDown)
            {
                if(Splitter.Contains(Event.current.mousePosition))
                {
                    Dragging = true;
                }
            }
            else if(Event.current.type == EventType.MouseDrag)
            {
                if(Dragging)
                {
                    SplitterX += Event.current.delta.x;
                    Repaint();
                }
            }
            else if(Event.current.type == EventType.MouseUp)
            {
                Dragging = false;
            }
        }
    }

    protected abstract void OnGUILeftView();
    protected abstract void OnGUIRightView();
}
