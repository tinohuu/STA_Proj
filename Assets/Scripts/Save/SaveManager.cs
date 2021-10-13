using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class SaveManager : MonoBehaviour
{
    public Save Save = null;
    public bool ClearOnAwake = false;
    bool StopSave = false;
    public static SaveManager Instance;
    private void Awake()
    {
        Save = SaveSystem.Load();
        if (!Instance) Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        //if (Save == null) Save = new Save();

        AttrBindAll();
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        //Save = SaveSystem.Load();
        if (Save != null) AttrBindAll();
    }

    void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        SaveSystem.Save(Save);
    }

    private void OnDestroy()
    {
        SaveSystem.Save(Save);
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public T Bind<T>(T initial)
    {
        T data = Save.Get<T>();
        if (data == null)
        {
            data = initial;
            Save.Set(data);
        }
        return data;
    }

    public object AttrBind(Type type, object initial = default)
    {
        object data = Save.AttrGet(type, initial);
        if (data == null)
        {
            data = initial;
            Save.AttrSet(type, data);
        }
        return data;
    }

    [ContextMenu("Clear")]
    public void Clear()
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

    private void OnDisable()
    {
        SaveSystem.Save(Save);
    }

    [ContextMenu("Save")]
    public void SaveData()
    {
        SaveSystem.Save(Save);
    }

    void AttrBindAll()
    {
        MonoBehaviour[] monos = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour mono in monos)
        {
            Type type = mono.GetType();
            FieldInfo[] objectFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            for (int i = 0; i < objectFields.Length; i++)
            {
                SavedData attr = Attribute.GetCustomAttribute(objectFields[i], typeof(SavedData)) as SavedData;
                if (attr != null)
                {
                    object field = objectFields[i].GetValue(mono);
                    if (attr.InitialData != null) field = attr.InitialData;
                    object data = AttrBind(objectFields[i].FieldType, field);
                    objectFields[i].SetValue(mono, data);
                    //Debug.LogWarning("Bind " + objectFields[i].FieldType);
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SavedData : Attribute
{
    public readonly object InitialData = null;
    public SavedData()
    {

    }

    public SavedData(object initial)
    {
        InitialData = initial;
    }
}

public interface IDataSavable
{
    void BindSavedData();
}