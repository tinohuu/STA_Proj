using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
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

    void UpdateEvents()
    {
        int length = Enum.GetValues(typeof(QuestEventType)).Length;
        for (int i = m_Events.Count; i < length; i++) m_Events.Add(new UnityEvent());
    }

    void UpdateData()
    {
        var configs = ConfigsAsset.GetConfigList<QuestConfig>();
        while (Data.QuestDatas.Count < 3)
        {
            int typeID = UnityEngine.Random.Range(1, Enum.GetValues(typeof(QuestType)).Length);
            var config = configs.Find(e => e.TypeID == typeID);
            float levelStep = (config.ExtraProgressEndLevel - config.ExtraProgressStartLevel) / (float)config.ExtraProgress;
            float extraProgress = Mathf.Clamp(MapManager.Instance.Data.CompleteLevel, 0, config.ExtraProgressEndLevel) / levelStep;
            int progress = 2 + (int)extraProgress;
            QuestData questData = new QuestData((QuestType)typeID, progress, 0);
            questData.Subscribe(true);
            Data.QuestDatas.Add(questData);
        }
    }
}

[Serializable]
public class QuestManagerData
{
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
