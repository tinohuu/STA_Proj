using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WindowExtensions
{
    public static string ToIcon(this string iconText)
    {
        return "<sprite name=\"" + iconText + "\">";
    }
}
