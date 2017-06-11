using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SpriteAttachmentSnapper : MonoBehaviour {

    private int lastChildCount;

	void Update ()
    {
        if (transform.childCount != lastChildCount)
        {
            foreach (Transform child in transform)
            {
                child.transform.localPosition = Vector3.zero;
            }
        }
        lastChildCount = transform.childCount;
    }
}
