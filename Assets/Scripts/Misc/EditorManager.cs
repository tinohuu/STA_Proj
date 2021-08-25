using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    public JsonReadWriteTest jsonReader;
    // Start is called before the first frame update
    void Start()
    {
        jsonReader.InitLevelData(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
