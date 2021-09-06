using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelBoost : MonoBehaviour
{
    public List<Image> BarImages;
    public List<Sprite> BarSprites;
    public List<Sprite> CardsSprites;
    public int BoostIndex = 0;
    public List<string> BoostTitles;
    public TMP_Text TitleText;
    public Image CardsImage;
    public Image MinusButton;
    public Image PlusButton;
    int maxIndex = 1;
    public GameObject LockImage;
    // Start is called before the first frame update
    void Start()
    {
        int unlockedLevel = MapManager.Instance.Data.CompleteLevel + 1;

        if (unlockedLevel >= MapManager.Instance.FunctionConfigsByFuncID[1022].FunctionParams)
        {
            maxIndex = 2;
        }
        else if (unlockedLevel >= MapManager.Instance.FunctionConfigsByFuncID[1021].FunctionParams)
        {
            maxIndex = 1;
        }

        UpdateView();
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
