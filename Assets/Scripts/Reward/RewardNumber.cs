using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardNumber : MonoBehaviour
{
    [SerializeField] bool m_RefreshOnDisable = false;

    public RewardType Type = RewardType.None;
    public bool BlockAnimation = false;

    static bool[] m_switches;

    public static RewardNumberSwitches Switches = new RewardNumberSwitches();

    int m_CurNumber = 0;
    Coroutine m_AnimCoroutine;
    TMP_Text m_NumberText;
    Vector3 m_OriScale;

    private void Awake()
    {
        if (m_switches == null)
        {
            m_switches = new bool[System.Enum.GetValues(typeof(RewardType)).Length];
            for (int i = 0; i < m_switches.Length; i++) m_switches[i] = true;
        }
        RewardManager.Instance.OnValueChanged[(int)Type].AddListener(() => Animate());
    }


    private void Start()
    {

        m_NumberText = GetComponent<TMP_Text>();
        m_OriScale = transform.localScale;

        m_CurNumber = Reward.Data[Type];
        m_NumberText.text = Reward.Data[Type].ToString("N0");
        Animate();
    }
    private void Update()
    {

    }

    private void OnEnable()
    {
        UpdateView();
    }

    void Animate(bool add)
    {
        Animate();
    }

    public void UpdateView()
    {
        //current = Reward.Data[Type];
        m_NumberText = GetComponent<TMP_Text>();
        m_NumberText.text = Reward.Data[Type].ToString("N0");
        //Animate();
    }

    public void Animate()
    {
        if (!Switches[Type] || !gameObject.activeInHierarchy)
        {
            //if (animCoroutine != null) StopCoroutine(animCoroutine);
            //current = Reward.Data[Type];
            //text.text = Reward.Data[Type].ToString("N0");
            //AnimateScale(false);
            //animCoroutine = null;
            return;
        }

        if (BlockAnimation)
        {
            m_NumberText.text = Reward.Data[Type].ToString("N0");
            return;
        }


        if (m_AnimCoroutine == null) m_AnimCoroutine = StartCoroutine(IAnimate());
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
        while (m_CurNumber < Reward.Data[Type] - 1)
        {
            AnimateScale(true);
            yield return new WaitForSeconds(1);
            int diff = Reward.Data[Type] - m_CurNumber;
            int target = m_CurNumber + (ParticleManager.Instance.ParticleGroup.childCount > 0 ? (int)(diff * 0.5f) : diff);
            DOTween.To(() => m_CurNumber, x => m_CurNumber = x, target, 1)
                .OnUpdate(() => m_NumberText.text = m_CurNumber.ToString("N0"));
        }
        m_NumberText.text = Reward.Data[Type].ToString("N0");
        AnimateScale(false);
        m_AnimCoroutine = null;
    }

    void AnimateScale(bool on = true)
    {
        if (on)
        {
            transform.DOKill();
            transform.localScale = m_OriScale;
            transform.DOScale(m_OriScale * 1.1f, 0.25f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            transform.DOKill();
            transform.DOScale(m_OriScale, 0.25f);
        }
    }

    private void OnDestroy()
    {
        RewardManager.Instance.OnValueChanged[(int)Type].RemoveListener(() => Animate());// -= new RewardManager.RewardHandler(Animate);
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
