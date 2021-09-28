using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrateCrop : MonoBehaviour
{
    [SerializeField] Image CropImage;
    [SerializeField] Image RewardImage;
    [SerializeField] TMP_Text RewardText;
    [SerializeField] GameObject m_Reward;
    CrateView m_CrateView;
    ButtonAnimator m_ButtonAnimator;
    private void Awake()
    {
        m_CrateView = GetComponentInParent<CrateView>();
        m_ButtonAnimator = GetComponent<ButtonAnimator>();

        m_ButtonAnimator.OnClick.AddListener(() => Open());
        m_ButtonAnimator.OnClick.AddListener(() => m_ButtonAnimator.Interactable = false);
    }

    public void Initialize(string cropName, RewardType type, int count)
    {
        CropImage.sprite = cropName.ToFruitSprite();
        RewardImage.sprite = type.ToSprite();
        RewardText.text = type == RewardType.Coin ? count.ToString() : "x" + count.ToString();
    }

    public void Open()
    {
        CropImage.transform.DOJump(m_CrateView.CrateTop.position, 1, 1, 1);
        CropImage.transform.DOScale(Vector3.zero, 1).SetEase(Ease.InSine).OnComplete(() => m_CrateView.Crate.DOShakeScale(0.2f, 0.2f, 2));
        m_Reward.gameObject.SetActive(true);
        m_Reward.transform.DOScale(Vector3.zero, 0.5f).From();
        m_CrateView.Open();
    }
}
