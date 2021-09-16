using STA.MapMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    public enum Mode { None, Level, Crop, Track, Wheel, Crate }

    [Header("Asset Ref")]
    [SerializeField] GameObject InputPrefab;
    [SerializeField] GameObject PlaceholderPrefab;

    [Header("Hierarchy Ref")]
    [SerializeField] Dropdown ModeDropdown;
    [SerializeField] Transform InputGroup;
    [SerializeField] Button ApplyButton;
    [SerializeField] Button AddButton;
    [SerializeField] Button DeleteButton;
    [SerializeField] Button SaveButton;
    [SerializeField] Slider ProgressSlider;
    [SerializeField] Text LogText;

    public static MapMaker Instance = null;

    List<IMapMakerModule> modules = new List<IMapMakerModule>();
    public IMapMakerModule CurModule { get => modules[ModeDropdown.value]; }

    private void Awake()
    {
        if (!Instance) Instance = this;

        ModeDropdown.onValueChanged.AddListener((int i) => CreatePlaceholders());
        AddButton.onClick.AddListener(() => AddItem());
        ApplyButton.onClick.AddListener(() => ApplyInputs());
        DeleteButton.onClick.AddListener(() => DeleteItem());
        SaveButton.onClick.AddListener(() => SaveConfig());
        //ProgressSlider.onValueChanged.AddListener(() => Map.Instance.pro)
    }

    public static void Log(string log)
    {
        if (Instance) Instance.LogText.text = log;
        Debug.LogWarning(log);
    }

    void Start()
    {
        var modules = FindObjectsOfType<MonoBehaviour>().OfType<IMapMakerModule>();
        this.modules = modules.ToList();
        SetModeDropdown();
        CreatePlaceholders();
        CreateInputs();
    }

    void SetModeDropdown()
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        int optionCount = modules.Count;

        for (int i = 0; i < optionCount; i++)
        {
            string modeName = modules[i].MapMaker_ItemType.ToString();
            modeName = Regex.Replace(modeName, "([a-z])([A-Z])", "$1 $2");
            Dropdown.OptionData optionData = new Dropdown.OptionData(modeName);
            optionDatas.Add(optionData);
        }
        ModeDropdown.options = optionDatas;
        ModeDropdown.onValueChanged.AddListener((int i) => CreatePlaceholders());
    }

    void CreatePlaceholders()
    {
        MapMakerPlaceholder.Target = null;

        MapMakerPlaceholder[] placeholders = FindObjectsOfType<MapMakerPlaceholder>();
        for (int i = 0; i < placeholders.Length; i++) Destroy(placeholders[i].gameObject);

        Type type = CurModule.MapMaker_ItemType;
        var items = FindObjectsOfType(type) as MonoBehaviour[];
        for (int i = 0; i < items.Length; i++) Instantiate(PlaceholderPrefab, items[i].transform);
    }

    public void CreateInputs()
    {
        InputGroup.DestroyChildrenImmediate();

        string[] infos = CurModule.MapMaker_InputInfos;
        foreach (string info in infos)
        {
            InputField input = Instantiate(InputPrefab, InputGroup).GetComponent<InputField>();
            input.placeholder.GetComponent<Text>().text = info;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(InputGroup.parent.GetComponent<RectTransform>());
    }

    public void Updatenputs()
    {
        if (InputGroup.childCount == 0) CreateInputs();

        string[] dataText = CurModule.MapMaker_UpdateInputs(MapMakerPlaceholder.Target.parent);
        var inputs = InputGroup.GetComponentsInChildren<InputField>();
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i].text = dataText[i];
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(InputGroup.parent.GetComponent<RectTransform>());
    }

    void ApplyInputs()
    {
        var inputs = InputGroup.GetComponentsInChildren<InputField>();
        string[] inputDatas = new string[inputs.Length];
        for (int i = 0; i < inputs.Length; i++) inputDatas[i] = inputs[i].text;
        CurModule.MapMaker_ApplyInputs(MapMakerPlaceholder.Target.parent, inputDatas);
    }

    void AddItem()
    {
        CurModule.MapMaker_AddItem();
        CreatePlaceholders();
    }

    void DeleteItem()
    {
        DestroyImmediate(MapMakerPlaceholder.Target.parent.gameObject);
    }

    public void SaveConfig()
    {
        foreach (var module in modules)
        {
            string file = Application.dataPath + ModuleFilename(module, 1);
            if (!File.Exists(file))
            {
                var fs = File.Create(file);
                fs.Close();
            }
            File.WriteAllText(file, module.MapMaker_ToConfig());
            if (File.Exists(file)) LogText.text = "Successfully saved " + module.MapMaker_ItemType.ToString();
        }
    }

    public static string GetConfigData(IMapMakerModule module, int mapId)
    {
        string fileName = ModuleFilename(module, mapId);
        string json;
        if (Debug.isDebugBuild && File.Exists(Application.dataPath + fileName))
            json = File.ReadAllText(Application.dataPath + fileName);
        else json = ConfigsAsset.GetConfig(fileName);
        return json;
    }

    public static string ModuleFilename(IMapMakerModule module, int mapId)
    {
        return "/MapMakerConfig_" + mapId + "_" + module.MapMaker_ItemType.ToString() + ".json";
    }
}

namespace STA.MapMaker
{
    public interface IMapMakerModule
    {
        public Type MapMaker_ItemType { get; }
        public string[] MapMaker_InputInfos { get; }
        public void MapMaker_CreateItems();
        public void MapMaker_AddItem();
        public string[] MapMaker_UpdateInputs(Transform target);
        public void MapMaker_ApplyInputs(Transform target, string[] inputDatas);
        public string MapMaker_ToConfig();
    }

    [System.Serializable]
    public class MapMaker_BaseConfig
    {
        public float PosX = 0;
        public float PosY = 0;
        public float PosZ = 0;
        public Vector2 LocPos { get => new Vector2(PosX, PosY); set { PosX = value.x; PosY = value.y; } }
    }
}
