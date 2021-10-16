using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    AudioSource m_AudioSource;
    //Dictionary<string, AudioClip> clipsByName;

    public static SoundManager Instance;
    public List<AudioClip> Clips = new List<AudioClip>();
    private void Awake()
    {
        if (!Instance) Instance = this;

        m_AudioSource = GetComponent<AudioSource>();

        SceneManager.sceneUnloaded += (Scene scene) => CleanClips();
        //AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        //clipsByName = clips.ToDictionary(p => p.name.ToUpper());
    }
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(ITest());
        //StartCoroutine(ITest());
        UpdateMusicVolume();
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= (Scene scene) => CleanClips();
    }

    void CleanClips()
    {
        if (!this) return;
        var sources = GetComponentsInChildren<AudioSource>().ToList();
        sources.RemoveAt(0);
        var loopSources = sources.FindAll(e => e.loop = true);
        foreach (var source in loopSources)
        {
            Clips.Remove(source.clip);
            Destroy(source.gameObject);
        }
    }

    public void PlaySFX(string name, bool isSingleTrack = false, bool isLoop = false)
    {
        // Return if muted
        if (!MenuManager.Instance.Data.SoundEffects) return;

        // Get clip by name
        string fileName = "Sounds/SFX_" + name.ToUpper();
        var clip = Resources.Load<AudioClip>(fileName);
        if (clip == null) return;

        // return if unique but playing already 
        if (isSingleTrack && Clips.Contains(clip)) return;

        // Create game object
        GameObject audioObj = new GameObject(fileName);
        audioObj.transform.SetParent(transform);

        // Create audio source
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
        audioSource.clip = clip;
        Clips.Add(clip);
        var sfx = audioObj.AddComponent<SoundEffect>();
        sfx.IsLoop = isLoop;
    }

    public void StopSFX(string name)
    {
        string fileName = "Sounds/SFX_" + name.ToUpper();
        var clip = Resources.Load<AudioClip>(fileName);
        if (clip == null || !Clips.Contains(clip)) return;

        var sounds = GetComponentsInChildren<AudioSource>();
        foreach (var sound in sounds)
        {
            if (sound.clip == clip)
            {
                sound.GetComponent<SoundEffect>().FadeOut();
                return;
            }
        }
    }

    public void PlayBGM(string name)
    {
        // Return if muted
        if (!MenuManager.Instance.Data.Music) return;

        // Get clip by name
        string fileName = "Sounds/BGM_" + name.ToUpper();
        var clip = Resources.Load<AudioClip>(fileName);
        if (clip == null) return;

        // Play
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
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
