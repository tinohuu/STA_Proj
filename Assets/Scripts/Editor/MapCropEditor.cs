using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Crop)), CanEditMultipleObjects]
public class MapCropEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Crop crop = target as Crop;
        /*
        if (GUILayout.Button("Update Levels"))
        {
            List<CropConfig> cropConfigs = MapManager.GetCropConfigs();
            Dictionary<string, CropConfig> configsByName = cropConfigs.ToDictionary(p => p.Name);
            MapCrop[] crops = FindObjectsOfType<MapCrop>();
            foreach (MapCrop crop in crops)
            {
                if (!configsByName.ContainsKey(crop.Name)) continue;
                crop.Config = configsByName[crop.Name];
            }
        }
        EditorGUILayout.HelpBox("Update all crops according to Crop Name and Configs.", MessageType.Info);*/

        if (GUILayout.Button(crop.Controllers.Count > 0 ? "Update Controllers" : "Create Conterollers"))
        {
            //string oriControllerGuid = (AssetDatabase.FindAssets("Controller_Crop", null))[0];
            //RuntimeAnimatorController oriController = GuidToAsset<RuntimeAnimatorController>(oriControllerGuid);
            RuntimeAnimatorController oriController = GetAssetByName<RuntimeAnimatorController>("Controller_Crop");
            crop.Controllers.Clear();

            int variantIndex = 0;
            do
            {
                AnimatorOverrideController controller = new AnimatorOverrideController();
                controller.runtimeAnimatorController = oriController;

                if (crop.HasState(Crop.State.locked))
                {
                    string assetName = crop.Name + "_" + variantIndex + "_Locked_Idle";
                    if (!UpdateConrtoller(controller, "Crop_Locked_Idle", assetName)) break;
                }

                if (crop.HasState(Crop.State.unlocking))
                {
                    string assetName = crop.Name + "_" + variantIndex + "_Unlocking_Idle";
                    if (!UpdateConrtoller(controller, "Crop_Unlocking_Idle", assetName)) break;
                }

                if (crop.HasState(Crop.State.immature))
                {
                    string assetName = crop.Name + "_" + variantIndex + "_Immature_Idle";
                    if (!UpdateConrtoller(controller, "Crop_Immature_Idle", assetName)) break;
                }

                if (crop.HasState(Crop.State.mature))
                {
                    string assetName = crop.Name + "_" + variantIndex + "_Mature_Idle";
                    if (!UpdateConrtoller(controller, "Crop_Mature_Idle", assetName)) break;

                    assetName = crop.Name + "_" + variantIndex + "_Mature_ToMature";
                    if (!UpdateConrtoller(controller, "Crop_Mature_ToMature", assetName)) break;

                    assetName = crop.Name + "_" + variantIndex + "_Mature_Harvest";
                    if (!UpdateConrtoller(controller, "Crop_Mature_Harvest", assetName)) break;

                    assetName = crop.Name + "_" + variantIndex + "_Mature_ToImmature";
                    if (!UpdateConrtoller(controller, "Crop_Mature_ToImmature", assetName)) break;
                }

                AssetDatabase.CreateAsset(controller, "Assets/Animations/Crops/Controller_" + crop.Name + "_" + variantIndex + ".overrideController");
                crop.Controllers.Add(controller);
                variantIndex++;
            }
            while (variantIndex < 5);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(crop), "Crop_" + crop.Name);
            crop.gameObject.name = "Crop_" + crop.Name;
            crop.SpinePrefab = GetAssetByName<GameObject>("Crop_" + crop.Name + "_Spine");
            EditorUtility.SetDirty(crop);
        }

        if (crop.Controllers.Count > 0)
        {
            if (GUILayout.Button("Randomize Variant"))
            {
                foreach (Object t in targets)
                {
                    Crop targetCrop = t as Crop;
                    if (targetCrop) targetCrop.Variant = Random.Range(0, crop.Controllers.Count);
                    EditorUtility.SetDirty(targetCrop);
                }
            }
        }
    }

    bool UpdateConrtoller(AnimatorOverrideController controller, string oldName, string newName)
    {
        /*string[] guids = AssetDatabase.FindAssets(newName, null);
        if (guids.Length == 0)
        {
            Debug.LogWarning("Controller creation stopped at finding " + newName);
            return false;
        }
        AnimationClip clip = GuidToAsset<AnimationClip>(guids[0]);*/

        AnimationClip clip = GetAssetByName<AnimationClip>(newName);
        if (clip == null) return false;
        controller[oldName] = clip;

        if (controller["Crop_Start"].name == "Crop_Start") controller["Crop_Start"] = clip;

        return true;
    }

    static T GuidToAsset<T>(string guid)
    {
        object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T));
        return (T)obj;
    }

    static T GetAssetByName<T>(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name, null);
        if (guids.Length == 0)
        {
            Debug.LogWarning("Can't find asset " + name);
            return default(T);
        }
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guids[0]));
        foreach (Object obj in objs)
        {
            if (obj.name == name)
            {
                return (T)(object)obj;
            }
        }
        Debug.LogWarning("Can't find asset " + name);
        return default(T);
    }
}