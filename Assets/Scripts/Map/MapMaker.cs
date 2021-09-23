using STA.Mapmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mapmaker : MonoBehaviour
{
    public enum Mode { None, Level, Crop, Track, Wheel, Crate }

    [Header("Asset Ref")]
    [SerializeField] GameObject InputPrefab;
    [SerializeField] GameObject PlaceholderPrefab;

    [Header("Self Ref")]
    [SerializeField] Dropdown ModeDropdown;
    [SerializeField] Transform InputGroup;
    [SerializeField] Button ApplyButton;
    [SerializeField] Button AddButton;
    [SerializeField] Button DeleteButton;
    [SerializeField] Button SaveButton;
    [SerializeField] Button CheatButton;
    [SerializeField] Slider ProgressSlider;
    [SerializeField] Text LogText;

    public static Mapmaker Instance = null;

    public static List<IMapmakerModule> Modules;
    public IMapmakerModule CurModule { get => Modules[ModeDropdown.value]; }

    private void Awake()
    {
        if (!Instance) Instance = this;

        ModeDropdown.onValueChanged.AddListener((int i) => ApplyMode());
        AddButton.onClick.AddListener(() => AddItem());
        ApplyButton.onClick.AddListener(() => ApplyInputs());
        DeleteButton.onClick.AddListener(() => DeleteItem());
        SaveButton.onClick.AddListener(() => SaveConfig());
        CheatButton.onClick.AddListener(() => CropHarvest.Instance.Cheat());
        ProgressSlider.onValueChanged.AddListener((float ratio) => MapManager.Instance.SetProgress(ratio));
        ProgressSlider.onValueChanged.AddListener((float ratio) => CropManager.Instance.UpdateCropsView());

        InitializeModules();
    }

    void Start()
    {
        InitializeDropdown();
        ApplyMode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            MapmakerPlaceholder[] placeholders = FindObjectsOfType<MapmakerPlaceholder>();
            foreach (var p in placeholders)
            {
                Color color = p.GetComponentInChildren<SpriteRenderer>().color;
                color.a = color.a == 0 ? 0.5f : 0;
                p.GetComponentInChildren<SpriteRenderer>().color = color;
            }
        }
    }

    public void InitializeModules()
    {
        var modules = FindObjectsOfType<MonoBehaviour>().OfType<IMapmakerModule>();

        if (Modules == null)
            foreach (var module in modules)
            {
                string json = GetConfig(module);
                if (json == "[]") continue;
            }
        Modules = modules.ToList();
    }

    void InitializeDropdown()
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        int optionCount = Modules.Count;

        for (int i = 0; i < optionCount; i++)
        {
            string modeName = Modules[i].Mapmaker_ItemType.ToString();
            modeName = modeName.Replace("Map", "");
            modeName = Regex.Replace(modeName, "([a-z])([A-Z])", "$1 $2");
            Dropdown.OptionData optionData = new Dropdown.OptionData(modeName);
            optionDatas.Add(optionData);
        }
        ModeDropdown.options = optionDatas;
        ModeDropdown.onValueChanged.AddListener((int i) => ApplyMode());
    }

    public static string GetConfig(IMapmakerModule module, int mapId)
    {
        string fileName = ModuleFilename(module, mapId);
        string json;
        if (Debug.isDebugBuild && File.Exists(Application.dataPath + "/" + fileName))
            json = File.ReadAllText(Application.dataPath + "/" + fileName);
        else json = ConfigsAsset.GetConfig("Mapmaker/" + fileName.Replace(".json", ""));
        return json;
    }
    public static string GetConfig(IMapmakerModule module) => GetConfig(module, MapManager.Instance.MapID);

    public static string ModuleFilename(IMapmakerModule module, int mapId)
    {
        return "MapmakerConfig_" + mapId + "_" + module.Mapmaker_ItemType.ToString() + ".json";
    }

    public static void Log(string log)
    {
        if (Instance) Instance.LogText.text = log;
        Debug.LogWarning(log);
    }

    #region UI
    void ApplyMode(bool recreateInputs = true)
    {
        MapmakerPlaceholder.Target = null;

        MapmakerPlaceholder[] placeholders = FindObjectsOfType<MapmakerPlaceholder>();
        for (int i = 0; i < placeholders.Length; i++) Destroy(placeholders[i].gameObject);

        Type type = CurModule.Mapmaker_ItemType;
        var items = FindObjectsOfType(type) as MonoBehaviour[];
        for (int i = 0; i < items.Length; i++) Instantiate(PlaceholderPrefab, items[i].transform);

        if (recreateInputs) CreateInputs();
    }

    public void CreateInputs()
    {
        InputGroup.DestroyChildrenImmediate();

        string[] infos = CurModule.Mapmaker_InputInfos;
        foreach (string info in infos)
        {
            InputField input = Instantiate(InputPrefab, InputGroup).GetComponent<InputField>();
            input.placeholder.GetComponent<Text>().text = info;
            //input.onEndEdit.AddListener((string s) => ApplyInputs());
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(InputGroup.parent.GetComponent<RectTransform>());
    }

    public void Updatenputs()
    {
        CreateInputs();

        string[] dataText = CurModule.Mapmaker_UpdateInputs(MapmakerPlaceholder.Target.parent);
        if (dataText == null) return;
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
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i].text == "") return;
            inputDatas[i] = inputs[i].text;
        }
        CurModule.Mapmaker_ApplyInputs(MapmakerPlaceholder.Target?.parent, inputDatas);
        ApplyMode();
    }

    void AddItem()
    {
        var item = CurModule.Mapmaker_AddItem();
        ApplyMode(false);
    }

    void DeleteItem()
    {
        if (MapmakerPlaceholder.Target)
            CurModule.Mapmaker_DeleteItem(MapmakerPlaceholder.Target.parent.gameObject);
    }

    public void SaveConfig()
    {
        string logText = "Successfully saved: ";
        foreach (var module in Modules)
        {
            string json = module.Mapmaker_ToConfig();
            if (json == "" || json == "[]" || json == null) continue;

            string file = Application.dataPath + "/" + ModuleFilename(module, MapManager.Instance.MapID);
            if (!File.Exists(file))
            {
                var fs = File.Create(file);
                fs.Close();
            }

            File.WriteAllText(file, module.Mapmaker_ToConfig());
            if (File.Exists(file)) logText += module.Mapmaker_ItemType.ToString() + ", ";
        }
        Log(logText);
    }
    #endregion
}

namespace STA.Mapmaker
{
    public interface IMapmakerModule
    {
        Type Mapmaker_ItemType { get; }
        string[] Mapmaker_InputInfos { get; }
        void Mapmaker_CreateItems(string config);
        string[] Mapmaker_UpdateInputs(Transform target);
        Transform Mapmaker_AddItem();
        void Mapmaker_ApplyInputs(Transform target, string[] inputDatas);
        void Mapmaker_DeleteItem(GameObject target);
        string Mapmaker_ToConfig();
    }

    [System.Serializable]
    public class Mapmaker_BaseConfig
    {
        public float PosX = 0;
        public float PosY = 0;
        public float PosZ = 0;
        public Vector2 LocPos { get => new Vector2(PosX, PosY); set { PosX = value.x; PosY = value.y; } }
    }
}