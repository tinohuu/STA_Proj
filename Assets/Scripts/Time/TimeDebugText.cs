using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDebugText : MonoBehaviour
{
    static Text Text = null;
    private void Awake()
    {
        if (!Text) Text = GetComponent<Text>();
    }

    public static void Log(string log)
    {
        if (Text)
        {
            Text.text += "\n" + log;
        }
    }

}
