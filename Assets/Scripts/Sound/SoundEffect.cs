using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SoundEffect : MonoBehaviour
{
    public bool IsLoop = false;

    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        audioSource.loop = IsLoop;
        if (!IsLoop) StartCoroutine(IComplete(audioSource.clip.length));
    }
    private IEnumerator IComplete(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        SoundManager.Instance.Clips.Remove(audioSource.clip);
        Destroy(gameObject);
    }

    public void FadeOut(float durtaion = 1f)
    {
        StartCoroutine(IFadeOut(durtaion));
    }

    private IEnumerator IFadeOut(float duration)
    {
        audioSource.DOFade(0, duration);
        yield return new WaitForSeconds(duration);
        SoundManager.Instance.Clips.Remove(audioSource.clip);
        Destroy(gameObject);
    }
}
