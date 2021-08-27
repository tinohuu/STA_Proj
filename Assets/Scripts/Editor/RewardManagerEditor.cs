using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RewardManager))]
public class RewardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        for (int i = 1; i < Reward.Data.Rewards.Count; i++)
        {
            EditorGUILayout.IntField(((RewardType)i).ToString(), Reward.Data.Rewards[i]);
        }
    }
}