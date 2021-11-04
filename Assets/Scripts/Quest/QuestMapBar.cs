using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class QuestMapBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float m_MaxWidth = 100;
    [SerializeField] float m_MinWidth = 10;

    [Header("Ref")]
    [SerializeField] RectTransform m_Main;
    [SerializeField] RectTransform m_Fill;
    [SerializeField] GameObject m_Completed;
    [SerializeField] TMP_Text m_ProgressText;

    [Header("Data")]
    [SerializeField] QuestData m_Data;
    //RectTransform Main => m_Main;
    UnityAction m_OnComplete;
    float m_Delay = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_Main.transform.localScale = Vector3.zero;
        SetBar(m_Data.ShownProgress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(QuestData data, UnityAction onComplete = null, float delay = 0)
    {
        m_Data = data;
        m_OnComplete = onComplete;
        m_Delay = delay;
        PlayAnim();
    }
    public void PlayAnim()
    {
        StartCoroutine(IPlayAnim());
    }

    IEnumerator IPlayAnim()
    {
        // wait for layout group
        yield return null;
        yield return null;


        // Get quest button pos
        Vector3 startPos = GetComponentInParent<HorizontalLayoutGroup>().transform.parent.position;

        // Move from button pos to real pos
        m_Main.transform.position = startPos;
        m_Main.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(m_Delay);

        m_Main.transform.DOLocalMove(Vector3.zero, 0.5f);
        m_Main.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.5f);

        int shownProgress = m_Data.ShownProgress;
        while (shownProgress < m_Data.Progress)
        {
            shownProgress++;
            SetBar(shownProgress);
            yield return new WaitForSeconds(1f/m_Data.MaxProgress);
        }

        if (shownProgress == m_Data.MaxProgress) yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.5f);

        m_Main.transform.DOMove(startPos, 0.5f);
        m_Main.transform.DOScale(Vector3.zero, 0.5f);
        yield return new WaitForSeconds(0.5f);

        m_Data.ShownProgress = shownProgress;
        m_OnComplete?.Invoke();
    }

    void SetBar(int progress)
    {
        progress = Mathf.Clamp(progress, 0, m_Data.MaxProgress);
        m_Fill.DOComplete();
        float sizeX = m_MinWidth + (m_MaxWidth - m_MinWidth) * progress / m_Data.MaxProgress;
        m_Fill.DOSizeDelta(new Vector2(sizeX, m_Fill.sizeDelta.y), 1f / m_Data.MaxProgress);
        m_ProgressText.text = progress + "/" + m_Data.MaxProgress;
        m_Completed.SetActive(progress == m_Data.MaxProgress);
    }
}
