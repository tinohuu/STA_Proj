using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    protected static Transform WindowCanvas = null;
    public void CreateWindow(GameObject windowPrefab)
    {
        CreateWindowPrefab(windowPrefab, null);
    }

    public static GameObject CreateWindowPrefab(GameObject windowPrefab, Transform transform = null)
    {
        if (WindowCanvas == null) WindowCanvas = FindObjectOfType<WindowManager>().transform;
        /*
        if (WindowCanvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) WindowCanvas = canvas.transform;
        }
        */
        return Instantiate(windowPrefab, WindowCanvas);
    }
}
