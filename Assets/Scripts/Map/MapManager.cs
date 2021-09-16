using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using STA.MapMaker;

public class MapManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] GameObject mapMakerPrefab;

    [Header("Debug")]
    public MapManagerData Data = new MapManagerData();

    public List<StageConfig> StageConfigs;
    public Dictionary<int, FunctionConfig> FunctionConfigsByFuncID;

    public static MapManager Instance = null;

    GameObject mapMaker;

    private void Awake()
    {
        if (!Instance) Instance = this; // Singleton

        StageConfigs = ConfigsAsset.GetConfigList<StageConfig>();


        FunctionConfigsByFuncID = ConfigsAsset.GetConfigList<FunctionConfig>().ToDictionary(p => p.FunctionID);

        UpdateLevelData();
    }

    public void UpdateLevelData()
    {
        var configs = ConfigsAsset.GetConfigList<StageConfig>();
        int maxLevelCount = configs.Count;
        if (Data.MapLevelDatas.Count < maxLevelCount)
        {
            for (int i = Data.MapLevelDatas.Count; i < maxLevelCount; i++)
            {
                Data.MapLevelDatas.Add(new MapLevelData(i + 1));
            }
        }
    }

    private void Update()
    {
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.M))
        {
            if (mapMaker)
            {
                //MapMaker.Instance.UpdateMode();
                Destroy(mapMaker.gameObject);
            }
            else mapMaker = Instantiate(mapMakerPrefab, transform);
        }
    }

    
    /*MapManagerData InitializeData()
    {
        MapManagerData n_data = Data;
        for (int i = 0; i < 2; i++)
        {
            int startingNumber = n_data.MapDatas.Count == 0 ? 1 : n_data.MapDatas.Last().MapLevelDatas.Last().Order + 1;
            MapData mapData = new MapData(i + 1, startingNumber, 200);
            n_data.MapDatas.Add(mapData);
        }
        n_data.LastHarvestTime = TimeManager.Instance.RealNow;

        return n_data;
    }*/

    /*public MapData LevelToMapData(int level)
    {
        for (int i = 0; i < Data.MapDatas.Count - 1; i++)
        {
            if (level >= Data.MapDatas[i].StartAt && level < Data.MapDatas[i + 1].StartAt)
            {
                return Data.MapDatas[i];
            }
        }
        return Data.MapDatas.Last();
    }*/
}

[System.Serializable]
public class MapManagerData
{
    public int CompleteLevel = 0;
    public int SelectedLevel = 0;
    public DateTime LastHarvestTime = new DateTime();
    public List<MapLevelData> MapLevelDatas = new List<MapLevelData>();
    //public List<MapData> MapDatas = new List<MapData>();
}

[System.Serializable]
public class MapData
{
    public int Order = 1;
    public int StartAt = 1;
    public List<MapLevelData> MapLevelDatas = null;

    public MapData(int order, int startingNumber, int count)
    {
        Order = order;
        StartAt = startingNumber;

        List<MapLevelData> n_mapLevelDatas = new List<MapLevelData>();
        for (int i = 0; i < count; i++)
        {
            MapLevelData levelData = new MapLevelData(startingNumber + i);
            n_mapLevelDatas.Add(levelData);
        }
        MapLevelDatas = n_mapLevelDatas;
    }
}

[System.Serializable]
public class MapLevelData
{
    public int ID = 1;
    public int Rating = 0;
    public bool IsComplete => Rating != 0;

    public MapLevelData(int order)
    {
        ID = order;
    }
}

[System.Serializable]
public class CropConfig
{
    public int ID = 0;
    public string Name = "Crop";
    public int Level = 0;
    public int MinLevel = 0;
    public int CoinReward = 50;
    public int VipReward = 0;
}