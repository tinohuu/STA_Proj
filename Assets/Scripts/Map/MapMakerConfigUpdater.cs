using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using STA.MapMaker;

public class MapMakerConfigUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            string json = File.ReadAllText(Application.dataPath + "/MapMakerConfigOld.json");
            MapMakerConfigOld config = JsonUtility.FromJson<MapMakerConfigOld>(json);
            MapMaker_Config newConfig = new MapMaker_Config();
            newConfig.MapDatas.Add(new STA.MapMaker.MapMaker_MapData());
            newConfig.MapDatas[0].CropDatas = config.CropDatas;
            newConfig.MapDatas[0].LevelDatas = config.LevelDatas;
            newConfig.MapDatas[0].TrackDatas = config.TrackDatas;
            json = JsonUtility.ToJson(newConfig);
            string file = Application.dataPath + "/MapMakerConfigNew.json";
            if (!File.Exists(file)) File.Create(file);
            File.WriteAllText(file, json);
        }*/
    }
}
