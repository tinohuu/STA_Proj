using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Renamer))]
public class RenamerEditor : Editor
{
    Renamer renamer;

    private void OnEnable()
    {
        renamer = (Renamer)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.HelpBox("Put the index into {0}: e.g. Image_{0}", MessageType.Info);
        if (GUILayout.Button("Rename"))
        {
            int count = renamer.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                renamer.transform.GetChild(i).name = string.Format(renamer.Name, i + renamer.IndexFrom);
            }
        }
    }
}
