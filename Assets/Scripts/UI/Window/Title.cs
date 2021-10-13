using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class Title : MonoBehaviour
{
    [SerializeField] float m_MinLoadingDuration = 1;
    [SerializeField] Image m_Background;
    [SerializeField] RectTransform m_ProgressBar;
    [SerializeField] RectTransform m_ProgressBarFill;
    [SerializeField] CanvasGroup m_Logo;
    void Start()
    {
        StartCoroutine(ILoadScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ILoadScene()
    {
        var load = SceneManager.LoadSceneAsync("MapTest");
        load.allowSceneActivation = false;
        Vector2 size = m_ProgressBar.rect.size;

        m_Logo.DOFade(0, 0.25f).SetDelay(m_MinLoadingDuration / 2);

        float fakeProgress = 0;
        do
        {
            //Debug.LogWarning(load.progress);
            fakeProgress = fakeProgress + Time.deltaTime;
            float shownProgress = Mathf.Min(load.progress, fakeProgress / m_MinLoadingDuration * 0.9f);

            //Debug.LogWarning(shownProgress / 0.9f * size.x);
            m_ProgressBarFill.sizeDelta = new Vector2(shownProgress / 0.9f * size.x, m_ProgressBarFill.sizeDelta.y);
            yield return null;
            //Debug.Log(load.progress + ":" + fakeProgress + ": " + m_Logo.alpha);
        }
        while (load.progress < 0.9f || fakeProgress < m_MinLoadingDuration || m_Logo.alpha > 0);

        WindowManager.Instance.BlackFadeIn(0.25f);
        yield return new WaitForSeconds(0.25f);
        load.allowSceneActivation = true;

    }
}
