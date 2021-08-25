using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This line makes the asset show up in the Create Asset menus:
[CreateAssetMenu(fileName = "Configs.asset", menuName = "STA Assets/Configs")]
public class ConfigsAsset : ScriptableObject
{
    public List<TextAsset> Configs = new List<TextAsset>();
    public string Label = "Config";
    public bool Auto = true;
    public List<string> Names = new List<string>();
    public List<string> Bundles = new List<string>();
    public List<string> Jsons = new List<string>();

    public static string GetConfig(string name)
    {
        // Load Configs Asset

        // First load from asset bundles
        //if (!Debug.isDebugBuild)
        //{

        //UnityEngine.Object levelFile = levelBundle.LoadAsset(strLevelName);
        /*IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
        foreach (AssetBundle bundle in bundles)
            foreach (string assetName in bundle.GetAllAssetNames())
            {
                string[] assetFileName = assetName.Split(new[] { "/" }, StringSplitOptions.None);
                if (name.ToLower()+".json" == assetFileName[assetFileName.Length - 1])
                {
                    Debug.Log("ConfigAsset::GetFromAssetBundles");
                    return bundle.LoadAsset(assetName).ToString();
                }
            }*/

        ConfigsAsset asset = Resources.Load<ConfigsAsset>("Configs");
        if (!asset) asset = Resources.Load<ConfigsAsset>("Configs/Configs");

        if (!asset)
        {
            Debug.LogWarning("Please create a Configs Asset in Resources/Configs folder");
            return "";
        }

        if (asset.Names.Count == 0 || asset.Jsons.Count == 0 || asset.Names.Count != asset.Jsons.Count)
        {
            Debug.LogWarning("Please reimport the Configs Asset.");
            return "";
        }
        int index = asset.Names.IndexOf(name);
        return asset.Jsons[index];
    }

    public static T GetConfigObject<T>(string name)
    {
        string checkedName = name == "" ? GetConfig(typeof(T).ToString()) : name;
        return JsonUtility.FromJson<T>(GetConfig(name));
    }

    public static List<T> GetConfigList<T>(string name = "")
    {
        string checkedName = name == "" ? GetConfig(typeof(T).ToString()) : name;
        return JsonExtensions.JsonToList<T>(checkedName);
    }

    public static T[] GetConfigArray<T>(string name = "")
    {
        return GetConfigList<T>(name).ToArray();
    }
}