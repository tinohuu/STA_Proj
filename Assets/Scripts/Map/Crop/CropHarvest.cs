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
    [Header("Settings")]
    [SerializeField] string m_MatureMainText = "TXT_CRP_Harvest_Harvest";
    [SerializeField] string m_MatureSecondarynText = "TXT_CRP_Harvest_Now";
    [SerializeField] string m_ImmatureMainText = "TXT_CRP_Harvest_NextHarvest";
    //string m_ImmatureSecondarynText = "";


    [Header("Assets Ref")]
    [SerializeField] Material m_FontGoldMat;
    [SerializeField] Material m_FontOutlineMat;

    [Header("Self Ref")]
    [SerializeField] TMP_Text m_MainText;
    [SerializeField] TMP_Text m_SecondaryText;
    [SerializeField] Transform m_TextGroup;
    [SerializeField] TMP_Text m_Summary;
    [SerializeField] ParticleSystem m_HarvestParticle;
    [SerializeField] GameObject m_Rocket;
    //[SerializeField] GameObject m_CoinBoost;

    public static CropHarvest Instance = null;

    Button m_Button;
    ButtonAnimator m_ButtonAnimator;
    Tween m_TextGroupTween;

    [Header("Debug")]
    [SerializeField] string _harvestTime;
    [SerializeField] string _nowTime;
    private void Awake()
    {
        if (!Instance) Instance = this;
        m_Button = GetComponent<Button>();
        m_ButtonAnimator = GetComponent<ButtonAnimator>();
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        RewardManager.Instance.OnValueChanged[(int)RewardType.Clock].AddListener(() => CheckClock());
        RewardManager.Instance.OnValueChanged[(int)RewardType.Rocket].AddListener(() => CheckRocket());
    }

    private void OnDisable()
    {
        RewardManager.Instance.OnValueChanged[(int)RewardType.Clock].RemoveListener(() => CheckClock());
        RewardManager.Instance.OnValueChanged[(int)RewardType.Rocket].RemoveListener(() => CheckRocket());
    }
    

    void Update()
    {
        TimeSpan timeSpan;
        timeSpan = MapManager.Instance.Data.LastHarvestTime + TimeSpan.FromHours(CropManager.Instance.IsTimeBoosting? 0.5f : 1) - TimeManager.Instance.RealNow;

        // Play mature animation in advance
        if (timeSpan.TotalSeconds <= 3 && !CropManager.Instance.IsMature && TimeManager.Instance.Data.CheckedAuthenticity == TimeAuthenticity.Authentic && !TimeManager.Instance.IsGettingTime)
        {
            CropManager.Instance.SetMature(true);
            CropManager.Instance.UpdateCropsAnimator(true);
            m_TextGroupTween = m_TextGroup.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            var configs = CropManager.Instance.CropConfigs.FindAll(e => e.Level <= MapManager.Instance.Data.CompleteLevel);
            var textureModule = m_HarvestParticle.textureSheetAnimation;
            textureModule.startFrame = new ParticleSystem.MinMaxCurve(0, configs.Count * 3f / (textureModule.numTilesX * textureModule.numTilesY));
            SoundManager.Instance.PlaySFX("CropGrowup");
        }
        m_MainText.text = timeSpan.TotalSeconds > 0 ? m_ImmatureMainText : m_MatureMainText;
        m_SecondaryText.text = timeSpan.TotalSeconds > 0 ? timeSpan.ToString(@"mm\:ss") : m_MatureSecondarynText;
        m_SecondaryText.fontMaterial = timeSpan.TotalSeconds > 0 ? m_FontOutlineMat : m_FontGoldMat;

        //m_TextGroup.enabled = timeSpan.TotalSeconds <= 0;
        //m_TextGroup.transform.localScale = Vector3.one;

        m_HarvestParticle.gameObject.SetActive(timeSpan.TotalSeconds <= 0);



        //Image.color = timeSpan.TotalSeconds > 0 ? Color.white : Color.white;
        m_Button.interactable = timeSpan.TotalSeconds <= 0;
        m_ButtonAnimator.Interactable = timeSpan.TotalSeconds <= 0;

        //IsReady = timeSpan.TotalSeconds <= 0; // TEST

        //Debug_AchorPos = CropList.rectTransform.anchoredPosition;
        //Debug_SizeDelta = CropList.rectTransform.sizeDelta;

        _harvestTime = MapManager.Instance.Data.LastHarvestTime.ToString();
        _nowTime = TimeManager.Instance.RealNow.ToString();

        m_Rocket.SetActive(CropManager.Instance.IsTimeBoosting);
    }

    public void CheckRocket()
    {
        if (Reward.Data[RewardType.Rocket] > 0)
        {
            CropManager.Instance.Data.TimeBoostUntil = TimeManager.Instance.RealNow + TimeSpan.FromHours(Reward.Data[RewardType.Rocket]);
            CropManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(59 + 3);
            Reward.Data[RewardType.Rocket] = 0;
            SoundManager.Instance.PlaySFX("itemClockUse");
        }
        
    }

    void CheckClock()
    {
        if (Reward.Data[RewardType.Clock] > 0)
        {
            CropManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(59 + 3);
            Reward.Data[RewardType.Clock] = 0;
            SoundManager.Instance.PlaySFX("itemClockUse");
        }
    }

    public void Harvest()
    {
        CropManager.Instance.SetMature(false);

        ResetTime(TimeManager.Instance.RealNow);


        //FindObjectOfType<RewardNumber>().Animate(4, 2);
        //m_TextGroupTween.Kill();
        m_TextGroup.DOKill();
        m_TextGroupTween = m_TextGroup.DOScale(1, 0.5f);

        CropManager.Instance.PlayHarvestEffects();
        Reward.Coin += UpdateHarvestText();
        CropManager.Instance.UpdateCropsAnimator(true);
    }

    public void Cheat()
    {
        if (!Debug.isDebugBuild) return;
        CropManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(55);
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
        m_Summary.rectTransform.anchoredPosition = Vector2.zero - Vector2.up * m_Summary.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        int coinCount = 2000;
        string text = "Farm " + "Coin".ToIcon() + " " + 2000;
        int firstLevelOfMap = 0;// MapManager.MapMakerConfig.LevelToStarting(MapManager.Instance.Data.CompleteLevel);

        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        if (configIndex > 0) m_Summary.text += "\nCrops 1 to " + (configIndex).ToString() + "Coin".ToIcon() + " " + ((configIndex) * 50).ToString();
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
        m_Summary.text = text;

        coinCount += (lastCropIndex + 1) * 50;
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_Summary.rectTransform);
        m_Summary.rectTransform.DOAnchorPosY(m_Summary.rectTransform.sizeDelta.y + m_Summary.transform.parent.GetComponent<RectTransform>().sizeDelta.y, 10);
        if (m_Summary.transform.childCount > 0 && !m_Summary.transform.GetChild(0).gameObject.GetComponent<SoftMaskable>())
        {
            m_Summary.transform.GetChild(0).gameObject.AddComponent<SoftMaskable>();
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
                TimeDebugText.Log("Clamped the harvest time. Now: " + now + " Last harvest: " + data.LastHarvestTime);
                CropManager.Instance.Data.LastHarvestTime = now;
            }
            //CropManager.Instance.UpdateCropsAnimator(true);
        }
        else ResetTime(now);
    }

    public void ResetTime(DateTime now)
    {
        //MapManagerData data = MapManager.Instance.Data;
        TimeDebugText.Log("Reset last harvest time.");
        CropManager.Instance.Data.LastHarvestTime = TimeManager.Instance.RealNow;
    }
}
