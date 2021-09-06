using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/****************************************************************************************
 * TODO: 
 * 1. 关卡文件的命名规则需要整理，应该是Chapter_XXXX_YYY.json的格式，XXXX是章节数，YYY是关卡序号；
 * 2. 读写接口也需要添加带参数的接口；
 ****************************************************************************************/
public class JsonReadWriteTest : MonoBehaviour
{
    public Chapters chapterData;
    public LevelData levelData;

    // Start is called before the first frame update
    void Start()
    {
        //InitLevelData(1);
        //Test_CreateChapterData();
        InitChapterData();
        Debug.Log("here we init the chapter and level json config file...");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitChapterData()
    {
        //string strChapterConfigName = "Resources/Configs/LevelInfo/ChapterData.json";
        string strChapterConfigName = Resources.Load<TextAsset>("Configs/LevelInfo/ChapterData.json").text;
        Debug.Log(strChapterConfigName);

#if UNITY_STANDALONE_WIN
        //string filePath = Application.dataPath + strChapterConfigName;

        //string[] fileContent = File.ReadAllLines(filePath);

        chapterData = JsonUtility.FromJson<Chapters>(strChapterConfigName);
#endif

#if UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, "chapter_data.chap");
        
        //Debug.Log("InitChapterData... the filePath is: " + filePath + " the leve name is: " + strLevelName);
        //todo: the following should be changed to read file ChapterData.json
        AssetBundle levelBundle = AssetBundle.LoadFromFile(filePath);
        UnityEngine.Object chapterFile = levelBundle.LoadAsset("ChapterData.json");
        string strChapterFile = chapterFile.ToString();

        chapterData = JsonUtility.FromJson<Chapters>(strChapterFile);
#endif

        /*Debug.Log("chapter count is: " + chapterData.nChapterCount);
        Debug.Log("chapter 1 is: " + chapterData.chapterInfos[0].strChapterName);
        Debug.Log("chapter 2 is: " + chapterData.chapterInfos[1].strChapterName);*/
    }

    public void InitLevelData(int nChapter)
    {
        //this statement is only used to generate a levelinfo file for test.
        //Test_CreateLevelData();

        //string strLevel = "/LevelInfo/Chapter_001/";
        string strLevel = string.Format("Configs/LevelInfo/Chapter_{0:D4}/", nChapter);
        //string strChapterConfigName = Resources.Load<TextAsset>("Configs/LevelInfo/ChapterData.json").text;
        Debug.Log(strLevel);

        //这里应该读取一个章节配置文件，然后得出章节中关卡的数目
        int nLevelCount = 3;

        ReadLevelData(strLevel, nChapter, nLevelCount);
    }

    void Test_CreateChapterData()
    {
        ChapterData chapterData = new ChapterData();
        chapterData.ChapterID = 1;
        chapterData.LevelCount = 3;
        chapterData.strChapterName = "Chapter_0001";

        /*ChapterData chapterData1 = new ChapterData();
        chapterData1.ChapterID = 2;
        chapterData1.LevelCount = 1;
        chapterData1.strChapterName = "Chapter_0002";*/

        Chapters chapters = new Chapters();
        chapters.nChapterCount = 1;
        chapters.chapterInfos = new List<ChapterData>();
        chapters.chapterInfos.Add(chapterData);
        //chapters.chapterInfos.Add(chapterData1);

        string jsonString = JsonUtility.ToJson(chapters);
        Debug.Log(jsonString);

#if UNITY_STANDALONE_WIN
        string path = Application.dataPath + "/Resources/Configs/LevelInfo/ChapterData.json";
        if (!File.Exists(path))
        {
            FileStream fStream = File.Create(path);
            fStream.Close();
        }

        File.WriteAllText(path, jsonString);
#endif
        //string strFileName = string.Format("ChapterData.json", 1);

#if UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, "chapter_0001.level");
        
        //Debug.Log("Test_ReadLevelData... the filePath is: " + filePath + " the leve name is: " + strLevelName);

        //AssetBundle levelBundle = AssetBundle.LoadFromFile(filePath);
        //UnityEngine.Object levelFile = levelBundle.LoadAsset(strLevelName);
        //string strLevelFile = levelFile.ToString();

        //levelData = JsonUtility.FromJson<LevelData>(strLevelFile);
#endif

    }

    //测试函数
    void Test_CreateLevelData()
    {
        LevelData levelData = new LevelData();
        levelData.levelName = "Level_1";
        levelData.handPokerCount = 10;
        levelData.pokerInfo.Add(new PokerInfo() { fPosX = 100.0f, fPosY = 200.0f, fRotation = 0.0f });
        levelData.pokerInfo.Add(new PokerInfo() { fPosX = 300.0f, fPosY = 200.0f, fRotation = 0.0f });

        string jsonString = JsonUtility.ToJson(levelData);

        Debug.Log(jsonString);
        string strFileName = string.Format("Level_0001_{0:D3}.json", 1);

#if UNITY_STANDALONE_WIN
        string path = Application.dataPath + "/Resources/Configs/LevelInfo/Chapter_0001/" + strFileName;
        if (!File.Exists(path))
        {
            FileStream fStream = File.Create(path);
            fStream.Close();
        }

        File.WriteAllText(path, jsonString);
#endif

#if UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, "chapter_0001.level");

        //AssetBundle levelBundle = AssetBundle.LoadFromFile(filePath);
        //UnityEngine.Object levelFile = levelBundle.LoadAsset(strLevelName);
        //string strLevelFile = levelFile.ToString();

        //levelData = JsonUtility.FromJson<LevelData>(strLevelFile);
#endif

        /*FileInfo fileInfo = new FileInfo(path);
        StreamWriter sw = fileInfo.CreateText();
        sw.WriteLine(jsonString);
        sw.Close();
        sw.Dispose();*/
    }

    //this function is used to read a level file named "Level_XXXX_XXX.json"
    void ReadLevelData(string strLevelFolder, int nChapterIndex, int nCount)
    {
        for (int i = 0; i < nCount; ++i)
        {
            string strLevelName = string.Format("Level_{0:D4}_{1:D3}.json", nChapterIndex, i + 1);
            //Debug.Log("Read file name is: " + strLevelName);

#if UNITY_STANDALONE_WIN
        string filePath = strLevelFolder + strLevelName;
        //string[] fileContent = File.ReadAllLines(filePath);
        string strLevel = Resources.Load<TextAsset>(filePath).text;

        levelData = JsonUtility.FromJson<LevelData>(strLevel);

         /*   foreach (string strLevel in fileContent)
        {
            Debug.Log(strLevel);
            

            //the folling code is for debug, so we can see whether our program read the data right.
            Debug.Log("level name is: " + levelData.levelName);
            for (int j=0; j<levelData.pokerInfo.Count; ++j)
            {
                Debug.Log("fPosX is: " + levelData.pokerInfo[j].fPosX + ",fPosY is: " + levelData.pokerInfo[j].fPosY + ",fRotation is: " + levelData.pokerInfo[j].fRotation);
            }

        }*/
#endif

#if UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, "chapter_0001.level");
        //string filePath = "jar:file://" + Application.dataPath + "!/assets/chapter_0001.level";
        //Debug.Log("Test_ReadLevelData... the filePath is: " + filePath + " the leve name is: " + strLevelName);

        AssetBundle levelBundle = AssetBundle.LoadFromFile(filePath);
        UnityEngine.Object levelFile = levelBundle.LoadAsset(strLevelName);
        string strLevelFile = levelFile.ToString();

        levelData = JsonUtility.FromJson<LevelData>(strLevelFile);
#endif

        }

    }

    public static void Test_ReadLevelData(string strLevelFolder, int nChapterIndex, int nLevelIndex, out JsonReadWriteTest.LevelData levelData)
    {
        //levelData.handPokerCount = 0;
        levelData = null;
        string strLevelName = string.Format("Level_{0:D4}_{1:D3}", nChapterIndex, nLevelIndex);

#if UNITY_STANDALONE_WIN
        //string filePath = Application.dataPath + strLevelFolder + strLevelName;
        //strLevelName = string.Format("Level_{0:D4}_{1:D3}.json", nChapterIndex, nLevelIndex);//todo: 2021.9.3 this is temporarily modified...
        string filePath = strLevelFolder + strLevelName;
        Debug.Log(filePath);

        string strLevel = Resources.Load<TextAsset>(filePath).text;
        //string[] fileContent = File.ReadAllLines(filePath);
        levelData = JsonUtility.FromJson<LevelData>(strLevel);

        /*foreach (string strLevel in fileContent)
        {
            levelData = JsonUtility.FromJson<LevelData>(strLevel);

            return;
        }*/
#endif

#if UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, "chapter_0001.level");
        //string filePath = "jar:file://" + Application.dataPath + "!/assets/chapter_0001.level";
        //Debug.Log("Test_ReadLevelData... the filePath is: " + filePath + " the leve name is: " + strLevelName);

        AssetBundle levelBundle = AssetBundle.LoadFromFile(filePath);
        UnityEngine.Object levelFile = levelBundle.LoadAsset(strLevelName);
        string strLevelFile = levelFile.ToString();

        levelData = JsonUtility.FromJson<LevelData>(strLevelFile);
        
#endif

    }

    //this class stores all chapter infos, we can get level count of each chapter, 
    //and when we add/delete a level, we should update this class and store the information.
    [Serializable]
    public class Chapters
    {
        public int nChapterCount;
        public List<ChapterData> chapterInfos;
    }

    [Serializable]
    public class ChapterData
    {
        public int ChapterID;
        public int LevelCount;
        public string strChapterName;
    }

    [Serializable]
    public class LevelData
    {
        public string levelName;
        public int handPokerCount;

        public List<PokerInfo> pokerInfo = new List<PokerInfo>();

        public int LockAreaCount = 0;
        public List<LockArea> lockAreas = new List<LockArea>();

        public int unlockAreaCount = 0;
        public List<UnlockArea> unlockAreas = new List<UnlockArea>();

        public LockGroups lockGroup = new LockGroups();
    }

    [Serializable]
    public class PokerInfo
    {
        public float fPosX;
        public float fPosY;

        public float fRotation;

        public int nGroupID;

        public int nItemType;
        public string strItemInfo = "";
    }

    [Serializable]
    public class LockArea
    {
        public int nAreaID;

        public float fPosX;
        public float fPosY;
        public float fWidth;
        public float fHeight;
    }

    [Serializable]
    public class UnlockArea
    {
        public int nAreaID;

        public float fPosX;
        public float fPosY;
        public float fWidth;
        public float fHeight;

        public string strUnlockPokers;
    }

    [Serializable]
    public class LockGroups
    {
        public int nLockGroupCount = 0;

        public List<LockGroup> lockGroups = new List<LockGroup>();
    }

    [Serializable]
    public class LockGroup
    {
        public int nLockCount = 0;
        public int nGroupID;

        public List<LockInfo> lockInfos = new List<LockInfo>();
    }

    [Serializable]
    public class LockInfo
    {
        public int nGroupID;

        public float fPosX = 0.0f;
        public float fPosY = 0.0f;

        public int nSuit = 0;
        public int nColor = 0;
        public int nNumber = 0;
    }
}
