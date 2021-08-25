using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static Save Save = null;
    public bool ClearOnAwake = false;
    private void Awake()
    {
        if (ClearOnAwake) Clear();


        Save = SaveSystem.Load();
        if (Save == null) Save = new Save();
    }

    public static T Bind<T>(T initial)
    {
        T data = Save.Get<T>();
        if (data == null)
        {
            data = initial;
            Save.Set(data);
        }
        return data;
    }

    public static void Clear()
    {
        SaveSystem.Clear();
        Save = new Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveSystem.Save(Save);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save(Save);
    }
}