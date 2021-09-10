using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WindowExtensions
{
    public static string ToIcon(this string iconText)
    {
        return "<sprite name=\"" + iconText + "\">";
    }

    public static void DestroyChildren(this Transform transform)
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Object.Destroy(child));
    }
}
