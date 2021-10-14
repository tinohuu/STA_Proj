using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopView : MonoBehaviour
{
    [SerializeField] GameObject m_ShopItemPrefab;
    [SerializeField] GameObject m_SmallShopItemGroupPrefab;
    [SerializeField] GameObject m_SmallShopItemPrefab;
    [SerializeField] Transform m_ShopItemGroup;
    [SerializeField] WindowAnimator m_PurchaseSucceedWindow;
    [SerializeField] Transform m_PurchaseSuccessWindowRewardGroup;
    //[SerializeField] GameObject m_ShopItemRewardPrefab;
    [SerializeField] ButtonAnimator m_PurchaseSuccessWindowButton;
    //[SerializeField] ScrollRect m_SrollRect;
    [SerializeField] Image m_BlackFront;

    [SerializeField] GameObject m_CoinWindowPrefab;
    // Start is called before the first frame update
    void Start()
    {
        UpdateView(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateView()
    {
        m_BlackFront.DOKill(true);
        m_BlackFront.DOFade(0, 1f).SetDelay(0.25f).OnComplete(() => m_BlackFront.gameObject.SetActive(false));

        m_ShopItemGroup.DestroyChildren();
        var configs = ConfigsAsset.GetConfigList<ShopConfig>();
        Transform group = null;
        foreach (var config in configs)
        {
            if (config.Times != -1 && config.Times <= ShopManager.Instance.Data.PurchaseTimes[config.ID - 1]) continue;
            if (config.IsSmall)
            {
                if (!group || group.childCount >= 2)
                    group = Instantiate(m_SmallShopItemGroupPrefab, m_ShopItemGroup).transform;
                ShopItem item = Instantiate(m_SmallShopItemPrefab, group).GetComponent<ShopItem>();
                item.SetID(config.ID);
            }
            else
            {
                ShopItem item = Instantiate(m_ShopItemPrefab, m_ShopItemGroup).GetComponent<ShopItem>();
                item.SetID(config.ID);
            }
        }
    }

    public void Purchase(int id)
    {

        RewardNumber.Switches[RewardType.Coin] = false;
        //ParticleManager.Instance.DestroyAll();
        m_BlackFront.DOKill(true);
        m_BlackFront.gameObject.SetActive(true);
        m_BlackFront.DOFade(0.9f, 0.5f);
        //m_PurchaseSucceedWindow.gameObject.SetActiveImmediately(true);

        // todo: check
        var configs = ConfigsAsset.GetConfigList<ShopConfig>();
        var config = configs[id - 1];

        // Set reward views
        var rewardConfigs = ConfigsAsset.GetConfigList<RewardConfig>();
        var rewardConfig = rewardConfigs[config.Reward - 1];

        Reward.Coin += rewardConfig.CoinReward;

        //m_PurchaseSuccessWindowRewardGroup.DestroyChildrenImmediate();
        SoundManager.Instance.PlaySFX("uiShopChargeSucceed");
        //var coin = Instantiate(m_ShopItemRewardPrefab, m_PurchaseSuccessWindowRewardGroup);
        //coin.GetComponentInChildren<TMP_Text>().text = rewardConfig.CoinReward.ToString("N0");

        var rewards = rewardConfig.ItemReward.ToRewards();
        foreach (var reward in rewards)
        {
            Reward.Data[reward.Item1] += reward.Item2;

            //var item = Instantiate(m_ShopItemRewardPrefab, m_PurchaseSuccessWindowRewardGroup);
            //item.GetComponentInChildren<Image>().sprite = reward.Item1.ToSprite();
            //item.GetComponentInChildren<TMP_Text>().text = "x" + reward.Item2.ToString();
        }

        ShopManager.Instance.Data.PurchaseTimes[id - 1]++;
        //UpdateView();
    }

    public void ClosePurchaseSuccessWindow()
    {
        StartCoroutine(IClosePurchaseSuccessWindow());
    }

    IEnumerator IClosePurchaseSuccessWindow()
    {
        for (int i = 0; i < m_PurchaseSuccessWindowRewardGroup.childCount; i++)
        {
            Transform item = m_PurchaseSuccessWindowRewardGroup.GetChild(i);
            item.DOMove(m_PurchaseSuccessWindowButton.transform.position, 0.5f);
            item.DOScale(Vector3.zero, 0.5f);
        }
        yield return new WaitForSeconds(0.5f);
        m_PurchaseSucceedWindow.FadeOut();
    }

    public void CreateCoinWindow(Vector3 startPos)
    {
        GetComponent<WindowAnimator>().Close();
        Window.CreateWindowPrefab(m_CoinWindowPrefab).GetComponent<ShopCoinWindow>().Initialise(startPos);
    }
}
