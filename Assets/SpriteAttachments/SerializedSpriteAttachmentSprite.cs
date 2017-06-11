using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializedSpriteAttachmentSprite 
{
    [HideInInspector]
    public string Name;
    public Vector2 pivot;
    public Rect textureRect;
    public List<SerializedSpriteAttachment> Attachments = new List<SerializedSpriteAttachment>();

    public SerializedSpriteAttachment this[string name]
    {
        get
        {
            foreach (SerializedSpriteAttachment attachment in Attachments)
            {
                if (attachment.Name == name)
                {
                    return attachment;
                }
            }
            return null;
        }
    }

    
}