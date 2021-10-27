using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPurchaseWindow : MonoBehaviour
{
    public RewardType Type;
    public static RewardType LastPurchasedReward = RewardType.None;

    [SerializeField] TMP_Text m_RewardName;
    [SerializeField] Image m_RewardImage;
    [SerializeField] TMP_Text m_RewardInfo;
    [SerializeField] TMP_Text m_RewardCost;
    [SerializeField] ButtonAnimator m_PurchaseButton;

    // Start is called before the first frame update
    void Start()
    {
        m_PurchaseButton.OnClick.AddListener(() => Purcahse());
        m_PurchaseButton.OnClick.AddListener(() => GetComponent<WindowAnimator>().Close());
        UpdateView();
    }

    void UpdateView()
    {
        m_RewardName.text = "TXT_RWD_" + (int)Type;
        m_RewardInfo.text = "TXT_RWD_" + (int)Type + "_Info";
        m_RewardImage.sprite = Type.ToSprite();
        int cost = RewardManager.GetRewardCost(Type);
        m_RewardCost.Format(cost);
    }

    void Purcahse()
    {
        int cost = RewardManager.GetRewardCost(Type);
        if (Reward.Coin < cost)
        {
            ShopManager.Instance.Open();
        }
        else
        {
            Reward.Coin -= cost;
            Reward.Data[Type]++;
            LastPurchasedReward = Type;
        }
    }
}
