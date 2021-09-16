using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    public float Scale = 1.2f;
    public float ScaleUpDuration = 0.3f;
    public float ScaleDownDuration = 0.2f;
    public bool Interactable = true;
    public bool OnceOnly = false;
    public UnityEvent OnClick = null;
    Vector3 oriLocalScale = Vector3.one;

    [HideInInspector] public Transform targetTransform = null;
    private void Awake()
    {
        oriLocalScale = transform.localScale;
    }
    private void Start()
    {
        if (!targetTransform) targetTransform = transform;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable) return;
        Animate(true);
        //SoundManager.Instance.PlaySFX("uiCommonClick");
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Interactable) return;
        Animate(false);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactable) return;
        Animate(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;
        Animate(false);
        OnClick?.Invoke();
        if (OnceOnly) Interactable = false;
    }

    void Animate(bool isUp)
    {
        if (targetTransform.transform.DOPlay() > 0) return;
        //transform.DOPause();
        targetTransform.transform.DOScale(oriLocalScale * (isUp ? Scale : 1), ScaleUpDuration).SetEase(Ease.OutSine);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    public void CreateWindowPrefab(GameObject windowPrefab)
    {
        Window.CreateWindowPrefab(windowPrefab, null);
    }
}
