using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.Mapmaker;
using System;

public class CrateManager : MonoBehaviour, IMapmakerModule
{
    [SerializeField] GameObject cratePrefab;
    [SerializeField] Transform crateGroup;

    public static CrateManager Instance = null;

    public Type Mapmaker_ItemType => typeof(Crate);
    public string[] Mapmaker_InputInfos => new string[] { "Level ID" };

    void Start()
    {
        if (!Instance) Instance = this;
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));
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

namespace STA.Mapmaker
{
    [System.Serializable]
    public class Mapmaker_CrateConfig : Mapmaker_BaseConfig
    {
        public int LevelID = 0;
    }
}
