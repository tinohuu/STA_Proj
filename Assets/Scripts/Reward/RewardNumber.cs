using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardNumber : MonoBehaviour
{
    public int current = 0;
    public RewardType Type = RewardType.None;

    TMP_Text text;
    Vector3 oriScale;
    Tween textTween = null;
    private void Start()
    {
        RewardManager.Instance.OnValueChanged[(int)Type] += new RewardManager.RewardHandler(Animate);
        text = GetComponent<TMP_Text>();
        oriScale = transform.localScale;
        Animate();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Reward.Data[Type] += 9999;
        }
    }
    void Animate(bool add)
    {
        Animate(0.5f);
    }

    public void Animate( float duration = 0.5f, float delay = 1)
    {
        textTween.Kill(false);
        if (textTween.IsActive()) duration = duration >= textTween.Duration() - textTween.Elapsed() ? duration : textTween.Duration() - textTween.Elapsed();
        textTween = DOTween.To(() => current, x => current = x, Reward.Data[Type], duration)
            .SetDelay(delay)
            .OnStart(() => AnimateScale(true))
            .OnUpdate(() => text.text = current.ToString("N0"))
            .OnComplete(() => AnimateScale(false));
    }


    void AnimateScale(bool on = true)
    {
        if (on)
        {
            transform.DOKill();
            transform.localScale = oriScale;
            transform.DOScale(1.2f, 0.25f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            transform.DOKill();
            transform.DOScale(oriScale, 0.25f);
        }
    }

    private void OnDestroy()
    {
        RewardManager.Instance.OnValueChanged[(int)Type] -= new RewardManager.RewardHandler(Animate);
    }
}
