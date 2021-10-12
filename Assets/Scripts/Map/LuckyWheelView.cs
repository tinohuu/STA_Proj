using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

public class LuckyWheelView : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] GameObject m_SlotPrefab;
    [SerializeField] Transform m_SlotGroup;
    [SerializeField] Transform m_InitPoint;
    [SerializeField] Sprite[] m_OuterLightSprites = new Sprite[2];
    [SerializeField] Sprite[] m_InnerLightSprites = new Sprite[2];
    [SerializeField] Image m_OuterLightImage;
    [SerializeField] Image m_InnerLightImage;
    [SerializeField] Image m_PointerImage;
    [SerializeField] Image m_SectorImage;
    [SerializeField] RectTransform m_Box;
    [SerializeField] CanvasGroup m_BoxText;
    [SerializeField] TMP_Text m_BoxTitleText;
    [SerializeField] TMP_Text m_BoxRewardText;
    [SerializeField] ButtonAnimator m_ButtonAnimator;
    [SerializeField] Graphic m_SpinText;
    [SerializeField] Graphic m_TickImage;
    [SerializeField] Button m_LuckyWheelButton;

    [SerializeField] bool isSpinning = false;
    [SerializeField] bool isSpun = false;
    int m_SlotIndex;
    List<LuckyWheelRewardSlot> m_Slots = new List<LuckyWheelRewardSlot>();
    LuckyWheel m_Wheel;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 firstSlotLocPos = m_InitPoint.localPosition;

        var rewards = LuckyWheelManager.GetRewards(m_Wheel.WheelID);
        //Debug.Log(m_Wheel.WheelID);
        for (int i = 0; i < rewards.Count; i++)
        {
            var slot = Instantiate(m_SlotPrefab, m_SlotGroup).GetComponent<LuckyWheelRewardSlot>();
            slot.transform.localPosition = Quaternion.AngleAxis(-45 * i, Vector3.forward) * firstSlotLocPos;
            slot.UpdateView(rewards[i].ElementAt(0).Key, rewards[i].ElementAt(0).Value);
            m_Slots.Add(slot);
        }

        m_ButtonAnimator.OnClick.AddListener(() => Spin());
        m_LuckyWheelButton.onClick.AddListener(() => Spin());

        FadeIn();
    }

    void FadeIn()
    {
        var window = GetComponent<WindowAnimator>();
        window.OnWindowEnable.AddListener(() => StartCoroutine(IAnimateWheelIcon()));
        window.FadeInDelay = 3;
    }
    
    IEnumerator IAnimateWheelIcon()
    {
        yield return null;
        MapManager.Instance.MoveMap(m_Wheel.transform.position);
        yield return new WaitForSeconds(1.5f);
        m_Wheel.ToBig();
    }

    private void OnEnable()
    {
        StartCoroutine(IOuterIdle());
        StartCoroutine(IInnerIdle());
    }

    public void SetWheel(LuckyWheel wheel)
    {
        m_Wheel = wheel;
    }

    void Spin()
    {
        m_ButtonAnimator.Interactable = false;
        m_LuckyWheelButton.interactable = false;
        m_SpinText.DOFade(0.5f, 0.25f);

        m_SlotIndex = LuckyWheelManager.Spin(m_Wheel.WheelID);
        m_PointerImage.rectTransform.DOKill();
        Vector3 rewardRot = -Vector3.forward * m_SlotIndex * 45;
        m_PointerImage.rectTransform.rotation = Quaternion.identity;
        m_PointerImage.rectTransform.DOLocalRotate(-Vector3.forward * (360 * 10) + rewardRot, 5, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine).OnComplete(() => Shine());
        isSpinning = true;
        m_OuterLightImage.sprite = m_OuterLightSprites[1];
        m_InnerLightImage.sprite = m_InnerLightSprites[1];

        m_BoxRewardText.text = m_Slots[m_SlotIndex].RewardText;

        var rewards = LuckyWheelManager.GetRewards(m_Wheel.WheelID)[m_SlotIndex];
        MapManager.Instance.Data.WheelCollectedLevel = m_Wheel.LevelID;
        Destroy(m_Wheel.gameObject);
        foreach (var type in rewards.Keys)
        {
            Reward.Data[type] += rewards[type];
        }
    }

    void Exit()
    {
        m_Slots[m_SlotIndex].transform.SetAsLastSibling();
        m_Slots[m_SlotIndex].transform.DOJump(m_ButtonAnimator.transform.position, 1, 1, 1)
            .OnComplete(() => GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => GetComponent<WindowAnimator>().Close()));
        m_Slots[m_SlotIndex].transform.DOScale(Vector3.zero, 1).SetEase(Ease.InSine);
    }

    void Shine()
    {
        m_OuterLightImage.DOFade(0, 3).SetEase(Ease.Flash, 8, 0);
        m_InnerLightImage.DOFade(0, 3).SetEase(Ease.Flash, 8, 0);
        m_SectorImage.DOFade(0, 3).SetEase(Ease.Flash, 8, 0);

        m_Box.DOSizeDelta(new Vector2(m_Box.sizeDelta.x, 600), 1);
        m_BoxText.DOFade(1, 1).SetDelay(0.5f);

        m_SpinText.DOFade(0, 0.25f);
        m_TickImage.DOFade(1, 0.25f);


        m_ButtonAnimator.Interactable = true;
        m_ButtonAnimator.OnClick.RemoveAllListeners();
        m_ButtonAnimator.OnClick.AddListener(() => Exit());
    }

    IEnumerator IOuterIdle()
    {
        while (true)
        {
            if (isSpinning)
            {
                m_OuterLightImage.rectTransform.localRotation = Quaternion.AngleAxis(Mathf.Round(m_PointerImage.rectTransform.rotation.eulerAngles.z / 45) * 45, Vector3.forward);
                m_SectorImage.rectTransform.localRotation = Quaternion.AngleAxis(Mathf.Round(m_PointerImage.rectTransform.rotation.eulerAngles.z / 45) * 45, Vector3.forward);
                yield return null;
            }
            else
            {
                m_OuterLightImage.rectTransform.Rotate(Vector3.forward, 22.5f);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    IEnumerator IInnerIdle()
    {
        while (true)
        {
            if (isSpinning)
            {
                m_InnerLightImage.rectTransform.localRotation = Quaternion.AngleAxis(Mathf.Round(m_PointerImage.rectTransform.rotation.eulerAngles.z / 45) * 45, Vector3.forward);
                yield return null;
            }
            else
            {
                m_InnerLightImage.rectTransform.Rotate(Vector3.forward, -22.5f);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
