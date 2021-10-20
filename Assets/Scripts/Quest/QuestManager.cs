using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public QuestManagerData Data = new QuestManagerData();

    public static QuestManager Instance = null;

    List<UnityEvent> m_Events = new List<UnityEvent>();
    List<Quest> m_Quests = new List<Quest>();

    private void Awake()
    {
        if (!Instance) Instance = this;
        CheckEvents();
    }

    public UnityEvent this[QuestEventType type]
    {
        get => m_Events[(int)type];
        //set => m_Events[(int)type] = value;
    }

    void CheckEvents()
    {
        int length = Enum.GetValues(typeof(QuestEventType)).Length;
        for (int i = m_Events.Count; i < length; i++) m_Events.Add(new UnityEvent());
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
    OnWinBoost,
    OnLose,
    OnCollectDeckCard,
    OnCollectStar,
    OnCompleteStreak
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
}
