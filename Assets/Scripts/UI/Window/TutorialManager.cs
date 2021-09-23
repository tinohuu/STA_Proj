using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    public delegate (bool, Vector3) TutorialCondition();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float GetSize(RectTransform rt)
    {
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt);
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    public static void Show(string name, UnityAction onClick, TutorialCondition message)
    {
        // if tutorial not shown yet
        (bool, Vector3) result = message();
        if (result.Item1)
        {
            // then show tip with name and callback 
            // make button onclick as callback
        }
    }
}
