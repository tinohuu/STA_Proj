using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : Window
{
    public GameObject Item;
    CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        TimeManager.Instance.OnGetTime += new TimeManager.TimeHandler(Fade);
    }

    void Fade()
    {
        bool isIn = TimeManager.Instance.IsGettingTime;
        canvasGroup.DOKill();
        Item.gameObject.SetActive(isIn);
        canvasGroup.blocksRaycasts = isIn;
        canvasGroup.DOFade(isIn ? 1 : 0, 1);
    }
}
