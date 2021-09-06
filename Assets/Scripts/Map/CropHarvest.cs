using Coffee.UISoftMask;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropHarvest : MonoBehaviour
{
    public TMP_Text MainText;
    public TMP_Text SecondaryText;
    public Material GoldMat;
    public Material WhiteMat;
    public Animator TextGroupAnimator;
    public TMP_Text CropList;
    public Image Image;
    public Button Button;
    public bool IsReady = false;
    public static CropHarvest Instance = null;
    public Vector2 Debug_AchorPos;
    public Vector2 Debug_SizeDelta;
    public string CoinString = "";
    public GameObject HarvestParticle;
    [SerializeField] string _harvestTime;
    [SerializeField] string _nowTime;
    private void Awake()
    {
        if (!Instance) Instance = this;
        TimeManager.Instance.TimeRefresher += RefreshHarvestTime;
    }

    void Update()
    {
        TimeSpan timeSpan;
        timeSpan = MapManager.Instance.Data.LastHarvestTime + TimeSpan.FromHours(1) - TimeManager.Instance.RealNow;

        // Play mature animation in advance
        if (timeSpan.TotalSeconds <= 3 && !CropManager.Instance.IsMature && TimeManager.Instance.Authenticity)
        {
            CropManager.Instance.IsMature = true;
            CropManager.Instance.UpdateCropsAnimator(true);
        }
        MainText.text = timeSpan.TotalSeconds > 0 ? "Next Harvest" : "Harvest";
        SecondaryText.text = timeSpan.TotalSeconds > 0 ? timeSpan.ToString(@"mm\:ss") : "Now";
        SecondaryText.fontMaterial = timeSpan.TotalSeconds > 0 ? WhiteMat : GoldMat;
        TextGroupAnimator.enabled = timeSpan.TotalSeconds <= 0;
        TextGroupAnimator.transform.localScale = Vector3.one;
        HarvestParticle.SetActive(timeSpan.TotalSeconds <= 0);

        //Image.color = timeSpan.TotalSeconds > 0 ? Color.white : Color.white;
        Button.interactable = timeSpan.TotalSeconds <= 0;

        IsReady = timeSpan.TotalSeconds <= 0; // TEST

        //Debug_AchorPos = CropList.rectTransform.anchoredPosition;
        //Debug_SizeDelta = CropList.rectTransform.sizeDelta;

        _harvestTime = MapManager.Instance.Data.LastHarvestTime.ToString();
        _nowTime = TimeManager.Instance.RealNow.ToString();
    }

    public void Harvest()
    {
        CropManager.Instance.IsMature = false;

        RefreshHarvestTime(false);

        Reward.Coin += UpdateHarvestText();
        FindObjectOfType<RewardNumber>().Animate(4, 2);

        CropManager.Instance.PlayHarvestEffects();
    }

    public void RefreshHarvestTime(bool isVerified)
    {
        MapManagerData data = MapManager.Instance.Data;
        if (isVerified)
        {
            bool clamp = TimeManager.Instance.RealNow < MapManager.Instance.Data.LastHarvestTime;
            TimeDebugText.Text.text += "\nChecking remaining harvest duration...";
            if (clamp)
            {
                TimeDebugText.Text.text += "\nClamped the harvest time.";
                //data.LastHarvestTime = TimeManager.Instance.RealNow;
                TimeManager.Instance.GetTime(true, false);
            }
        }
        else
        {
            TimeDebugText.Text.text += "\nPunished the harvest time.";
            data.LastHarvestTime = TimeManager.Instance.RealNow;
        }
    }

    public void Cheat()
    {
        if (!Debug.isDebugBuild) return;
        MapManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(50);
    }

    int UpdateHarvestText()
    {
        CropList.rectTransform.anchoredPosition = Vector2.zero - Vector2.up * CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        int coinCount = 2000;
        CropList.text = "Farm " + "Coin".ToIcon() + " " + 2000;
        int firstLevelOfMap = MapManager.Instance.LevelToMapData(MapManager.Instance.Data.CompleteLevel).StartAt;
        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        if (configIndex > 0) CropList.text += "\nCrops 1 to " + (configIndex).ToString() + "Coin".ToIcon() + " " + ((configIndex) * 50).ToString();
        int lastCropIndex = configIndex;
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
            {
                CropList.text += "\n" + cropConfig.Name + " " + "Coin".ToIcon() + " 50";
                lastCropIndex = i;
            }
            else
            {
                break;
            }
        }
        coinCount += (lastCropIndex + 1) * 50;
        LayoutRebuilder.ForceRebuildLayoutImmediate(CropList.rectTransform);
        CropList.rectTransform.DOAnchorPosY(CropList.rectTransform.sizeDelta.y + CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y, 10);
        if (CropList.transform.childCount > 0) CropList.transform.GetChild(0).gameObject.AddComponent<SoftMaskable>();
        return coinCount;
    }
}
