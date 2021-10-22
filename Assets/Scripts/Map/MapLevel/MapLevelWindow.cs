using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLevelWindow : Window
{
    [SerializeField] ButtonAnimator m_PlayButton;
    public TMP_Text OrderText = null;
    public Transform StarGroup = null;
    public TMP_Text CostText = null;
    public MapLevelData LevelData;
    [SerializeField] TMP_Text m_InfoText;

    List<MapLevelWindowPowerup> m_Powerups = new List<MapLevelWindowPowerup>();
    MapLevelWindowBoost m_Boost = null;
    WindowAnimator m_WindowAnimator;

    public Transform PlayerButton => m_PlayButton.transform;

    private void Awake()
    {
        m_Boost = GetComponentInChildren<MapLevelWindowBoost>();
        m_Powerups = GetComponentsInChildren<MapLevelWindowPowerup>().ToList();
        m_WindowAnimator = GetComponent<WindowAnimator>();

        m_PlayButton.OnClick.AddListener(() => Play());
        m_PlayButton.OnClick.AddListener(() => m_WindowAnimator.Close());
    }

    private void Start()
    {
        if (m_Powerups.Count > 0)
        {
            var rawRewards = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].PowerUpsOrder.Split('_');
            for (int i = 0; i < rawRewards.Length; i++)
            {
                int rewardID;
                if (int.TryParse(rawRewards[i], out rewardID))
                    m_Powerups[i].Initialise((RewardType)rewardID);
            }
        }

        UpdateView();

        if (TutorialManager.Instance.HasTutorial("RemoveCards", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.RemoveCards && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("RemoveCards", 1, item.gameObject,
                    onStart: () => Reward.Data[RewardType.RemoveCards]++,
                    onExit: () => TutorialManager.Instance.Show("RemoveCards", 2, m_PlayButton.gameObject)
                    ));
            }
        }
        if (TutorialManager.Instance.HasTutorial("ClearPlayables", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.ClearPlayables && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("ClearPlayables", 1, item.gameObject,
                    onStart: () => Reward.Data[RewardType.ClearPlayables]++,
                    onExit: () => TutorialManager.Instance.Show("ClearPlayables", 2, m_PlayButton.gameObject)
                    )) ;
            }
        }
        if (TutorialManager.Instance.HasTutorial("WildDrop", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.WildDrop && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("WildDrop", 1, item.gameObject,
                    onStart: () => Reward.Data[RewardType.WildDrop]++,
                    onExit: () => TutorialManager.Instance.Show("WildDrop", 2, m_PlayButton.gameObject)
                    )) ;
            }
        }

        if (TutorialManager.Instance.HasTutorial("RemoveValueChangers", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.RemoveValueChangers && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("RemoveValueChangers", 1, item.gameObject,
                    onStart: () => { Reward.Data[RewardType.RemoveValueChangers]++; item.UpdateView(); },
                    onExit: () => TutorialManager.Instance.Show("RemoveValueChangers", 2, m_PlayButton.gameObject)
                    )) ;
            }
        }
        if (TutorialManager.Instance.HasTutorial("RemoveCodeBreakers", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.RemoveCodeBreakers && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("RemoveCodeBreakers", 1, item.gameObject,
                    onStart: () => { Reward.Data[RewardType.RemoveCodeBreakers]++; item.UpdateView(); },
                    onExit: () => TutorialManager.Instance.Show("RemoveCodeBreakers", 2, m_PlayButton.gameObject)
                    )) ;
            }
        }
        if (TutorialManager.Instance.HasTutorial("RemoveBombs", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.RemoveBombs && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("RemoveBombs", 1, item.gameObject,
                    onStart: () => { Reward.Data[RewardType.RemoveBombs]++; item.UpdateView(); },
                    onExit: () => TutorialManager.Instance.Show("RemoveBombs", 2, m_PlayButton.gameObject)
                    )) ;
            }
        }
    }

    public void UpdateView()
    {
        OrderText.text = LevelData.ID.ToString();
        for (int i = 0; i < StarGroup.childCount; i++)
        {
            Image image = StarGroup.GetChild(i).GetComponent<Image>();
            image.color = i < LevelData.Rating ? Color.white : Color.gray;
        }

        var costs = GetCosts();
        string text = "";
        if (costs.Count == 1 && costs[0].Item1 != RewardType.Coin)
        {
            text = "FreeRound".ToIcon() + " " + Reward.Data[RewardType.FreeRound] + " Left";
        }
        else
        {
            text = "Cost ";
            for (int i = 0; i < costs.Count; i++)
            {
                if (i > 0) text += " + ";
                text += costs[i].Item1.ToString().ToIcon() + " " + costs[i].Item2.ToString(); 
            }
        }
        CostText.text = text;

        /*int cost = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].Cost;


        bool hasFreeRound = Reward.Data[RewardType.FreeRound] > 0;

        string coinText = "Cost " + "<sprite name=\"Coin\"> " + cost.ToString("N0");
        string freeRoundText = "<sprite name=\"FreeRound\"> " + Reward.Data[RewardType.FreeRound] + " Left";

        if (m_Boost)
        {
            int boost = m_Boost.GetBoostModifer(hasFreeRound);
            cost *= boost;

            string freeRoundBoostText = "Cost " + "FreeRound".ToIcon() + " 1 + " + "Coin".ToIcon() + " " + cost.ToString("N0");
            CostText.text = hasFreeRound ? boost == 0 ? freeRoundText : freeRoundBoostText : coinText;
        }
        else CostText.text = hasFreeRound ? freeRoundText : coinText;*/

        if (m_InfoText)
        {
            var textConfigs = ConfigsAsset.GetConfigList<TextConfig>();
            if (LevelData.Rating >= 3)
            {
                m_InfoText.text = "TXT_LVL_WININFO_FULL";
            }
            else if (LevelData.Rating > 0)
            {
                m_InfoText.text = "TXT_LVL_WININFO_UNLOCK1";
            }
            else
            {
                if (LevelData.ID == 1)
                {
                    m_InfoText.text = "";
                    return;
                }
                var cropConfig = CropManager.Instance.LevelToCropConfig(LevelData.ID);
                int textID = (LevelData.ID - 1) % 4 + 1;
                m_InfoText.text = "TXT_LVL_WININFO_LOCK" + textID;
                m_InfoText.Format(cropConfig.Level - MapManager.Instance.Data.CompleteLevel);
            }
        }
    }

    List<(RewardType, int)> GetCosts()
    {
        List<(RewardType, int)> costs = new List<(RewardType, int)>();
        int coinCost = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].Cost;
        bool hasFreeRound = Reward.Data[RewardType.FreeRound] > 0;

        if (m_Boost)
        {
            int boost = m_Boost.GetBoostModifer(hasFreeRound);
            coinCost *= boost;

            if (hasFreeRound) costs.Add((RewardType.FreeRound, 1));
            if (coinCost > 0) costs.Add((RewardType.Coin, coinCost));
            return costs;
        }
        else
        {
            if (hasFreeRound)costs.Add((RewardType.FreeRound, 1));
            else costs.Add((RewardType.Coin, coinCost));
            return costs;
        }
    }

    public void Play()
    {
        /*int cost = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].Cost;
        if (m_Boost) cost *= m_Boost.GetBoostModifer();

        if (Reward.Coin < cost) return;
        Reward.Coin -= cost;*/


        var costs = GetCosts();
        foreach (var cost in costs)
        {
            if (Reward.Data[cost.Item1] < cost.Item2)
            {
                ShopManager.Instance.Open();
                return;
            }
        }

        foreach (var cost in costs) Reward.Data[cost.Item1] -= cost.Item2;

        List<RewardType> inUseItems = new List<RewardType>();

        if (m_Powerups.Count > 0)
        {
            for (int i = 0; i < m_Powerups.Count; i++)
            {
                if (m_Powerups[i].InUse)
                {
                    inUseItems.Add(m_Powerups[i].RewardType);
                    Reward.Data[m_Powerups[i].RewardType] -= 1;
                }
            }
        }


        if (m_Boost)
        {
            // todo: tell gm what boost to be used
        }

        STAGameManager.Instance.InUseItems = inUseItems;
        STAGameManager.Instance.nLevelID = MapManager.Instance.Data.SelectedLevel;
        //MapDataManager.Instance.NewRatings = 0;
        WindowManager.Instance.LoadSceneWithFade("GameScene");
    }
}
