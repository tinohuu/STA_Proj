using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapLevelManager))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapLevelManager map = (MapLevelManager)target;
        if (GUILayout.Button("Create AOC"))
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
            AssetDatabase.CreateAsset(animatorOverrideController, "Assets/Animations/Crops/OverrideController_Test2.overrideController");
            //animatorOverrideController.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Animations/Crops/AnimatorController_Crop.controller", typeof(RuntimeAnimatorController));
            Debug.LogWarning(AssetDatabase.FindAssets("CabbageImmatureIdle01", null)[0]);
        }
    }
}
