using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    [SavedData] public MapManagerData Data = new MapManagerData();
    public static MapDataManager Instance;
    public int RetriedLevel = 0;
    public int NewRating = 0;
    public int NewRatingLevel = 0;

    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateLevelData();
    }

    private void Update()
    {
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.P))
        {
            SetLevelRating(STAGameManager.Instance.nLevelID, 3);
            SetRetry(STAGameManager.Instance.nLevelID + 1);
        }
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

        //Data.MapLevelDatas.Find(e => e.ID == 85).Rating = 63;
    }

    public static MapLevelData GetLevelData(int levelID)
    {
        if (levelID <= Instance.Data.MapLevelDatas.Count)
            return Instance.Data.MapLevelDatas[levelID - 1];
        return null;
    }

    public static void SetRetry(int levelID)
    {
        Instance.RetriedLevel = levelID;
    }

    public static int GetLevelRating(int levelID)
    {
        if (levelID <= Instance.Data.MapLevelDatas.Count)
            return Instance.Data.MapLevelDatas[levelID - 1].Rating;
        return 0;
    }

    public static void SetLevelRating(int levelID, int rating)
    {
        Debug.Log("MapDataManager::SetLevelRating");
        if (levelID <= Instance.Data.MapLevelDatas.Count)
        {
            Instance.NewRating = rating - Instance.Data.MapLevelDatas[levelID - 1].Rating;
            Instance.Data.MapLevelDatas[levelID - 1].Rating = rating;
            Instance.NewRatingLevel = levelID;
        }

        if (levelID > Instance.Data.CompleteLevel) Instance.Data.CompleteLevel = levelID;
    }
}

[System.Serializable]
public class MapManagerData
{
    public int CompleteLevel = 0;
    public int SelectedLevel = 1;
    public DateTime LastHarvestTime => CropManager.Instance.Data.LastHarvestTime;

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

