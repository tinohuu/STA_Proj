using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using STA.Mapmaker;
using System;

public class MapLevelManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    [SerializeField] Transform LevelGroup;
    [SerializeField] GameObject LevelPrefab;

    [Header("Data")]

    public static MapLevelManager Instance = null;

    public Type Mapmaker_ItemType => typeof(MapLevel);
    public string[] Mapmaker_InputInfos => new string[] { "Level ID" };

    private void Awake()
    {
        if (!Instance) Instance = this;

        Mapmaker_CreateItems(Mapmaker.GetConfig(this));
    }

    void Start()
    {
        //if (l)
        //MapLevel level = LevelGroup.GetChild(MapManager.Instance.Data.SelectedLevel - 1).GetComponent<MapLevel>();
        //if (level) MapPlayer.Instance.MoveToLevel(level, false);

    }

    #region Mapmaker
    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        LevelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_BaseConfig>(json);
        for (int i = 0; i < configs.Count; i++)
        {
            var level = Instantiate(LevelPrefab, LevelGroup).GetComponent<MapLevel>();
            level.transform.localPosition = configs[i].LocPos;
            level.Data = MapManager.Instance.Data.MapLevelDatas[i + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];
        }
    }

    public Transform Mapmaker_AddItem()
    {
        if (LevelGroup.childCount >= MapManager.Instance.CurMapStageConfigs.Count)
        {
            Mapmaker.Log("Level count can't be great than " + MapManager.Instance.CurMapStageConfigs.Count);
            return null;
        }

        var level = Instantiate(LevelPrefab, LevelGroup).GetComponent<MapLevel>();
        level.transform.localPosition = LevelGroup.InverseTransformPoint(Vector3.zero);
        level.Data = MapManager.Instance.Data.MapLevelDatas[level.transform.GetSiblingIndex() + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];

        return level.transform;
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var level = target.GetComponent<MapLevel>();
        string[] inputs = new string[] { level.Data.ID.ToString() };
        return inputs;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputDatas)
    {
        var level = target.GetComponent<MapLevel>();
        level.transform.SetSiblingIndex(int.Parse(inputDatas[0]) - MapManager.Instance.CurMapStageConfigs[0].LevelID);

        var levels = LevelGroup.GetComponentsInChildren<MapLevel>().ToList();

        for (int i = 0; i < levels.Count; i++)
        {
            levels[i].Data = MapManager.Instance.Data.MapLevelDatas[i + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];
            levels[i].UpdateView();
        }
    }

    public string Mapmaker_ToConfig()
    {
        var levels = LevelGroup.GetComponentsInChildren<MapLevel>();
        var configs = new List<Mapmaker_BaseConfig>();
        foreach (var level in levels)
        {
            var config = new Mapmaker_BaseConfig();
            config.LocPos = level.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }
    #endregion
}
