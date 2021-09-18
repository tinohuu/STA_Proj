using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LuckyWheelManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    [SerializeField] GameObject wheelPrefab;
    [SerializeField] Transform wheelGroup;

    public static LuckyWheelManager Instance = null;

    private void Awake()
    {
        if (!Instance) Instance = this;

    }

    private void Start()
    {
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Spin(1);
        }
    }

    public List<Dictionary<RewardType, int>> GetRewards(int wheelID)
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

    public int Spin(int wheelID)
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

    #region Mapmaker
    public string[] Mapmaker_InputInfos => new string[] { "Level ID", "Wheel ID" };

    public Type Mapmaker_ItemType => typeof(LuckyWheel);

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        wheelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_WheelConfig>(json);
        foreach (var config in configs)
        {
            var luckyWheel = Instantiate(wheelPrefab, wheelGroup).GetComponent<LuckyWheel>();
            luckyWheel.transform.localPosition = config.LocPos;
        }
    }
    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(wheelPrefab, wheelGroup);
        obj.transform.localPosition = wheelGroup.InverseTransformPoint(Vector3.zero);
        return obj.transform;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputs)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        wheel.LevelId = int.Parse(inputs[0]);
        wheel.WheelId = int.Parse(inputs[1]);
    }

    public string Mapmaker_ToConfig()
    {
        var wheels = wheelGroup.GetComponentsInChildren<LuckyWheel>();
        var configs = new List<Mapmaker_WheelConfig>();
        foreach (var wheel in wheels)
        {
            var config = new Mapmaker_WheelConfig();
            config.LevelID = wheel.LevelId;
            config.WheelID = wheel.WheelId;
            config.LocPos = wheel.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        var inputDatas = new string[] { wheel.LevelId.ToString(), wheel.WheelId.ToString()};
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
