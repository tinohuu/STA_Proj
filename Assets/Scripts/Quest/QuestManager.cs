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

    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateEvents();
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
}

[Serializable]
public class QuestManagerData
{
    public List<Quest> Quests = new List<Quest>();
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
