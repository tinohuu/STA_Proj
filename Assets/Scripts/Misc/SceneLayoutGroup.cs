using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLayoutGroup : MonoBehaviour
{
    public Vector3 Offset = Vector3.right;
    private void Reset()
    {
        if (!gameObject.name.Contains("SceneLayoutGroup"))
        {
            gameObject.name += " (SceneLayoutGroup)";
        }
    }
}
