using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteShadowTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        GetComponent<Renderer>().receiveShadows = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Reset()
    {

    }
}
