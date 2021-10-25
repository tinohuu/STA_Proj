using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollRectModifier : MonoBehaviour
{
    public void AddVertical(float addition)
    {
        var scroll = GetComponent<ScrollRect>();
        float target = Mathf.Clamp(scroll.verticalNormalizedPosition + addition, 0, 1);
        scroll.DOVerticalNormalizedPos(target, 0.25f);
    }
}
