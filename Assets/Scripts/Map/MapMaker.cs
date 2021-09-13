using STA.MapMaker;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    // Add a layer called MapMaker
    public Dropdown ModeDropdown;
    public InputField CropNameInput;
    public InputField CropScaleInput;
    public InputField CropVariantInput;
    public InputField LevelNumberInput;
    public GameObject SaveCropButton;
    public Text LogText;
    //public Transform TrackPointGroup;
    //public Transform CropGroup;
    //public Transform LevelGroup;
    public GameObject PlaceholderPrefab;
    public Slider ProgressSlider;
    public static MapMaker Instance = null;
    public MapMakerConfig test;
    private void Awake()
    {
        test = JsonUtility.FromJson<MapMakerConfig>(ConfigsAsset.GetConfig("MapMakerConfig"));
        //TimeManager.Instance.TimeRefresherOld += (bool authenic) => LogText.text = authenic ? "Time is authentic" : "Time is not authentic";
    }
    void Start()
    {
        if (!Instance) Instance = this;
        ProgressSlider.onValueChanged.AddListener((float f) => Map.Instance.SetProgress(f));
        ProgressSlider.value = MapManager.Instance.Data.CompleteLevel / 186f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInputView()
    {
        if (MapMakerPlaceholder.Target)
        {
            if (ModeDropdown.value == 1)
            {
                LevelNumberInput.text = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>().Data.Order.ToString();
            }
            else if (ModeDropdown.value == 2)
            {
                CropNameInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Name;
                CropScaleInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Scale.ToString();
                CropVariantInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Variant.ToString();
            }
            else if (ModeDropdown.value == 3)
            {

            }
        }
        else
        {
            LevelNumberInput.text = "";
            CropNameInput.text = "";
            CropScaleInput.text = "";
            CropVariantInput.text = "";
        }
    }

    public void UpdateMode(int mode = -1)
    {
        if (mode != -1) ModeDropdown.value = mode;

        MapMakerPlaceholder[] placeholders = FindObjectsOfType<MapMakerPlaceholder>();
        for (int i = 0; i < placeholders.Length; i++) Destroy(placeholders[i].gameObject);

        if (ModeDropdown.value == 1)
        {
            MapLevel[] mapLevels = FindObjectsOfType<MapLevel>();
            for (int i = 0; i < mapLevels.Length; i++) Instantiate(PlaceholderPrefab, mapLevels[i].transform);
        }
        else if (ModeDropdown.value == 2)
        {
            Crop[] crops = FindObjectsOfType<Crop>();
            for (int i = 0; i < crops.Length; i++) Instantiate(PlaceholderPrefab, crops[i].transform);
        }
        else if (ModeDropdown.value == 3)
        {
            for (int i = 0; i < MapTrack.Instance.TrackPointsGroup.childCount; i++) Instantiate(PlaceholderPrefab, MapTrack.Instance.TrackPointsGroup.GetChild(i));
        }
        LevelNumberInput.gameObject.SetActive(ModeDropdown.value == 1);
        CropNameInput.gameObject.SetActive(ModeDropdown.value == 2);
        CropScaleInput.gameObject.SetActive(ModeDropdown.value == 2);
        CropVariantInput.gameObject.SetActive(ModeDropdown.value == 2);
        MapMakerPlaceholder.Target = null;
        UpdateInputView();
    }

    public void Save()
    {
        if (MapMakerPlaceholder.Target == null) return;
        if (ModeDropdown.value == 2)
        {
            Dictionary<string, CropConfig> configsByName = CropManager.Instance.CropConfigs.ToDictionary(p => p.Name);

            if (!configsByName.ContainsKey(CropNameInput.text))
            {
                LogText.text = "Please check crop name.";
                return;
            }
            Crop crop = MapMakerPlaceholder.Target.parent.GetComponent<Crop>();
            crop.Name = CropNameInput.text;
            crop.Scale = float.Parse(CropScaleInput.text);
            crop.Variant = int.Parse(CropVariantInput.text);

            CropManager.Instance.UpdateCropType(crop);
        }
        else if (ModeDropdown.value == 1)
        {
            /*if (int.Parse(LevelNumberInput.text) >= Map.Instance.Data.MapLevelDatas.Count)
            {
                LogText.text = "Level cannot be greater than " + Map.Instance.Data.MapLevelDatas.Count;
                return;
            }*/
            MapLevel level = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>();
            Map.Instance.ChangeLevelOrder(level, int.Parse(LevelNumberInput.text));
        }
        UpdateMode();
    }

    public void SaveAll()
    {
        //if (Application.isEditor) return;
        MapMakerConfig config = Config;
        var mapData = config.GetCurMapData();
        foreach (MapLevel mapLevel in Map.Instance.mapLevels)
        {
            LevelData data = new LevelData();
            data.PosX = mapLevel.transform.localPosition.x;
            data.PosY = mapLevel.transform.localPosition.y;
            mapData.LevelDatas.Add(data);
        }
        foreach (Crop crop in FindObjectsOfType<Crop>())
        {
            CropData data = new CropData();
            data.PosX = crop.transform.localPosition.x;
            data.PosY = crop.transform.localPosition.y;
            data.Name = crop.Name;
            data.Scale = crop.Scale;
            data.Variant = crop.Variant;
            mapData.CropDatas.Add(data);
        }
        for (int i = 0; i < MapTrack.Instance.TrackPointsGroup.childCount; i++)
        {
            TrackData data = new TrackData();
            data.PosX = MapTrack.Instance.TrackPointsGroup.GetChild(i).localPosition.x;
            data.PosY = MapTrack.Instance.TrackPointsGroup.GetChild(i).localPosition.y;
            mapData.TrackDatas.Add(data);
        }
        string json = JsonUtility.ToJson(config);

        string file = Application.dataPath + "/MapMakerConfig.json";
        if (!File.Exists(file)) File.Create(file);
        File.WriteAllText(file, json);
    }

    public void Add()
    {
        if (ModeDropdown.value == 1)
        {
            Map.Instance.AddLevelButton(Map.Instance.LevelsGroup.InverseTransformPoint(Vector3.zero));
        }
        else if (ModeDropdown.value == 2)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Crops/Crop_Cabbage"), CropManager.Instance.CropGrpup);
            obj.transform.localPosition = CropManager.Instance.CropGrpup.InverseTransformPoint(Vector3.zero);
            CropManager.Instance.UpdateCropsView();
        }
        else if (ModeDropdown.value == 3)
        {

        }
        UpdateMode();
    }

    public void Delete()
    {
        if (ModeDropdown.value == 1)
        {
            MapLevel maplevel = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>();
            Map.Instance.mapLevels.Remove(maplevel);
            Map.Instance.UpdateLevelButtons();
        }
        else if (ModeDropdown.value == 2)
        {
            Crop crop = MapMakerPlaceholder.Target.parent.GetComponent<Crop>();
            Destroy(crop.gameObject);
        }
        else if (ModeDropdown.value == 3)
        {

        }
        UpdateMode();
    }

    public void ClearSave()
    {
        SaveManager.Instance.ClearAndRestart();
    }
    public void CheatHarvest()
    {
        CropHarvest.Instance.Cheat();
    }


    public static MapMakerConfig Config
    {
        get
        {
            string json;
            if (Debug.isDebugBuild && File.Exists(Application.dataPath + "/MapMakerConfig.json"))
            {
                json = File.ReadAllText(Application.dataPath + "/MapMakerConfig.json");
            }
            else
            {
                json = ConfigsAsset.GetConfig("MapMakerConfig");
            }
            MapMakerConfig config = JsonUtility.FromJson<MapMakerConfig>(json);
            return config;
        }
    }
}

[System.Serializable]
public class MapMakerConfigOld
{
    public List<LevelData> LevelDatas = new List<LevelData>();
    public List<CropData> CropDatas = new List<CropData>();
    public List<TrackData> TrackDatas = new List<TrackData>();
}

[System.Serializable]
public class MapMakerConfig
{
    public List<STA.MapMaker.MapData> MapDatas = new List<STA.MapMaker.MapData>();
    public STA.MapMaker.MapData GetCurMapData()
    {
        return MapDatas[MapManager.Instance.CurMapNumber - 1];
    }

    public int LevelToStarting(int levelNumber)
    {
        int levelCount = 0;
        for (int i = 0; i < MapDatas.Count; i++)
        {
            if (levelNumber <= levelCount + MapDatas[i].LevelDatas.Count)
                return levelCount + 1;
            levelCount += MapDatas[i].LevelDatas.Count;
        }
        return levelCount + 1;
    }

    public int GetLevelCount(int mapNumber)
    {
        int levelCount = 0;
        for (int i = 0; i < mapNumber; i++)
        {
            levelCount += MapDatas[i].LevelDatas.Count;
        }
        return levelCount;
    }

    public int LevelToMap(int levelNumber)
    {
        int levelCount = 0;
        for (int i = 0; i < MapDatas.Count; i++)
        {
            if (levelNumber <= levelCount + MapDatas[i].LevelDatas.Count)
                return i + 1;
            levelCount += MapDatas[i].LevelDatas.Count;
        }
        return MapDatas.Count;
    }
}


namespace STA.MapMaker
{
    [System.Serializable]
    public class MapData
    {
        public List<LevelData> LevelDatas = new List<LevelData>();
        public List<CropData> CropDatas = new List<CropData>();
        public List<TrackData> TrackDatas = new List<TrackData>();
        public List<LuckyWheelData> WheelDatas = new List<LuckyWheelData>();
    }

    [System.Serializable]
    public class LevelData
    {
        public float PosX = 0;
        public float PosY = 0;
    }

    [System.Serializable]
    public class CropData
    {
        public float PosX = 0;
        public float PosY = 0;
        public string Name = "Crop";
        public float Scale = 0.5f;
        public int Variant = 0;
    }

    [System.Serializable]
    public class TrackData
    {
        public float PosX = 0;
        public float PosY = 0;
    }

    [System.Serializable]
    public class LuckyWheelData
    {
        public float PosX = 0;
        public float PosY = 0;
        public int LevelNumber = 0;
    }
}
