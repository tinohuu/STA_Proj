using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyGoodiesCalendar : MonoBehaviour
{
    [SerializeField] Transform weekGroup;
    RectTransform[] weekRts;
    RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.DOAnchorPosY(200, 1).From();
        UpdateWeekView(4);
    }

    public void UpdateWeekView(int week)
    {
        if (weekRts == null)
        {
            weekRts = new RectTransform[weekGroup.childCount];
            for (int i = 0; i < weekGroup.childCount; i++)
            {
                weekRts[i] = weekGroup.GetChild(i).GetComponent<RectTransform>();
            }
        }
        week--;
        for (int i = 0; i < weekRts.Length; i++)
        {
            weekRts[i].DOSizeDelta(Vector2.one * (i == week ? 64 : 51.2f), 0.5f);
            weekRts[i].GetComponent<CanvasGroup>().DOFade(i == week ? 1 : 0.6f, 0.5f);
            weekRts[i].Find("Image_Tick").gameObject.SetActive(DailyGoodiesManager.Instance.Data.StreakDays / 7 - 1 >= i);
            //weekRts[i].Find("Image_Tick").DOScale(i == week ? Vector3.one : Vector3.zero, 0.5f);
        }
    }
}
