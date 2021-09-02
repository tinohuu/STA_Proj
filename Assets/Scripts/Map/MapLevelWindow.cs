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

    List<MapLevelPowerup> powerups = new List<MapLevelPowerup>();
    MapLevelBoost boost = null;
    private void Start()
    {
        boost = GetComponentInChildren<MapLevelBoost>();
        powerups = GetComponentsInChildren<MapLevelPowerup>().ToList();

    }
    public void UpdateView(MapLevelData data)
    {
        OrderText.text = data.Order.ToString();
        for (int i = 0; i < StarGroup.childCount; i++)
        {
            Image image = StarGroup.GetChild(i).GetComponent<Image>();
            image.color = i < data.Rating ? Color.white : Color.gray;
        }

        CostText.text = Reward.Data[RewardType.FreeRound] > 0
            ? "<sprite name=\"FreeRound\"> " + Reward.Data[RewardType.FreeRound] + " Left"
            : "Cost " + "<sprite name=\"Coin\"> " + "2,500"; // todo: cost curve
    }

    public void Play()
    {
        if (powerups.Count > 0)
        {
            for (int i = 0; i < powerups.Count; i++)
            {
                if (powerups[i].InUse) Reward.Data[powerups[i].RewardType] -= 1;
            }
            // todo: tell gm what powerups to be used
        }


        if (boost)
        {
            // todo: tell gm what boost to be used
        }

        STAGameManager.Instance.nLevelID = MapManager.Instance.Data.SelectedLevel;
        SceneManager.LoadScene("GameScene");
    }
}
