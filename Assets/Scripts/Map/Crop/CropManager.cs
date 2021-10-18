using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CropManager : MonoBehaviour, IMapmakerModule
{
    [Header("Setting")]
    [SerializeField] bool m_EnableEndpoint = true;
    public bool EnableEndpoint => m_EnableEndpoint;

    [Header("Ref")]
    [SerializeField] Transform m_CropGroup;
    [SerializeField] RectTransform HarvestButton;
    [SerializeField] GameObject m_CropPrefab;
    [SerializeField] GameObject m_CropEndpointPrefab;



    [Header("Data")]
    [SavedData] public CropManagerData Data = new CropManagerData();
    //public CropManagerData Data => m_Data;

    public List<CropConfig> CropConfigs { get; private set; }
    public bool IsMature { get; private set; }

    public static CropManager Instance = null;
    List<Transform> m_Particles = new List<Transform>();

    private void Awake()
    {
        if (!Instance) Instance = this;
        CropConfigs = GetCropConfigs();

        //Data = SaveManager.Instance.Bind(InitialiseData());
    }
    private void Start()
    {
        HarvestButton.gameObject.SetActive(MapManager.Instance.Data.CompleteLevel >= CropConfigs[0].Level);
        /*var crop = CropConfigs.Find(e => e.ID > Data.LastCropGrowthID && e.Level <= MapManager.Instance.Data.CompleteLevel);
        if (crop != null)
        {
            Window.CreateWindowPrefab(CropGrowthWindow).GetComponent<CropGrowthWindow>().SetCrop(crop.Name);
            Data.LastCropGrowthID = crop.ID;
        }*/

        /*var crops = CropConfigs.FindAll(e => e.ID > Data.LastCropGrowthID && e.Level <= MapManager.Instance.Data.CompleteLevel);
        foreach (var c in crops)
        {
            Window.CreateWindowPrefab(CropGrowthWindow).GetComponent<CropGrowthWindow>().SetCrop(c.Name);
            Data.LastCropGrowthID = c.ID;
        }*/
        CreateEndpoints();

        Mapmaker_CreateItems(Mapmaker.GetConfig(this));


    }

    public void SetMature(bool isMature)
    {
        IsMature = isMature;
    }

    void CreateEndpoints()
    {
        if (Data.CollectedClockLevelID == 0) Data.LastHarvestTime = TimeManager.Instance.RealNow;
        var configs = CropConfigs.FindAll(e => e.Level > Data.CollectedClockLevelID); // todo: 2nd map check
        foreach (var config in configs)
        {
            var levelButton = MapLevelManager.Instance.GetLevelButton(config.Level);
            if (!levelButton) continue;
            var endpoint = Instantiate(m_CropEndpointPrefab, transform);
            endpoint.GetComponent<CropEndpoint>().Initialise(config.Level, config.Name);
            levelButton.SetAsIcon(endpoint.transform, false);
        }
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
            if (!configsByName.ContainsKey(crop.CropName)) continue;

            if (!cropsByName.ContainsKey(crop.CropName)) cropsByName.Add(crop.CropName, new List<Crop>());
            cropsByName[crop.CropName].Add(crop);
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
        var crops = m_CropGroup.GetComponentsInChildren<Crop>();
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
        StartCoroutine(IPlayHarvestSound());
    }

    IEnumerator IPlayHarvestEffects()
    {
        //SoundManager.Instance.PlaySFX("coinIncreaseHarvest", isLoop: true);
        var crops = m_CropGroup.GetComponentsInChildren<Crop>();

        List<string> shownCropNames = new List<string>();
        foreach (Crop crop in crops)
        {
            crop.UpdateState();
            if (crop.IsVisible && crop.CropState == Crop.State.immature)
            {
                CreateParticle(crop.CropName, crop.transform.position);
                shownCropNames.Add(crop.CropName);
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
                CreateParticle(config.Name, leftSide ? ParticleManager.Instance.LeftSide.position : ParticleManager.Instance.RightSide.position);
                CreateParticle(config.Name, leftSide ? ParticleManager.Instance.LeftSide.position : ParticleManager.Instance.RightSide.position);
            }
        }

        yield return new WaitForSeconds(5);
        foreach (var paticle in m_Particles) Destroy(paticle.gameObject);
        m_Particles.Clear();
    }

    IEnumerator IPlayHarvestSound()
    {
        do
        {
            SoundManager.Instance.PlaySFX("coinIncreaseUnit");
            yield return new WaitForSeconds(1);
        }
        while (m_Particles.Count != 0);
        SoundManager.Instance.PlaySFX("harvestFinish");
    }

    void CreateParticle(string cropName, Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Crops/HarvestParticles/FXHarvest" + cropName);
        Debug.Log(cropName + prefab);
        GameObject obj = ParticleManager.Instance.CreateParticle(prefab, pos);
        m_Particles.Add(obj.transform);
    }

    #region Mapmaker
    public Type Mapmaker_ItemType => typeof(Crop);

    public string[] Mapmaker_InputInfos => new string[] { "Crop Name", "Crop Scale", "Crop Variant", "Spine Name" };

    public Transform Mapmaker_AddItem()
    {
        GameObject obj = Instantiate(m_CropPrefab, m_CropGroup);
        obj.transform.localPosition = m_CropGroup.InverseTransformPoint(Vector3.zero);
        UpdateCropsView();
        obj.GetComponent<Crop>().UpdateView();
        return obj.transform;
    }

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        m_CropGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_CropConfig>(json);
        foreach (var config in configs)
        {
            var crop = Instantiate(m_CropPrefab, m_CropGroup).GetComponent<Crop>();
            crop.CropName = config.Name;
            crop.SpineName = config.SpineName;
            crop.Variant = config.Variant;
            crop.transform.localPosition = config.LocPos;
            crop.transform.localScale = new Vector3(config.Scale, Mathf.Abs(config.Scale), 1);
            crop.UpdateView(true);
        }
        UpdateCropsView();
    }

    public string[] Mapmaker_UpdateInputs(Transform target)
    {
        var crop = target.GetComponent<Crop>();
        var inputDatas = new string[] { crop.CropName, crop.transform.localScale.x.ToString(), crop.Variant.ToString(), crop.SpineName };
        return inputDatas;
    }

    public void Mapmaker_ApplyInputs(Transform target, string[] inputDatas)
    {
        var crop = target.GetComponent<Crop>();
        //var config = crop.MapmakerConfig;

        if (CropConfigs.Find(e => e.Name == inputDatas[0]) != null)
            crop.CropName = inputDatas[0];
        else
        {
            Mapmaker.Log("Can't match the crop name.");
            return;
        }
        //crop = Instantiate(prefab, CropGroup).GetComponent<Crop>();

        float scale = int.Parse(inputDatas[1]);
        crop.transform.localScale = new Vector3(scale, Mathf.Abs(scale), 1);
        crop.Variant = int.Parse(inputDatas[2]);
        crop.SpineName = inputDatas[3];
        crop.UpdateView();
    }

    public string Mapmaker_ToConfig()
    {
        var crops = m_CropGroup.GetComponentsInChildren<Crop>();
        var configs = new List<Mapmaker_CropConfig>();
        foreach (var crop in crops)
        {
            Mapmaker_CropConfig config = new Mapmaker_CropConfig();
            config.Name = crop.CropName;
            config.LocPos = crop.transform.localPosition;
            config.Scale = crop.transform.localScale.x;
            config.Variant = crop.Variant;
            config.SpineName = crop.SpineName;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        Destroy(target.gameObject);
        UpdateCropsView();
    }

    #endregion

    public bool IsTimeBoosting => Data.TimeBoostUntil - TimeManager.Instance.RealNow > TimeSpan.Zero;
}

[Serializable]
public class CropManagerData
{
    //public int LastCropGrowthID = 0;
    public int CollectedClockLevelID = 0;
    public DateTime LastHarvestTime = new DateTime();
    public DateTime TimeBoostUntil = new DateTime();
}

namespace STA.Mapmaker
{
    [System.Serializable]
    public class Mapmaker_CropConfig : Mapmaker_BaseConfig
    {
        public string Name = "Cabbage";
        public string SpineName = "";
        public float Scale = 0.5f;
        public int Variant = 0;
    }
}

public static class CropExtension
{
    public static Sprite ToFruitSprite(this string name)
    {
        return Resources.Load<Sprite>("Sprites/Crops/Crop_Fruit_" + name);
    }
}
