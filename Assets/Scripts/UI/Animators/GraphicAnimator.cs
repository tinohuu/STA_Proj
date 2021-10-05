using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GraphicAnimator : MonoBehaviour
{
    Graphic m_Graphic;
    // Start is called before the first frame update
    void Start()
    {
        m_Graphic = GetComponent<Graphic>();
        m_Graphic.DOFade(0.5f, 0.25f).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
