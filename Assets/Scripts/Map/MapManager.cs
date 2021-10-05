using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using STA.Mapmaker;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] GameObject m_MapmakerPrefab;
    [SerializeField] GameObject m_MapImagePrefab;
    [SerializeField] RectTransform m_MapImageGroup;
    [SerializeField] ScrollRect m_ScrollRect;
    public Canvas UICanvas;

    [Header("Data")]
    public int MapID = 1;
    public List<StageConfig> CurMapStageConfigs;
    public MapManagerData Data => MapDataManager.Instance.Data;
    public List<StageConfig> StageConfigs;
    public Dictionary<int, FunctionConfig> FunctionConfigsByFuncID;

    public static MapManager Instance = null;
    public List<Image> mapImages = new List<Image>();

    GameObject mapMaker;

    private void Awake()
    {
        if (!Instance) Instance = this; // Singleton

        StageConfigs = ConfigsAsset.GetConfigList<StageConfig>();
        CurMapStageConfigs = MapManager.Instance.StageConfigs.Where(e => e.ChapterNum == MapManager.Instance.MapID).ToList();
        FunctionConfigsByFuncID = ConfigsAsset.GetConfigList<FunctionConfig>().ToDictionary(p => p.FunctionID);

        UpdateView();
    }

    private void Update()
    {
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (mapMaker) Destroy(mapMaker.gameObject);
            else mapMaker = Instantiate(m_MapmakerPrefab, transform);
        }
    }

    void UpdateView()
    {
        mapImages.ForEach(e => DestroyImmediate(e));
        List<Image> _mapImages = new List<Image>();
        for (int i = 0; i < 16; i++)
        {
            Image image = Instantiate(m_MapImagePrefab, m_MapImageGroup).GetComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>("Maps/Map_" + MapID + "_" + i);
            image.sprite = sprite;
            _mapImages.Add(image);
        }
        mapImages = _mapImages;
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_MapImageGroup);
    }

    public void SetProgress(float ratio)
    {
        Data.CompleteLevel = CurMapStageConfigs[0].LevelID + Mathf.CeilToInt(ratio * (CurMapStageConfigs.Last().LevelID - CurMapStageConfigs[0].LevelID));
    }

    public void MoveMap(Vector3 target, float duration = 1)
    {
        StartCoroutine(IMoveMap(target, duration));
    }
    IEnumerator IMoveMap(Vector3 target, float duration)
    {
        yield return null;
        Vector3[] corners = new Vector3[4];
        m_ScrollRect.content.GetWorldCorners(corners);
        float width = corners[3].x - corners[0].x - 20; // Full screen width is 20 in world space
        //var levels = FindObjectsOfType<MapLevel>();
        //var curLevel = System.Array.Find(levels, e => e.Data.ID == levelID);
        float levelWidth = target.x - corners[0].x - 10;

        if (duration == 0) m_ScrollRect.normalizedPosition = new Vector3(levelWidth / width, m_ScrollRect.normalizedPosition.y);
        else
        {
            m_ScrollRect.DOKill();
            m_ScrollRect.DOHorizontalNormalizedPos(levelWidth / width, duration)
                .OnStart(() => { m_ScrollRect.horizontal = false; } )
                .OnComplete(() => { m_ScrollRect.horizontal = true; } );
        }
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