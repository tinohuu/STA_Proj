using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager)), CanEditMultipleObjects]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //SaveManager saveManager = target as SaveManager;
        if (SaveManager.Save != null)
        foreach (object data in SaveManager.Save.Datas)
        {
            EditorGUILayout.LabelField(" - " + data.GetType().ToString());
            //GUI.Label(data.GetType().ToString());
        }
    }
}
