using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelWindowInfoPowerup : MonoBehaviour
{
    [SerializeField] RewardType m_RewardType;
    [SerializeField] Image m_RewardImage;
    [SerializeField] TMP_Text m_RewardName;
    [SerializeField] TMP_Text m_RewardInfo;

    void Start()
    {
        SetReward(m_RewardType);
    }

    public void SetReward(RewardType rewardType)
    {
        m_RewardImage.sprite = rewardType.ToSprite();
        m_RewardName.text = "TXT_RWD_" + (int)rewardType;
        m_RewardInfo.text = "TXT_RWD_" + (int)rewardType + "_Info";
    }
}
