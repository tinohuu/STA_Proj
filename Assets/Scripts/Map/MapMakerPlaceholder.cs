using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapMakerPlaceholder : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public static Transform Target = null;
    private void Awake()
    {
        GetComponent<ButtonAnimator>().TargetTransform = transform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 camPos = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10));
        transform.parent.position = camPos;
        Target = transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Target = transform;
        Mapmaker.Instance.Updatenputs();
    }
}
