using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLevelWindow : Window
{
    public TMP_Text OrderText = null;
    public Transform StarGroup = null;
    public TMP_Text CostText = null;
    public MapLevelData LevelData;

    List<MapLevelWindowPowerup> powerups = new List<MapLevelWindowPowerup>();
    MapLevelWindowBoost boost = null;
    private void Awake()
    {
        boost = GetComponentInChildren<MapLevelWindowBoost>();
        powerups = GetComponentsInChildren<MapLevelWindowPowerup>().ToList();
    }

    private void Start()
    {
        if (powerups.Count > 0)
        {
            var rawRewards = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].PowerUpsOrder.Split('_');
            for (int i = 0; i < rawRewards.Length; i++)
            {
                int rewardID;
                if (int.TryParse(rawRewards[i], out rewardID))
                    powerups[i].RewardType = (RewardType)rewardID;
            }
        }

        UpdateView();
    }

    public void UpdateView()
    {
        OrderText.text = LevelData.ID.ToString();
        for (int i = 0; i < StarGroup.childCount; i++)
        {
            Image image = StarGroup.GetChild(i).GetComponent<Image>();
            image.color = i < LevelData.Rating ? Color.white : Color.gray;
        }

        int cost = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].Cost;
        if (boost) cost *= boost.GetBoostModifer();

        CostText.text = Reward.Data[RewardType.FreeRound] > 0
            ? "<sprite name=\"FreeRound\"> " + Reward.Data[RewardType.FreeRound] + " Left"
            : "Cost " + "<sprite name=\"Coin\"> " + cost.ToString("N0"); // todo: cost curve
    }

    public void Play()
    {
        int cost = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].Cost;
        if (boost) cost *= boost.GetBoostModifer();

        if (Reward.Coin < cost) return;
        Reward.Coin -= cost;

        List<RewardType> inUseItems = new List<RewardType>();

        if (powerups.Count > 0)
        {
            for (int i = 0; i < powerups.Count; i++)
            {
                if (powerups[i].InUse)
                {
                    inUseItems.Add(powerups[i].RewardType);
                    Reward.Data[powerups[i].RewardType] -= 1;
                }
            }
        }


        if (boost)
        {
            // todo: tell gm what boost to be used
        }

        STAGameManager.Instance.InUseItems = inUseItems;
        STAGameManager.Instance.nLevelID = MapManager.Instance.Data.SelectedLevel;
        SceneManager.LoadScene("GameScene");
    }
}
