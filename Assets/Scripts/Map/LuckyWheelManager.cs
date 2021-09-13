using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyWheelManager : MonoBehaviour
{
    [SerializeField] GameObject luckyWheelPrefab;
    [SerializeField] Transform luckyWheelGroup;
    [SavedData] public LuckyWheelManagerData Data = new LuckyWheelManagerData();
    private void Start()
    {
        
    }
    public void UpdateAllViews()
    {
        var wheels = GetComponentsInChildren<LuckyWheel>();
        foreach (var wheel in wheels) wheel.UpdateView();
    }

    public void CreateWheels(List<Vector2> locPoses, List<int> levelNumbers)
    {
        luckyWheelGroup.DestroyChildren();

        for (int i = 0; i < locPoses.Count; i++)
        {
            LuckyWheel luckyWheel = Instantiate(luckyWheelPrefab, luckyWheelGroup).GetComponent<LuckyWheel>();
            luckyWheel.transform.localPosition = locPoses[i];
            luckyWheel.LevelNumber = levelNumbers[i];
        }
        UpdateAllViews();
    }    
}

[System.Serializable]
public class LuckyWheelManagerData
{
    public int CollectedLevel = 0;
}
