using STA.MapMaker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CropManager : MonoBehaviour, IMapMakerModule
{
    [Header("Ref")]
    public Transform ForceFieldGroup;
    public Transform TriggerGroup;
    public Transform LeftSide;
    public Transform RightSide;
    public Transform CropGroup;

    [Header("Debug")]
    public bool IsMature = false;
    public List<CropConfig> CropConfigs = new List<CropConfig>();
    public static CropManager Instance = null;
    //public Dictionary<string, List<Crop>> CropsByName = new Dictionary<string, List<Crop>>();

    ParticleSystemForceField[] fields;
    Collider[] triggers;
    List<Transform> particles = new List<Transform>();
    private void Awake()
    {
        if (!Instance) Instance = this;
        CropConfigs = GetCropConfigs();
        fields = ForceFieldGroup.GetComponentsInChildren<ParticleSystemForceField>();
        triggers = TriggerGroup.GetComponentsInChildren<Collider>();
        //Map.Instance.MapScrollView.onValueChanged.AddListener((Vector2 v) => UpdateCropsAnimator(false));
    }
    private void Start()
    {
        CreateItems();
    }

    public CropConfig LevelToCropConfig(int level)
    {
        foreach (CropConfig cropConfig in CropConfigs)
        {
            if (level >= cropConfig.MinLevel && level <= cropConfig.Level) return cropConfig;
        }
        return null;
    }

    public static List<CropConfig> GetCropConfigs()
    {
        List<CropConfig> cropConfigs = ConfigsAsset.GetConfigList<CropConfig>();
        for (int i = 1; i < cropConfigs.Count; i++)
        {
            cropConfigs[i].MinLevel = cropConfigs[i - 1].Level + 1;
        }
        return cropConfigs;
    }

    public void UpdateCropsView()
    {
        List<CropConfig> cropConfigs = CropManager.Instance.CropConfigs;

        // Create Name to CropConfig dictionary
        Dictionary<string, CropConfig> configsByName = cropConfigs.ToDictionary(p => p.Name);

        // Create Name to Crops dictionary
        Dictionary<string, List<Crop>> _cropsByName = new Dictionary<string, List<Crop>>();

        Crop[] allCrops = FindObjectsOfType<Crop>();
        foreach (Crop crop in allCrops)
        {
            if (!configsByName.ContainsKey(crop.Name)) continue;

            if (!_cropsByName.ContainsKey(crop.Name)) _cropsByName.Add(crop.Name, new List<Crop>());
            _cropsByName[crop.Name].Add(crop);

            crop.Config = configsByName[crop.Name];
        }
        //CropsByName = _cropsByName;

        // Sort crops list
        foreach (List<Crop> crops in _cropsByName.Values)
        {
            //Debug.LogWarning("Sort " + crops[0].Name);
            crops.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
            // Record order for each crop
            for (int i = 0; i < crops.Count; i++)
            {
                crops[i].OrderRatio = (i + 1f) / crops.Count;
            }
        }
        foreach (List<Crop> crops in _cropsByName.Values)
        {
            foreach (Crop crop in crops) crop.UpdateView();
        }
    }

    public void UpdateCropsAnimator(bool includeState)
    {
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
                CreateParticle(crop.Name, crop.transform.position);
                shownCropNames.Add(crop.Name);
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

    public void CreateItems()
    {
        RecordItemMakerData();

        CropGroup.DestroyChildren();

        var datas = MapManager.MapMakerConfig.GetCurMapData().CropDatas;
        foreach (var data in datas)
        {
            Crop crop = Instantiate(Resources.Load<GameObject>("Crops/Crop_" + data.Name), CropGroup).GetComponent<Crop>();
            crop.transform.localPosition = new Vector2(data.PosX, data.PosY);
            crop.Name = data.Name;
            crop.Scale = data.Scale;
            crop.Variant = data.Variant;
        }

        UpdateCropsView();
    }

    void CreateParticle(string cropName, Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + cropName);
        GameObject obj = ParticleManager.Instance.CreateParticle(prefab, pos);
        particles.Add(obj.transform);
    }

    public void RecordItemMakerData()
    {
        if (CropGroup.childCount == 0) return;
        var datas = new List<MapMaker_CropData>();
        var crops = CropGroup.GetComponentsInChildren<Crop>().ToList();

        var overlapped = new List<Crop>();
        for (int i = 0; i < crops.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if ((crops[i].transform.localPosition - crops[j].transform.localPosition).magnitude <= 0.001f)
                    overlapped.Add(crops[j]);
            }
        }

        foreach (Crop crop in crops)
        {
            if (overlapped.Contains(crop)) continue;
            MapMaker_CropData data = new MapMaker_CropData();
            data.PosX = crop.transform.localPosition.x;
            data.PosY = crop.transform.localPosition.y;
            data.Name = crop.Name;
            data.Scale = crop.Scale;
            data.Variant = crop.Variant;
            datas.Add(data);
        }

        MapManager.MapMakerConfig.GetCurMapData().CropDatas = datas;
    }

    public void AddNewItem()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Crops/Crop_Cabbage"), CropManager.Instance.CropGroup);
        obj.transform.localPosition = CropGroup.InverseTransformPoint(Vector3.zero);
        UpdateCropsView();
    }
}

namespace STA.MapMaker
{
    [System.Serializable]
    public class MapMaker_CropData
    {
        public float PosX = 0;
        public float PosY = 0;
        public string Name = "Crop";
        public float Scale = 0.5f;
        public int Variant = 0;
    }
}
