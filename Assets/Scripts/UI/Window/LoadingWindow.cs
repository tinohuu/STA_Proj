using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : Window
{
    public GameObject Item;
    CanvasGroup canvasGroup;
    public static LoadingWindow Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        //TimeManager.Instance.OnGetTime += new TimeManager.TimeHandler(Fade);
    }

    public void Play(bool fadeIn)
    {
        StopAllCoroutines();
        StartCoroutine(IPlay(fadeIn));
    }

    IEnumerator IPlay(bool fadeIn)
    {
        yield return null;
        Instance.canvasGroup.DOKill(true);

        Instance.Item.gameObject.SetActive(fadeIn);

        Instance.canvasGroup.blocksRaycasts = fadeIn;
        Instance.canvasGroup.DOFade(fadeIn ? 1 : 0, 1);
    }
}
