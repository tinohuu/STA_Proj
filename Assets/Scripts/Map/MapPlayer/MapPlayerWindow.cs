using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MapPlayerWindow : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] TMP_Text TitleText;
    [SerializeField] TMP_Text CropText;
    [SerializeField] TMP_Text CurLevelText;
    [SerializeField] TMP_Text RatingCountText;
    [SerializeField] TMP_Text DailyGoodiesCountText;
    [SerializeField] TMP_Text LuckyWheelCountText;

    private void Start()
    {
        RefreshView();
    }


    void RefreshView()
    {
        CropConfig curCrop = CropManager.Instance.LevelToCropConfig(MapManager.Instance.Data.CompleteLevel);
        if (curCrop != null) CropText.Format(curCrop.ID);

        CurLevelText.Format(MapManager.Instance.Data.CompleteLevel);

        int ratingCount = MapManager.Instance.Data.MapLevelDatas.FindAll(e => e.ID <= MapManager.Instance.Data.CompleteLevel).Sum(e => e.Rating);
        RatingCountText.Format(ratingCount.ToString("N0"));

        DailyGoodiesCountText.Format(DailyGoodiesManager.Instance.Data.StreakDays.ToString());

        LuckyWheelCountText.Format(LuckyWheelManager.Instance.Data.WheelCollectedLevel);

        RefreshTitle(ratingCount);
    }

    void RefreshTitle(int ratingCount)
    {
        var config = ConfigsAsset.GetConfigObject<PlayerPublicConfig>();
        int levelCountPerTitle = MapManager.Instance.Data.MapLevelDatas.Count / config.Rank;
        int titleID = Mathf.Clamp(MapManager.Instance.Data.CompleteLevel / levelCountPerTitle + 1, 1, config.Title);

        int levelCountPerPrefix = levelCountPerTitle / config.Rank;
        int prefixID = Mathf.Clamp(ratingCount / levelCountPerPrefix + 1, 1, config.Rank);

        TitleText.Format(("TXT_PLY_RNK" + prefixID).ToLocalized(), ("TXT_PLY_TIT" + titleID).ToLocalized());
    }
}

public class PlayerPublicConfig
{
    public int Rank;
    public int Title;
}
