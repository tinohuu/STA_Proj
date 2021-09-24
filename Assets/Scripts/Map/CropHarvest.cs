using Coffee.UISoftMask;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropHarvest : MonoBehaviour, ITimeRefreshable
{
    [SerializeField] TMP_Text MainText;
    public TMP_Text SecondaryText;
    public Material GoldMat;
    public Material WhiteMat;
    public Animator TextGroupAnimator;
    public TMP_Text CropList;
    public Image Image;
    Button button;
    ButtonAnimator buttonAnimator;
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
        button = GetComponent<Button>();
        buttonAnimator = GetComponent<ButtonAnimator>();
        //TimeManager.Instance.Refresher += RefreshHarvestTime;
    }

    private void Start()
    {
        TutorialManager.Instance.Show("Harvest", buttonAnimator.gameObject);
    }

    void Update()
    {
        TimeSpan timeSpan;
        timeSpan = MapManager.Instance.Data.LastHarvestTime + TimeSpan.FromHours(1) - TimeManager.Instance.RealNow;

        // Play mature animation in advance
        if (timeSpan.TotalSeconds <= 3 && !CropManager.Instance.IsMature && TimeManager.Instance.Data.CheckedAuthenticity == TimeAuthenticity.Authentic && !TimeManager.Instance.IsGettingTime)
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
        button.interactable = timeSpan.TotalSeconds <= 0;
        buttonAnimator.Interactable = timeSpan.TotalSeconds <= 0;

        IsReady = timeSpan.TotalSeconds <= 0; // TEST

        //Debug_AchorPos = CropList.rectTransform.anchoredPosition;
        //Debug_SizeDelta = CropList.rectTransform.sizeDelta;

        _harvestTime = MapManager.Instance.Data.LastHarvestTime.ToString();
        _nowTime = TimeManager.Instance.RealNow.ToString();
    }

    public void Harvest()
    {
        CropManager.Instance.IsMature = false;

        ResetTime(TimeManager.Instance.RealNow);


        //FindObjectOfType<RewardNumber>().Animate(4, 2);

        CropManager.Instance.PlayHarvestEffects();
        Reward.Coin += UpdateHarvestText();
    }

    public void Cheat()
    {
        if (!Debug.isDebugBuild) return;
        MapManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(55);
    }

    public int GetHarvestCoin()
    {
        int coinCount = 2000;
        int firstLevelOfMap = 0;// MapManager.MapMakerConfig.LevelToStarting(MapManager.Instance.Data.CompleteLevel);
        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        int lastCropIndex = configIndex;
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
            {
                lastCropIndex = i;
            }
            else
            {
                break;
            }
        }
        coinCount += (lastCropIndex + 1) * 50;
        return coinCount;
    }

    int UpdateHarvestText()
    {
        CropList.rectTransform.anchoredPosition = Vector2.zero - Vector2.up * CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        int coinCount = 2000;
        string text = "Farm " + "Coin".ToIcon() + " " + 2000;
        int firstLevelOfMap = 0;// MapManager.MapMakerConfig.LevelToStarting(MapManager.Instance.Data.CompleteLevel);

        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        if (configIndex > 0) CropList.text += "\nCrops 1 to " + (configIndex).ToString() + "Coin".ToIcon() + " " + ((configIndex) * 50).ToString();
        int lastCropIndex = configIndex;
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
            {
                text += "\n" + cropConfig.Name + " " + "Coin".ToIcon() + " 50";
                lastCropIndex = i;
            }
            else
            {
                break;
            }
        }
        CropList.text = text;

        coinCount += (lastCropIndex + 1) * 50;
        LayoutRebuilder.ForceRebuildLayoutImmediate(CropList.rectTransform);
        CropList.rectTransform.DOAnchorPosY(CropList.rectTransform.sizeDelta.y + CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y, 10);
        if (CropList.transform.childCount > 0 && !CropList.transform.GetChild(0).gameObject.GetComponent<SoftMaskable>())
        {
            CropList.transform.GetChild(0).gameObject.AddComponent<SoftMaskable>();
        }
        return coinCount;
    }


    public void RefreshTime(DateTime now, TimeSource source, TimeAuthenticity timeAuthenticity)
    {
        MapManagerData data = MapManager.Instance.Data;

        if (timeAuthenticity != TimeAuthenticity.Unauthentic)
        {
            bool clamp = now < data.LastHarvestTime;
            if (clamp)
            {
                TimeDebugText.Log("Clamped the harvest time.");
                data.LastHarvestTime = now;
            }
            //CropManager.Instance.UpdateCropsAnimator(true);
        }
        else ResetTime(now);
    }
    public void ResetTime(DateTime now)
    {
        MapManagerData data = MapManager.Instance.Data;
        data.LastHarvestTime = now;
    }
}
