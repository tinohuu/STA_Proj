using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapPlayerWindow : MonoBehaviour
{
    public TMP_Text CropText;
    public TMP_Text CurLevelText;
    public TMP_Text RatingCountText;
    private void Start()
    {
        CropConfig curCrop = CropManager.Instance.LevelToCropConfig(MapManager.Instance.Data.CompelteLevel);
        CropText.text = "Crop " + (CropManager.Instance.CropConfigs.IndexOf(curCrop) + 1).ToString() + " " + curCrop.Name;
        CurLevelText.text = "Level " + MapManager.Instance.Data.CompelteLevel;
        RatingCountText.text = GetRatingCount().ToString();

    }

    int GetRatingCount()
    {
        int count = 0;
        foreach (MapData mapData in MapManager.Instance.Data.MapDatas)
        {
            foreach (MapLevelData levelData in mapData.MapLevelDatas)
            {
                count += levelData.Rating;
                if (levelData.Order == MapManager.Instance.Data.CompelteLevel) return count;
            }
        }
        return count;
    }
}
