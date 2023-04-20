using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Org.BouncyCastle.Asn1.Mozilla;

public class ResizeTextMeshPro : MonoBehaviour
{
    private Vector2 startParentPos;

    public TMP_Text tmpText;
    public RectTransform parentRectTransform;
    public float marginBottom = 50f;

    private void Start()
    {
        startParentPos = parentRectTransform.sizeDelta;
    }

    void Update()
    {
        parentRectTransform.sizeDelta = startParentPos;

        int numLines = tmpText.textInfo.lineCount;
        float textHeight = tmpText.preferredHeight + marginBottom;
        float parentHeight = parentRectTransform.sizeDelta.y;

        if (numLines > 0 && textHeight > parentHeight)
        {
            parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, textHeight);
            tmpText.rectTransform.sizeDelta = new Vector2(tmpText.rectTransform.sizeDelta.x, textHeight);
        }
    }
}
