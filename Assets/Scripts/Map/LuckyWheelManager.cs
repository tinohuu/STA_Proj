using STA.MapMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyWheelManager : MonoBehaviour, IMapMakerModule
{
    [SerializeField] GameObject wheelPrefab;
    [SerializeField] Transform wheelGroup;
    public static LuckyWheelManager Instance = null;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    private void Start()
    {
        
    }

    public void CreateItems()
    {
        // Get config data
        var datas = MapManager.MapMakerConfig.GetCurMapData().WheelDatas;

        // Recreate new lucky wheels
        wheelGroup.DestroyChildren();
        foreach (var data in datas)
        {
            LuckyWheel luckyWheel = Instantiate(wheelPrefab, wheelGroup).GetComponent<LuckyWheel>();
            luckyWheel.transform.localPosition = new Vector2(data.PosX, data.PosY);
            luckyWheel.LevelNumber = data.LevelNumber;
        }
        UpdateAllViews();
    }

    public void RecordItemMakerData()
    {
        var wheels = wheelGroup.GetComponentsInChildren<LuckyWheel>();
        var datas = new List<MapMaker_WheelData>();
        foreach (var wheel in wheels)
        {
            MapMaker_WheelData data = new MapMaker_WheelData();
            data.LevelNumber = wheel.LevelNumber;
            data.PosX = wheel.transform.localPosition.x;
            data.PosY = wheel.transform.localPosition.y;
            datas.Add(data);
        }
        MapManager.MapMakerConfig.GetCurMapData().WheelDatas = datas;
    }

    public void UpdateAllViews()
    {
        var wheels = wheelGroup.GetComponentsInChildren<LuckyWheel>();
        foreach (var wheel in wheels) wheel.UpdateView();
    }

    public void AddNewItem()
    {
        GameObject obj = Instantiate(wheelPrefab, wheelGroup);
        obj.transform.localPosition = wheelGroup.InverseTransformPoint(Vector3.zero);
        UpdateAllViews();
    }
}
