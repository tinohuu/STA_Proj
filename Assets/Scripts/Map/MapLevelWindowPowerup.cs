using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelWindowPowerup : MonoBehaviour
{
    [Header("Ref")]
    public GameObject PromptWindowPrefab;
    public GameObject PurchaseImage;
    public GameObject InUseImage;
    public GameObject TextImage;
    [SerializeField] RewardNumber rewardNumber;

    [Header("Settings")]
    [HideInInspector] public RewardType RewardType;

    [Header("Data")]
    public bool InUse = false;
    public bool Interactable = false;

    ButtonAnimator button;
    CanvasGroup canvasGroup;
    Image image;
    private void Awake()
    {
        button = GetComponent<ButtonAnimator>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        var icons = Resources.LoadAll<Sprite>("Sprites/IconAtlas");
        Sprite sprite = Array.Find(icons, e => e.name == RewardType.ToString());
        image.sprite = sprite;
        rewardNumber.Type = RewardType;
        Interactable = MapManager.Instance.Data.CompleteLevel + 1 >= MapManager.Instance.FunctionConfigsByFuncID[(int)RewardType + 1012 - 8].FunctionParams;
        canvasGroup.alpha = Interactable ? 1 : 0.5f;
    }
    private void Start()
    {

    }

    private void Update()
    {
        UpdateIconImage();
    }
    public void OnClick()
    {
        if (Interactable)
        {
            if (Reward.Data[RewardType] == 0)
            {
                RewardPurchaseWindow window = Window.CreateWindowPrefab(Resources.Load<GameObject>("Windows/Window_PowerupPurchase")).GetComponent<RewardPurchaseWindow>();
                window.Type = RewardType;
            }
            else
            {
                InUse = !InUse;
            }

        }
        // Show the window waiting to be locked
        else
        {
            TMP_Text text = Window.CreateWindowPrefab(PromptWindowPrefab).GetComponentInChildren<TMP_Text>();
            string rawString = text.text;
            rawString = string.Format(text.text, RewardType.ToString(), MapManager.Instance.FunctionConfigsByFuncID[(int)RewardType + 1012 - 8].FunctionParams);
            rawString = System.Text.RegularExpressions.Regex.Replace(rawString, "([a-z])_?([A-Z])", "$1 $2");
            text.text = rawString;
        }
    }

    void UpdateIconImage()
    {
        InUseImage.SetActive(Interactable && InUse);
        PurchaseImage.SetActive(Interactable && !InUse && Reward.Data[RewardType] == 0);
        TextImage.SetActive(Interactable && !InUse && Reward.Data[RewardType] != 0);
    }
}
