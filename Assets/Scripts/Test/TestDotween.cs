using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDotween : MonoBehaviour
{
    public Transform Target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) transform.DOJump(Target.position, 2, 1, 1);//.SetEase(Ease.InOutSine);
    }
}
