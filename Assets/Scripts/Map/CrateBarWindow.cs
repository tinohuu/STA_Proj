using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateBarWindow : MonoBehaviour
{
    Transform m_Crate;
    [SerializeField] GameObject m_CrateProgressBarPrefab;
    public void Initialise(Transform crate)
    {
        m_Crate = crate;
    }

    void Start()
    {
        FadeIn();
    }

    void FadeIn()
    {
        var window = GetComponent<WindowAnimator>();
        window.OnWindowEnable.AddListener(() => StartCoroutine(IAnimateWheelIcon()));
        //window.FadeInDelay = 3;
    }

    IEnumerator IAnimateWheelIcon()
    {
        yield return null;
        MapManager.Instance.MoveMap(m_Crate.transform.position);
    }
}
