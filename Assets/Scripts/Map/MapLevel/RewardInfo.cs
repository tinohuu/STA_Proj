using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardInfo : MonoBehaviour
{
    [SerializeField] RewardType m_RewardType;
    [SerializeField] Image m_RewardImage;
    [SerializeField] TMP_Text m_RewardName;
    [SerializeField] TMP_Text m_RewardInfo;

    void Start()
    {
        UpdateView();
    }

    public void Initialize(RewardType rewardType)
    {
        m_RewardType = rewardType;
    }

    void UpdateView()
    {
        m_RewardImage.sprite = m_RewardType.ToSprite();
        m_RewardName.text = "TXT_RWD_" + (int)m_RewardType;
        m_RewardInfo.text = "TXT_RWD_" + (int)m_RewardType + "_Info";
    }
}
