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
    [SerializeField] Transform m_LevelGroup;
    [SerializeField] GameObject m_LevelPrefab;

    [Header("Data")]

    public static MapLevelManager Instance = null;

    public Type Mapmaker_ItemType => typeof(MapLevel);
    public string[] Mapmaker_InputInfos => new string[] { "Level ID" };

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));
        //if (l)
        //MapLevel level = LevelGroup.GetChild(MapManager.Instance.Data.SelectedLevel - 1).GetComponent<MapLevel>();
        TutorialManager.Instance.Show("EnterLevel", 1, (GameObject)null, 0.1f,
            onStart: () => MapManager.Instance.MoveMap(GetLevelButton(1).transform.position, 0.5f),
            onExit: () => Play(1));
    }

    public void UpdateLevelsView()
    {
        var levels = m_LevelGroup.GetComponentsInChildren<MapLevel>();
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].Data = MapManager.Instance.Data.MapLevelDatas[i + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];
            levels[i].UpdateView();
        }
    }

    public void ShowTutorial()
    {
        TutorialManager.Instance.Show("Harvest", 3, () => GetLevelButton(5).gameObject, 0.5f);
    }

    public MapLevel GetLevelButton(int level)
    {
        var levels = m_LevelGroup.GetComponentsInChildren<MapLevel>().ToList();
        return levels.Find(e => e.Data.ID == level);
    }

    public void Play(int levelID)
    {
        TutorialManager.Instance.Show("EnterLevel", 2, (GameObject)null, 1);
        // STAGameManager.Instance.InUseItems = inUseItems;
        STAGameManager.Instance.nLevelID = levelID;
        //MapDataManager.Instance.NewRatings = 0;
        WindowManager.Instance.LoadSceneWithFade("GameScene");
    }

    #region Mapmaker
    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        m_LevelGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_BaseConfig>(json);
        MapLevel curLevel = null;
        MapLevel retriedLevel = null;
        for (int i = 0; i < configs.Count; i++)
        {
            var level = Instantiate(m_LevelPrefab, m_LevelGroup).GetComponent<MapLevel>();
            level.transform.localPosition = configs[i].LocPos;
            level.Data = MapManager.Instance.Data.MapLevelDatas[i + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];
            if (MapDataManager.Instance.RetriedLevel > 0 && level.Data.ID == MapDataManager.Instance.RetriedLevel) retriedLevel = level;
            if (MapManager.Instance.Data.SelectedLevel == level.Data.ID) curLevel = level;
        }


        if (MapDataManager.Instance.RetriedLevel > 0 && retriedLevel)
        {

            MapPlayer.Instance.MoveToLevel(curLevel, false, false);
            //MapPlayer.Instance.OnClickRomote(0);
            MapPlayer.Instance.MoveToLevel(retriedLevel, WindowAnimator.WindowQueue.Count == 0, true);
            MapDataManager.Instance.RetriedLevel = 0;
        }
        else if (curLevel)
        {
            MapPlayer.Instance.MoveToLevel(curLevel, false, false);
        }

        MapPlayer.Instance.OnClickRomote(0);
        //MapManager.Instance.MoveMap(curLevel.transform.position, 0);
    }

    public Transform Mapmaker_AddItem()
    {
        if (m_LevelGroup.childCount >= MapManager.Instance.CurMapStageConfigs.Count)
        {
            Mapmaker.Log("Level count can't be great than " + MapManager.Instance.CurMapStageConfigs.Count);
            return null;
        }

        var level = Instantiate(m_LevelPrefab, m_LevelGroup).GetComponent<MapLevel>();
        level.transform.localPosition = m_LevelGroup.InverseTransformPoint(Vector3.zero);
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

        var levels = m_LevelGroup.GetComponentsInChildren<MapLevel>().ToList();

        for (int i = 0; i < levels.Count; i++)
        {
            levels[i].Data = MapManager.Instance.Data.MapLevelDatas[i + MapManager.Instance.CurMapStageConfigs[0].LevelID - 1];
            levels[i].UpdateView();
        }
    }

    public string Mapmaker_ToConfig()
    {
        var levels = m_LevelGroup.GetComponentsInChildren<MapLevel>();
        var configs = new List<Mapmaker_BaseConfig>();
        foreach (var level in levels)
        {
            var config = new Mapmaker_BaseConfig();
            config.LocPos = level.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        DestroyImmediate(target);
        UpdateLevelsView();
    }
    #endregion
}
