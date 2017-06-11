using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpriteAttachments : MonoBehaviour
{
    public SerializedSpriteAttachmentSheet sheet;
    public SpriteRenderer spriteRenderer;

    public Dictionary<string, Transform> attachments = new Dictionary<string, Transform>();
   
    private void Awake()
    {
        SpriteAttachment[] found = GetComponentsInChildren<SpriteAttachment>();

        if (found.Count() < GetAttachments(spriteRenderer.sprite.name).Count())
        {
            Debug.Log("[SpriteAttachments] No or too few attachments present, recreating them");
            CreateAttachments();
        }
        else
        {
            found = GetComponentsInChildren<SpriteAttachment>();
            foreach (SpriteAttachment att in found)
            {
                attachments.Add(att.gameObject.name, att.transform);
            }
        }
    }

    public void CreateAttachments()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("[SpriteAttachments] Sprite renderer is null");
            return;
        }

        if (spriteRenderer.sprite == null)
        {
            Debug.LogError("[SpriteAttachments] Sprite in the sprite renderer is null");
            return;
        }

        DeleteAttachments();    
        foreach (SerializedSpriteAttachment attachment in GetAttachments(spriteRenderer.sprite.name))
        {
            GameObject go = new GameObject(attachment.Name);
            SpriteAttachment att = go.AddComponent<SpriteAttachment>();
            go.AddComponent<SpriteAttachmentSnapper>();
            att.Attachments = this;
            go.transform.SetParent(transform);
            attachments.Add(attachment.Name, go.transform);
            AdjustAttachment(attachment);
        }
    }

    public void DeleteAttachments()
    {
        SpriteAttachment[] found = GetComponentsInChildren<SpriteAttachment>();
        for (int i = 0; i < found.Length; i++)
        {
            DestroyImmediate(found[i].gameObject);
        }
        attachments.Clear();
    }

    private void LateUpdate()
    {
        foreach (SerializedSpriteAttachment attachment in GetAttachments(spriteRenderer.sprite.name))
        {
            AdjustAttachment(attachment);
        }
    }

    
    private void AdjustAttachment(SerializedSpriteAttachment attachment)
    {
        int flipX = spriteRenderer.flipX ? -1 : 1;
        int flipY = spriteRenderer.flipY ? -1 : 1;
        
        Transform trans = attachments[attachment.Name];

        trans.localPosition = new Vector3(
            (spriteRenderer.sprite.bounds.size.x * attachment.NormalizedPosition.x) * flipX,
            (spriteRenderer.sprite.bounds.size.y * attachment.NormalizedPosition.y) * flipY,
            trans.position.z);
        trans.localRotation = attachment.Rotation;
    }

    public IEnumerable<SerializedSpriteAttachment> GetAttachments(string spriteName)
    {
        if(sheet == null)
        {
            Debug.LogError("[SpriteAttachments] No Sprite Attachment Sheet has been assigned");
            return null;
        }

        if(sheet[spriteName] == null)
        {
            Debug.LogError(string.Format("[SpriteAttachments] Sheet does not contain sprite called '{0}'", spriteName));
            return null;
        }
        return sheet[spriteName].Attachments;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach(KeyValuePair<string, Transform> kvp in attachments)
        {
            if(kvp.Value != null && spriteRenderer.sprite != null)
                Gizmos.DrawWireSphere(kvp.Value.position, spriteRenderer.sprite.bounds.size.magnitude / 20);
        }
        Gizmos.color = Color.white;
    }
}
