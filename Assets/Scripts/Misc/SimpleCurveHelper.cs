using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCurveHelper : MonoBehaviour
{
    Vector3 startPoint;
    Vector3 midPoint;
    Vector3 endPoint;

    Vector3 firstStageSpeed;
    Vector3 secondStageSpeed;

    float fCurveTime = 0.0f;
    float fFirstStageTotalTime = 0.0f;
    float fTotalTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCurveParams(Vector3 start, Vector3 mid, Vector3 end, float fFirstStage, float fTotal)
    {
        startPoint = start;
        midPoint = mid;
        endPoint = end;

        fCurveTime = 0.0f;
        fFirstStageTotalTime = fFirstStage;
        fTotalTime = fTotal;

        firstStageSpeed.x = (mid.x - start.x) / fFirstStageTotalTime;
        firstStageSpeed.y = (mid.y - start.y) / fFirstStageTotalTime;
        firstStageSpeed.z = (mid.z - start.z) / fFirstStageTotalTime;
        firstStageSpeed = (mid - start) / fFirstStageTotalTime;

        secondStageSpeed = (end - mid) / (fTotalTime - fFirstStageTotalTime);

        Debug.Log("the firstStageSpeed is: " + firstStageSpeed);
    }

    public Vector3 UpdatePos()
    {
        fCurveTime += Time.deltaTime;

        if (fCurveTime < fFirstStageTotalTime)
        {
            return firstStageSpeed * Time.deltaTime;
        }
        else if( fCurveTime < fTotalTime )
        {
            return secondStageSpeed * Time.deltaTime;
        }

        return Vector3.zero;
    }
}
