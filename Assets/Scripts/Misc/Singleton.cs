using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance {
        get
        {
            if(instance == null)
            {
                GameObject obj= new GameObject();
                instance = obj.AddComponent<T>();
                obj.name = typeof(T).Name;

                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    

}
