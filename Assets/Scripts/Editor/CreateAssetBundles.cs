using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles/Windows")]
    static void BuildAllAssetBundlesWindows()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Build AssetBundles/Android")]
    static void BuildAllAssetBundlesAndroid()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.Android);
    }

    [MenuItem("Assets/Build AssetBundles/iOS")]
    static void BuildAllAssetBundlesiOS()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.iOS);
    }

    [MenuItem("Tools/Set Level Name/Chapter_0001")]
    static void SetLevelAssetBundleName_0001()
    {
        string assetBundleDirectory = "Assets/Resources/Configs/LevelInfo/Chapter_0001";
        DirectoryInfo directoryInfo = new DirectoryInfo(assetBundleDirectory);
        if(directoryInfo == null)
        {
            Debug.LogError("************* Tools/Set Level Name/Chapter_0001 not exist!!! ****************");
        }

        FileSystemInfo[] fileInfos = directoryInfo.GetFileSystemInfos();
        foreach(FileSystemInfo fileInfo in fileInfos)
        {
            if(fileInfo is FileInfo  && fileInfo.Extension != ".meta")
            {
                int nIndex = fileInfo.FullName.LastIndexOf('\\');
                string fileName = fileInfo.FullName.Substring(nIndex);

                AssetImporter assetImporter = AssetImporter.GetAtPath(assetBundleDirectory + fileName);
                if(assetImporter !=  null)
                {
                    //Debug.Log(assetBundleDirectory + fileName);
                    assetImporter.assetBundleName = "chapter_0001";
                    assetImporter.assetBundleVariant = "level";
                }
            }
        }
    }
}