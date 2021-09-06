using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyGoodyView : MonoBehaviour
{
    public Transform WeekGroup;
    RectTransform[] WeekRects;

    // Start is called before the first frame update
    void Start()
    {
        UpdateWeekView();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateWeekView()
    {
        if (WeekRects == null)
        {
            WeekRects = new RectTransform[WeekGroup.childCount];
            for (int i = 0; i < WeekGroup.childCount; i++)
            {
                WeekRects[i] = WeekGroup.GetChild(i).GetComponent<RectTransform>();
            }
        }

        int week = DailyGoodyManager.Instance.Data.StreakDays / 7;
        for (int i = 0; i < WeekRects.Length; i++)
        {
            WeekRects[i].sizeDelta = Vector2.one * (i == week? 64 : 51.2f);
            WeekRects[i].GetComponent<CanvasGroup>().alpha = i == week? 1 : 0.6f;
            WeekRects[i].Find("Image_Tick").gameObject.SetActive(i == week);
        }
    }
}
