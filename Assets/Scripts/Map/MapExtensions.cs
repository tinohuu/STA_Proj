using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapExtensions
{
    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    public static void SetActiveImmediately(this GameObject obj, bool enable)
    {
        if (enable)
        {
            if (obj.activeSelf) obj.SetActive(false);
            obj.SetActive(true);
        }
        else
        {
            if (!obj.activeSelf) obj.SetActive(true);
            obj.SetActive(false);
        }
    }
}
