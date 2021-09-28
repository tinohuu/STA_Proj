using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    public MapManagerData Data = new MapManagerData();
    public static MapDataManager Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
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

    public static MapLevelData GetLevelData(int levelID)
    {
        if (levelID <= Instance.Data.MapLevelDatas.Count)
            return Instance.Data.MapLevelDatas[levelID - 1];
        return null;
    }

    public static int GetLevelRating(int levelID)
    {
        if (levelID <= Instance.Data.MapLevelDatas.Count)
            return Instance.Data.MapLevelDatas[levelID - 1].Rating;
        return 0;
    }

    public static void SetLevelRating(int levelID, int rating)
    {
        if (levelID <= Instance.Data.MapLevelDatas.Count)
            Instance.Data.MapLevelDatas[levelID - 1].Rating = rating;

        if (levelID > Instance.Data.CompleteLevel) Instance.Data.CompleteLevel = levelID;
    }
}

[System.Serializable]
public class MapManagerData
{
    public int CompleteLevel = 0;
    public int SelectedLevel = 1;
    public DateTime LastHarvestTime = new DateTime();
    public int WheelCollectedLevel = 0;
    public int WheelTimesSinceGrand = 0;
    public List<MapLevelData> MapLevelDatas = new List<MapLevelData>();
}

[System.Serializable]
public class MapLevelData
{
    public int ID = 1;
    public int Rating = 0;
    public bool IsComplete => Rating != 0;

    public MapLevelData(int id)
    {
        ID = id;
    }
}

