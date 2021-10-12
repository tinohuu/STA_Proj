using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class WindowManager : MonoBehaviour
{
    public List<GameObject> Views = new List<GameObject>();
    public WindowAnimator CurView;
    [SerializeField] Image m_SceneFade;
    [SerializeField] GameObject[] m_MapButtons;
    [SerializeField] GameObject[] m_GameButtons;

    public int TestNumber = 0;

    public static WindowManager Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        SceneManager.sceneLoaded += OnLoadScene;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoadScene;
    }

    void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        foreach (var button in m_MapButtons) button.gameObject.SetActive(scene.name == "MapTest");
        foreach (var button in m_GameButtons) button.gameObject.SetActive(scene.name != "MapTest");


        float delay = 0;
        /*if (!m_SceneFade.gameObject.activeSelf)
        {
            m_SceneFade.gameObject.SetActive(true);
            m_SceneFade.color = Color.black;
            m_SceneFade.DOFade(1, 0.25f);
            delay = 0.25f;
        }*/
        if (scene.name == "MapTest") delay = 0.5f;

        m_SceneFade.gameObject.SetActive(true);
        m_SceneFade.color = Color.black;
        m_SceneFade.DOFade(0, 0.5f).SetDelay(delay).OnComplete(() => m_SceneFade.gameObject.SetActive(false));
    }
}
