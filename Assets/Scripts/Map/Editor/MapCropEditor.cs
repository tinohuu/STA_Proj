using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CropSpine)), CanEditMultipleObjects]
public class MapCropEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CropSpine cropSpine = target as CropSpine;

        if (GUILayout.Button(cropSpine.Controllers.Count > 0 ? "Update Controllers" : "Create Conterollers"))
        {
            RuntimeAnimatorController oriController = GetAssetByName<RuntimeAnimatorController>("Controller_Crop");
            cropSpine.Controllers.Clear();

            int variantIndex = 0;

            while (true)
            {
                AnimatorOverrideController controller = new AnimatorOverrideController();
                controller.runtimeAnimatorController = oriController;

                string assetName;

                List<int> animCounts = new List<int> { 0, 0, 0, 0 };

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Locked_Idle";
                if (UpdateConrtoller(controller, "Crop_Locked_Idle", assetName)) animCounts[(int)Crop.State.locked]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Unlocking_Idle";
                if (UpdateConrtoller(controller, "Crop_Unlocking_Idle", assetName)) animCounts[(int)Crop.State.unlocking]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Immature_Idle";
                if (UpdateConrtoller(controller, "Crop_Immature_Idle", assetName)) animCounts[(int)Crop.State.immature]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Mature_Idle";
                if (UpdateConrtoller(controller, "Crop_Mature_Idle", assetName)) animCounts[(int)Crop.State.mature]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Mature_ToMature";
                if (UpdateConrtoller(controller, "Crop_Mature_ToMature", assetName)) animCounts[(int)Crop.State.mature]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Mature_Harvest";
                if (UpdateConrtoller(controller, "Crop_Mature_Harvest", assetName)) animCounts[(int)Crop.State.mature]++;

                assetName = cropSpine.SpineName + "_" + variantIndex + "_Mature_ToImmature";
                if (UpdateConrtoller(controller, "Crop_Mature_ToImmature", assetName)) animCounts[(int)Crop.State.mature]++;

                AssetDatabase.CreateAsset(controller, "Assets/Animations/Crops/Controller_" + cropSpine.SpineName + "_" + variantIndex + ".overrideController");
                cropSpine.Controllers.Add(controller);
                variantIndex++;

                if (animCounts.Sum() <= 0) break;

                cropSpine.MinState = (Crop.State)animCounts.FindIndex(e => e > 0);
                cropSpine.MaxState = (Crop.State)animCounts.FindLastIndex(e => e > 0);
            }
            EditorUtility.SetDirty(cropSpine);
        }

        /*
        if (crop.Controllers.Count > 0)
        {
            if (GUILayout.Button("Randomize Variant"))
            {
                foreach (Object t in targets)
                {
                    Crop targetCrop = t as Crop;
                    if (targetCrop) targetCrop.MapmakerConfig.Variant = Random.Range(0, crop.Controllers.Count);
                    EditorUtility.SetDirty(targetCrop);
                }
            }
        }*/
    }

    bool UpdateConrtoller(AnimatorOverrideController controller, string oldName, string newName)
    {
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