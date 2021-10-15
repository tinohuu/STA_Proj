using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneBG : MonoBehaviour
{
    public RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        float aspectRatio = Screen.width * 1.0f / Screen.height;
        Debug.Log("the screen width is: " + Screen.width + "  the screen height is: " + Screen.height + "  the aspectRatio is: " + aspectRatio);
        /*if (aspectRatio > 1.7778f)
        {
            GetComponent<Transform>().localScale = new Vector3(Screen.width / 1000.0f, 1.08f, 1.0f);
        }
        else
        {
            GetComponent<Transform>().localScale = new Vector3(Screen.width / 1000.0f, Screen.height / 1000.0f, 1.0f);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
