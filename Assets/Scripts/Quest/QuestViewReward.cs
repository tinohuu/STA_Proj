using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestViewReward : MonoBehaviour
{
    [SerializeField] GameObject m_UICoinPrefab;

    RewardType m_RewardType;
    Image m_RewardImage;
    TMP_Text m_RewardText;
    private void Awake()
    {
        m_RewardImage = GetComponentInChildren<Image>();
        m_RewardText = GetComponentInChildren<TMP_Text>();
    }

    public void SetReward(RewardType type, int count)
    {
        m_RewardType = type;
        m_RewardImage.sprite = type.ToSprite();
        m_RewardText.text = count.ToString("N0");
    }

    public void PlayCollectAnim(Vector3 pos)
    {
        if (m_RewardType == RewardType.Coin)
            ParticleManager.Instance.CreateParticle(m_UICoinPrefab, transform.position, true);
        else
        {
            transform.DOJump(pos, 5, 1, 1);
        }

    }
}
