using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropHarvest : MonoBehaviour
{
    public TMP_Text Text;
    public TMP_Text CropList;
    public Image Image;
    public Button Button;
    public bool IsReady = false;
    public static CropHarvest Instance = null;
    public Vector2 Debug_AchorPos;
    public Vector2 Debug_SizeDelta;
    public string CoinString = "";
    private void Awake()
    {
        Instance = this;
        TimeManager.Instance.TimeRefresher += new TimeManager.TimeHandler(RefreshTime);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Button.onClick.AddListener(() => Harvest());
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan timeSpan;
        timeSpan = MapManager.Instance.Data.LastHarvestTime + TimeSpan.FromHours(1) - TimeManager.Instance.SystemNow;

        // Play mature animation in advance
        if (timeSpan.TotalSeconds <= 3)
        {

        }

        Text.text = timeSpan.TotalSeconds > 0 ? "Harvest...\n" + timeSpan.ToString(@"dd\:hh\:mm\:ss") : "Harvest!";
        Image.color = timeSpan.TotalSeconds > 0 ? Color.white : Color.white;
        Button.interactable = timeSpan.TotalSeconds <= 0;

        IsReady = timeSpan.TotalSeconds <= 0; // TEST

        //Debug_AchorPos = CropList.rectTransform.anchoredPosition;
        //Debug_SizeDelta = CropList.rectTransform.sizeDelta;
    }

    public void Harvest()
    {
        MapManager.Instance.Data.LastHarvestTime = TimeManager.Instance.SystemNow;
        CropList.rectTransform.anchoredPosition = Vector2.zero - Vector2.up * CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        //Debug.Log(CropList.rectTransform.anchoredPosition.y);
        CropList.text = "Farm " + "Coin".ToIcon() + " " + 2000;

        int firstLevelOfMap = MapManager.Instance.LevelToMapData(MapManager.Instance.Data.CompelteLevel).StartAt;
        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        if (configIndex > 0) CropList.text += "\nCrops 1 to " + (configIndex).ToString() + "Coin".ToIcon() + " " + ((configIndex) * 50).ToString();
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompelteLevel)
            {
                CropList.text += "\n" + cropConfig.Name + " " + "Coin".ToIcon() + " 50";
            }
            else break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(CropList.rectTransform);

        CropList.rectTransform.DOAnchorPosY(CropList.rectTransform.sizeDelta.y + CropList.transform.parent.GetComponent<RectTransform>().sizeDelta.y, 10);
    }

    void RefreshTime(bool b)
    {
        //Debug.LogWarning("RefreshTime");
        MapManager.Instance.Data.LastHarvestTime = TimeManager.Instance.SystemNow;
    }
}
