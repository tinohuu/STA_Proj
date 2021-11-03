using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWindow : MonoBehaviour
{
    [SerializeField] CanvasGroup m_View;
    [SerializeField] GameObject m_QuestViewPrefab;
    [SerializeField] RectTransform m_QuestViewGroup;
    [SerializeField] RectTransform m_CompletedQuestViewGroup;
    [SerializeField] RectTransform m_Foreground;
    [SerializeField] ButtonAnimator m_CollectButton;
    [SerializeField] CanvasGroup m_MissionCompleted;


    // Start is called before the first frame update
    void Start()
    {
        m_CollectButton.transform.localScale = Vector3.zero;
        m_MissionCompleted.gameObject.SetActive(false);

        UpdateView();

        m_CollectButton.OnClick.AddListener(() => StartCoroutine(ICollect()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Update View")]
    void UpdateView()
    {
        m_QuestViewGroup.DestroyChildren();
        foreach (var data in QuestManager.Instance.Data.QuestDatas)
        {
            var view = Instantiate(m_QuestViewPrefab, m_QuestViewGroup).GetComponent<QuestView>();
            view.Data = data;
        }

        AnimateQuestViews();
    }

    [ContextMenu("Animate")]
    void AnimateQuestViews()
    {
        StartCoroutine(IAnimateQuestViews());
    }

    IEnumerator IAnimateQuestViews()
    {
        var questViews = m_QuestViewGroup.GetComponentsInChildren<QuestView>();
        var newQuestViews = Array.FindAll(questViews, e => e.Data.ShownProgress == -1);
        if (newQuestViews.Length > 0)
        {
            for (int i = 0; i < newQuestViews.Length; i++)
            {
                newQuestViews[i].transform.DOScale(0, 0.5f).From().SetEase(Ease.OutBack).SetDelay(0.3f + i * 0.3f);
                newQuestViews[i].Data.ShownProgress = 0;
            }
        }
        else
        {
            for (int i = 0; i < questViews.Length; i++)
                questViews[i].transform.DOScale(0.15f, 0.5f).SetRelative().SetEase(Ease.Flash, 2).SetDelay(0.3f + i * 0.3f);
        }
        yield return new WaitForSeconds(2);
        AnimateCompletedQuestViews();

    }

    [ContextMenu("Animate Completed")]
    void AnimateCompletedQuestViews()
    {
        StartCoroutine(IAnimateComepletedQuestViews());
    }

    IEnumerator IAnimateComepletedQuestViews()
    {
        var views = m_QuestViewGroup.GetComponentsInChildren<QuestView>();
        var completedViews = Array.FindAll(views, e => e.Data.IsCompleted);

        if (completedViews.Length  == 0) yield break;

        List<(QuestView, QuestView)> oldNewQuestViews = new List<(QuestView, QuestView)>();

        m_CompletedQuestViewGroup.DestroyChildren();
        foreach (var completedView in completedViews)
        {
            var newView = Instantiate(completedView.gameObject, m_CompletedQuestViewGroup).GetComponent<QuestView>();
            newView.Data = completedView.Data;
            newView.BlockAnim = true;
            newView.View.alpha = 0;
            oldNewQuestViews.Add((completedView, newView));
        }

        yield return null;
        yield return null;

        for (int i = 0; i < oldNewQuestViews.Count; i++)
        {
            var oldView = oldNewQuestViews[i].Item1;
            var newView = oldNewQuestViews[i].Item2;

            newView.View.transform.position = oldView.View.transform.position;
            oldView.View.alpha = 0;
            newView.View.alpha = 1;
            newView.View.transform.DOJump(newView.transform.position, 5, 1, 1).SetDelay(i * 0.125f);
            newView.View.transform.DOScale(0.2f, 2).SetRelative().SetEase(Ease.InOutBack);
            newView.View.transform.DORotateQuaternion(Quaternion.identity, 0.25f);
        }

        yield return new WaitForSeconds(0.5f);
        m_View.DOFade(0, 0.5f);
        m_CompletedQuestViewGroup.transform.SetParent(transform);
        yield return new WaitForSeconds(0.5f);

;
        m_MissionCompleted.DOFade(1, 0.5f).OnStart(() => m_MissionCompleted.gameObject.SetActive(true));

        m_CollectButton.transform.DOScale(Vector3.one, 0.25f);
        m_CollectButton.Interactable = true;
    }

    IEnumerator ICollect()
    {
        m_MissionCompleted.DOFade(0, 0.5f).OnComplete(() => m_MissionCompleted.gameObject.SetActive(false));

        m_CollectButton.Interactable = false;
        m_CollectButton.transform.DOKill();
        m_CollectButton.transform.DOScale(Vector3.zero, 0.25f);

        QuestManager.Instance.ComepleteData();

        var rewardViews = m_CompletedQuestViewGroup.GetComponentsInChildren<QuestViewReward>();
        Array.ForEach(rewardViews, e => e.PlayCollectAnim(Vector3.zero));

        yield return new WaitForSeconds(2);

        var completedQuestViews = m_CompletedQuestViewGroup.GetComponentsInChildren<QuestView>();
        Array.ForEach(completedQuestViews, e => e.View.DOFade(0, 0.25f));

        yield return new WaitForSeconds(0.25f);

        m_CompletedQuestViewGroup.transform.SetParent(m_Foreground.parent);
        m_Foreground.SetAsLastSibling();
        m_View.DOFade(1, 0.5f);
        m_CompletedQuestViewGroup.DestroyChildren();

        UpdateView();
    }
}
