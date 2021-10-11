using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LuckyWheelManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    [SerializeField] GameObject m_WheelViewPrefab;
    [SerializeField] GameObject m_WheelPrefab;
    [SerializeField] Transform m_WheelGroup;
    public bool EnableWheel = true;

    public static LuckyWheelManager Instance = null;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start()
    {
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));

        var wheel = GetAvailableWheel();
        if (wheel && EnableWheel)
        {
            var view = Instantiate(m_WheelViewPrefab, MapManager.Instance.UICanvas.transform).GetComponent<LuckyWheelView>();
            //var view = WindowManager.Instance.OpenView(m_WheelViewPrefab).GetComponent<LuckyWheelView>();
            view.SetWheel(wheel);
        }
    }

    public void ShowView(LuckyWheel wheel)
    {
        if (wheel && EnableWheel)
        {
            var view = Instantiate(m_WheelViewPrefab, MapManager.Instance.UICanvas.transform).GetComponent<LuckyWheelView>();
            //var view = WindowManager.Instance.OpenView(m_WheelViewPrefab).GetComponent<LuckyWheelView>();
            view.SetWheel(wheel);
        }
    }

    LuckyWheel GetAvailableWheel()
    {
        var wheels = m_WheelGroup.GetComponentsInChildren<LuckyWheel>();
        foreach (var wheel in wheels)
        {
            if (wheel.LevelID <= MapManager.Instance.Data.CompleteLevel && wheel.LevelID > MapManager.Instance.Data.WheelCollectedLevel)
            {
                return wheel;
            }
        }
        return null;
    }

    public static List<Dictionary<RewardType, int>> GetRewards(int wheelID)
    {
        var wheelConfigs = ConfigsAsset.GetConfigList<LuckyWheelConfig>().FindAll(e => e.WheelNum == wheelID);
        var rewardConfigs = ConfigsAsset.GetConfigList<RewardConfig>();

        var rewards = new List<Dictionary<RewardType, int>>();
        foreach (var config in wheelConfigs)
        {
            string rewardString = rewardConfigs.Find(e => e.RewardID == config.WheelReward).ItemReward;
            var reward = Reward.StringToReward(rewardString);
            rewards.Add(reward);
        }

        return rewards;
    }

    public static int Spin(int wheelID)
    {
        // This method returns slot INDEX

        // Get 8 slots configs of current wheel
        var slotConfigs = ConfigsAsset.GetConfigList<LuckyWheelConfig>().FindAll(e => e.WheelNum == wheelID);

        // Modify grand slots weights
        var grandSlotConfigs = slotConfigs.FindAll(e => e.IsGrand);
        if (grandSlotConfigs?.Count > 0)
        {
            MapManager.Instance.Data.WheelTimesSinceGrand++;
            var grandDrawConfig = ConfigsAsset.GetConfigList<LuckyWheelDrawConfig>();
            int grandDrawIndex = Mathf.Clamp(MapManager.Instance.Data.WheelTimesSinceGrand - 1, 0, grandDrawConfig.Count - 1);
            float grandWeight = grandDrawConfig[grandDrawIndex].GrandPrizeWeight;
            grandSlotConfigs.ForEach(e => e.Weight = grandWeight);
            TimeDebugText.Log("Cur Wheel Times: " + MapManager.Instance.Data.WheelTimesSinceGrand);
            TimeDebugText.Log("Cur Grand Weight: " + grandWeight);
        }

        // Randomnize
        var weights = slotConfigs.Select(e => e.Weight).ToArray();
        float weightSum = weights.Sum();
        float randomWeight = UnityEngine.Random.Range(0, weightSum);

        // Get
        float weightCount = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            weightCount += weights[i];
            if (randomWeight < weightCount)
            {
                if (slotConfigs[i].IsGrand) MapManager.Instance.Data.WheelTimesSinceGrand = 0;

                TimeDebugText.Log("Lucky Wheel Spin Result: " + (i + 1).ToString());
                return i;
            }
        }
        throw new Exception();
    }

    public void ParentInGroup(LuckyWheel luckyWheel)
    {
        luckyWheel.transform.SetParent(m_WheelGroup);
    }

    #region Mapmaker
    public string[] Mapmaker_InputInfos => new string[] { "Level ID", "Wheel ID" };

    public Type Mapmaker_ItemType => typeof(LuckyWheel);

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        m_WheelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_WheelConfig>(json);
        foreach (var config in configs)
        {
            if (config.LevelID > MapManager.Instance.Data.WheelCollectedLevel)
            {
                var luckyWheel = Instantiate(m_WheelPrefab, m_WheelGroup).GetComponent<LuckyWheel>();
                luckyWheel.transform.localPosition = config.LocPos;
                luckyWheel.WheelID = config.WheelID;
                luckyWheel.LevelID = config.LevelID;
            }
        }
    }
    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(m_WheelPrefab, m_WheelGroup);
        obj.transform.localPosition = m_WheelGroup.InverseTransformPoint(Vector3.zero);
        return obj.transform;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputs)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        wheel.LevelID = int.Parse(inputs[0]);
        wheel.WheelID = int.Parse(inputs[1]);
    }

    public string Mapmaker_ToConfig()
    {
        var wheels = m_WheelGroup.GetComponentsInChildren<LuckyWheel>();
        var configs = new List<Mapmaker_WheelConfig>();
        foreach (var wheel in wheels)
        {
            var config = new Mapmaker_WheelConfig();
            config.LevelID = wheel.LevelID;
            config.WheelID = wheel.WheelID;
            config.LocPos = wheel.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        var inputDatas = new string[] { wheel.LevelID.ToString(), wheel.WheelID.ToString()};
        return inputDatas;
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        Destroy(target);
    }
    #endregion
}

namespace STA.Mapmaker
{
    [Serializable]
    public class Mapmaker_WheelConfig : Mapmaker_BaseConfig
    {
        public int LevelID = 0;
        public int WheelID = 0;
    }
}
