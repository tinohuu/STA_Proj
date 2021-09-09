using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoodies : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] RectTransform boxGroup;
    [SerializeField] GameObject boxPrefab;
    [SerializeField] ButtonAnimator CollectButton;

    [SerializeField] GameObject GoodyTextPrefab;
    [SerializeField] Transform TextGroup;

    [SerializeField] RectTransform Title;
    [SerializeField] Button OpenBoxButton;
    [SerializeField] CanvasGroup View;
    [SerializeField] Image Shade;
    public float Speed = 20;


    List<DailyGoodiesBox> boxes = new List<DailyGoodiesBox>();

    DailyGoodiesCalendar calendar;
    public static DailyGoodies Instance;
    Sequence sequence;
    private void Awake()
    {
        if (!Instance) Instance = this;
        CreateBoxes();
        calendar = GetComponentInChildren<DailyGoodiesCalendar>();
    }

    void Start()
    {
        AnimateBox();

        CollectButton.transform.localScale = Vector3.zero;
        CollectButton.OnClick.AddListener(() => Collect());
    }

    public void AnimateBox()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(boxGroup);

        // Animate Title
        Title.DOAnchorPosY(200, 2).From().SetEase(Ease.OutBounce);
        Title.DOShakeRotation(1, Vector3.forward * 10, 5, 5).SetEase(Ease.OutSine).SetDelay(0.75f);

        // Animate the last box, and then all boxes
        int oldStkDays = DailyGoodiesManager.Instance.Data.StreakDays;
        RectTransform rt = boxGroup.GetChild(27).GetComponent<RectTransform>();
        boxGroup.anchoredPosition = new Vector2(-rt.anchoredPosition.x + boxGroup.parent.GetComponent<RectTransform>().sizeDelta.x / 2, boxGroup.anchoredPosition.y);
        Tween tween = rt.DOScale(Vector3.zero, 1.5f).From().SetEase(Ease.OutBack)
            .OnComplete(() => UpdateBoxeseView(oldStkDays));
        sequence = DOTween.Sequence();
        OpenBoxButton.onClick.AddListener(() => ForceCompleteBoxSequence());
        sequence.Append(tween);
        sequence.AppendInterval(1);

        // Then scroll boxes by week
        int curStkDays = DailyGoodiesManager.Instance.CheckDate(false);
        for (int i = 21; i > curStkDays - 7; i -= 7)
        {
            Debug.Log(boxGroup.childCount + ":" + i);
            rt = boxGroup.GetChild(i).GetComponent<RectTransform>();
            int week = i / 7 + 1;
            tween = boxGroup.DOAnchorPosX(-rt.anchoredPosition.x + rt.sizeDelta.x / 2, 1.5f).SetEase(Ease.InOutBack).OnStart(() => calendar.UpdateWeekView(week));
            sequence.Append(tween);
        }

        // Then restore boxes if streak broken
        sequence.AppendCallback(() => UpdateBoxeseView(curStkDays - 1));
        sequence.AppendInterval(0.25f);

        // Then box jump
        var box = boxGroup.GetChild(curStkDays - 1).GetComponent<DailyGoodiesBox>();
        sequence.Append(box.Box.DOJump(Vector3.back * 2 + Vector3.down * 2, 3, 1, 1.5f).SetEase(Ease.OutSine));
        sequence.Join(box.Box.DOScale(Vector3.one * 1.5f, 2));
        sequence.AppendCallback(() => OpenBoxButton.gameObject.SetActive(true));
        OpenBoxButton.onClick.AddListener(() => ShowBox(box));

        // Then show box
        //sequence.AppendCallback(() => ShowBox());

        // Play
        sequence.PlayForward();
    }

    public void ForceCompleteBoxSequence()
    {
        if (sequence != null)
        {
            sequence.Complete();
        }
    }

    void CreateBoxes()
    {
        for (int i = 0; i < 28; i++)
        {
            var box = Instantiate(boxPrefab, boxGroup).GetComponent<DailyGoodiesBox>();
            box.Day = i + 1;
            boxes.Add(box);
        }
    }

    void UpdateBoxeseView(int collectedDays)
    {
        foreach (var box in boxes) box.UpdateView(collectedDays);
    }

    public void ShowBox(DailyGoodiesBox box)
    {
        OpenBoxButton.onClick.RemoveAllListeners();
        OpenBoxButton.gameObject.SetActive(false);
        // todo: animate

        boxGroup.transform.DestroyChildren();
        RewardConfig config = DailyGoodiesManager.Instance.GetGoodyConfig(box.Day);
        var items = Reward.StringToReward(config.ItemReward);
        Debug.Log(JsonUtility.ToJson(config));

        List<DailyGoodiesRewardText> goodyTexts = new List<DailyGoodiesRewardText>();
        if (config.CoinReward > 0)
        {
            var goodyText = Instantiate(GoodyTextPrefab, TextGroup).GetComponent<DailyGoodiesRewardText>();
            goodyText.RewardType = RewardType.Coin;
            goodyText.Count = DailyGoodiesManager.Instance.GetCoin(box.Day);
            goodyTexts.Add(goodyText);
        }
        foreach (var type in items.Keys)
        {
            var goodyText = Instantiate(GoodyTextPrefab, TextGroup).GetComponent<DailyGoodiesRewardText>();
            goodyText.RewardType = type;
            goodyText.Count = items[type];
            goodyTexts.Add(goodyText);
        }

        for (int i = 0; i < goodyTexts.Count; i++)
        {
            goodyTexts[i].UpdateView(i * 0.5f);
        }

        CollectButton.transform.DOScale(Vector3.one, 0.5f).SetDelay(goodyTexts.Count * 0.5f);
    }

    public void Collect()
    {
        int day = DailyGoodiesManager.Instance.CheckDate(true);
        var config = DailyGoodiesManager.Instance.GetGoodyConfig(day);
        DailyGoodiesManager.Instance.CollectGoodyReward(config);

        var texts = TextGroup.GetComponentsInChildren<DailyGoodiesRewardText>();
        foreach (var text in texts)
        {
            text.OnCollect(CollectButton.transform.position);
        }
        Shade.DOFade(0, 0.5f);
        View.DOFade(0, 0.5f).OnComplete(() => Destroy(View.gameObject)).SetDelay(0.5f);
        // todo: exit
    }
}
