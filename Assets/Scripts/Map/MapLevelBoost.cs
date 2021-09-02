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
    // Start is called before the first frame update
    void Start()
    {
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
    }

    public void Adjust(bool add)
    {
        if (add)
        {
            if (BoostIndex >= BarImages.Count - 1) BoostIndex = BarImages.Count - 1;
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
