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
        SaveManager saveManager = target as SaveManager;
        if (saveManager.Save == null) return;
        foreach (object data in saveManager.Save.Datas)
        {
            EditorGUILayout.LabelField(" - " + data.GetType().ToString());
            //GUI.Label(data.GetType().ToString());
        }
        Repaint();
    }
}
