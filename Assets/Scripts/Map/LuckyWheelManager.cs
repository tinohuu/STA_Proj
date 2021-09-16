using STA.MapMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyWheelManager : MonoBehaviour, IMapMakerModule
{
    [SerializeField] GameObject wheelPrefab;
    [SerializeField] Transform wheelGroup;
    public static LuckyWheelManager Instance = null;

    public string[] MapMaker_InputInfos => new string[] { "Level ID", "Wheel ID" };
    public Type MapMaker_ItemType => typeof(LuckyWheel);

    private void Awake()
    {
        if (!Instance) Instance = this;
        MapMaker_CreateItems();
    }

    public void MapMaker_CreateItems()
    {
        string json = MapMaker.GetConfigData(this, 1);

        if (json == null) return;

        wheelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<MapMaker_WheelConfig>(json);
        foreach (var config in configs)
        {
            var luckyWheel = Instantiate(wheelPrefab, wheelGroup).GetComponent<LuckyWheel>();
            luckyWheel.transform.localPosition = config.LocPos;
        }
    }
    public void MapMaker_AddItem()
    {
        GameObject obj = Instantiate(wheelPrefab, wheelGroup);
        obj.transform.localPosition = wheelGroup.InverseTransformPoint(Vector3.zero);
    }

    public void MapMaker_ApplyInputs(Transform target, string[] inputs)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        wheel.LevelId = int.Parse(inputs[0]);
        wheel.WheelId = int.Parse(inputs[1]);
    }

    public string MapMaker_ToConfig()
    {
        var wheels = wheelGroup.GetComponentsInChildren<LuckyWheel>();
        var configs = new List<MapMaker_WheelConfig>();
        foreach (var wheel in wheels)
        {
            var config = new MapMaker_WheelConfig();
            config.LevelID = wheel.LevelId;
            config.WheelID = wheel.WheelId;
            config.LocPos = wheel.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public string[] MapMaker_UpdateInputs(Transform target)
    {
        var wheel = target.GetComponent<LuckyWheel>();
        var inputDatas = new string[] { wheel.LevelId.ToString(), wheel.WheelId.ToString()};
        return inputDatas;
    }

    [Serializable]
    public class MapMaker_WheelConfig : MapMaker_BaseConfig
    {
        public int LevelID = 0;
        public int WheelID = 0;
    }
}
