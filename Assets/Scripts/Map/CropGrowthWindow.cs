using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropGrowthWindow : MonoBehaviour
{
    [SerializeField] Image CropImage;
    [SerializeField] TMP_Text CropText;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void SetCrop(string name)
    {
        CropImage.sprite = Resources.Load<Sprite>("Sprites/Crops/Crop_Fruit_" + name);
        CropText.text = name;
    }
}
