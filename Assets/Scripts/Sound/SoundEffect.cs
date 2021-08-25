using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        StartCoroutine(IComplete(audioSource.clip.length));
    }
    private IEnumerator IComplete(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        SoundManager.Instance.Clips.Remove(audioSource.clip);
        Destroy(gameObject);
    }
}
