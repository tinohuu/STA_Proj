using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelWindowPowerup : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] bool m_ForcePurchase = false;
    [SerializeField] bool m_DiableOnLocked = false;
    [SerializeField] RewardType m_RewardType;

    [Header("Ref")]
    [SerializeField] GameObject m_LockWindowPrefab;
    [SerializeField] GameObject m_PlusMark;
    [SerializeField] GameObject m_TickMark;
    [SerializeField] GameObject m_TextMark;

    [SerializeField] RewardNumber m_RewardNumber;
    [SerializeField] GameObject m_PowerupPurchaseWindowPrefab;

    public RewardType RewardType => m_RewardType;

    [Header("Data")]
    public bool InUse = false;
    public bool Interactable = false;

    ButtonAnimator m_Button;
    CanvasGroup m_CanvasGroup;
    Image m_Image;
    private void Awake()
    {
        m_Button = GetComponent<ButtonAnimator>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_Image = GetComponent<Image>();

        m_Button.OnClick.AddListener(() => OnClick());
    }

    private void OnEnable()
    {
        UpdateView();
    }

    private void Start()
    {
        UpdateView();
    }

    public void Initialize(RewardType rewardType)
    {
        m_RewardType = rewardType;
    }

    public void UpdateView()
    {
        m_RewardNumber.Type = RewardType;

        m_Image.sprite = RewardType.ToSprite();

        Interactable = MapManager.Instance.Data.CompleteLevel + 1 >= MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == (int)RewardType + 1012 - 8).FunctionParams;

        if (m_DiableOnLocked && !Interactable)
            gameObject.SetActive(false);
        else
        {
            m_CanvasGroup.alpha = Interactable ? 1 : 0.5f;
            m_RewardNumber.UpdateView();
            m_TickMark.SetActive(Interactable && InUse);
            m_PlusMark.SetActive(Interactable && !InUse && Reward.Data[RewardType] == 0);
            m_TextMark.SetActive(Interactable && !InUse && Reward.Data[RewardType] != 0);
        }
    }

    public void OnClick()
    {
        if (m_ForcePurchase)
        {
            RewardPurchaseWindow window = Window.CreateWindowPrefab(m_PowerupPurchaseWindowPrefab).GetComponent<RewardPurchaseWindow>();
            window.Type = RewardType;
            return;
        }

        if (Interactable)
        {
            if (Reward.Data[RewardType] == 0)
            {
                RewardPurchaseWindow window = Window.CreateWindowPrefab(m_PowerupPurchaseWindowPrefab).GetComponent<RewardPurchaseWindow>();
                window.Type = RewardType;
            }
            else InUse = !InUse;

        }
        else
        {
            TMP_Text text = Window.CreateWindowPrefab(m_LockWindowPrefab).GetComponentInChildren<TMP_Text>();
            string rawString = text.text;
            rawString = string.Format(text.text, RewardType.ToString(), MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == (int)RewardType + 1012 - 8).FunctionParams);
            rawString = System.Text.RegularExpressions.Regex.Replace(rawString, "([a-z])_?([A-Z])", "$1 $2");
            text.text = rawString;
        }

        UpdateView();
    }
}
