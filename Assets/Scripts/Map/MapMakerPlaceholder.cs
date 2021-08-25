using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapMakerPlaceholder : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public static Transform Target = null;
    private void Awake()
    {
        GetComponent<ButtonAnimator>().targetTransform = transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 camPos = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.parent.position = camPos;
        Target = transform;

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Target = transform;
        MapMaker.Instance.UpdateInputView();
    }
}
