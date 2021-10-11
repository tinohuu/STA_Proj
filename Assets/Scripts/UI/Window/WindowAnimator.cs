using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowAnimator : Window
{
    [Header("Config")]
    [Tooltip("Please add Canvas Group component to animated UI elements")]
    public List<WindowAnimatorElement> Elements = new List<WindowAnimatorElement>();
    public bool InverseOnFadeOut = false;
    public bool ShowPanel = true;
    public bool CanCloseByPanel = true;
    public bool InQueue = true;
    public float IntervalMultiplerOnFadeOut = 1;

    public static List<WindowAnimator> WindowQueue = new List<WindowAnimator>();
    public delegate void WindowHandler();
    public event WindowHandler OnFadeIn = null;
    public static event WindowHandler OnQueueChanged = null;

    bool m_IsDestroying = false;
    //Coroutine currentCoroutine = null;
    private void Awake()
    {
        RecordWinodws();
    }
    private void OnEnable()
    {
        ResetWinodws();
        //ResetWinodws();
        FadeIn();
    }
    private void OnDisable()
    {
        CompleteWinodws();
        ResetWinodws();


        if (m_IsDestroying)
        {
            if (InQueue)
            {
                if (WindowQueue.Contains(this)) WindowQueue.Remove(this);
                OnQueueChanged?.Invoke();
            }
            //ResetWinodws();
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (WindowQueue.Contains(this)) WindowQueue.Remove(this);
    }


    [ContextMenu("Fade In")] public void FadeIn() { StopAllCoroutines(); StartCoroutine(IFadeIn()); }
    [ContextMenu("Fade Out")] void FadeOut_Menu() { StopAllCoroutines(); StartCoroutine(IFadeOut(false)); }
    public void FadeOut(bool destroy = false, bool disable = true) { StopAllCoroutines(); StartCoroutine(IFadeOut(destroy, disable)); }

    public void Close()
    {
        m_IsDestroying = true;
        FadeOut(true);
    }

    IEnumerator IFadeIn()
    {
        //SoundManager.Instance.PlaySFX("uiPanelOpen");
        CompleteWinodws();
        ResetWinodws();

        if (InQueue)
        {
            if (WindowQueue.Count > 0 && WindowQueue.Last() != this)
            {
                WindowQueue.Last().FadeOut();
            }
            if (!WindowQueue.Contains(this)) WindowQueue.Add(this);
        }

        OnFadeIn?.Invoke();
        OnQueueChanged?.Invoke();

        foreach (WindowAnimatorElement window in Elements)
        {
            window.CanvasGroup.DOFade(1, window.Duration);
            window.CanvasGroup.blocksRaycasts = true;
            //else obj.CanvasGroup.DOFade(1, obj.Duration).OnComplete(() => obj.CanvasGroup.transform.DOShakeScale(ShakeDuration, ShakeStrength, ShakeVibarato, ShakeRandomness));

            if (!window.IsScale) window.CanvasGroup.transform.DOLocalMove(window.FromOffset, window.Duration).From(true).SetEase(Ease.OutCubic);
            else window.CanvasGroup.transform.DOScale(window.FromOffset, window.Duration).From(true);
            yield return new WaitForSeconds(window.Interval);
        }
    }

    IEnumerator IFadeOut(bool destroy = false, bool disable = true)
    {

        SoundManager.Instance.PlaySFX("uiPanelClose");
        CompleteWinodws();
        Tween tween = null;
        for (int i = Elements.Count - 1; i >= 0; i--)
        {
            WindowAnimatorElement window = Elements[i];

            window.CanvasGroup.DOFade(0, window.Duration);//.PlayBackwards();
            window.CanvasGroup.blocksRaycasts = false;
            if (!window.IsScale)
                tween = window.CanvasGroup.transform.DOLocalMove(window.CanvasGroup.transform.localPosition + window.FromOffset * (InverseOnFadeOut ? -1 : 1), window.Duration).SetEase(Ease.OutCubic);//.PlayBackwards();
            else
                tween = window.CanvasGroup.transform.DOScale(window.CanvasGroup.transform.localScale + window.FromOffset * (InverseOnFadeOut ? -1 : 1), window.Duration);//.PlayBackwards();
            yield return i == 0 ? new WaitForSeconds(0) : new WaitForSeconds(Elements[i - 1].Interval * IntervalMultiplerOnFadeOut);
        }


        if (destroy)
        {
            if (InQueue)
            {
                if (WindowQueue.Contains(this)) WindowQueue.Remove(this);
                OnQueueChanged?.Invoke();
                if (WindowQueue.Count > 0)
                {
                    WindowQueue.Last().gameObject.SetActive(true);
                    WindowQueue.Last().FadeIn();
                }
            }

            yield return new WaitForSeconds(Elements.Count == 0 ? 0 : Elements[0].Duration);
            //ResetWinodws();
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(tween.Duration());
            if (disable) gameObject.SetActive(false);
        }
    }

    

    void RecordWinodws()
    {
        // Record original position and scale
        foreach (WindowAnimatorElement obj in Elements)
        {
            if (obj == null) continue;
            obj.OriPos = obj.CanvasGroup.transform.position;
            obj.OriScale = obj.CanvasGroup.transform.localScale;
            obj.CanvasGroup.alpha = 0;
        }
    }
    void ResetWinodws()
    {
        foreach (WindowAnimatorElement window in Elements)
        {
            window.CanvasGroup.transform.position = window.OriPos;
            window.CanvasGroup.transform.localScale = window.OriScale;
            window.CanvasGroup.alpha = 0;
        }
    }
    void CompleteWinodws()
    {
        foreach (WindowAnimatorElement window in Elements)
        {
            int kill = window.CanvasGroup.DOComplete();
            kill = window.CanvasGroup.transform.DOComplete();
        }
    }
}

[System.Serializable]
public class WindowAnimatorElement
{
    public CanvasGroup CanvasGroup = null;
    public Vector3 FromOffset = new Vector3(0, -20, 0);
    public float Duration = 0.5f;
    public float Interval = 0.1f;
    public bool IsScale = false;
    [HideInInspector] public Vector3 OriPos = new Vector3();
    [HideInInspector] public Vector3 OriScale = new Vector3();
    //public bool ShakeOnComplete = false;
}