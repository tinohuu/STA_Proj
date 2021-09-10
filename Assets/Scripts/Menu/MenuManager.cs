using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SavedData] public SettingsData Data = new SettingsData();
    public static MenuManager Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    public bool Sync { get => Data.Sync; set => Data.Sync = value; }
    public bool SoundEffects { get => Data.SoundEffects; set => Data.SoundEffects = value; }
    public bool Music { get => Data.Music; set { Data.Music = value; SoundManager.Instance.UpdateMusicVolume(); } }
    public bool Notifications { get => Data.Notifications; set => Data.Notifications = value; }
    public bool PersonalizedAds { get => Data.PersonalizedAds; set => Data.PersonalizedAds = value; }
}

[System.Serializable]
public class SettingsData
{
    public bool Sync = true;
    public bool SoundEffects = true;
    public bool Music = true;
    public bool Notifications = true;
    public bool PersonalizedAds = false;
}
