using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MapResizer : MonoBehaviour
{
    public bool UpdateResolution = true;
    RectTransform m_RT;
    RectTransform m_childRT;
    void Start()
    {
        m_RT = GetComponent<RectTransform>();
        m_childRT = transform.GetChild(0).GetComponent<RectTransform>();
        Resize();
        //rectTransform.anchoredPosition = new Vector2(Screen.width, Screen.height) / -200;
        //size = GetComponent<RectTransform>().sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        if (Debug.isDebugBuild && UpdateResolution)
        {
            Resize();
        }

        //rectTransform.anchoredPosition = new Vector2(Screen.width, Screen.height) / -200;

        // TODO: call when changing resolution
        //transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        //GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / (float)Screen.height * 1080, 1080);
    }

    void Resize()
    {
        m_RT.sizeDelta = new Vector2(Screen.width, Screen.height);
        m_childRT.sizeDelta = new Vector2((float)Screen.width / Screen.height * 1080, 1080);
    }
}
