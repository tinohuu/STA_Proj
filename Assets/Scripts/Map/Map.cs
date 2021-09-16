using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using STA.MapMaker;
using System;

public class Map : MonoBehaviour, IMapMakerModule
{
    [Header("Ref")]
    public Transform LevelGroup;
    public GameObject LevelPrefab;


    public GameObject MapMakerPrefab;
    public ScrollRect MapScrollView;

    [Header("Config")]
    public bool ShowPath = true;

    [Header("Debug")]
    public List<StageConfig> CurMapStageConfigs;
    public int MapId = 1;
    //[SerializeField] List<MapLevel> mapLevels = new List<MapLevel>();
    public static Map Instance = null;

    public Type MapMaker_ItemType => typeof(MapLevel);
    public string[] MapMaker_InputInfos => new string[] { "Level ID" };

    private void Awake()
    {
        if (!Instance) Instance = this;

        CurMapStageConfigs = MapManager.Instance.StageConfigs.Where(e => e.ChapterNum == MapId).ToList();
        MapMaker_CreateItems();
    }

    void Start()
    {
        //MapPlayer.Instance.MoveToLevel(LevelGroup.GetChild(MapManager.Instance.Data.SelectedLevel - 1).GetComponent<MapLevel>(), false);
    }

    private void OnDrawGizmos()
    {
        /*if (ShowPath && LevelPointsGroup && LevelPointsGroup.childCount > 0)
        {
            for (int i = 0; i < LevelPointsGroup.childCount; i++)
            {
                Transform child = LevelPointsGroup.GetChild(i);
                Gizmos.color = Color.blue;
                //Gizmos.DrawSphere(child.position, 0.2f);
                if (i > 0) Gizmos.DrawLine(child.position, LevelPointsGroup.GetChild(i - 1).position);
            }
        }*/
    }

    public void MapMaker_CreateItems()
    {
        string json = MapMaker.GetConfigData(this, MapId);

        if (json == null) return;

        LevelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<MapMaker_BaseConfig>(json);
        foreach (var config in configs)
        {
            var level = Instantiate(LevelPrefab, LevelGroup).GetComponent<MapLevel>();
            level.transform.localPosition = config.LocPos;
            level.Data.ID = level.transform.GetSiblingIndex() + CurMapStageConfigs[0].LevelID;
        }
    }

    public void MapMaker_AddItem()
    {
        if (LevelGroup.childCount >= CurMapStageConfigs.Count)
        {
            MapMaker.Log("Level count can't be great than " + CurMapStageConfigs.Count);
            return;
        }

        var level = Instantiate(LevelPrefab, LevelGroup).GetComponent<MapLevel>();
        level.transform.localPosition = LevelGroup.InverseTransformPoint(Vector3.zero);
        level.Data.ID = level.transform.GetSiblingIndex() + CurMapStageConfigs[0].LevelID;
    }

    public string[] MapMaker_UpdateInputs(Transform target)
    {
        var level = target.GetComponent<MapLevel>();
        string[] inputs = new string[] { level.Data.ID.ToString() };
        return inputs;
    }

    public void MapMaker_ApplyInputs(Transform target, string[] inputDatas)
    {
        var level = target.GetComponent<MapLevel>();
        level.transform.SetSiblingIndex(int.Parse(inputDatas[0]) - CurMapStageConfigs[0].LevelID);
    }

    public string MapMaker_ToConfig()
    {
        var levels = LevelGroup.GetComponentsInChildren<MapLevel>();
        var configs = new List<MapMaker_BaseConfig>();
        foreach (var level in levels)
        {
            var config = new MapMaker_BaseConfig();
            config.LocPos = level.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }
    /*
public void MapMaker_AddItem()
{
Vector2 locPos = Map.Instance.LevelsGroup.InverseTransformPoint(Vector3.zero);
var levelDatas = MapManager.MapMakerConfig.CurMapData.LevelDatas;
levelDatas.Add(new MapMaker_LevelData(locPos.x, locPos.y));
MapMaker_CreateItems();
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
MapMaker_CreateItems();
//foreach (MapLevel lvl in FindObjectsOfType<MapLevel>()) lvl.UpdateView();
}

public MapLevel GetMapLevelButton(int level)
{
int firstLevelNumber = MapManager.MapMakerConfig.LevelToStarting(level);
int index = level - firstLevelNumber;
if (index < 0 || index >= mapLevels.Count) return null;
else return mapLevels[index];
}

public void MapMaker_RecordData()
{
if (LevelsGroup.childCount == 0) return;

List<MapMaker_LevelData> datas = new List<MapMaker_LevelData>();
foreach (MapLevel level in mapLevels)
{
  if (!level) continue;
  MapMaker_LevelData data = new MapMaker_LevelData(level.transform.localPosition.x, level.transform.localPosition.y);
  datas.Add(data);
}
MapManager.MapMakerConfig.CurMapData.LevelDatas = datas;
}*/
}
