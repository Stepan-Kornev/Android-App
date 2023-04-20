using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResizeMessages : MonoBehaviour
{
    public TMP_Text tmpText;
    public RectTransform parentRectTransform;

    public float heightPerCharacter = 32f;

    void Update()
    {
        // ѕолучаем стартовые значени€ родительской панели и самого текста
        Vector2 startParentRTSizeDelta = parentRectTransform.sizeDelta;
        Vector2 startTmpTextSizeDelta = tmpText.rectTransform.sizeDelta;

        // ѕолучаем предпочтительные значени€ дл€ нашего текста
        Vector2 prefferedValues = tmpText.GetPreferredValues(tmpText.text);

        float preferredHeight = prefferedValues.y;

        // ”величиваем предпочтительную высоту, учитыва€ каждый символ
        preferredHeight += heightPerCharacter * tmpText.textInfo.lineCount;

        // «адаем новые размеры дл€ текста и родительской панели
        tmpText.rectTransform.sizeDelta = new Vector2(tmpText.rectTransform.sizeDelta.x, preferredHeight);
        parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, preferredHeight);

        //  орректируем размер родительской панели, учитыва€ его первоначальный размер
        parentRectTransform.sizeDelta = startParentRTSizeDelta + (tmpText.rectTransform.sizeDelta - startTmpTextSizeDelta);
    }
}
