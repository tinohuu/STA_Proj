using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapTrackManager))]
public class MapTrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

    }

}