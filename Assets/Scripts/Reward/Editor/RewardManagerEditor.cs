using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(RewardManager))]
public class RewardManagerEditor : Editor
{
    RewardManager rewardManager;

    private void OnEnable()
    {
        rewardManager = (RewardManager)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        for (int i = 1; i < RewardManager.Instance.Data.Rewards.Count; i++)
        {
            string name = ((RewardType)i).ToString();
            EditorGUILayout.LabelField(name + ": " + rewardManager.Data.Rewards[i]);
        }
        Repaint();
    }
}