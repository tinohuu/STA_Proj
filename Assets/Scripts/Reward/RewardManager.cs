using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [SavedData] public static RewardManagerData Data = new RewardManagerData();
    public static RewardManager Instance = null;
    public delegate void RewardHandler(bool add);
    public RewardHandler[] OnValueChanged = new RewardHandler[Data.Rewards.Count];

    private void Awake()
    {
        if (!Instance) Instance = this;
        for (int i = 0; i < OnValueChanged.Length; i++) OnValueChanged[i] = null;
    }
}

public static class Reward
{
    public static RewardManagerData Data = RewardManager.Data;

    public static int Coin { get => Data[RewardType.Coin]; set => Data[RewardType.Coin] = value; }

    public static void Get(RewardType rewardType, int count = 1)
    {
        Data[rewardType] += count;
    }
    public static void Get(RewardType[] rewardTypes, int[] counts)
    {
        for (int i = 0; i < rewardTypes.Length; i++)
            Data[rewardTypes[i]] += counts[i];
    }
    public static void Get(List<RewardType> rewardTypes, List<int> counts)
    {
        for (int i = 0; i < rewardTypes.Count; i++)
            Data[rewardTypes[i]] += counts[i];
    }

    public static void Use(RewardType rewardType, int count = 1)
    {
        Data[rewardType] -= count;
    }
    public static void Use(RewardType[] rewardTypes, int[] counts)
    {
        for (int i = 0; i < rewardTypes.Length; i++)
            Data[rewardTypes[i]] -= counts[i];
    }
    public static void Use(List<RewardType> rewardTypes, List<int> counts)
    {
        for (int i = 0; i < rewardTypes.Count; i++)
            Data[rewardTypes[i]] -= counts[i];
    }

    public static Dictionary<RewardType, int> StringToReward(string itemString)
    {
        string[] raws = itemString.Split('_');
        Dictionary<RewardType, int> itemsByType = new Dictionary<RewardType, int>();
        for (int i = 0; i < raws.Length; i += 2)
        {
            if (i + 1 >= raws.Length) break;
            RewardType type = (RewardType)int.Parse(raws[i]);
            int count = int.Parse(raws[i + 1]);
            itemsByType.Add(type, count);
        }
        return itemsByType;
    }
}

[System.Serializable]
public class RewardManagerData
{
    public List<int> Rewards = new List<int>();

    public RewardManagerData()
    {
        int length = Enum.GetValues(typeof(RewardType)).Length;
        for (int i = 0; i < length; i++) Rewards.Add(0);
    }
    public RewardManagerData(List<int> rewards)
    {
        int length = Enum.GetValues(typeof(RewardType)).Length;
        for (int i = 0; i < rewards.Count; i++) Rewards.Add(rewards[i]);
    }

    public void UpdateData()
    {
        int length = Enum.GetValues(typeof(RewardType)).Length;
        if (length > Rewards.Count)
        {
            for (int i = Rewards.Count; i < length; i++)
            {
                Rewards.Add(0);
            }
        }
    }

    public int this[RewardType type]
    {
        get => Rewards[(int)type];
        set
        {
            Rewards[(int)type] = Mathf.Clamp(value, 0, int.MaxValue);
            RewardManager.Instance.OnValueChanged[(int)type]?.Invoke(value >= Rewards[(int)type]);
        }
    }
}

[System.Serializable]
public enum RewardType
{
    None,
    Coin,
    Undo,
    WildCard,
    MoreCards,
    FreeRound,
    SilverFreeRound,
    GoldenFreeRound,
    RemoveCards,
    ClearPlayables,
    WildDrop,
    RemoveValueChangers,
    RemoveCodeBreakers,
    RemoveBombs,
    DoubleCredits,
    Rocket,
    Clock
}