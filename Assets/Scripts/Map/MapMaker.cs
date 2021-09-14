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
    public enum Mode { None, Level, Crop, Track, Wheel, Crate }
    // Add a layer called MapMaker
    [SerializeField] Transform InputGroup;
    [SerializeField] Dropdown ModeDropdown;
    [SerializeField] InputField CropNameInput;
    [SerializeField] InputField CropScaleInput;
    [SerializeField] InputField CropVariantInput;
    [SerializeField] InputField LevelNumberInput;
    [SerializeField] Button ApplyButton;
    [SerializeField] Button AddButton;
    [SerializeField] Text LogText;
    [SerializeField] GameObject PlaceholderPrefab;
    [SerializeField] Slider ProgressSlider;

    public static MapMaker Instance = null;
    public Mode CurMode => (Mode)ModeDropdown.value;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        // Set mode dropdown according to enum
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        int optionCount = System.Enum.GetValues(typeof(Mode)).Length;
        for (int i = 0; i < optionCount; i++)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData(((Mode)i).ToString());
            optionDatas.Add(optionData);
        }
        ModeDropdown.options = optionDatas;
        ModeDropdown.onValueChanged.AddListener((int i) => UpdateMode());

        // Initialize tools
        ApplyButton.onClick.AddListener(() => Apply());
        ProgressSlider.onValueChanged.AddListener((float f) => Map.Instance.SetProgress(f));
        ProgressSlider.value = MapManager.Instance.Data.CompleteLevel / 186f;
    }

    void Refresh(bool record = false, bool recreate = false)
    {
        // Record or recreate items for all system implementing MapMakerModule
        var modules = FindObjectsOfType<MonoBehaviour>().OfType<IMapMakerModule>();
        foreach (var module in modules)
        {
            if (record) module.RecordItemMakerData();
            if (recreate) module.CreateItems();
        }
    }

    public void UpdateInputView()
    {
        // Reset input texts
        var inputs = GetComponentsInChildren<InputField>();
        foreach (InputField input in inputs) input.text = "";

        // Activate corresponding inputs
        LevelNumberInput.gameObject.SetActive(CurMode == Mode.Level || CurMode == Mode.Wheel);
        CropNameInput.gameObject.SetActive(ModeDropdown.value == 2);
        CropScaleInput.gameObject.SetActive(ModeDropdown.value == 2);
        CropVariantInput.gameObject.SetActive(ModeDropdown.value == 2);

        // Show selection details
        if (!MapMakerPlaceholder.Target) return;
        switch (CurMode)
        {
            case Mode.Level:
                LevelNumberInput.text = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>().Data.Number.ToString();
                break;

            case Mode.Crop:
                CropNameInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Name;
                CropScaleInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Scale.ToString();
                CropVariantInput.text = MapMakerPlaceholder.Target.parent.GetComponent<Crop>().Variant.ToString();
                break;

            case Mode.Track:

                break;

            case Mode.Wheel:
                LevelNumberInput.text = MapMakerPlaceholder.Target.parent.GetComponent<LuckyWheel>().LevelNumber.ToString();
                break;

            case Mode.Crate:
                break;
        }
    }

    public void UpdateMode()
    {
        // Clear all placeholders (gear buttons)
        MapMakerPlaceholder[] placeholders = FindObjectsOfType<MapMakerPlaceholder>();
        for (int i = 0; i < placeholders.Length; i++) Destroy(placeholders[i].gameObject);

        // Create placeholders
        switch (CurMode)
        {
            case Mode.Level:
                MapLevel[] mapLevels = FindObjectsOfType<MapLevel>();
                for (int i = 0; i < mapLevels.Length; i++) Instantiate(PlaceholderPrefab, mapLevels[i].transform);
                break;

            case Mode.Crop:
                Crop[] crops = FindObjectsOfType<Crop>();
                for (int i = 0; i < crops.Length; i++) Instantiate(PlaceholderPrefab, crops[i].transform);
                break;

            case Mode.Track:
                for (int i = 0; i < MapTrack.Instance.TrackPointsGroup.childCount; i++) Instantiate(PlaceholderPrefab, MapTrack.Instance.TrackPointsGroup.GetChild(i));
                break;

            case Mode.Wheel:
                var wheels = FindObjectsOfType<LuckyWheel>();
                foreach (var wheel in wheels) Instantiate(PlaceholderPrefab, wheel.transform);
                break;

            case Mode.Crate:
                break;
        }

        MapMakerPlaceholder.Target = null;
        UpdateInputView();
    }

    public void Apply()
    {
        if (!MapMakerPlaceholder.Target) return;

        switch (CurMode)
        {
            case Mode.Level:
                MapLevel level = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>();
                Map.Instance.ChangeLevelNumber(level, int.Parse(LevelNumberInput.text));
                break;

            case Mode.Crop:
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
                break;

            case Mode.Track:
                LuckyWheel wheel = MapMakerPlaceholder.Target.parent.GetComponent<LuckyWheel>();
                wheel.LevelNumber = int.Parse(LevelNumberInput.text);
                break;

            case Mode.Wheel:
                break;
            case Mode.Crate:
                break;
        }

        Refresh(true, true);
        UpdateMode();
    }

    public void Add()
    {
        switch (CurMode)
        {
            case Mode.Level:
                Map.Instance.AddNewItem();
                break;

            case Mode.Crop:
                CropManager.Instance.AddNewItem();
                break;

            case Mode.Track:
                for (int i = 0; i < MapTrack.Instance.TrackPointsGroup.childCount; i++) Instantiate(PlaceholderPrefab, MapTrack.Instance.TrackPointsGroup.GetChild(i));
                break;

            case Mode.Wheel:
                LuckyWheelManager.Instance.AddNewItem();
                break;

            case Mode.Crate:
                break;
        }
        UpdateMode();
    }

    public void Delete()
    {
        switch (CurMode)
        {
            case Mode.Level:
                MapLevel maplevel = MapMakerPlaceholder.Target.parent.GetComponent<MapLevel>();
                DestroyImmediate(maplevel.gameObject);
                break;
            case Mode.Crop:
                Crop crop = MapMakerPlaceholder.Target.parent.GetComponent<Crop>();
                DestroyImmediate(crop.gameObject);
                break;
            case Mode.Track:
                for (int i = 0; i < MapTrack.Instance.TrackPointsGroup.childCount; i++) Instantiate(PlaceholderPrefab, MapTrack.Instance.TrackPointsGroup.GetChild(i));
                break;
            case Mode.Wheel:
                LuckyWheel wheel = MapMakerPlaceholder.Target.parent.GetComponent<LuckyWheel>();
                DestroyImmediate(wheel.gameObject);
                break;
            case Mode.Crate:
                break;
        }
        Refresh(true, true);
    }

    public void Save()
    {
        var modules = FindObjectsOfType<MonoBehaviour>().OfType<IMapMakerModule>();
        foreach (var module in modules) module.CreateItems();
        string json = JsonUtility.ToJson(MapManager.MapMakerConfig);
        string file = Application.dataPath + "/MapMakerConfig.json";
        if (!File.Exists(file))
        {
            var fs = File.Create(file);
            fs.Close();
        }
        File.WriteAllText(file, json);
        LogText.text = "Successfully saved.";
    }

    public void CheatHarvest()
    {
        CropHarvest.Instance.Cheat();
    }

    private void OnDestroy()
    {
        ModeDropdown.value = 0;
        UpdateMode();
        Refresh(false, true);
    }
}

namespace STA.MapMaker
{
    [System.Serializable]
    public class MapMaker_Config
    {
        public List<MapMaker_MapData> MapDatas = new List<STA.MapMaker.MapMaker_MapData>();
        public MapMaker_MapData GetCurMapData()
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

    [System.Serializable]
    public class MapMaker_MapData
    {
        public int MapNumber = 0;
        public List<MapMaker_LevelData> LevelDatas = new List<MapMaker_LevelData>();
        public List<MapMaker_CropData> CropDatas = new List<MapMaker_CropData>();
        public List<MapMaker_TrackData> TrackDatas = new List<MapMaker_TrackData>();
        public List<MapMaker_WheelData> WheelDatas = new List<MapMaker_WheelData>();
    }

    [System.Serializable]
    public class MapMaker_TrackData
    {
        public float PosX = 0;
        public float PosY = 0;
    }

    [System.Serializable]
    public class MapMaker_WheelData
    {
        public float PosX = 0;
        public float PosY = 0;
        public int LevelNumber = 0;
    }

    public interface IMapMakerModule
    {
        public void RecordItemMakerData();
        public void CreateItems();
        public void AddNewItem();
    }
}
