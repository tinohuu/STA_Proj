using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public static class SaveSystem
{
    public  const string Path = "/STA_Games";
    public static string FileName = "/Save.carta";
    public static string Root = Application.persistentDataPath;

    public static bool Save(string root, string path, string fileName, Save save = null)
    {
        if (save == null || save.Datas.Count == 0)
        {
            return false;
            save = new Save();
            // Config your data here:
        }

        // Convert class to bytes
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, save);
        byte[] byteSave = ms.GetBuffer();
        ms.Close();

        // Encrypt
        byte[] encryptedByte = Encrypt.AESHelper.Encrypt(byteSave);

        // Create directory
        if (!Directory.Exists(root + path))
            Directory.CreateDirectory(root + path);

        // Save to file
        FileStream fileStream = File.Create(root + path + fileName);
        bf.Serialize(fileStream, encryptedByte);
        fileStream.Close();

        Debug.LogWarning("Saved to " + root + path + fileName + " with " + save.Datas.Count + " datas.");

        return File.Exists(root + path + fileName);
    }
    public static bool Save(Save save = null)
    {
        return Save(Root, Path, FileName, save);
    }
    public static Save Load(string root, string path, string fileName)
    {
        if (File.Exists(root + path + fileName))
        {
            // Load
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fileStream = File.Open(root + path + fileName, FileMode.Open);
            byte[] encryptedByte = (byte[])bf.Deserialize(fileStream);
            fileStream.Close();

            // Dncrypt
            byte[] byteSave = Encrypt.AESHelper.Decrypt(encryptedByte);

            // Convert bytes to class
            MemoryStream ms = new MemoryStream(byteSave);
            Save save = (Save)bf.Deserialize(ms);
            Debug.LogWarning("Loaded from " + root + path + fileName + " with " + save.Datas.Count + " datas.");
            ms.Close();

            return save;
        }
        Debug.LogWarning("Can't find any save and then create a empty one.");
        return new Save();
    }
    public static Save Load()
    {
        return Load(Root, Path, FileName);
    }
    public static void Clear(string root, string path, string fileName)
    {
        if (File.Exists(root + path + fileName))
            File.Delete(root + path + fileName);
    }
    public static void Clear()
    {
        Clear(Root, Path, FileName);
    }
}

namespace Encrypt
{
    public class AESHelper
    {
        /// <summary>
        /// 默认密钥-密钥的长度必须是32
        /// </summary>
        private const string PublicKey = "1234567890123456";

        /// <summary>
        /// 默认向量
        /// </summary>
        private const string Iv = "abcdefghijklmnop";
        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="str">需要加密字符串</param>  
        /// <returns>加密后字符串</returns>  
        public static Byte[] Encrypt(Byte[] data)
        {
            return Encrypt(data, PublicKey);
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="str">需要解密字符串</param>  
        /// <returns>解密后字符串</returns>  
        public static Byte[] Decrypt(Byte[] data)
        {
            return Decrypt(data, PublicKey);
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="str">需要加密的字符串</param>
        /// <param name="key">32位密钥</param>
        /// <returns>加密后的字符串</returns>
        public static Byte[] Encrypt(Byte[] data, string key)
        {
            Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
            Byte[] toEncryptArray = data;
            var rijndael = new System.Security.Cryptography.RijndaelManaged();
            rijndael.Key = keyArray;
            rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
            rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
            System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return resultArray;
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="str">需要解密的字符串</param>
        /// <param name="key">32位密钥</param>
        /// <returns>解密后的字符串</returns>
        public static Byte[] Decrypt(Byte[] data, string key)
        {
            Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
            Byte[] toEncryptArray = data;// Convert.FromBase64String(str);
            var rijndael = new System.Security.Cryptography.RijndaelManaged();
            rijndael.Key = keyArray;
            rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
            rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
            System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return resultArray;
        }
    }
}

[System.Serializable]
public class Save
{
    public List<object> Datas = new List<object>();

    public void Set<T>(T setObj)
    {
        for (int i = 0; i < Datas.Count; i++)
        {
            if (Datas[i] is T obj)
            {
                Datas[i] = setObj;
                return;
            }
        }
        Datas.Add(setObj);
    }

    public void AttrSet(Type type, object setObj)
    {
        for (int i = 0; i < Datas.Count; i++)
        {
            if (Datas[i].GetType() == type)
            {
                Datas[i] = setObj;
                return;
            }
        }
        Datas.Add(Convert.ChangeType(setObj, type));
    }


    public T Get<T>(T initial = default)
    {
        for (int i = 0; i < Datas.Count; i++)
            if (Datas[i] is T obj) return obj;
        return initial;
    }

    public object AttrGet(Type type, object initial = default)
    {
        for (int i = 0; i < Datas.Count; i++)
        {
            if (Datas[i].GetType() == type) return Datas[i];
        }
        return null;

    }


    public bool Has<T>()
    {
        for (int i = 0; i < Datas.Count; i++)
            if (Datas[i] is T obj) return true;
        return false;
    }
}
