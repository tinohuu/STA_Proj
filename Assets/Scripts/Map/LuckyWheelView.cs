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

    [SerializeField] bool isSpinning = false;
    List<LuckyWheelRewardSlot> m_Slots = new List<LuckyWheelRewardSlot>();
    int m_WheelID = 1;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IOuterIdle());
        StartCoroutine(IInnerIdle());

        Vector3 firstSlotLocPos = m_InitPoint.localPosition;

        var rewards = LuckyWheelManager.Instance.GetRewards(m_WheelID);
        for (int i = 0; i < 8; i++)
        {
            var slot = Instantiate(m_SlotPrefab, m_SlotGroup).GetComponent<LuckyWheelRewardSlot>();
            slot.transform.localPosition = Quaternion.AngleAxis(-45 * i, Vector3.forward) * firstSlotLocPos;
            slot.UpdateView(rewards[i].ElementAt(0).Key, rewards[i].ElementAt(0).Value);
            m_Slots.Add(slot);
        }

        m_ButtonAnimator.OnClick.AddListener(() => Spin());
    }
    
    void Spin()
    {
        m_ButtonAnimator.Interactable = false;
        m_SpinText.DOFade(0.5f, 0.25f);

        int rewardIndex = LuckyWheelManager.Instance.Spin(m_WheelID);
        Debug.Log(rewardIndex);
        m_PointerImage.rectTransform.DOKill();
        Vector3 rewardRot = -Vector3.forward * rewardIndex * 45;
        m_PointerImage.rectTransform.rotation = Quaternion.identity;
        m_PointerImage.rectTransform.DOLocalRotate(-Vector3.forward * (360 * 4) + rewardRot, 10, RotateMode.LocalAxisAdd).SetEase(Ease.OutCirc).OnComplete(() => Shine());
        isSpinning = true;
        m_OuterLightImage.sprite = m_OuterLightSprites[1];
        m_InnerLightImage.sprite = m_InnerLightSprites[1];

        m_BoxRewardText.text = m_Slots[rewardIndex].RewardText;
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


        //m_ButtonAnimator.Interactable = true;
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
