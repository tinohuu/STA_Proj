using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        AttrBindAll();
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

    public static object AttrBind(Type type, object initial = default)
    {
        object data = Save.AttrGet(type, initial);
        if (data == null)
        {
            data = initial;
            Save.AttrSet(type, data);
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