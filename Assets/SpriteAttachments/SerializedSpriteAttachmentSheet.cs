using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializedSpriteAttachmentSheet : ScriptableObject
{
    public Texture2D Texture;

    public List<SerializedSpriteAttachmentSprite> AttachmentSprites = new List<SerializedSpriteAttachmentSprite>();
    
    public SerializedSpriteAttachmentSprite this[string name]
    {
        get
        {
            foreach (SerializedSpriteAttachmentSprite sprite in AttachmentSprites)
            {
                if (sprite.Name == name)
                {
                    return sprite;
                }
            }
            return null;
        }
    }
}