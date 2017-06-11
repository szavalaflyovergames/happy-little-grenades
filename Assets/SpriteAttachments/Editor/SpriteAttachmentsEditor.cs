using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SpriteAttachments))]
public class SpriteAttachmentsEditor : Editor
{
    public SpriteAttachments Target { get; private set; }
    private SerializedSpriteAttachmentSheet oldSheet;
    private SpriteRenderer oldSpriteRenderer;

    public override void OnInspectorGUI()
    {
        Target = (SpriteAttachments)target;

        if (Target.sheet != oldSheet)
        {
            if (Target.sheet != null && Target.spriteRenderer != null)
            {
                Target.CreateAttachments();
            }
            else
            {
                Target.DeleteAttachments();
            }
        }

        if (Target.spriteRenderer != oldSpriteRenderer)
        {
            if (Target.spriteRenderer != null && Target.sheet != null)
            {
                Target.CreateAttachments();
            }
            else
            {
                Target.DeleteAttachments();
            }
        }

        oldSpriteRenderer = Target.spriteRenderer;
        oldSheet = Target.sheet;

        EditorGUILayout.HelpBox("Will recreate all attachment on runtime if you manually destroy one of them", MessageType.Info);
        base.OnInspectorGUI();
    }
}
