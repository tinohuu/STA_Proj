using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestViewReward : MonoBehaviour
{
    Image m_RewardImage;
    TMP_Text m_RewardText;
    private void Awake()
    {
        m_RewardImage = GetComponentInChildren<Image>();
        m_RewardText = GetComponentInChildren<TMP_Text>();
    }

    public void SetReward(RewardType type, int count)
    {
        m_RewardImage.sprite = type.ToSprite();
        m_RewardText.text = count.ToString("N0");
    }
}
