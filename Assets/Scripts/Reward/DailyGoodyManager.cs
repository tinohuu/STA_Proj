using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyGoodyManager : MonoBehaviour
{
    [SerializeField] int m_MaxWeeks = 4;
    [SerializeField] int m_MaxVersions = 2;

    public DailyGoodyData Data = new DailyGoodyData();
    public static DailyGoodyManager Instance = null;
    public Dictionary<int, DailyGoodiesConfig> ConfigsByDay;
    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateConfig();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public RewardConfig GetGoodyConfig(int day)
    {
        RewardConfig[] rewardConfigs = ConfigsAsset.GetConfigArray<RewardConfig>();
        Dictionary<int, RewardConfig> rewardsById = rewardConfigs.ToDictionary(p => p.RewardID);
        DailyGoodiesConfig goodyCfg = ConfigsByDay[day];
        RewardConfig rewardCfg = rewardsById[goodyCfg.Goodies];
        return rewardCfg;
    }

    public void CollectGoodyReward(RewardConfig config)
    {
        Reward.Coin += config.CoinReward;
        var rewards = Reward.StringToReward(config.ItemReward);
        foreach (RewardType type in rewards.Keys)
        {
            Reward.Data[type] += rewards[type];
        }
    }

    public bool HasGoody => TimeManager.Instance.RealNow.Date > Data.LastGoodyTime.Date;

    public int CheckDate(bool record)
    {
        TimeSpan offset = TimeManager.Instance.RealNow - Data.LastGoodyTime;
        int curDay = 0;
        if (offset >= new TimeSpan(1, 0, 0, 0) && offset < new TimeSpan(2, 0, 0, 0))
        {
            curDay = Data.StreakDays + 1;
        }
        else curDay = (curDay / 7) * 7 + 1;

        if (record)
        {
            Data.StreakDays = curDay % (m_MaxVersions * 7);
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
}

[Serializable]
public class DailyGoodyData
{
    public DateTime LastGoodyTime = new DateTime();
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