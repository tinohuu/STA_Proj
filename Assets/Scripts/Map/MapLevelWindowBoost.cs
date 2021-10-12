using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelWindowBoost : MonoBehaviour
{
    [Header("Asset Ref")]
    [SerializeField] List<Sprite> BarSprites;
    [SerializeField] List<Sprite> CardsSprites;

    [Header("Self Ref")]
    [SerializeField] List<Image> BarImages;
    [SerializeField] TMP_Text TitleText;
    [SerializeField] Image CardsImage;
    [SerializeField] GameObject LockImage;
    [SerializeField] Image MinusButton;
    [SerializeField] Image PlusButton;

    [Header("Config & Data")]
    [SerializeField] List<string> BoostTitles;
    public int BoostIndex = 0;

    int maxIndex = 1;

    void Start()
    {
        UpdateMaxIndex();
        UpdateView();
    }

    void UpdateMaxIndex()
    {
        int unlockedLevel = MapManager.Instance.Data.CompleteLevel + 1;
        if (unlockedLevel >= MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == 1022).FunctionParams)
            maxIndex = 2;
        else if (unlockedLevel >= MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == 1021).FunctionParams)
            maxIndex = 1;
    }

    void UpdateView()
    {
        for (int i = 0; i < BarImages.Count; i++)
        {
            BarImages[i].sprite = BarSprites[BoostIndex];
            BarImages[i].transform.DOKill();
            BarImages[i].transform.DOScaleY(i <= BoostIndex ? 1 : 0, 0.5f);
        }
        TitleText.text = BoostTitles[BoostIndex];
        CardsImage.sprite = CardsSprites[BoostIndex];

        PlusButton.color = BoostIndex < maxIndex ? Color.white : Color.gray;
        MinusButton.color = BoostIndex > 0 ? Color.white : Color.gray;
        PlusButton.GetComponent<ButtonAnimator>().Interactable = BoostIndex < maxIndex;
        MinusButton.GetComponent<ButtonAnimator>().Interactable = BoostIndex > 0;
        LockImage.SetActive(maxIndex < 2);
    }

    public void Adjust(bool add)
    {
        if (add)
        {
            if (BoostIndex >= maxIndex) BoostIndex = maxIndex;
            else BoostIndex++;
        }
        else
        {
            if (BoostIndex <= 0) BoostIndex = 0;
            else BoostIndex--;
        }
        UpdateView();
        GetComponentInParent<MapLevelWindow>().UpdateView();
    }

    public int GetBoostModifer(bool hasFreeRound = false)
    {
        if (BoostIndex == 0) return hasFreeRound ? 0 : 1;
        else if (BoostIndex == 1) return hasFreeRound ? 1 : 2;
        else return hasFreeRound ? 2 : 4;
    }
}
