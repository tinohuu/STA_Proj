using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrateUnlockWindow : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] Image TitleImage;
    [SerializeField] Sprite[] TitleSprites = new Sprite[5];
    [SerializeField] Image CrateImage;
    [SerializeField] Sprite[] CrateSprites = new Sprite[5];
    [SerializeField] TMP_Text RatingText;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialise(Crate.Quality quality, int rating)
    {
        TitleImage.sprite = TitleSprites[(int)quality];
        CrateImage.sprite = CrateSprites[(int)quality];
        RatingText.text = rating.ToString();
    }
}
