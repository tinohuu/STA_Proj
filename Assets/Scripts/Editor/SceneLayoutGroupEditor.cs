using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneLayoutGroup))]
public class SceneLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SceneLayoutGroup group = (SceneLayoutGroup)target;

        if (group.transform.childCount == 0) return;
        for (int i = 0; i < group.transform.childCount; i++)
        {
            group.transform.GetChild(i).localPosition = Vector3.zero + group.Offset * i;
        }
    }
}