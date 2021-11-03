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
    [SerializeField] Transform m_ButtonGroup;
    [SerializeField] GameObject[] m_MapButtons;
    [SerializeField] GameObject[] m_GameButtons;

    public int TestNumber = 0;

    public static WindowManager Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        SceneManager.sceneLoaded += OnLoadScene;

    }

    void OnEnable()
    {

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoadScene;
    }

    public void LoadSceneWithFade(string sceneName, float durtaion = 0.5f)
    {
        StartCoroutine(ILoadSceneWithFade(sceneName, durtaion));
    }
    IEnumerator ILoadSceneWithFade(string sceneName, float durtaion = 0.5f)
    {
        BlackFadeIn(durtaion);
        yield return new WaitForSeconds(durtaion);
        SceneManager.LoadScene(sceneName);

    }

    public void BlackFadeIn(float duration = 0.5f)
    {
        m_SceneFade.gameObject.SetActive(true);
        m_SceneFade.DOFade(1f, duration);
    }

    void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        m_ButtonGroup.gameObject.SetActive(!scene.name.Contains("Map"));
        /*for (int i = 0; i < m_ButtonGroup.childCount; i++)
        {
            m_ButtonGroup.GetChild(i).gameObject.SetActive(false);
        }

        GameObject[] buttons = new GameObject[0];
        switch (scene.name)
        {
            case "MapTest":
                buttons = m_MapButtons;
                break;
            case "GameScene":
                buttons = m_GameButtons;
                break;
        }
        foreach (var button in buttons) button.gameObject.SetActive(true);*/


        float delay = 0;
        /*if (!m_SceneFade.gameObject.activeSelf)
        {
            m_SceneFade.gameObject.SetActive(true);
            m_SceneFade.color = Color.black;
            m_SceneFade.DOFade(1, 0.25f);
            delay = 0.25f;
        }*/
        if (scene.name == "MapTest") delay = 0.1f;

        m_SceneFade.gameObject.SetActive(true);
        m_SceneFade.color = Color.black;
        m_SceneFade.DOFade(0, 0.25f).SetDelay(delay).OnComplete(() => m_SceneFade.gameObject.SetActive(false));

        foreach (var winodw in WindowAnimator.WindowQueue)
        {
            if (!winodw.IsCrossScene) winodw.Close();
        }
    }
}
