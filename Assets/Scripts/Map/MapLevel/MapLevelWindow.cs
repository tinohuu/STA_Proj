using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLevelWindow : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] ButtonAnimator m_PlayButton;
    [SerializeField] TMP_Text m_LevelIDText = null;
    [SerializeField] Transform m_StarGroup = null;
    [SerializeField] TMP_Text m_CostText = null;

    [Header("Window Info (Type A Only)")]
    [SerializeField] TMP_Text m_InfoText;
    [SerializeField] List<string> m_InfoTextLock = new List<string>() { "TXT_LVL_WinInfo_Lock1" };
    [SerializeField] List<string> m_InfoTextUnlcok = new List<string>() { "TXT_LVL_WinInfo_Unlcok" };
    [SerializeField] List<string> m_InfoTextFull = new List<string>() { "TXT_LVL_WinInfo_Full" };

    [Header("Powerup Info (Type B & C Only)")]
    [SerializeField] ButtonAnimator m_PowerupInfoButton;
    [SerializeField] GameObject m_PowerupInfoWindowPrefab;

    [Header("Data")]
    public MapLevelData LevelData;

    List<MapLevelWindowPowerup> m_Powerups = new List<MapLevelWindowPowerup>(); // Powerup buttons
    MapLevelWindowBoost m_Boost = null;
    WindowAnimator m_WindowAnimator;

    //public Transform PlayerButton => m_PlayButton.transform;

    private void Awake()
    {
        m_Boost = GetComponentInChildren<MapLevelWindowBoost>();
        m_Powerups = GetComponentsInChildren<MapLevelWindowPowerup>().ToList();
        m_WindowAnimator = GetComponent<WindowAnimator>();

        m_PlayButton.OnClick.AddListener(() => Play());
        m_PlayButton.OnClick.AddListener(() => m_WindowAnimator.Close());

        m_PowerupInfoButton.OnClick.AddListener(() => ShowPowerupInfo());
    }

    private void Start()
    {
        UpdateView();
        ShowTutorial();
    }

    public void UpdateView()
    {
        // Level ID
        m_LevelIDText.text = LevelData.ID.ToString();

        // Stars
        for (int i = 0; i < m_StarGroup.childCount; i++)
        {
            Image image = m_StarGroup.GetChild(i).GetComponent<Image>();
            image.color = i < LevelData.Rating ? Color.white : Color.gray;
        }

        // Powerup buttons
        if (m_Powerups.Count > 0)
        {
            var rawRewards = MapManager.Instance.CurMapStageConfigs[LevelData.ID - 1].PowerUpsOrder.Split('_');
            for (int i = 0; i < rawRewards.Length; i++)
            {
                int rewardID;
                if (int.TryParse(rawRewards[i], out rewardID))
                    m_Powerups[i].Initialize((RewardType)rewardID);
            }
        }

        // Costs text
        var costs = GetCosts();
        string text = "";
        if (costs.Count == 1 && costs[0].Item1 != RewardType.Coin)
            text = "FreeRound".ToIcon() + " " + Reward.Data[RewardType.FreeRound] + " Left";
        else
        {
            text = "Cost ";
            for (int i = 0; i < costs.Count; i++)
            {
                if (i > 0) text += " + ";
                text += costs[i].Item1.ToString().ToIcon() + " " + costs[i].Item2.ToString(); 
            }
        }
        m_CostText.text = text;

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

        // Type-A only
        if (m_InfoText)
        {
            var textConfigs = ConfigsAsset.GetConfigList<TextConfig>();
            if (LevelData.Rating >= 3)
            {
                m_InfoText.text = m_InfoTextFull[(LevelData.ID - 1) % m_InfoTextFull.Count];
            }
            else if (LevelData.Rating > 0)
            {
                m_InfoText.text = m_InfoTextUnlcok[(LevelData.ID - 1) % m_InfoTextUnlcok.Count];
            }
            else
            {
                if (LevelData.ID == 1)
                {
                    m_InfoText.text = "";
                    return;
                }
                m_InfoText.text = m_InfoTextLock[(LevelData.ID - 1) % m_InfoTextLock.Count];
                m_InfoText.Format(CropManager.Instance.LevelToCropConfig(LevelData.ID).Level - MapManager.Instance.Data.CompleteLevel);
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
        // Open shop when costs not enough
        var costs = GetCosts();
        foreach (var cost in costs)
        {
            if (Reward.Data[cost.Item1] < cost.Item2)
            {
                ShopManager.Instance.Open();
                return;
            }
        }

        // Cost
        foreach (var cost in costs) Reward.Data[cost.Item1] -= cost.Item2;

        // Use powerups
        List<RewardType> inUseItems = m_Powerups.FindAll(e => e.InUse).Select(e => e.RewardType).ToList();
        inUseItems.ForEach(e => Reward.Data[e]--);

        /*if (m_Powerups.Count > 0)
        {
            for (int i = 0; i < m_Powerups.Count; i++)
            {
                if (m_Powerups[i].InUse)
                {
                    inUseItems.Add(m_Powerups[i].RewardType);
                    Reward.Data[m_Powerups[i].RewardType] -= 1;
                }
            }
        }*/


        if (m_Boost)
        {
            // todo: tell gm what boost to be used
        }

        STAGameManager.Instance.InUseItems = inUseItems;
        STAGameManager.Instance.nLevelID = MapManager.Instance.Data.SelectedLevel;
        //MapDataManager.Instance.NewRatings = 0;
        WindowManager.Instance.LoadSceneWithFade("GameScene");
    }

    void ShowPowerupInfo()
    {
        var window = Window.CreateWindowPrefab(m_PowerupInfoWindowPrefab).GetComponent<RewardInfoWindow>();
        window.Initialize(m_Powerups.Select(e => e.RewardType).ToList());
        //for (int i = 0; i < powerups.Length; i++) powerups[i].Initialize(m_Powerups[i].RewardType);
    }

    void ShowTutorial()
    {
        if (TutorialManager.Instance.HasTutorial("RemoveCards", 1))
        {
            var item = m_Powerups.Find(e => e.RewardType == RewardType.RemoveCards && e.Interactable);
            if (item)
            {
                if (TutorialManager.Instance.Show("RemoveCards", 1, item.gameObject,
                    onStart: () => Reward.Data[RewardType.RemoveCards]++,
                    onExit: () => TutorialManager.Instance.Show("RemoveCards", 2, m_PlayButton.gameObject)
                    )) ;
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
}
