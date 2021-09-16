using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static void DestroyChildren(this Transform transform)
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Object.Destroy(child));
    }

    public static void DestroyChildrenImmediate(this Transform transform)
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Object.DestroyImmediate(child));
    }
}
