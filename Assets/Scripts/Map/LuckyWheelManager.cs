using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
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
