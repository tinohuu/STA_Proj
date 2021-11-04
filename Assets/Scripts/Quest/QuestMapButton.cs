using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine.UI;

public class QuestMapButton : MonoBehaviour
{
    [SerializeField] GameObject m_QuestMapBarWindowPrefab;
    [SerializeField] Transform m_Collectable;
    [SerializeField] Sprite m_TimerSprite;
    [SerializeField] Sprite m_HighlightTimerSprite;
    [SerializeField] Image m_TimerImage;
    [SerializeField] TMP_Text m_TimerText;
    // Start is called before the first frame update
    void Start()
    {
        m_Collectable.localScale = Vector3.zero;
        m_Collectable.gameObject.SetActive(false);

        if (QuestManager.Instance.GetDatasToShow().Count > 0)
        {
            Window.CreateWindowPrefab(m_QuestMapBarWindowPrefab);
        }
        else
        {
            CheckCollectable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan remaingTime = QuestManager.Instance.GetRemaingTime();
        m_TimerImage.sprite = remaingTime < TimeSpan.FromHours(1) ? m_HighlightTimerSprite : m_TimerSprite;
        m_TimerText.text = remaingTime.ToString(@"hh\:mm\:ss");
        //Debug.Log(remaingTime.ToString());
    }

    public void CheckCollectable()
    {
        if (QuestManager.Instance.Data.QuestDatas.FindAll(e => e.IsCompleted).Count > 0)
        {
            m_Collectable.localScale = Vector3.zero;
            m_Collectable.gameObject.SetActive(true);
            m_Collectable.DOScale(Vector3.one, 0.25f);
        }
        else
        {
            m_Collectable.localScale = Vector3.zero;
            m_Collectable.gameObject.SetActive(false);
        }
    }
}
