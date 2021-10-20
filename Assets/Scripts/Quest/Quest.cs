using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Quest
{
    public QuestData Data = null;
    public int ShownProgress = 0;

    public void Subscribe()
    {
        switch (Data.Type)
        {
            case QuestType.WinLevel:
                QuestManager.Instance[QuestEventType.OnWin].AddListener(() => Data.Progress++);
                break;
            case QuestType.WinLevelFirst:
                QuestManager.Instance[QuestEventType.OnWinFirst].AddListener(() => Data.Progress++);
                break;
            case QuestType.WinLevelRow:
                QuestManager.Instance[QuestEventType.OnWin].AddListener(() => Data.Progress++);
                QuestManager.Instance[QuestEventType.OnLose].AddListener(() => Data.Progress = 0);
                break;
            case QuestType.WinLevelBoost:
                QuestManager.Instance[QuestEventType.OnWinBoost].AddListener(() => Data.Progress++);
                break;
            case QuestType.CollectDeckCards:
                QuestManager.Instance[QuestEventType.OnCollectDeckCard].AddListener(() => Data.Progress++);
                break;
            case QuestType.CollectStars:
                QuestManager.Instance[QuestEventType.OnCollectStar].AddListener(() => Data.Progress++);
                break;
        }
    }
}

public enum QuestType
{
    None,
    WinLevel,
    WinLevelFirst,
    WinLevelRow,
    WinLevelBoost,
    CollectDeckCards,
    CollectStars
}

[Serializable]
public class QuestData
{
    public QuestType Type = QuestType.None;
    public int MaxProgress = 0;
    public int Reward = 0;

    public int Progress { get => m_Progress; set => Mathf.Clamp(m_Progress, 0, MaxProgress); }
    public bool IsCompleted => m_Progress == MaxProgress;

    int m_Progress = 0;

    public QuestData(QuestType type, int maxProgress, int reward)
    {
        Type = type;
        MaxProgress = maxProgress;
        Reward = reward;
    }
}

