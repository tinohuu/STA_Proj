using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_BezierCurve : MonoBehaviour
{
    public Transform PointA = null;
    public Transform PointB = null;
    public Transform PointC = null;
    public float Timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = (Time.time - Timer) / 3;
        transform.position = (1 - t) * (1 - t) * PointA.position + 2 * t * (1 - t) * PointB.position + t * t * PointC.position;
        if (Time.time - Timer >= 3) Timer = Time.time;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(PointA.position, PointB.position);
        Gizmos.DrawLine(PointB.position, PointC.position);
    }
}
