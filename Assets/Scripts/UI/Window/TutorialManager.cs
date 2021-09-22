using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public float b;
    // Start is called before the first frame update
    void Start()
    {
        b = GetSize(GetComponent<RectTransform>());
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
}
