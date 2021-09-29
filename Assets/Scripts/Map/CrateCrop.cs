using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrateCrop : MonoBehaviour
{
    [SerializeField] Image m_CropImage;
    [SerializeField] Image m_RewardImage;
    [SerializeField] TMP_Text m_RewardText;
    [SerializeField] GameObject m_Reward;
    [SerializeField] Image m_Light;

    public bool IsOpen = false;

    CrateView m_CrateView;
    ButtonAnimator m_ButtonAnimator;
    Transform m_Target;
    
    private void Awake()
    {
        m_CrateView = GetComponentInParent<CrateView>();
        m_ButtonAnimator = GetComponent<ButtonAnimator>();

        m_ButtonAnimator.OnClick.AddListener(() => Open());
        m_ButtonAnimator.OnClick.AddListener(() => m_ButtonAnimator.Interactable = false);
    }

    private void Start()
    {

    }

    public void Initialize(string cropName, RewardType type, int count)
    {
        m_CropImage.sprite = cropName.ToFruitSprite();
        m_RewardImage.sprite = type.ToSprite();
        m_RewardText.text = type == RewardType.Coin ? count.ToString() : "x" + count.ToString();

    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
    }

    public void Open()
    {
        m_CropImage.transform.DOJump(m_Target.position, 1, 1, 1);
        m_CropImage.transform.DOScale(Vector3.zero, 1).SetEase(Ease.InSine).OnComplete(() => m_CrateView.ShakeCrate());
        m_Reward.gameObject.SetActive(true);
        m_Reward.transform.DOScale(Vector3.zero, 0.5f).From();
        m_CrateView.Open(this);

        IsOpen = true;
    }

    public void Collect()
    {
        
        if (IsOpen)
        {
            m_Reward.transform.DOJump(m_Target.position, 3, 1, 0.5f).SetDelay(1);
            m_Reward.transform.DOScale(Vector3.one * 0.5f, 0.5f).SetDelay(1);
        }
        else
        {
            m_CropImage.DOColor(new Color(1, 1, 1, 0), 0.25f);
            //m_CropImage.transform.DOJump(Quaternion.AngleAxis(Random.Range(-90, 90), Vector3.forward) * Vector3.up * 15, Random.Range(1, 3), 1, 0.5f); ;
            //m_CropImage.rectTransform.DOAnchorPosY(800, 1).SetRelative().SetEase(Ease.InSine);

            m_Reward.gameObject.SetActive(true);
            m_RewardImage.color = new Color(1, 1, 1, 0.5f);

            m_Reward.transform.DOScale(Vector3.zero, 0.5f).From();
            m_Light.color = Color.clear;
            //m_CropImage.DOFade(0, 0.25f);
        }
    }
}
