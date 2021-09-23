using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CropManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    [SerializeField] Transform LeftSide;
    [SerializeField] Transform RightSide;
    [SerializeField] Transform CropGroup;

    [Header("Config & Data")]
    public List<CropConfig> CropConfigs = new List<CropConfig>();
    public bool IsMature = false;

    public static CropManager Instance = null;

    List<Transform> particles = new List<Transform>();

    private void Awake()
    {
        if (!Instance) Instance = this;
        CropConfigs = GetCropConfigs();
    }
    private void Start()
    {
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));
    }

    public CropConfig LevelToCropConfig(int level) => CropConfigs.Find(e => level >= e.MinLevel && level <= e.Level);

    public static List<CropConfig> GetCropConfigs()
    {
        List<CropConfig> cropConfigs = ConfigsAsset.GetConfigList<CropConfig>();

        // Count min level from (max) level
        for (int i = 1; i < cropConfigs.Count; i++)
            cropConfigs[i].MinLevel = cropConfigs[i - 1].Level + 1;
        return cropConfigs;
    }

    public void UpdateCropsView()
    {
        // Create Name to CropConfig dictionary
        Dictionary<string, CropConfig> configsByName = CropConfigs.ToDictionary(p => p.Name);

        // Create Name to Crops dictionary
        Dictionary<string, List<Crop>> cropsByName = new Dictionary<string, List<Crop>>();

        Crop[] allCrops = FindObjectsOfType<Crop>();
        foreach (Crop crop in allCrops)
        {
            if (!configsByName.ContainsKey(crop.MapmakerConfig.Name)) continue;

            if (!cropsByName.ContainsKey(crop.MapmakerConfig.Name)) cropsByName.Add(crop.MapmakerConfig.Name, new List<Crop>());
            cropsByName[crop.MapmakerConfig.Name].Add(crop);
        }

        // Set crop with unlocking level and unlocked level
        foreach (string name in cropsByName.Keys)
        {
            cropsByName[name].Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
            int levelCount = configsByName[name].Level - configsByName[name].MinLevel;
            List<Crop> crops = cropsByName[name];
            for (int i = 0; i < crops.Count; i++)
            {
                float localLevelCount = (float)i / crops.Count * levelCount;
                crops[i].UnlockingLevel = configsByName[name].MinLevel + (int)localLevelCount;
                crops[i].UnlockedLevel = configsByName[name].Level;
            }
        }

        // Update all views
        foreach (List<Crop> crops in cropsByName.Values)
            foreach (Crop crop in crops)
                crop.UpdateView(false);
    }

    public void UpdateCropsAnimator(bool includeState)
    {
        // todo: scroll rect opt
        var crops = CropGroup.GetComponentsInChildren<Crop>();
        foreach (Crop crop in crops)
        {
            if (!crop) continue;
            crop.UpdateState();
            crop.UpdateAnimator(includeState);
        }
    }

    public void PlayHarvestEffects()
    {
        StopAllCoroutines();
        StartCoroutine(IPlayHarvestEffects());
    }

    IEnumerator IPlayHarvestEffects()
    {
        var crops = CropGroup.GetComponentsInChildren<Crop>();

        List<string> shownCropNames = new List<string>();
        foreach (Crop crop in crops)
        {
            crop.UpdateState();
            if (crop.UpdateAnimator() && crop.CropState == Crop.State.immature)
            {
                CreateParticle(crop.MapmakerConfig.Name, crop.transform.position);
                shownCropNames.Add(crop.MapmakerConfig.Name);
            }
        }

        bool leftSide = true;
        foreach (CropConfig config in CropConfigs)
        {
            if (config.Level > MapManager.Instance.Data.CompleteLevel) break;

            if (shownCropNames.Contains(config.Name))
            {
                leftSide = false;
                continue;
            }
            else
            {
                CreateParticle(config.Name, leftSide ? LeftSide.position : RightSide.position);
                CreateParticle(config.Name, leftSide ? LeftSide.position : RightSide.position);
            }
        }

        yield return new WaitForSeconds(5);
        foreach (var paticle in particles) Destroy(paticle.gameObject);
        particles.Clear();
    }

    void CreateParticle(string cropName, Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + cropName);
        GameObject obj = ParticleManager.Instance.CreateParticle(prefab, pos);
        particles.Add(obj.transform);
    }

    #region Mapmaker
    public Type Mapmaker_ItemType => typeof(Crop);

    public string[] Mapmaker_InputInfos => new string[] { "Crop Name", "Crop Scale", "Crop Variant" };

    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Crops/Crop_Cabbage"), CropManager.Instance.CropGroup);
        obj.transform.localPosition = CropGroup.InverseTransformPoint(Vector3.zero);
        UpdateCropsView();
        obj.GetComponent<Crop>().UpdateView();
        return obj.transform;
    }

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        CropGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_CropConfig>(json);
        foreach (var config in configs)
        {
            GameObject prefab = Resources.Load<GameObject>("Crops/Crop_" + config.Name);
            var crop = Instantiate(prefab, CropGroup).GetComponent<Crop>();
            crop.MapmakerConfig = config;
            crop.UpdateView(true);
        }
        UpdateCropsView();
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var crop = target.GetComponent<Crop>();
        var inputDatas = new string[] { crop.MapmakerConfig.Name, crop.MapmakerConfig.Scale.ToString(), crop.MapmakerConfig.Variant.ToString() };
        return inputDatas;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputDatas)
    {
        var crop = target.GetComponent<Crop>();
        var config = crop.MapmakerConfig;

        if (CropConfigs.Find(e => e.Name == inputDatas[0]) != null)
            config.Name = inputDatas[0];
        else
        {
            Mapmaker.Log("Can't match the crop name.");
            return;
        }
        config.Scale = float.Parse(inputDatas[1]);
        config.Variant = int.Parse(inputDatas[2]);
        config.LocPos = crop.transform.localPosition;

        Destroy(crop.gameObject);
        GameObject prefab = Resources.Load<GameObject>("Crops/Crop_" + config.Name);
        crop = Instantiate(prefab, CropGroup).GetComponent<Crop>();
        crop.MapmakerConfig = config;
        crop.UpdateView(true);
        Debug.Log(config);
        UpdateCropsView();
    }

    public string Mapmaker_ToConfig()
    {
        var crops = CropGroup.GetComponentsInChildren<Crop>();
        var configs = new List<Mapmaker_CropConfig>();
        foreach (var crop in crops)
        {
            crop.MapmakerConfig.LocPos = crop.transform.localPosition;
            configs.Add(crop.MapmakerConfig);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        Destroy(target.gameObject);
        UpdateCropsView();
    }

    #endregion
}

namespace STA.Mapmaker
{
    [System.Serializable]
    public class Mapmaker_CropConfig : Mapmaker_BaseConfig
    {
        public string Name = "Cabbage";
        public float Scale = 0.5f;
        public int Variant = 0;
    }
}
