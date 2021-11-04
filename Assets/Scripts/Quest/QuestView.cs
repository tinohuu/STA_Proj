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
    [SerializeField] CanvasGroup m_View;
    [SerializeField] Image m_BackImage;
    [SerializeField] Image m_IconImage;
    [SerializeField] TMP_Text m_NameText;
    [SerializeField] RectTransform m_RewardGroup;
    [SerializeField] Image m_ProgressBarImage;
    [SerializeField] TMP_Text m_ProgressText;
    [SerializeField] CanvasGroup m_CompletedIcon;
    [SerializeField] CanvasGroup m_CompletedBar;

    public CanvasGroup View => m_View;

    [Header("Data")]
    public QuestData Data = new QuestData(QuestType.CollectStar, 3, 0);
    public bool BlockAnim = false;

    void Awake()
    {
        m_CompletedIcon.alpha = 0;
        m_CompletedBar.alpha = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_BackImage.sprite = m_BackSprites[Random.Range(0, m_BackSprites.Length)];
        transform.Rotate(Vector3.forward * Random.Range(-1, 2) * 1.5f);

        UpdateView();
    }



    void UpdateView(bool blockAnim = false)
    {
        m_IconImage.sprite = Data.Type.ToIcon();
        m_NameText.text = m_NameTextPrefix + (int)Data.Type;

        // Progress bar
        /*float oldWidth = m_MinBarWidth + (m_MaxBarWidth - m_MinBarWidth) * Data.ShownProgress / Data.MaxProgress;
        Vector2 oldSize = new Vector2(oldWidth, m_ProgressBarImage.rectTransform.sizeDelta.y);
        m_ProgressBarImage.rectTransform.sizeDelta = oldSize;
        Data.ShownProgress = Data.Progress;*/

        float targetWidth = m_MinBarWidth + (m_MaxBarWidth - m_MinBarWidth) * Data.Progress / Data.MaxProgress;
        Vector2 targetSize = new Vector2(targetWidth, m_ProgressBarImage.rectTransform.sizeDelta.y);
        m_ProgressBarImage.rectTransform.DOSizeDelta(targetSize, 0.5f);
        m_ProgressText.text = Data.Progress + "/" + Data.MaxProgress;

        if (Data.Progress == Data.MaxProgress)
        {
            if (BlockAnim)
            {
                m_CompletedIcon.alpha = 1;
                m_IconImage.color = Color.clear;
                //m_CompletedIcon.transform.DOScale(0.2f, 1).From(true).SetDelay(1);
                m_CompletedBar.alpha = 1;
            }
            else
            {
                m_CompletedIcon.DOFade(1, 0.5f).SetDelay(1);
                m_IconImage.DOFade(0, 0.5f).SetDelay(1);
                m_CompletedIcon.transform.DOScale(1, 0.75f).From(true).SetDelay(1);
                m_CompletedBar.DOFade(1, 0.5f).SetDelay(1);
            }

        }


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
