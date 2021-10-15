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
    [SerializeField] Canvas m_UICanvas;
    public Canvas UICanvas => m_UICanvas;

    [Header("Data")]
    public int MapID = 1; // todo: temp

    public MapManagerData Data => MapDataManager.Instance.Data;

    // Configs
    public List<StageConfig> StageConfigs { get; private set; }
    public List<StageConfig> CurMapStageConfigs { get; private set; }
    public List<FunctionConfig> FunctionConfigs { get; private set; }

    public static MapManager Instance = null;

    List<Image> m_MapImages = new List<Image>();
    GameObject m_MapMakerObj;

    public bool HasMapmaker => m_MapMakerObj;

    private void Awake()
    {
        if (!Instance) Instance = this;

        // Import configs
        StageConfigs = ConfigsAsset.GetConfigList<StageConfig>();
        FunctionConfigs = ConfigsAsset.GetConfigList<FunctionConfig>();
        CurMapStageConfigs = StageConfigs.Where(e => e.ChapterNum == MapID).ToList();

        UpdateView();
    }

    private void Update()
    {
        // Open map maker
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (m_MapMakerObj) Destroy(m_MapMakerObj.gameObject);
            else m_MapMakerObj = Instantiate(m_MapmakerPrefab, transform);
        }
    }

    void UpdateView()
    {
        m_MapImages.ForEach(e => DestroyImmediate(e));
        List<Image> _mapImages = new List<Image>();
        for (int i = 0; i < 16; i++)
        {
            Image image = Instantiate(m_MapImagePrefab, m_MapImageGroup).GetComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>("Maps/Map_" + MapID + "_" + i);
            image.sprite = sprite;
            _mapImages.Add(image);
        }
        m_MapImages = _mapImages;
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
                .SetEase(Ease.OutBack, 0.6f)
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