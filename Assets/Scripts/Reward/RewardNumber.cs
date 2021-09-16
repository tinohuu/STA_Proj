using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardNumber : MonoBehaviour
{
    public int current = 0;
    public RewardType Type = RewardType.None;

    static bool[] m_switches;

    public static RewardNumberSwitches Switches = new RewardNumberSwitches();
    public Transform ParticleGroup;
    Coroutine animCoroutine;

    TMP_Text text;
    Vector3 oriScale;
    Tween textTween = null;

    private void Awake()
    {
        if (m_switches == null)
        {
            m_switches = new bool[System.Enum.GetValues(typeof(RewardType)).Length];
            for (int i = 0; i < m_switches.Length; i++) m_switches[i] = true;
        }
        ParticleGroup = ParticleManager.Instance.ParticleGroup;
    }

    private void Start()
    {
        RewardManager.Instance.OnValueChanged[(int)Type] += new RewardManager.RewardHandler(Animate);
        text = GetComponent<TMP_Text>();
        oriScale = transform.localScale;
        Animate();
    }
    private void Update()
    {

    }
    void Animate(bool add)
    {
        Animate();
    }

    public void Animate()
    {
        if (!Switches[Type]) return;
        if (animCoroutine == null) animCoroutine = StartCoroutine(IAnimate());
        /*textTween.Kill(false);
        if (textTween.IsActive()) duration = duration >= textTween.Duration() - textTween.Elapsed() ? duration : textTween.Duration() - textTween.Elapsed();
        textTween = DOTween.To(() => current, x => current = x, Reward.Data[Type], duration)
            .SetDelay(delay)
            .OnStart(() => AnimateScale(true))
            .OnUpdate(() => text.text = current.ToString("N0"))
            .OnComplete(() => AnimateScale(false));*/
    }

    IEnumerator IAnimate()
    {
        AnimateScale(true);
        while (current < Reward.Data[Type])
        {
            int diff = Reward.Data[Type] - current;
            int target = current + (ParticleGroup.childCount > 0 ? (int)(diff * 0.5f) : diff);
            DOTween.To(() => current, x => current = x, target, 1)
                .OnUpdate(() => text.text = current.ToString("N0"));
            yield return new WaitForSeconds(1);
        }
        AnimateScale(false);
        animCoroutine = null;
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

    public class RewardNumberSwitches
    {
        public bool this[RewardType type]
        {
            get => m_switches[(int)type];
            set => m_switches[(int)type] = value;
        }
    }
}
