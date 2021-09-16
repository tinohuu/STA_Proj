using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.MapMaker;

public class CrateManager : MonoBehaviour//IMapMakerModule
{
    [SerializeField] GameObject cratePrefab;
    [SerializeField] Transform crateGroup;

    //public MapMaker_BaseData MapMakerData => throw new System.NotImplementedException();

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
