using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyGoodyView : MonoBehaviour
{
    public GameObject GoodyTextPrefab;
    public Transform TextGroup;
    public Transform WeekGroup;
    RectTransform[] WeekRects;

    // Start is called before the first frame update
    void Start()
    {
        UpdateWeekView();
        ShowGoodies();
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

    void ShowGoodies()
    {
        int day = DailyGoodyManager.Instance.CheckDate(false);

        RewardConfig config = DailyGoodyManager.Instance.GetGoodyConfig(day);
        var items = Reward.StringToReward(config.ItemReward);

        List<DailyGoodyText> goodyTexts = new List<DailyGoodyText>();
        if (config.CoinReward > 0)
        {
            var goodyText = Instantiate(GoodyTextPrefab, TextGroup).GetComponent<DailyGoodyText>();
            goodyText.RewardType = RewardType.Coin;
            goodyText.Count = config.CoinReward;
            goodyTexts.Add(goodyText);
        }
        foreach (var type in items.Keys)
        {
            var goodyText = Instantiate(GoodyTextPrefab, TextGroup).GetComponent<DailyGoodyText>();
            goodyText.RewardType = type;
            goodyText.Count = items[type];
            goodyTexts.Add(goodyText);
        }

        for (int i = 0; i < goodyTexts.Count; i++)
        {
            goodyTexts[i].UpdateView(i * 0.5f);
        }
    }
}
