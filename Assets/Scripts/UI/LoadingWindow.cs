using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : Window
{
    public GameObject Item;
    CanvasGroup canvasGroup;
    static LoadingWindow instance;
    private void Awake()
    {
        if (!instance) instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        //TimeManager.Instance.OnGetTime += new TimeManager.TimeHandler(Fade);
    }

    public static void Play(bool fadeIn)
    {
        instance.canvasGroup.DOKill();
        instance.Item.gameObject.SetActive(fadeIn);
        instance.canvasGroup.blocksRaycasts = fadeIn;
        instance.canvasGroup.DOFade(fadeIn ? 1 : 0, 1);
    }
}
