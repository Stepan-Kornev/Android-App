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
        // �������� ��������� �������� ������������ ������ � ������ ������
        Vector2 startParentRTSizeDelta = parentRectTransform.sizeDelta;
        Vector2 startTmpTextSizeDelta = tmpText.rectTransform.sizeDelta;

        // �������� ���������������� �������� ��� ������ ������
        Vector2 prefferedValues = tmpText.GetPreferredValues(tmpText.text);

        float preferredHeight = prefferedValues.y;

        // ����������� ���������������� ������, �������� ������ ������
        preferredHeight += heightPerCharacter * tmpText.textInfo.lineCount;

        // ������ ����� ������� ��� ������ � ������������ ������
        tmpText.rectTransform.sizeDelta = new Vector2(tmpText.rectTransform.sizeDelta.x, preferredHeight);
        parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, preferredHeight);

        // ������������ ������ ������������ ������, �������� ��� �������������� ������
        parentRectTransform.sizeDelta = startParentRTSizeDelta + (tmpText.rectTransform.sizeDelta - startTmpTextSizeDelta);
    }
}
