using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RectTransformEditorExtension
{

    [MenuItem("CONTEXT/RectTransform/Size to Anchors", false, 150)]
    static void SizeToAnchors()
    {
        RectTransform rt = Selection.activeGameObject.GetComponent<RectTransform>();
        RectTransform prt = rt.parent.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        Vector3[] prtCorners = new Vector3[4];

        rt.GetWorldCorners(corners);
        prt.GetWorldCorners(prtCorners);

        float prtWidth = prtCorners[3].x - prtCorners[0].x;
        float prtHeight = prtCorners[1].y - prtCorners[0].y;

        float minX = (corners[0].x - prtCorners[0].x) / prtWidth;
        float minY = (corners[0].y - prtCorners[0].y) / prtHeight;

        float maxX = 1 - (prtCorners[3].x - corners[3].x) / prtWidth;
        float maxY = 1 - (prtCorners[1].y - corners[1].y) / prtHeight;

        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        //rt.SetRight(0);
        //rt.SetTop(0);
        //rt.SetBottom(0);
    }
}
