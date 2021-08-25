using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotTest : MonoBehaviour
{
    public Vector2 pivot = new Vector2();
    RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.anchoredPosition = pivot;
    }
}
