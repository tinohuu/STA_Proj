using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundChanger : MonoBehaviour
{
    public float Time = 0.2f;
    public float PitchA = 1.2f;
    public float PitchB = 1f;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(Change());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Change()
    {
        while (true)
        {
            yield return new WaitForSeconds(Time);
            audioSource.pitch = PitchA;
            yield return new WaitForSeconds(Time);
            audioSource.pitch = PitchB;
        }
    }
}
