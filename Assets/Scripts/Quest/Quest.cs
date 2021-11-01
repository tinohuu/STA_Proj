using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Quest
{
    public int ShownProgress = 0;

    QuestType m_Type = QuestType.None;
    int m_MaxProgress = 0;
    int m_Progress = 0;
    List<RewardType> m_rewardTypes = new List<RewardType>();
    List<int> m_rewardCounts = new List<int>();

    public int Progress { get => m_Progress; set => Mathf.Clamp(m_Progress, 0, m_MaxProgress); }
    public bool IsCompleted => m_Progress == m_MaxProgress;
    
    public Dictionary<RewardType, int> Rewards
    {
        get => m_rewardTypes.Zip(m_rewardCounts, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        set
        {
            m_rewardTypes = value.Keys.ToList();
            m_rewardCounts = value.Values.ToList();
        }
    }

    public Quest(QuestType type, int maxProgress, int rewardValue)
    {
        m_Type = type;
        m_MaxProgress = maxProgress;
    }

    public void Subscribe()
    {
        switch (m_Type)
        {
            case QuestType.WinLevel:
                QuestManager.Instance[QuestEventType.OnWin].AddListener(() =>Progress++);
                break;
            case QuestType.WinLevelFirst:
                QuestManager.Instance[QuestEventType.OnWinFirst].AddListener(() => Progress++);
                break;
            case QuestType.WinLevelRow:
                QuestManager.Instance[QuestEventType.OnWin].AddListener(() => Progress++);
                QuestManager.Instance[QuestEventType.OnLose].AddListener(() => Progress = 0);
                break;
            case QuestType.WinLevelBoost:
                QuestManager.Instance[QuestEventType.OnWinBoost].AddListener(() => Progress++);
                break;
            case QuestType.CollectDeckCards:
                QuestManager.Instance[QuestEventType.OnCollectDeckCard].AddListener(() => Progress++);
                break;
            case QuestType.CollectStars:
                QuestManager.Instance[QuestEventType.OnCollectStar].AddListener(() => Progress++);
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

