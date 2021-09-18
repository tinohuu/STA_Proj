using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;
    Dictionary<string, AudioClip> clipsByName;

    public static SoundManager Instance;
    public List<AudioClip> Clips = new List<AudioClip>();
    private void Awake()
    {
        if (!Instance) Instance = this;

        audioSource = GetComponent<AudioSource>();

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        clipsByName = clips.ToDictionary(p => p.name.ToUpper());
    }
    // Start is called before the first frame update
    void Start()
    {
        PlayBGM("main");
        StartCoroutine(ITest());
        StartCoroutine(ITest());
        UpdateMusicVolume();
    }

    public void PlaySFX(string name, bool isSingle = false)
    {
        // Return if muted
        if (!MenuManager.Instance.Data.SoundEffects) return;

        // Get clip by name
        string fileName = "SFX_" + name.ToUpper();
        if (!clipsByName.ContainsKey(fileName)) return;
        AudioClip clip = clipsByName[fileName];

        // return if unique but playing already 
        if (isSingle && Clips.Contains(clip)) return;

        // Create game object
        GameObject audioObj = new GameObject(fileName);
        audioObj.transform.SetParent(Instance.transform);

        // Create audio source
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
        audioSource.clip = clip;
        Clips.Add(clip);
        audioObj.AddComponent<SoundEffect>();
    }

    public void PlayBGM(string name)
    {
        // Return if muted
        if (!MenuManager.Instance.Data.Music) return;

        // Get clip by name
        string fileName = "BGM_" + name.ToUpper();
        if (!clipsByName.ContainsKey(fileName)) return;
        AudioClip clip = clipsByName[fileName];

        // Play
        audioSource.clip = clip;
        audioSource.Play();
    }

    IEnumerator ITest()
    {
        yield return new WaitForSeconds(3);
    }

    public void UpdateMusicVolume()
    {
        if (MenuManager.Instance.Data.Music) GetComponent<AudioSource>().Play();
        else GetComponent<AudioSource>().Pause();
    }
}
