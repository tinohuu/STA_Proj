using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] bool m_FollowTarget = true;
    [SerializeField] bool m_ForceUpdate = false;
    [SerializeField] bool m_CloseOnNull = true;

    [Header("Ref")]
    [SerializeField] Button m_ClickButton;
    //[SerializeField] Button m_CloseButton;
    [SerializeField] List<GameObject> m_FollowingObjects = new List<GameObject>();

    [Header("Debug")]
    [SerializeField] GameObject m_Target;

    TutorialConfig m_Config;

    float m_StartTime = 0;
    UnityAction m_OnStart;
    UnityAction m_OnExit;
    //Vector3 m_TutorialPos;

    //enum Type { Circle, Flat, Auto }

    //[SerializeField] RectTransform m_Text;
    //[SerializeField] RectTransform[] m_Arrows = new RectTransform[4];
    //[SerializeField] RectTransform m_Hand;


    //[SerializeField] Button m_CirclePanel;
    //[SerializeField] Button m_FlatPanel;

    //[SerializeField] bool m_IsExiting = false;


    //int m_ArrowIndex = 0;
    //Vector2 m_ArrowOffset = Vector2.one;


    private void Awake()
    {
        //m_CirclePanel.gameObject.SetActive(false);
        //m_Hand.gameObject.SetActive(false);
        //m_FlatPanel.gameObject.SetActive(false);
    }
    void Start()
    {
        m_StartTime = Time.time;
        //m_Hand.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        m_OnStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (m_Target && (Type)(m_Config.Type) == Type.Auto)
        {
            m_Arrows[m_ArrowIndex].transform.position = Camera.main.WorldToScreenPoint(m_Target.transform.position);
            m_Arrows[m_ArrowIndex].anchoredPosition += m_ArrowOffset;
        }

        if (!m_IsExiting && (!m_Target || !m_Target.activeSelf))
        {
            m_IsExiting = true;
            GetComponent<WindowAnimator>().Close();
            if (Time.time - m_StartTime > 1) TutorialManager.Instance.Finish(m_Config);
            m_OnExit?.Invoke();
        }*/

        if (m_FollowTarget)
        {
            if (m_Target && m_ForceUpdate)
            {
                FollowTarget();
            }
            
            if (m_CloseOnNull && !m_Target && Time.time - m_StartTime > 1)
            {
                Close();
            }
        }
    }

    public void Close()
    {
        //TutorialManager.Instance.Finish(m_Config);
        if (Time.time - m_StartTime > 0.1f) TutorialManager.Instance.Finish(m_Config);
        m_OnExit?.Invoke();
        GetComponent<WindowAnimator>().Close();
    }

    public void SetTutorial(TutorialConfig config, GameObject target, UnityAction onClick = null, UnityAction onStart = null, UnityAction onExit = null)
    {
        m_Target = target;
        m_Config = config;
        m_OnStart = onStart;
        m_OnExit = onExit;
        //transform.localScale = Vector3.one * scale;

        /*m_ArrowIndex = targetPos.x > Screen.width / 2f ? 2 : 0;
        m_ArrowIndex += targetPos.y > Screen.height / 2f ? 1 : 0;
        foreach (var a in m_Arrows) a.gameObject.SetActive(false);

       //float xOffset = targetPos.x > Screen.width / 2f ? -300 : 300;
        float yOffset = targetPos.y > Screen.height / 2f ? -200 : 200;
        m_ArrowOffset = new Vector2(0, yOffset);
        targetPos += m_ArrowOffset;*/

        /*if (config.Content != "")
        {
            m_Arrows[m_ArrowIndex].gameObject.SetActive(true);
            m_Arrows[m_ArrowIndex].transform.position = m_TutorialPos;
            m_Arrows[m_ArrowIndex].anchoredPosition += m_ArrowOffset;

            config.Content = config.Content.Replace("\\n", "\n");
            m_Arrows[m_ArrowIndex].GetComponentInChildren<TMP_Text>().text = config.Content;
            //m_Arrows[arrowIndex].GetComponentInChildren<TMP_Text>().GetRenderedValues();
        }*/

        //if ((Type)(config.Type) == Type.Circle)
        if (m_ClickButton)
        {
            //m_ClickButton.gameObject.SetActive(true);
            m_ClickButton.onClick.AddListener(onClick);
            m_ClickButton.onClick.AddListener(() => Close());
            m_ClickButton.onClick.AddListener(() => m_ClickButton.interactable = false);
            //m_ClickButton.onClick.AddListener(() => GetComponent<WindowAnimator>().Close());
            //m_ClickButton.onClick.AddListener(() => TutorialManager.Instance.Finish(config));


            //m_ClickMark.gameObject.SetActive(true);
            //m_ClickMark.anchoredPosition += new Vector2(50, -50);// new Vector2(targetPos.x > Screen.width / 2f ? 100 : -100, -100);
        }
        /*else if ((Type)(config.Type) == Type.Flat)
        {
            m_FlatPanel.gameObject.SetActive(true);
            m_FlatPanel.onClick.AddListener(() => GetComponent<WindowAnimator>().Close());
            m_FlatPanel.onClick.AddListener(() => TutorialManager.Instance.Finish(config));
            m_FlatPanel.onClick.AddListener(() => m_CirclePanel.interactable = false);
        }*/
        FollowTarget();
    }

    void FollowTarget()
    {
        if (!m_Target) return;
        var canvas = m_Target.GetComponentInParent<Canvas>();
        bool isWorldElement = canvas && (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace);
        var targetTutorialPos = isWorldElement ? Camera.main.WorldToScreenPoint(m_Target.transform.position) : m_Target.transform.position;
        m_FollowingObjects.ForEach(e => e.transform.position = targetTutorialPos);
    }
}
