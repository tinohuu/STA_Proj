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
    [SerializeField] TMP_Text rewardNameText;
    [SerializeField] Image rewardImage;
    [SerializeField] TMP_Text purchasePromptText;
    [SerializeField] ButtonAnimator purchaseButton;

    // Start is called before the first frame update
    void Start()
    {
        purchaseButton.OnClick.AddListener(() => Purcahse());
        purchaseButton.OnClick.AddListener(() => GetComponent<WindowAnimator>().Close());
        UpdateView();
    }

    void UpdateView()
    {
        // Reward name text
        string rewardName = Type.ToString();
        rewardName = Regex.Replace(rewardName, "([a-z])_?([A-Z])", "$1 $2");
        rewardNameText.text = rewardName;

        // Reward image
        var icons = Resources.LoadAll<Sprite>("Sprites/IconAtlas");
        Sprite sprite = Array.Find(icons, e => e.name == Type.ToString());
        rewardImage.sprite = sprite;

        int cost = RewardManager.GetRewardCost(Type);
        purchasePromptText.text = string.Format(purchasePromptText.text, cost);
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
