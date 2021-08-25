using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [Header("Ref")]
    public GameObject LevelButtonPrefab;
    public Transform LevelPointsGroup;
    public Transform LevelsGroup;
    public GameObject MapMakerPrefab;
    [Header("Config")]
    public bool ShowPath = true;
    [Header("Debug")]
    public MapData Data = null;
    public List<MapLevel> mapLevels = new List<MapLevel>();
    public static Map Instance = null;
    public MapMakerConfig Config;
    private void Awake()
    {
        Instance = this;

        // Test: get map data
        Data = MapManager.Instance.Data.MapDatas[0];

    }

    void Start()
    {
        CreateLevelButtons();
        MapPlayer.Instance.MoveToLevel(mapLevels[0], false);
        if (Debug.isDebugBuild) Instantiate(MapMakerPrefab);

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


    void CreateLevelButtons()
    {
        Config = MapMaker.Config;
        List<Vector2> locPoints = new List<Vector2>();
        foreach (MapMakerLevelData data in MapMaker.Config.LevelDatas)
        {
            locPoints.Add(new Vector2(data.PosX, data.PosY));
        }

        /*
        if (LevelPointsGroup && LevelPointsGroup.childCount > 0)
        {
            for (int i = 0; i < LevelPointsGroup.childCount; i++)
            {
                if (i >= Data.MapLevelDatas.Count) break;
                locPoints.Add(LevelsGroup.InverseTransformPoint(LevelPointsGroup.GetChild(i).position));
            }
        }*/
        UpdateLevelButtons(locPoints);
    }

    public void UpdateLevelButtons(List<Vector2> locPoints = null)
    {
        if (locPoints == null)
        {
            locPoints = new List<Vector2>();
            foreach (MapLevel level in mapLevels) locPoints.Add(level.transform.localPosition);
        }

        LevelsGroup.DestroyChildren();
        mapLevels.Clear();
        for (int i = 0; i < locPoints.Count; i++)
        {
            MapLevel mapLevel = Instantiate(LevelButtonPrefab, LevelsGroup).GetComponent<MapLevel>();
            mapLevel.transform.localPosition = locPoints[i];
            mapLevel.Data = Data.MapLevelDatas[i];
            mapLevels.Add(mapLevel);
        }
    }

    public void AddLevelButton(Vector2 locPos)
    {
        /*if (mapLevels.Count >= Data.MapLevelDatas.Count) return;
        MapLevel mapLevel = Instantiate(LevelButtonPrefab, LevelsGroup).GetComponent<MapLevel>();
        mapLevel.transform.localPosition = locPos;
        mapLevel.Data = Data.MapLevelDatas[mapLevels.Count];
        mapLevels.Add(mapLevel);*/
        List<Vector2> locPoints = new List<Vector2>();
        foreach (MapLevel level in mapLevels) locPoints.Add(level.transform.localPosition);
        locPoints.Add(locPos);
        UpdateLevelButtons(locPoints);
    }

    public void ChangeLevelOrder(MapLevel mapLevel, int level)
    {
        mapLevels.Remove(mapLevel);
        mapLevels.Insert(level - 1, mapLevel);
        UpdateLevelButtons();
    }

    public void SetProgress(float ratio)
    {
        MapManager.Instance.Data.CompelteLevel = (int)(ratio * 200);
        CropManager.Instance.UpdateCropsView();
        UpdateLevelButtons();
        //foreach (MapLevel lvl in FindObjectsOfType<MapLevel>()) lvl.UpdateView();
    }
}
