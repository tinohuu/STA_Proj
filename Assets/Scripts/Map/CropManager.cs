using STA.MapMaker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    [Header("Ref")]
    public Transform ForceFieldGroup;
    public Transform TriggerGroup;
    public Transform LeftSide;
    public Transform RightSide;
    public Transform CropGrpup;

    [Header("Debug")]
    public bool IsMature = false;
    public List<CropConfig> CropConfigs = new List<CropConfig>();
    public static CropManager Instance = null;
    public Dictionary<string, List<Crop>> CropsByName = new Dictionary<string, List<Crop>>();

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
        InitializeCrops();
    }

    private void Update()
    {

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
        CropsByName = _cropsByName;

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
        foreach (List<Crop> crops in CropsByName.Values)
        {
            foreach (Crop crop in crops) crop.UpdateView();
        }
    }

    public void UpdateCropsAnimator(bool includeState)
    {
        foreach (List<Crop> crops in CropsByName.Values)
        {
            foreach (Crop crop in crops)
            {
                if (!crop) continue; 
                crop.UpdateState();
                crop.UpdateAnimator(includeState);
            }
        }
    }

    public void PlayHarvestEffects()
    {
        StopAllCoroutines();
        StartCoroutine(IPlayHarvestEffects());
    }

    IEnumerator IPlayHarvestEffects()
    {

        List<string> shownCropNames = new List<string>();
        foreach (List<Crop> crops in CropsByName.Values)
        {
            foreach (Crop crop in crops)
            {
                //Vector2 pos = crop.transform.position;
                crop.UpdateState();
                if (crop.UpdateAnimator() && crop.CropState == Crop.State.immature)
                {
                    CreateParticle(crop.Name, crop.transform.position);
                    shownCropNames.Add(crop.Name);
                }
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

    void InitializeCrops()
    {
        MapMakerConfig config = MapMaker.Config;
        if (config.GetCurMapData().CropDatas.Count > 0)
        {
            foreach (Crop crop in FindObjectsOfType<Crop>()) Destroy(crop.gameObject);
            foreach (CropData data in config.GetCurMapData().CropDatas)
            {
                Crop crop = Instantiate(Resources.Load<GameObject>("Crops/Crop_" + data.Name), CropGrpup).GetComponent<Crop>();
                crop.transform.localPosition = new Vector2(data.PosX, data.PosY);
                crop.Name = data.Name;
                crop.Scale = data.Scale;
                crop.Variant = data.Variant;
            }
        }
        UpdateCropsView();
    }

    void CreateParticle(string cropName, Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + cropName);
        GameObject obj = ParticleManager.Instance.CreateParticle(prefab, pos);
        particles.Add(obj.transform);
    }

    public void UpdateCropType(Crop crop)
    {
        // Record old data
        Vector2 locPos = crop.transform.localPosition;
        string name = crop.Name;
        float scale = crop.Scale;
        int variant = crop.Variant;
        Destroy(crop.gameObject);

        // Create using old data
        Crop newCrop = Instantiate(Resources.Load<GameObject>("Crops/Crop_" + name), CropGrpup).GetComponent<Crop>();
        newCrop.transform.localPosition = locPos;
        newCrop.Name = name;
        newCrop.Scale = scale;
        newCrop.Variant = variant;
        newCrop.UpdateView();
    }
}
