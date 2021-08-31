using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("Debug")]
    public MapManagerData Data = new MapManagerData();
    public static MapManager Instance = null;
    public List<PublicConfig> PublicConfigs;
    public Text Text;
    GameObject obj;
    private void Awake()
    {
        if (!Instance) Instance = this; // Singleton

        Data = SaveManager.Bind(InitializeData());  // Bind to save
    }

    MapManagerData InitializeData()
    {
        MapManagerData n_data = Data;
        for (int i = 0; i < 2; i++)
        {
            int startingNumber = n_data.MapDatas.Count == 0 ? 1 : n_data.MapDatas.Last().MapLevelDatas.Last().Order + 1;
            MapData mapData = new MapData(i + 1, startingNumber, 200);
            n_data.MapDatas.Add(mapData);
        }
        return n_data;
    }

    public MapData LevelToMapData(int level)
    {
        for (int i = 0; i < Data.MapDatas.Count - 1; i++)
        {
            if (level >= Data.MapDatas[i].StartAt && level < Data.MapDatas[i + 1].StartAt)
            {
                return Data.MapDatas[i];
            }
        }
        return Data.MapDatas.Last();
    }
}


[Serializable]
public class PublicConfig
{
    public int HarvestTime = 123;
}

[System.Serializable]
public class MapManagerData
{
    public int CompelteLevel = 0;
    public int SelectedLevel = 0;
    public DateTime LastHarvestTime = new DateTime();
    public List<MapData> MapDatas = new List<MapData>();
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
    public int Order = 1;
    public int Rating = 0;
    public bool IsComplete => Rating != 0;

    public MapLevelData(int order)
    {
        Order = order;
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