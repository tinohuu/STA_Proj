using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TransformAnimator : MonoBehaviour
{

    public float ScaleTo = 1.2f;
    public float Duration = 0.5f;
    public bool SmoothExit = true;
    Vector3 oriScale;
    Tween scaleTween;
    // Start is called before the first frame update
    void Start()
    {
        oriScale = transform.localScale;
        scaleTween = transform.DOScale(Vector3.one * ScaleTo, Duration).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (SmoothExit)
        {
            scaleTween.Kill(true);
            //transform.DOScale(oriScale, Duration);
        }
    }
}
