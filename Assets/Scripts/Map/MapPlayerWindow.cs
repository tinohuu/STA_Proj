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
        CropConfig curCrop = CropManager.Instance.LevelToCropConfig(MapManager.Instance.Data.CompleteLevel);
        CropText.text = "Crop " + (CropManager.Instance.CropConfigs.IndexOf(curCrop) + 1).ToString() + " " + curCrop.Name;
        CurLevelText.text = "Level " + MapManager.Instance.Data.CompleteLevel;
        RatingCountText.text = GetRatingCount().ToString();

    }

    int GetRatingCount()
    {
        int count = 0;
        foreach (MapLevelData levelData in MapManager.Instance.Data.MapLevelDatas)
        {
            count += levelData.Rating;
            if (levelData.Number == MapManager.Instance.Data.CompleteLevel) return count;
        }
        return count;
    }
}
