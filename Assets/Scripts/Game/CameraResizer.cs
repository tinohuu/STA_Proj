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
        Debug.Log("aspectRatio is: " + aspectRatio);

        float cameraWidth = orthoSize * 2 * aspectRatio;
        Debug.Log("Camera Width is: " + cameraWidth);

        //Screen.SetResolution(1920, 1080, true);

        /*Resolution[] resolutions = Screen.resolutions;
        int nWidth = resolutions[0].width;
        int nHeight = resolutions[0].height;
        for(int i = 0; i < resolutions.Length; ++i)
        {
            if(resolutions[i].width > nWidth)
            {
                nWidth = resolutions[i].width;
                nHeight = resolutions[i].height;
            }
            if(resolutions[i].width == nWidth && nHeight > resolutions[i].height)
            {
                nWidth = resolutions[i].width;
                nHeight = resolutions[i].height;
            }
        }

        Debug.Log("the resolution width is: " + nWidth + "  height is: " + nHeight);
        Screen.SetResolution(nWidth, nHeight, true);*/
        //Screen.fullScreen = true;

        /*if(cameraWidth < devWidth * 2)
        {
            orthoSize = devWidth / (2 / aspectRatio);
            Debug.Log("new orthoSize is: " + orthoSize);
            GetComponent<Camera>().orthographicSize = orthoSize;
        }*/

        if (aspectRatio > 1.78f)
        {
            //GetComponent<Transform>().localScale = new Vector3(Screen.width / 1000.0f, 1.08f, 1.0f);
            //Screen.SetResolution(Screen.width, 1080, true);
            GetComponent<Camera>().orthographicSize = orthoSize / (aspectRatio / 1.78f);
        }
        else
        {
            //GetComponent<Transform>().localScale = new Vector3(Screen.width / 1000.0f, Screen.height / 1000.0f, 1.0f);
            ;// Screen.SetResolution(Screen.width, Screen.height, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
