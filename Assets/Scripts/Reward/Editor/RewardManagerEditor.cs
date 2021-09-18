using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(RewardManager))]
public class RewardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        for (int i = 1; i < RewardManager.Data.Rewards.Count; i++)
        {
            string name = ((RewardType)i).ToString();
            EditorGUILayout.LabelField(name + ": " + RewardManager.Data.Rewards[i]);
        }
        Repaint();
    }
}