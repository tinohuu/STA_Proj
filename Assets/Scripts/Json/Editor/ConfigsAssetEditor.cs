using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(ConfigsAsset))]
public class ConfigsAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var asset = (ConfigsAsset)target;

        if (!Resources.Load<ConfigsAsset>("Configs/Configs"))
        {
            EditorGUILayout.HelpBox("Please have this asset moved to 'Resources/Configs' folder, and named 'Configs'.", MessageType.Error);
            if (GUILayout.Button("Fix"))
            {
                string path = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.MoveAsset(path, "Assets/Resources/Configs/Configs.asset");
            }
        }
        else
        {
            if (asset.Auto) EditorGUILayout.HelpBox("Automatically label, associate, and import configs when importing or deleting assets named with Config and followed by .json.", MessageType.Info);

            if (GUILayout.Button("Associate")) Associate(asset);
            EditorGUILayout.HelpBox("Find assets labeled: " + asset.Label + ".", MessageType.None);

            if (GUILayout.Button(asset.Names.Count == 0 ? "Import" : "Reimport")) Import(asset);

            if (asset.Names.Count == 0 || asset.Jsons.Count == 0 || asset.Names.Count != asset.Jsons.Count)
                EditorGUILayout.HelpBox("Please reimport configs.", MessageType.Warning);
            else EditorGUILayout.HelpBox(asset.Names.Count + " configs imported.", MessageType.None);

            if (GUILayout.Button("Associate & Import"))
            {
                Associate(asset);
                Import(asset);
            }
        }
    }

    public static void Associate(ConfigsAsset asset)
    {
        Undo.RecordObject(asset, "Associate Labeled Configs");

        string[] guids = AssetDatabase.FindAssets("l: " + asset.Label);
        asset.Configs.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            if (textAsset && !asset.Configs.Contains(textAsset)) asset.Configs.Add(textAsset);
        }
        EditorUtility.SetDirty(asset);
    }

    public static void Import(ConfigsAsset asset)
    {
        Undo.RecordObject(asset, "Import Jsons");
        asset.Names = new List<string>();
        asset.Jsons = new List<string>();
        foreach (TextAsset config in asset.Configs)
        {
            asset.Names.Add(config.name);
            string[] bundleFolders = AssetDatabase.GetAssetPath(config).Split(new[] { "/" }, StringSplitOptions.None);
            asset.Bundles.Add(bundleFolders[bundleFolders.Length - 2]);
            asset.Jsons.Add(config.text);
        }
        EditorUtility.SetDirty(asset);
    }
}

public class ConfigsAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        ConfigsAsset configsAsset = Resources.Load<ConfigsAsset>("Configs/Configs");
        if (!configsAsset || !configsAsset.Auto) return;

        bool isChanged = false;
        foreach (string asset in importedAssets)
        {
            string[] strings = asset.Split(".".ToCharArray());
            if (strings[strings.Length - 1] == "json" && asset.Contains("Config"))
            {
                isChanged = true;
                TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(asset, typeof(TextAsset));
                AssetDatabase.SetLabels(textAsset, new string[] { configsAsset.Label });
            }
        }
        foreach (string asset in deletedAssets)
        {
            string[] strings = asset.Split(".".ToCharArray());
            if (strings[strings.Length - 1] == "json" && asset.Contains("Config"))
            {
                isChanged = true;
            }
        }

        if (isChanged)
        {
            ConfigsAssetEditor.Associate(configsAsset);
            ConfigsAssetEditor.Import(configsAsset);
        }
    }
}