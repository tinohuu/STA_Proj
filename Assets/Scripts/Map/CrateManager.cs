using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.Mapmaker;
using System;
using System.Linq;

public class CrateManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    //[SerializeField] public GameObject CrateProgressBarPrefab;
    [SerializeField] GameObject m_CratePrefab;
    [SerializeField] Transform m_CrateGroup;
    [SerializeField] GameObject m_CrateViewPrefab;

    [Header("Setting")]
    public bool EnableCrate = true;
    public bool ForceShowLevelProgress = false;

    [Header("Data")]
    [SavedData] public CrateManagerData Data = new CrateManagerData();
    public Crate CurrentCrate { get; private set; }
    public Type Mapmaker_ItemType => typeof(Crate);
    public string[] Mapmaker_InputInfos => new string[] { "Level ID" };

    public static CrateManager Instance = null;
    CrateView m_CrateView;

    void Start()
    {

        if (!Instance) Instance = this;
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));

        //ShowCrateView();
    }

    void ShowCrateView()
    {
        if (EnableCrate)
        {
            var crates = m_CrateGroup.GetComponentsInChildren<Crate>().ToList();
            var crate = crates.Find(e => e.LevelID <= MapManager.Instance.Data.CompleteLevel);
            if (!m_CrateView && crate)
            {
                m_CrateView = Instantiate(m_CrateViewPrefab, MapManager.Instance.UICanvas.transform).GetComponent<CrateView>();
                m_CrateView.LevelID = crate.LevelID;
                CurrentCrate = crate;
            }
        }
    }

    public void ShowCrateView(Crate crate)
    {
        m_CrateView = Instantiate(m_CrateViewPrefab, MapManager.Instance.UICanvas.transform).GetComponent<CrateView>();
        m_CrateView.LevelID = crate.LevelID;
        m_CrateView.Quality = crate.CrateQuality;
        CurrentCrate = crate;
    }

    public void Collect(int levelID)
    {
        for (int i = 0; i < Data.ResumedIndexes.Count; i++)
        {
            RewardType rewardType = Data.ResumedRewardTypes[Data.ResumedIndexes[i]];
            int count = Data.ResumedCounts[Data.ResumedIndexes[i]];
            Reward.Data[rewardType] += count;

            Debug.LogWarning("Collected from the crate: " + rewardType.ToString() + ":" + count);
        }
        Data.ResumedRewardTypes.Clear();
        Data.ResumedCounts.Clear();
        Data.ResumedIndexes.Clear();
        Data.CollectedCrateLevels.Add(levelID);
    }

    public void UpdateCratesView()
    {
        var crates = m_CrateGroup.GetComponentsInChildren<Crate>();
        foreach (var crate in crates)
        {
            crate.UpdateView();
        }
    }

    #region Mapmaker
    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(m_CratePrefab, m_CrateGroup);
        obj.transform.localPosition = m_CrateGroup.InverseTransformPoint(Vector3.zero);
        return obj.transform;
    }

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        m_CrateGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_CrateConfig>(json);
        foreach (var config in configs)
        {
            //if (config.LevelID <= Data.CollectedCrateLevel) continue;
            var crate = Instantiate(m_CratePrefab, m_CrateGroup).GetComponent<Crate>();
            crate.transform.localPosition = config.LocPos;
            crate.LevelID = config.LevelID;
        }
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var crate = target.GetComponent<Crate>();
        var inputDatas = new string[] { crate.LevelID.ToString() };
        return inputDatas;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputDatas)
    {
        var crate = target.GetComponent<Crate>();
        crate.LevelID = int.Parse(inputDatas[0]);
    }

    public string Mapmaker_ToConfig()
    {
        var crates = m_CrateGroup.GetComponentsInChildren<Crate>();
        var configs = new List<Mapmaker_CrateConfig>();
        foreach (var crate in crates)
        {
            var config = new Mapmaker_CrateConfig();
            config.LevelID = crate.LevelID;
            config.LocPos = crate.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        Destroy(target);
    }
    #endregion
}

[System.Serializable]
public class CrateManagerData
{
    public List<int> CollectedCrateLevels = new List<int>();
    public List<RewardType> ResumedRewardTypes = new List<RewardType>();
    public List<int> ResumedCounts = new List<int>();
    public List<int> ResumedIndexes = new List<int>();
}

namespace STA.Mapmaker
{
    [System.Serializable]
    public class Mapmaker_CrateConfig : Mapmaker_BaseConfig
    {
        public int LevelID = 0;
    }
}
