using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string m_NameTextPrefix = "TXT_QST_Type";
    [SerializeField] float m_MaxBarWidth = 100;
    [SerializeField] float m_MinBarWidth = 10;

    [Header("Asset Ref")]
    [SerializeField] Sprite[] m_BackSprites = new Sprite[2];
    [SerializeField] GameObject m_RewardPrefab;

    [Header("Object Ref")]
    [SerializeField] Image m_BackImage;
    [SerializeField] Image m_IconImage;
    [SerializeField] TMP_Text m_NameText;
    [SerializeField] RectTransform m_RewardGroup;
    [SerializeField] Image m_ProgressBarImage;
    [SerializeField] TMP_Text m_ProgressText;

    [Header("Data")]
    public QuestData Data = new QuestData(QuestType.CollectStar, 3, 0);


    // Start is called before the first frame update
    void Start()
    {
        m_BackImage.sprite = m_BackSprites[Random.Range(0, m_BackSprites.Length)];
        transform.Rotate(Vector3.forward * Random.Range(-1, 2) * 1.5f);

        UpdateView();
    }

    void UpdateView()
    {
        m_IconImage.sprite = Data.Type.ToIcon();
        m_NameText.text = m_NameTextPrefix + (int)Data.Type;

        // Progress bar
        float targetWidth = m_MinBarWidth + (m_MaxBarWidth - m_MinBarWidth) * Data.Progress / Data.MaxProgress;
        Vector2 targetSize = new Vector2(targetWidth, m_ProgressBarImage.rectTransform.sizeDelta.y);
        m_ProgressBarImage.rectTransform.DOSizeDelta(targetSize, 0.5f);
        m_ProgressText.text = Data.Progress + "/" + Data.MaxProgress;

        // Rewards
        m_RewardGroup.DestroyChildren();
        var rewards = Data.Rewards;
        foreach (var type in rewards.Keys)
        {
            var rewardView = Instantiate(m_RewardPrefab, m_RewardGroup).GetComponent<QuestViewReward>();
            rewardView.SetReward(type, rewards[type]);
        }
    }
}
