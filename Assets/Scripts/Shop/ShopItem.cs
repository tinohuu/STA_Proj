using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("Purcashe Successful")]
    [SerializeField] RectTransform m_PurchaseSuccessful;

    [Header("Other Ref")]
    [SerializeField] Image m_BackgroundImage;
    [SerializeField] TMP_Text m_BackgroundText;
    [SerializeField] ButtonAnimator m_PurchaseButton;

    [Header("Debug")]
    public bool IsSmall = false;
    int m_ItemID = 1;
    void Start()
    {
        m_PurchaseSuccessful.gameObject.SetActive(false);
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
            m_BackgroundImage.sprite = GetSprite("Background_" + config.Background);
        }
        else
        {
            m_BackgroundImage.sprite = GetSprite("Background_" + config.Background);
            m_BackgroundText.text = config.BackgroundText;// == "Normal" ? "" : System.Text.RegularExpressions.Regex.Replace(config.Background, "([a-z])_?([A-Z])", "$1 $2");
        }


        // Set tag views
        m_Tag.gameObject.SetActive(config.TagIcon != "");
        if (m_Tag.gameObject.activeSelf)
        {
            m_Tag.sprite = GetSprite("Tag_" + config.TagIcon);
            //var texts = config.TagText.Split('_');
            m_TagTextA.text = config.TagTextA;
            m_TagTextB.text = config.TagTextB;
            //if (texts.Length >= 2) m_TagTextB.text = texts[1];
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
        StartCoroutine(IPurchaseSucessful());
    }

    IEnumerator IPurchaseSucessful()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 50;
        gameObject.AddComponent<GraphicRaycaster>();

        m_PurchaseButton.Interactable = false;
        m_PurchaseButton.transform.DOKill(true);

        m_PurchaseButton.transform.DOScale(Vector3.zero, 0.25f);
        yield return new WaitForSeconds(0.25f);
        var size = m_BackgroundImage.rectTransform.sizeDelta;
        if (!IsSmall) size.y *= 0.8f;
        //size = new Vector2(size.x, size.y * 0.8f);
        m_BackgroundImage.rectTransform.DOSizeDelta(size, 0.5f);
        yield return new WaitForSeconds(0.75f);
        transform.DOJump(GetComponentInParent<ShopView>().transform.position - Vector3.right * 250, -100, 1, 0.75f);
        yield return new WaitForSeconds(0.5f);
        m_PurchaseSuccessful.DOAnchorPosX(0, 0.5f);
        //m_PurchaseSuccessful.GetComponentInChildren<ButtonAnimator>().OnClick.AddListener(() => GetComponentInParent<ShopView>().UpdateView());
        var startPos = Camera.main.ScreenToWorldPoint(m_MainRewardImage.transform.position);
        startPos.z = 0;
        m_PurchaseSuccessful.GetComponentInChildren<ButtonAnimator>().OnceOnly = true;
        m_PurchaseSuccessful.GetComponentInChildren<ButtonAnimator>().OnClick.AddListener(() => GetComponentInParent<ShopView>().CreateCoinWindow(startPos));
        

        m_PurchaseSuccessful.gameObject.SetActive(true);
    }
}
