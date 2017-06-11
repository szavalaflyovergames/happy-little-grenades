using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpriteAttachment : MonoBehaviour
{
    public bool flipChildren = true;

    public SpriteAttachments Attachments { get; set; }

    private List<SpriteRenderer> spriteRendererChildren = new List<SpriteRenderer>();

    private void Awake()
    {
        Attachments = GetComponentInParent<SpriteAttachments>();
        spriteRendererChildren = GetComponentsInChildren<SpriteRenderer>().ToList();
    }

    private void Update()
    {
        if (Attachments == null)
            Attachments = GetComponentInParent<SpriteAttachments>();

        if(flipChildren)
        {
            foreach(SpriteRenderer childRenderer in spriteRendererChildren)
            {
                childRenderer.flipX = Attachments.spriteRenderer.flipX;
                childRenderer.flipY = Attachments.spriteRenderer.flipY;
            }        
        }
    }
}
