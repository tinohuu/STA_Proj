using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyGoodiesManager : MonoBehaviour, ITimeRefreshable
{
    [Header("Setting")]
    [SerializeField] bool m_Enable = true;
    [SerializeField] bool m_ForceOpen = false;
    [SerializeField] int m_MaxWeeks = 4;
    [SerializeField] int m_MaxVersions = 2;

    [Header("Ref")]
    [SerializeField] GameObject m_ViewPrefab;

    [Header("Data")]
    [SerializeField] bool m_IsSuspended = false;

    public DailyGoodiesManagerData Data = new DailyGoodiesManagerData();
    public Dictionary<int, DailyGoodiesConfig> ConfigsByDay;
    public static DailyGoodiesManager Instance = null;

    GameObject m_CurView = null;
    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateConfig();
        Data = SaveManager.Instance.Bind(InitializeData());
    }

    DailyGoodiesManagerData InitializeData()
    {
        var data = Data;
        data.LastGoodiesTime = TimeManager.Instance.RealNow.Date;
        return data;
    }

    void Start()
    {
        bool canOpen = !m_CurView && m_Enable && (m_ForceOpen || TimeManager.Instance.RealNow.Date > Data.LastGoodiesTime.Date && !m_IsSuspended && !TimeManager.Instance.IsGettingTime);
        if (canOpen) // for local time specific + Data.CheckedSystemOffset.ToTimeSpan();
        {
            TimeDebugText.Log("Last Goody Time: " + Data.LastGoodiesTime.Date.ToString());
            m_CurView = Instantiate(m_ViewPrefab, MapManager.Instance.UICanvas.transform);
            //view = WindowManager.Instance.OpenView(viewPrefab);
        }
    }

    public RewardConfig GetGoodyConfig(int day)
    {
        RewardConfig[] rewardConfigs = ConfigsAsset.GetConfigArray<RewardConfig>();
        Dictionary<int, RewardConfig> rewardsById = rewardConfigs.ToDictionary(p => p.RewardID);
        DailyGoodiesConfig goodyCfg = ConfigsByDay[day];
        RewardConfig rewardCfg = rewardsById[goodyCfg.Goodies];
        return rewardCfg;
    }

    public void CollectGoodyReward(Dictionary<RewardType, int> rewards)
    {
        foreach (RewardType type in rewards.Keys)
        {
            Reward.Data[type] += rewards[type];
        }
        ResetTime(TimeManager.Instance.RealNow.Date);
    }

    public int GetCoin(int streakDays)
    {
        var publicConfigList = ConfigsAsset.GetConfigList<PublicConfig>();
        PublicConfig publicConfig = publicConfigList[0];
        var rawNumbers = publicConfig.DailyCoins.Split('_');
        int[] baseNumbers = new int[rawNumbers.Length]; // "DailyCoins": "2500_50_500_-500_1000",
        for (int i = 0; i < rawNumbers.Length; i++) baseNumbers[i] = int.Parse(rawNumbers[i]);

        rawNumbers = publicConfig.DailyCoinsCoefficient.Split('_');
        float[] mulNumbers = new float[rawNumbers.Length]; // "DailyCoinsCoefficient": "1.4_2"
        for (int i = 0; i < rawNumbers.Length; i++) mulNumbers[i] = float.Parse(rawNumbers[i]);

        float coin = baseNumbers[0];
        coin += MapManager.Instance.Data.CompleteLevel / baseNumbers[1] * baseNumbers[2];
        coin += UnityEngine.Random.Range(baseNumbers[3], baseNumbers[4]);
        coin *= streakDays == 28 ? mulNumbers[1] : (streakDays / 7 == 0 ? mulNumbers[0] : 1);
        coin = Mathf.FloorToInt(coin / 500) * 500;
        return (int)coin;
    }

    public bool HasGoody => TimeManager.Instance.RealNow.Date > Data.LastGoodiesTime.Date;

    public int CheckDate(bool record)
    {
        TimeSpan offset = TimeManager.Instance.RealNow - Data.LastGoodiesTime;
        int curDay = 0;
        if (offset >= new TimeSpan(1, 0, 0, 0) && offset < new TimeSpan(2, 0, 0, 0))
        {
            curDay = Data.StreakDays + 1;
        }
        else
        {
            curDay = (Data.StreakDays / 7) * 7 + 1;
        }

        if (record)
        {
            Data.StreakDays = curDay % (m_MaxWeeks * 7);
            if (Data.StreakDays == 0)
            {
                Data.Version = (Data.Version + 1) % (m_MaxVersions + 1);
                UpdateConfig();
            }
        }

        return curDay;
    }

    void UpdateConfig()
    {
        DailyGoodiesConfig[] configs = ConfigsAsset.GetConfigArray<DailyGoodiesConfig>();
        DailyGoodiesConfig[] curVerConfigs = configs.Where(x => x.Version == Data.Version).ToArray();
        ConfigsByDay = curVerConfigs.ToDictionary(p => p.Day);
    }

    public void RefreshTime(DateTime now, TimeSource source, TimeAuthenticity timeAuthenticity)
    {
        if (timeAuthenticity == TimeAuthenticity.Authentic)
        {
            if (now.Date < Data.LastGoodiesTime)
            {
                // Clamp
                Debug.Log("Clamp goodies");
                Data.LastGoodiesTime = now.Date;
            }
            if (source == TimeSource.Internet) m_IsSuspended = false;
        }
        else if (timeAuthenticity == TimeAuthenticity.Unauthentic)
        {
            Debug.Log("Punish goodies");
            if (source == TimeSource.Internet)
            {
                // Must be cheating
                ResetTime(now);
            }
            else if (source == TimeSource.System)
            {
                m_IsSuspended = true;
            }
        }
        else
        {
            //ResetTime(now);
        }
    }

    public void ResetTime(DateTime now)
    {
        TimeDebugText.Log("Reset goodies time");
        Data.LastGoodiesTime = now.Date;
    }
}

[Serializable]
public class DailyGoodiesManagerData
{
    public DateTime LastGoodiesTime = new DateTime();
    public int StreakDays = 0; // 1 is the first day
    public int Version = 1;
}

[Serializable]
public class DailyGoodiesConfig
{
    public int ID = 0;
    public int Day = 0;
    public int Week = 0;
    public int Goodies = 0;
    public int StageNum = 0;
    public int Version = 0;
}