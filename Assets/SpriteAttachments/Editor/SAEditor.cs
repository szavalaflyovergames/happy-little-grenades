using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class SAEditor : EditorSplitWindowVertical
{
    public Texture2D circleTexture;

    public static SAEditor Window { get; private set; }

    public SerializedSpriteAttachmentSheet SpriteSheet { get; set; }

    public float UniScale { get; set; }

    private Texture2D oldTexture;

    private int selectedSprite;
    private int selectedAttachmentIndex;

    private List<string> gridSpriteNames = new List<string>();
    private List<Sprite> sprites = new List<Sprite>();

    private bool showSprites = true;
    private bool showAttachments = true;
    private bool dirty = true;
    private bool popupActive;
    private EditorPopupEnterString popup;
    private SerializedSpriteAttachment dragging;
    private UnityEngine.Object oldSelection;


    [MenuItem("Window/Sprite Sheet Attachments")]
    private static void Init()
    {
        Window = GetWindow<SAEditor>("Sprite Attachments Editor");
    }

    protected override void Awake()
    {
        base.Awake();
        UniScale = 1;
        selectedSprite = 0;
        selectedAttachmentIndex = 0;
        oldSelection = null;
        oldTexture = null;
        sprites.Clear();
        gridSpriteNames.Clear();
        Undo.undoRedoPerformed += UndoRedoCallback;
    }

    private void OnDestroy()
    {
        Undo.ClearUndo(SpriteSheet);
    }

    private void UndoRedoCallback()
    {
        dirty = true;
        Repaint();
    }

    private void Update()
    {
        if (Window == null)
            Window = GetWindow<SAEditor>();

        if (oldSelection != Selection.activeObject)
        {
            if (Selection.activeObject != null)
            {
                if (Selection.activeObject.GetType() == typeof(SerializedSpriteAttachmentSheet))
                {
                    SpriteSheet = (SerializedSpriteAttachmentSheet)Selection.activeObject;
                    OnSpriteSheetLoaded();
                }
                else
                    SpriteSheet = null;
                Repaint();
            }
        }

        oldSelection = Selection.activeObject;

        if (SpriteSheet != null)
        {
            if (dirty)
            {
                //AssetDatabase.Refresh();
                EditorUtility.SetDirty(SpriteSheet);
                AssetDatabase.SaveAssets();
                dirty = false;
            }

            if (SpriteSheet.Texture != null && oldTexture == null)
            {
                OnSpriteSheetLoaded();
                Repaint();
            }
            oldTexture = SpriteSheet.Texture;


            if (sprites.Count > selectedSprite)
            {
                foreach (SerializedSpriteAttachmentSprite attachmentSprite in SpriteSheet.AttachmentSprites)
                {
                    foreach (SerializedSpriteAttachment attachment in attachmentSprite.Attachments)
                    {
                        float sW = (attachmentSprite.textureRect.width * UniScale);
                        float sH = attachmentSprite.textureRect.height * UniScale;

                        Vector3 pivot = new Vector3(attachmentSprite.pivot.x * UniScale, attachmentSprite.pivot.y * UniScale, 0.0f);
                        Vector2 iconSize = new Vector2(circleTexture.width / 2, circleTexture.height / 2);
                        float percentX = (attachment.EditorPosition.x - pivot.x + iconSize.x) / sW;
                        float percentY = (attachment.EditorPosition.y - pivot.y + iconSize.y) / sH;

                        attachment.NormalizedPosition = new Vector2(percentX, -percentY);
                    }
                }
            }
        }
    }

    protected override void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", EditorStyles.toolbar, GUILayout.MaxWidth(Screen.width));
        EditorGUILayout.EndHorizontal();
        base.OnGUI();
    }

    protected override void OnGUILeftView()
    {
        EditorGUIUtility.labelWidth = 80;
        EditorGUILayout.LabelField("");

        if (SpriteSheet == null)
        {
            gridSpriteNames.Clear();
            selectedSprite = 0;
        }

        if (SpriteSheet != null)
        {
            GUILayout.Space(10);
            showSprites = EditorGUILayout.Foldout(showSprites, "Sprites");

            if (showSprites)
            {
                selectedSprite = GUILayout.SelectionGrid(selectedSprite, gridSpriteNames.ToArray(), 1);
            }

            showAttachments = EditorGUILayout.Foldout(showAttachments, "Attachments");

            if (showAttachments)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.green;

                if (popup == null)
                    popupActive = false;

                if (GUILayout.Button("Create") && !popupActive)
                {
                    popupActive = true;
                    popup = GetWindow<EditorPopupEnterString>();
                    popup.m_DemandInput = true;
                    popup.Focus();
                    popup.OnConfirm += (string name) =>
                    {
                        foreach (SerializedSpriteAttachmentSprite sprite in SpriteSheet.AttachmentSprites)
                        {
                            if (sprite.Attachments.SingleOrDefault(x => x.Name == name) != null)
                            {
                                Debug.LogError(string.Format("[SAEditor] Sprite Attachment with name '{0}' already exists.", name));
                                break;
                            }
                            SerializedSpriteAttachment attachment = new SerializedSpriteAttachment();
                            attachment.Name = name;
                            Undo.RecordObject(SpriteSheet, "Add Attachment");
                            sprite.Attachments.Add(attachment);
                            dirty = true;
                        }
                        Repaint();
                        popupActive = false;
                    };
                }


                GUI.backgroundColor = Color.red;

                if (GUILayout.Button("Delete"))
                {
                    foreach (SerializedSpriteAttachmentSprite sprite in SpriteSheet.AttachmentSprites)
                    {
                        SerializedSpriteAttachment toDelete = null;
                        foreach (SerializedSpriteAttachment attachment in sprite.Attachments)
                        {
                            if (sprite.Attachments[selectedAttachmentIndex].Name == attachment.Name)
                            {
                                toDelete = attachment;
                                break;
                            }
                        }

                        if (toDelete != null)
                        {
                            Undo.RecordObject(SpriteSheet, "Delete Attachment");
                            sprite.Attachments.Remove(toDelete);
                            dirty = true;
                        }
                    }
                    selectedAttachmentIndex = 0;
                    Repaint();
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (SpriteSheet.AttachmentSprites.Count > 0)
                {
                    string spriteName = sprites[selectedSprite].name;
                    SerializedSpriteAttachmentSprite sprite = null;
                    foreach (SerializedSpriteAttachmentSprite s in SpriteSheet.AttachmentSprites)
                    {
                        if (s.Name == spriteName)
                        {
                            sprite = s;
                            break;
                        }
                    }

                    if (sprite.Attachments.Count > 0)
                    {
                        List<string> attachmentNames = new List<string>();
                        sprite.Attachments.ForEach(x => attachmentNames.Add(x.Name));
                        selectedAttachmentIndex = GUILayout.SelectionGrid(selectedAttachmentIndex, attachmentNames.ToArray(), 1);
                    }
                }
            }
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Select Sprite Attachment Sheet Asset");
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    protected override void OnGUIRightView()
    {
        if (SpriteSheet != null)
        {
            if (SpriteSheet.Texture == null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("No texture");
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (sprites.Count > 0)
            {
                Texture t = sprites[selectedSprite].texture;
                Rect tr = sprites[selectedSprite].rect;
                Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height);

                float sW = sprites[selectedSprite].rect.width * UniScale;
                float sH = sprites[selectedSprite].rect.height * UniScale;

                GUI.DrawTextureWithTexCoords(new Rect(0, 0, sW, sH), t, r);



                if (dragging != null)
                {
                    Undo.RecordObject(SpriteSheet, "Positioning Attachment");
                    dragging.EditorPosition = Event.current.mousePosition -
                        new Vector2(circleTexture.width / 2, circleTexture.height / 2);
                    Repaint();
                }

                int index = 0;
                if (SpriteSheet.AttachmentSprites.Count > 0)
                {
                    foreach (SerializedSpriteAttachment attachment in SpriteSheet[sprites[selectedSprite].name].Attachments)
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            Rect attRect = new Rect(
                                attachment.EditorPosition.x,
                                attachment.EditorPosition.y,
                                circleTexture.width,
                                circleTexture.height);

                            Rect mouseRect = new Rect(
                                Event.current.mousePosition.x,
                                Event.current.mousePosition.y, 2, 2);

                            if (mouseRect.Overlaps(attRect) && dragging == null)
                            {
                                selectedAttachmentIndex = index;
                                dragging = attachment;
                            }
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            dragging = null;
                            dirty = true;
                        }


                        if (circleTexture != null)
                        {
                            Rect circleRect = new Rect(attachment.EditorPosition.x, attachment.EditorPosition.y, circleTexture.width, circleTexture.height);
                            GUIStyle grayStyle = GUIStyle.none;
                            grayStyle.normal.textColor = Color.grey;

                            if (attachment == SpriteSheet[sprites[selectedSprite].name].Attachments[selectedAttachmentIndex])
                                grayStyle.normal.textColor = Color.green;

                            Rect textRect = new Rect(
                                circleRect.x - (attachment.Name.Length / 2),
                                circleRect.y - 21.0f, 400, 100);

                            GUI.Label(textRect, attachment.Name, grayStyle);


                            if (attachment == SpriteSheet[sprites[selectedSprite].name].Attachments[selectedAttachmentIndex])
                                GUI.color = Color.green;

                            GUI.DrawTexture(circleRect, circleTexture);
                            GUI.color = Color.white;
                        }
                        index++;
                    }
                }
            }
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Select Sprite Attachment Sheet Asset");
            GUILayout.Label("Or create a new one by clicking: Assets -> Create -> Sprite Attachment Sheet");
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }


    private void OnSpriteSheetLoaded()
    {
        string path = AssetDatabase.GetAssetPath(SpriteSheet.Texture);

        Sprite[] foundSprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        sprites = foundSprites.ToList();
        selectedSprite = 0;
        selectedAttachmentIndex = 0;
        gridSpriteNames.Clear();

        foreach (Sprite sprite in sprites)
        {
            bool present = false;
            foreach (SerializedSpriteAttachmentSprite attSprite in SpriteSheet.AttachmentSprites)
            {
                if (sprite.name == attSprite.Name)
                {
                    present = true;
                    break;
                }
            }

            if (!present)
            {
                SerializedSpriteAttachmentSprite ssaSprite = new SerializedSpriteAttachmentSprite();
                ssaSprite.Name = sprite.name;
                SpriteSheet.AttachmentSprites.Add(ssaSprite);
            }
        }

        foreach (Sprite sprite in sprites)
        {
            foreach (SerializedSpriteAttachmentSprite attSprite in SpriteSheet.AttachmentSprites)
            {
                if (sprite.name == attSprite.Name)
                {
                    attSprite.pivot = sprite.pivot;
                    attSprite.textureRect = sprite.rect;
                }
            }
        }

        foreach (Sprite sprite in sprites)
        {
            SerializedSpriteAttachmentSprite ssaSprite = new SerializedSpriteAttachmentSprite();
            ssaSprite.Name = sprite.name;
            gridSpriteNames.Add(ssaSprite.Name);
        }

        if (sprites.Count > selectedSprite)
            CalculateUniScale(sprites[selectedSprite]);
        Repaint();
    }

    private void CalculateUniScale(Sprite sprite)
    {

        if (Screen.width > sprite.rect.width || Screen.height > sprite.rect.height)
        {
            float pixlDiffX = (Screen.width - sprite.rect.width) / (float)sprite.rect.width;
            float pixlDiffY = (Screen.height - sprite.rect.height) / (float)sprite.rect.height;
            UniScale = (1.0f + ((pixlDiffX + pixlDiffY) / 2.0f)) * 0.5f;
            UniScale = 1.0f + (1.0f / ((sprite.rect.width + sprite.rect.height) / 2.0f)) * 500.0f;
        }

    }
}
