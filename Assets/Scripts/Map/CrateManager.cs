using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.MapMaker;

public class CrateManager : MonoBehaviour, IMapMakerModule
{
    [SerializeField] GameObject cratePrefab;
    [SerializeField] Transform crateGroup;

    public static CrateManager Instance = null;
    void Start()
    {
        if (!Instance) Instance = this;
        MapMaker_CreateItems();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MapMaker_AddItem()
    {
        GameObject obj = Instantiate(cratePrefab, crateGroup);
        obj.transform.localPosition = crateGroup.InverseTransformPoint(Vector3.zero);
        // todo: update all views
    }

    public void MapMaker_CreateItems()
    {
        // Get config data
        var datas = MapManager.MapMakerConfig.CurMapData.CrateDatas;

        // Recreate new lucky wheels
        crateGroup.DestroyChildren();

        foreach (var data in datas)
        {
            Crate crate = Instantiate(cratePrefab, crateGroup).GetComponent<Crate>();
            crate.transform.localPosition = new Vector2(data.PosX, data.PosY);
            crate.LevelNumber = data.LevelNumber;
        }
        // todo: update all views
    }

    public void MapMaker_RecordData()
    {
        var crates = crateGroup.GetComponentsInChildren<Crate>();
        var datas = new List<MapMaker_CrateData>();
        foreach (var crate in crates)
        {
            MapMaker_CrateData data = new MapMaker_CrateData();
            data.LevelNumber = crate.LevelNumber;
            data.PosX = crate.transform.localPosition.x;
            data.PosY = crate.transform.localPosition.y;
            datas.Add(data);
        }
        MapManager.MapMakerConfig.CurMapData.CrateDatas = datas;
    }
}

namespace STA.MapMaker
{
    [System.Serializable]
    public class MapMaker_CrateData
    {
        public float PosX = 0;
        public float PosY = 0;
        public int LevelNumber = 0;
    }
}
