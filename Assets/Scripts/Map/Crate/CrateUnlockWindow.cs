using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CrateUnlockWindow : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] Image m_TitleImage;
    [SerializeField] Sprite[] m_TitleSprites = new Sprite[5];
    [SerializeField] Image[] m_CrateImages = new Image[8];

    [SerializeField] TMP_Text RatingText;

    void Start()
    {
        SoundManager.Instance.PlaySFX("chestLevelUp");
        m_CrateImages[0].transform.DOShakeScale(0.25f, Vector3.one * -0.1f, 40, 45, false).SetDelay(0.5f);
        for (int i = 1; i < m_CrateImages.Length; i++)
        {
            m_CrateImages[i].transform.DOMove(Quaternion.Euler(Vector3.forward * Random.Range(0, 180f)) * Vector3.right * 1280, 1f)
                .SetRelative(true)
                .SetEase(Ease.OutCubic)
                .SetDelay(0.7f);
        }
    }

    public void Initialise(Crate.Quality quality = Crate.Quality.Wood, int rating = 8)
    {
        m_TitleImage.sprite = m_TitleSprites[(int)quality];
        //CrateImage.sprite = CrateSprites[(int)quality];
        for (int i = 0; i < m_CrateImages.Length; i++)
        {
            Sprite sprite = Resources.Load<Sprite>("Sprites/Crate/BigBox" + quality.ToString() + i);
            m_CrateImages[i].sprite = sprite;
            m_CrateImages[i].color = sprite ? Color.white : Color.clear;
        }

        RatingText.text = rating.ToString();
    }
}
