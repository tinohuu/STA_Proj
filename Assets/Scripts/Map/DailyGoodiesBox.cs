using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoodiesBox : MonoBehaviour
{
    [SerializeField] TMP_Text m_Text;
    public Transform  BoxParent;
    Animator m_BoxAnimator;
    [SerializeField] Transform m_RewardGroup;
    [SerializeField] GameObject m_Tick;
    [SerializeField] GameObject m_RewardPrefab;
    [SerializeField] GameObject[] m_BoxPrefabs = new GameObject[3];
    public int Day = 0;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
        int index;
        if (Day == 28) index = 2;
        else if (Day % 7 == 0) index = 1;
        else index = 0;
        m_BoxAnimator = Instantiate(m_BoxPrefabs[index], BoxParent).GetComponent<Animator>();
    }

    public void SetState(string state)
    {
        m_BoxAnimator.CrossFadeInFixedTime(state, 0.25f);
    }

    public void UpdateView(int collectedDays)
    {
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
        m_Text.text = "Day " + Day;

        bool isCollected = Day <= collectedDays;
        m_Tick.transform.DOScale(isCollected ? Vector3.one : Vector3.zero, 0.5f);
        m_BoxAnimator.transform.DOScale(!isCollected ? Vector3.one : Vector3.zero, 0.5f);

        m_RewardGroup.DestroyChildren();
        if (isCollected)
        {
            var config = DailyGoodiesManager.Instance.GetGoodyConfig(Day);
            var countsByType = Reward.StringToReward(config.ItemReward);
            if (config.CoinReward > 0)
            {
                Instantiate(m_RewardPrefab, m_RewardGroup);
            }

            int i = 0;
            foreach (var type in countsByType.Keys)
            {
                Image image = Instantiate(m_RewardPrefab, m_RewardGroup).GetComponent<Image>();
                var icons = Resources.LoadAll<Sprite>("Sprites/IconAtlas");
                Sprite sprite = Array.Find(icons, e => e.name == type.ToString());
                image.sprite = sprite;
                image.transform.SetAsFirstSibling();
                i++;
                image.rectTransform.anchoredPosition = Vector2.left * i * 50 + Vector2.up * i * 50;
            }

        }
        else
        {
            m_RewardGroup.DestroyChildren();
        }
    }
}
