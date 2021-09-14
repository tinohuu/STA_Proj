using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using STA.MapMaker;

public class Map : MonoBehaviour, IMapMakerModule
{
    [Header("Ref")]
    public GameObject LevelButtonPrefab;
    public Transform LevelPointsGroup;
    public Transform LevelsGroup;
    public GameObject MapMakerPrefab;
    public ScrollRect MapScrollView;
    [Header("Config")]
    public bool ShowPath = true;
    [Header("Debug")]
    [SerializeField] List<MapLevel> mapLevels = new List<MapLevel>();
    public static Map Instance = null;

    private void Awake()
    {
        if (!Instance) Instance = this;

        CreateItems();
    }

    void Start()
    {
        MapPlayer.Instance.MoveToLevel(mapLevels[MapManager.Instance.Data.SelectedLevel - 1], false);
    }

    private void OnDrawGizmos()
    {
        if (ShowPath && LevelPointsGroup && LevelPointsGroup.childCount > 0)
        {
            for (int i = 0; i < LevelPointsGroup.childCount; i++)
            {
                Transform child = LevelPointsGroup.GetChild(i);
                Gizmos.color = Color.blue;
                //Gizmos.DrawSphere(child.position, 0.2f);
                if (i > 0) Gizmos.DrawLine(child.position, LevelPointsGroup.GetChild(i - 1).position);
            }
        }
    }

    public void CreateItems()
    {
       MapManager.Instance.UpdateLevelData();

        var datas = MapManager.MapMakerConfig.GetCurMapData().LevelDatas;

        LevelsGroup.DestroyChildren();
        mapLevels.Clear();

        int startingIndex = MapManager.MapMakerConfig.GetLevelCount(MapManager.Instance.CurMapNumber - 1);
        Debug.Log(datas.Count);
        for (int i = 0; i < datas.Count; i++)
        {
            MapLevel mapLevel = Instantiate(LevelButtonPrefab, LevelsGroup).GetComponent<MapLevel>();
            mapLevel.transform.localPosition = new Vector2(datas[i].PosX, datas[i].PosY);
            mapLevel.Data = MapManager.Instance.Data.MapLevelDatas[i + startingIndex];
            mapLevels.Add(mapLevel);
        }
    }

    public void AddNewItem()
    {
        Vector2 locPos = Map.Instance.LevelsGroup.InverseTransformPoint(Vector3.zero);
        var levelDatas = MapManager.MapMakerConfig.GetCurMapData().LevelDatas;
        levelDatas.Add(new MapMaker_LevelData(locPos.x, locPos.y));
        CreateItems();
    }

    public void ChangeLevelNumber(MapLevel mapLevel, int level)
    {
        mapLevels.Remove(mapLevel);
        int starting = MapManager.MapMakerConfig.LevelToStarting(level);
        mapLevels.Insert(level - starting, mapLevel);
        //RecordMapMakerData();
        //CreateItems();
    }

    public void SetProgress(float ratio)
    {
        MapManager.Instance.Data.CompleteLevel = Mathf.Clamp((int)(ratio * 186), 1, int.MaxValue);
        CropManager.Instance.UpdateCropsView();
        CreateItems();
        //foreach (MapLevel lvl in FindObjectsOfType<MapLevel>()) lvl.UpdateView();
    }

    public MapLevel GetMapLevelButton(int level)
    {
        int firstLevelNumber = MapManager.MapMakerConfig.LevelToStarting(level);
        int index = level - firstLevelNumber;
        if (index < 0 || index >= mapLevels.Count) return null;
        else return mapLevels[index];
    }

    public void RecordItemMakerData()
    {
        if (LevelsGroup.childCount == 0) return;

        List<MapMaker_LevelData> datas = new List<MapMaker_LevelData>();
        foreach (MapLevel level in mapLevels)
        {
            if (!level) continue;
            MapMaker_LevelData data = new MapMaker_LevelData(level.transform.localPosition.x, level.transform.localPosition.y);
            datas.Add(data);
        }
        MapManager.MapMakerConfig.GetCurMapData().LevelDatas = datas;
    }
}

namespace STA.MapMaker
{
    [System.Serializable]
    public class MapMaker_LevelData
    {
        public float PosX = 0;
        public float PosY = 0;

        public MapMaker_LevelData(float posX, float posY)
        {
            PosX = posX;
            PosY = posY;
        }
    }
}
