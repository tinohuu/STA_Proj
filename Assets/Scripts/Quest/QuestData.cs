using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class QuestData
{
    public int ShownProgress = 0;

    QuestType m_Type = QuestType.None;
    int m_MaxProgress = 0;
    int m_Progress = 0;
    List<RewardType> m_rewardTypes = new List<RewardType>();
    List<int> m_rewardCounts = new List<int>();

    // Properties
    public QuestType Type => m_Type;
    public int MaxProgress => m_MaxProgress;
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

    public QuestData(QuestType type, int maxProgress, float rewardValue)
    {
        m_Type = type;
        m_MaxProgress = maxProgress;
        m_rewardTypes = new List<RewardType>() { RewardType.Coin };
        m_rewardCounts = new List<int>() { 500 };
    }

    Dictionary<RewardType, int> ValueToRewards(float rewardValue)
    {
        var rewards = new Dictionary<RewardType, int>();

        if (rewardValue < 2)
        {
            rewards.Add(RewardType.Coin, 500);
        }
        else
        {

        }


        return rewards;
    }

    public void Subscribe(bool add = true)
    {
        switch (m_Type)
        {
            case QuestType.WinLevel:
                QuestManager.Instance[QuestEventType.OnWin].SubListener(add, () =>Progress++);
                break;
            case QuestType.WinLevelFirst:
                QuestManager.Instance[QuestEventType.OnWinFirst].SubListener(add, () => Progress++);
                break;
            case QuestType.WinLevelRow:
                QuestManager.Instance[QuestEventType.OnWin].SubListener(add, () => Progress++);
                QuestManager.Instance[QuestEventType.OnLose].SubListener(add, () => Progress = 0);
                break;
            case QuestType.WinLevelBoost2:
                QuestManager.Instance[QuestEventType.OnWinBoost2].SubListener(add, () => Progress++);
                break;
            case QuestType.WinLevelBoost4:
                QuestManager.Instance[QuestEventType.OnWinBoost4].SubListener(add, () => Progress++);
                break;
            case QuestType.CollectDeckCard:
                QuestManager.Instance[QuestEventType.OnCollectDeckCard].SubListener(add, () => Progress++);
                break;
            case QuestType.CollectStar:
                QuestManager.Instance[QuestEventType.OnCollectStar].SubListener(add, () => Progress++);
                break;
            case QuestType.CollectStreakBonus:
                QuestManager.Instance[QuestEventType.OnCollectStreakBonus].SubListener(add, () => Progress++);
                break;
        }
    }
}


