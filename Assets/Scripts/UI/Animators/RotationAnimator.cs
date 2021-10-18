using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnimator : MonoBehaviour
{
    public float Speed = 20;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * Speed);
    }
}
