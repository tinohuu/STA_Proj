using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    const float devHeight = 5.4f;
    const float devWidth = 9.6f;

    // Start is called before the first frame update
    void Start()
    {
        float screenHeight = Screen.height;
        Debug.Log("screenHeight is : " + screenHeight);

        float orthoSize = GetComponent<Camera>().orthographicSize;

        Debug.Log("orthoSize is : " + orthoSize);

        float aspectRatio = Screen.width * 1.0f / Screen.height;

        float cameraWidth = orthoSize * 2 * aspectRatio;
        Debug.Log("Camera Width is: " + cameraWidth);

        if(cameraWidth < devWidth)
        {
            orthoSize = devWidth / (2 / aspectRatio);
            Debug.Log("new orthoSize is: " + orthoSize);
            GetComponent<Camera>().orthographicSize = orthoSize;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
