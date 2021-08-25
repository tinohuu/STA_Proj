using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public List<CropConfig> CropConfigs = new List<CropConfig>();

    public static CropManager Instance = null;
    public Dictionary<string, List<Crop>> CropsByName = new Dictionary<string, List<Crop>>();
    public Transform CropGrpup;
    private void Awake()
    {
        Instance = this;

        // Import config
        CropConfigs = GetCropConfigs();

        TimeManager.Instance.TimeRefresher += RefreshHarvestTime;
    }
    private void Start()
    {
        InitializeCrops();
    }
    void RefreshHarvestTime(bool isVerified)
    {
        MapManagerData data = MapManager.Instance.Data;
        if (isVerified)
            data.LastHarvestTime = (data.LastHarvestTime - TimeManager.Instance.SystemNow).TotalHours < 1 ?
                data.LastHarvestTime : TimeManager.Instance.SystemNow;
        else
            data.LastHarvestTime = TimeManager.Instance.SystemNow;
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

    void InitializeCrops()
    {
        MapMakerConfig config = MapMaker.Config;
        if (config.CropDatas.Count > 0)
        {
            foreach (Crop crop in FindObjectsOfType<Crop>()) Destroy(crop.gameObject);
            foreach (MapMakerCropData data in config.CropDatas)
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

    public void UpdateCropType(Crop crop)
    {
        Vector2 locPos = crop.transform.localPosition;
        string name = crop.Name;
        float scale = crop.Scale;
        int variant = crop.Variant;
        Destroy(crop.gameObject);
        Crop newCrop = Instantiate(Resources.Load<GameObject>("Crops/Crop_" + name), CropGrpup).GetComponent<Crop>();
        newCrop.transform.localPosition = locPos;
        newCrop.Name = name;
        newCrop.Scale = scale;
        newCrop.Variant = variant;
        newCrop.UpdateView();
    }
}
