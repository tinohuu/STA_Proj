using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour, ITimeRefreshable
{
    [SavedData] public QuestManagerData Data = new QuestManagerData();

    public static QuestManager Instance = null;

    List<UnityEvent> m_Events = new List<UnityEvent>();

    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateEvents();
        UpdateData();
    }

    public UnityEvent this[QuestEventType type]
    {
        get => m_Events[(int)type];
        //set => m_Events[(int)type] = value;
    }

    private void Update()
    {
        RefreshQuest();
    }

    void UpdateEvents()
    {
        int length = Enum.GetValues(typeof(QuestEventType)).Length;
        for (int i = m_Events.Count; i < length; i++) m_Events.Add(new UnityEvent());
    }

    void UpdateData()
    {
        while (Data.QuestDatas.Count < 3)
        {
            QuestData questData = CreateQuestData();
            Data.QuestDatas.Add(questData);
        }

        Data.QuestDatas.ForEach(e => e.Subscribe());
    }

    public void RefreshQuest()
    {
        if (Data.LastRefreshTime.Date + TimeSpan.FromDays(1) + TimeSpan.FromHours(10) < TimeManager.Instance.RealNow)
        {
            for (int i = 0; i < Data.QuestDatas.Count; i++)
            {
                if (Data.QuestDatas[i].Progress == 0)
                {
                    Data.QuestDatas[i].Subscribe(false);
                    QuestData questData = CreateQuestData();
                    questData.Subscribe(true);
                    Data.QuestDatas[i] = questData;
                }
            }

            ResetTime(TimeManager.Instance.RealNow);
        }
    }

    public TimeSpan GetRemaingTime()
    {
        return Data.LastRefreshTime.Date + TimeSpan.FromDays(1) + TimeSpan.FromHours(10) - TimeManager.Instance.RealNow;
    }

    public void ComepleteData()
    {
        for (int i = 0; i < Data.QuestDatas.Count; i++)
        {
            if (Data.QuestDatas[i].IsCompleted)
            {
                var rewards = Data.QuestDatas[i].Rewards;
                foreach (var rewardType in rewards.Keys)
                {
                    Reward.Data[rewardType] += rewards[rewardType];
                }

                Data.QuestDatas[i].Subscribe(false);
                QuestData questData = CreateQuestData();
                questData.Subscribe(true);
                Data.QuestDatas[i] = questData;
            }
        }
    }

    QuestData CreateQuestData()
    {
        var configs = ConfigsAsset.GetConfigList<QuestConfig>();
        int typeID = UnityEngine.Random.Range(1, Enum.GetValues(typeof(QuestType)).Length);
        var config = configs.Find(e => e.TypeID == typeID);
        //float levelStep = (config.ExtraProgressEndLevel - config.ExtraProgressStartLevel) / (float)config.ExtraProgress;
        int step = (MapManager.Instance.Data.CompleteLevel - config.ExtraProgressStartLevel) / config.StepLevel + 1;
        step = Mathf.Clamp(step, 1, config.ExtraProgress);

        //float extraProgress =  Mathf.Clamp(MapManager.Instance.Data.CompleteLevel, 0, config.ExtraProgressEndLevel) / levelStep;
        int minProgress = 2 + (step - 1) * config.ExtraProgress;
        int maxProgress = 2 + step * config.ExtraProgress;
        int progress = UnityEngine.Random.Range(minProgress, maxProgress);
        return new QuestData((QuestType)typeID, progress, 0);
    }

    public List<QuestData> GetDatasToShow()
    {
        return Data.QuestDatas.FindAll(e => e.ShownProgress < e.Progress && e.Progress > 0);
    }

    public void RefreshTime(DateTime now, TimeSource source, TimeAuthenticity timeAuthenticity)
    {
        if (timeAuthenticity != TimeAuthenticity.Unauthentic)
        {
            bool clamp = now < Data.LastRefreshTime;
            if (clamp)
            {
                TimeDebugText.Log("Clamped Quest. Now: " + now + " Last: " + Data.LastRefreshTime);
                ResetTime(now - TimeSpan.FromDays(1));
            }
        }
        else ResetTime(now);
    }

    public void ResetTime(DateTime now)
    {
        Data.LastRefreshTime = now.Date + TimeSpan.FromHours(10);
    }
}

[Serializable]
public class QuestManagerData
{
    public DateTime LastRefreshTime = new DateTime();
    public List<QuestData> QuestDatas = new List<QuestData>();
}

public enum QuestEventType
{
    None,
    OnWin,
    OnWinFirst,
    OnWinBoost2,
    OnWinBoost4,
    OnLose,
    OnCollectDeckCard,
    OnCollectStar,
    OnCollectStreakBonus
}

public enum QuestType
{
    None,
    WinLevel,
    WinLevelFirst,
    WinLevelRow,
    WinLevelBoost2,
    WinLevelBoost4,
    CollectDeckCard,
    CollectStar,
    CollectStreakBonus,
}

public static class QuestExtensions
{
    public static void MultiInvoke(this UnityEvent unityEvent, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            unityEvent.Invoke();
        }
    }
    public static Sprite ToIcon(this QuestType type)
    {
        var icons = Resources.LoadAll<Sprite>("Sprites/QuestIconAtlas");
        Sprite sprite = Array.Find(icons, e => e.name == type.ToString());
        return sprite;
    }

    public static Sprite ToQuestIcon(this string type)
    {
        var icons = Resources.LoadAll<Sprite>("Sprites/QuestIconAtlas");
        Sprite sprite = Array.Find(icons, e => e.name == type);
        return sprite;
    }

    public static void SubListener(this UnityEvent unityEvent, bool add, UnityAction unityAction)
    {
        if (add)
            unityEvent.AddListener(unityAction);
        else
            unityEvent.RemoveListener(unityAction);
    }

}
