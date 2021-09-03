using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDebugText : MonoBehaviour
{
    public static Text Text = null;
    private void Awake()
    {
        if (!Text) Text = GetComponent<Text>();
    }
}
