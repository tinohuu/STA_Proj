using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityEngine
{
    public static class JsonExtensions
    {
        public static string GetJson(string resourcesFileName)
        {
            return Resources.Load<TextAsset>(resourcesFileName).text;
        }

        public static T[] JsonToArray<T>(string json)
        {
            json = json.Split("[".ToCharArray())[1];
            json = json.Split("]".ToCharArray())[0];
            string[] jsons = json.Split(new[] { "}," }, StringSplitOptions.None);
            T[] objs = new T[jsons.Length];
            for (int i = 0; i < jsons.Length; i++)
            {
                if (i < jsons.Length - 1)
                    jsons[i] += "}";
                objs[i] = JsonUtility.FromJson<T>(jsons[i]);
            }
            return objs;
        }

        public static List<T> JsonToList<T>(string json)
        {
            return JsonToArray<T>(json).ToList();
        }
    }
}
