using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManagerData Data = new RewardManagerData();
    public static RewardManager Instance = null;
    private void Awake()
    {
        Instance = this;
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
        set => Rewards[(int)type] = Mathf.Clamp(value, 0, int.MaxValue);
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
