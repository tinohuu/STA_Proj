using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.Mapmaker;
using System;
using System.Linq;

public class CrateManager : MonoBehaviour, IMapmakerModule
{
    [SerializeField] GameObject cratePrefab;
    [SerializeField] Transform crateGroup;
    [SerializeField] GameObject m_CrateViewPrefab;

    [SavedData] public CrateManagerData Data = new CrateManagerData();

    public static CrateManager Instance = null;
    CrateView m_CrateView;
    public bool EnableCrate = true;

    public Type Mapmaker_ItemType => typeof(Crate);
    public string[] Mapmaker_InputInfos => new string[] { "Level ID" };

    void Start()
    {
        if (!Instance) Instance = this;
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));

        ShowCrateView();
    }

    void ShowCrateView()
    {
        if (EnableCrate)
        {
            var crates = crateGroup.GetComponentsInChildren<Crate>().ToList();
            var crate = crates.Find(e => e.LevelID <= MapManager.Instance.Data.CompleteLevel);
            if (!m_CrateView && crate)
            {
                Destroy(crate.gameObject);
                m_CrateView = Instantiate(m_CrateViewPrefab, MapManager.Instance.UICanvas.transform).GetComponent<CrateView>();
                m_CrateView.LevelID = crate.LevelID;
            }
        }
    }

    public void Collect(int levelID)
    {
        for (int i = 0; i < Data.ResumedRewardTypes.Count; i++)
        {
            RewardType rewardType = Data.ResumedRewardTypes[i];
            int count = Data.ResumedCounts[i];
            Reward.Data[rewardType] += count;
        }
        Data.ResumedRewardTypes.Clear();
        Data.ResumedCounts.Clear();
        Data.ResumedIndexes.Clear();
        Data.CollectedCrateLevel = levelID;
    }

    #region Mapmaker
    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(cratePrefab, crateGroup);
        obj.transform.localPosition = crateGroup.InverseTransformPoint(Vector3.zero);
        return obj.transform;
    }

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        crateGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_CrateConfig>(json);
        foreach (var config in configs)
        {
            if (config.LevelID <= Data.CollectedCrateLevel) continue;
            var crate = Instantiate(cratePrefab, crateGroup).GetComponent<Crate>();
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
        var crates = crateGroup.GetComponentsInChildren<Crate>();
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
    public int CollectedCrateLevel = 0;
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
