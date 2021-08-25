using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCallTest : MonoBehaviour
{
    public Texture2D Texture;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Texture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
