using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapResizer : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: call when changing resolution
        //transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        //GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / (float)Screen.height * 1080, 1080);
    }

    private void Reset()
    {

    }
}
