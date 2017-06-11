using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializedSpriteAttachment 
{
    [HideInInspector]
    public string Name = "";
    public Vector2 NormalizedPosition;
    public Vector2 EditorPosition;
    public Quaternion Rotation = Quaternion.identity;
}