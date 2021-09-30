using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] GameObject m_RewardPrefab;
    [SerializeField] Transform m_RewardGrid;
    [SerializeField] Image m_MainRewardImage;
    [SerializeField] TMP_Text m_MainRewardText;

    [Header("Tag")]
    [SerializeField] Image m_Tag;
    [SerializeField] TMP_Text m_TagTextA;
    [SerializeField] TMP_Text m_TagTextB;

    [Header("Other Ref")]
    [SerializeField] Image m_BackgroundImage;
    [SerializeField] TMP_Text m_BackgroundText;
    [SerializeField] ButtonAnimator m_PurchaseButton;

    [Header("Debug")]
    public bool IsSmall = false;
    int m_ItemID = 1;
    void Start()
    {
        var configs = ConfigsAsset.GetConfigList<ShopConfig>();
        var config = configs[m_ItemID - 1];

        // Set reward views
        var rewardConfigs = ConfigsAsset.GetConfigList<RewardConfig>();
        var rewardConfig = rewardConfigs[config.Reward - 1];

        m_MainRewardText.text = rewardConfig.CoinReward.ToString("N0");
        if (!IsSmall)
        {
            var rewards = rewardConfig.ItemReward.ToRewards();
            foreach (var reward in rewards)
            {
                GameObject rewardCell = Instantiate(m_RewardPrefab, m_RewardGrid);
                rewardCell.GetComponentInChildren<Image>().sprite = reward.Item1.ToSprite();
                rewardCell.GetComponentInChildren<TMP_Text>().text = "x" + reward.Item2.ToString();
            }
        }
        m_MainRewardImage.sprite = GetSprite("Icon_" + config.Icon);

        // Set background view
        if (IsSmall)
        {
            m_BackgroundImage.sprite = GetSprite("Background_Small_" + config.Background);
        }
        else
        {
            m_BackgroundImage.sprite = GetSprite("Background_" + config.Background);
            m_BackgroundText.text = config.Background == "Normal" ? "" : System.Text.RegularExpressions.Regex.Replace(config.Background, "([a-z])_?([A-Z])", "$1 $2");
        }


        // Set tag views
        m_Tag.gameObject.SetActive(config.TagText != "");
        if (m_Tag.gameObject.activeSelf)
        {
            m_Tag.sprite = GetSprite("Tag_" + config.TagIcon);
            var texts = config.TagText.Split('_');
            m_TagTextA.text = texts[0];
            if (texts.Length >= 2) m_TagTextB.text = texts[1];
        }

        m_PurchaseButton.GetComponentInChildren<TMP_Text>().text = (config.Price / 100f).ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-au"));

        m_PurchaseButton.OnClick.AddListener(() => Purcahse());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetID(int id)
    {
        m_ItemID = id;
    }

    Sprite GetSprite(string name)
    {
        return Resources.Load<Sprite>("Sprites/Shop/ShopItem_" + name);
    }

    public void Purcahse()
    {
        GetComponentInParent<ShopView>().Purchase(m_ItemID);
        //ShopManager.Instance.Purchase(m_ItemID);
    }
}
