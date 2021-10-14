using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopScrollView : MonoBehaviour, IBeginDragHandler
{
    //ScrollRect m_ScrollRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        var scroll = GetComponent<ScrollRect>();
        SoundManager.Instance.PlaySFX(scroll.velocity.x > 0 ? "uiShopSlideShort" : "uiShopSlideLong");
    }
}
