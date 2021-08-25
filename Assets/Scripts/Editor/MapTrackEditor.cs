using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapTrack))]
public class MapTrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

    }

}