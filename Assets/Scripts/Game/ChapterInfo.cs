using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static JsonReadWriteTest;

public class ChapterInfo
{
    public Chapters chapterData = new JsonReadWriteTest.Chapters(); 
    public string strChapterName{get; set;}
    public List<JsonReadWriteTest.LevelData> levelDataList = new List<JsonReadWriteTest.LevelData>();
    int nChapterID { get; set; }

    // Start is called before the first frame update
    public void Init(int nChapterIndex)
    {
        //2021.7.20 added, here we read chapter data file first.
        string strChapter = "/Resources/Configs/LevelInfo/ChapterData.json";
        ReadChapterData(strChapter);

        //then we read first chapter's level information
        nChapterID = nChapterIndex;
        strChapterName = "Chapter 1";

        string strLevel = string.Format("/Resources/Configs/LevelInfo/Chapter_{0:D4}/", nChapterIndex);

        //todo: 这里以后要改成具体的章的索引。
        int nLevelCount = chapterData.chapterInfos[0].LevelCount;
        ReadLevelData(strLevel, nChapterIndex, nLevelCount);
    }

    //2021.7.20 added,
    void ReadChapterData(string strFileName)
    {
        string filePath = Application.dataPath + strFileName;
        string[] fileContent = File.ReadAllLines(filePath);

        chapterData = JsonUtility.FromJson<Chapters>(fileContent[0]);

        //Debug.Log("ChapterInfo::ReadChapterData... the chapter count is: " + chapterData.nChapterCount);
    }

    public void SaveChapterData()
    {
        string jsonString = JsonUtility.ToJson(chapterData);
        //Debug.Log(jsonString);

        //string strFileName = string.Format("ChapterData.json", 1);
        string path = Application.dataPath + "/Resources/Configs/LevelInfo/ChapterData.json";
        if (!File.Exists(path))
        {
            FileStream fStream = File.Create(path);
            fStream.Close();
        }

        File.WriteAllText(path, jsonString);
    }

    //this function is used to read a level file named "Level_XXXX_XXX.json"
    void ReadLevelData(string strLevelFolder, int nChapterIndex, int nCount)
    {
        for(int i=0; i<nCount; ++i)
        {
            string strLevelName = string.Format("Level_{0:D4}_{1:D3}.json", nChapterIndex, i+1);
            //Debug.Log("Read file name is: " + strLevelName);
            string filePath = Application.dataPath + strLevelFolder + strLevelName;

            if(File.Exists(filePath))
            {
                string[] fileContent = File.ReadAllLines(filePath);

                foreach (string strLevel in fileContent)
                {
                    //Debug.Log(strLevel);
                    JsonReadWriteTest.LevelData levelData = JsonUtility.FromJson<LevelData>(strLevel);
                    levelDataList.Add(levelData);

                    //Debug.Log("ReadLevelData... level lockArea Count is: " + levelData.LockAreaCount);

                    /*Debug.Log("level name is: " + levelData.levelName);
                    for(int j=0; j<levelData.pokerInfo.Count; ++j)
                    {
                        Debug.Log("fPosX is: " + levelData.pokerInfo[j].fPosX + ",fPosY is: " + levelData.pokerInfo[j].fPosY + ",fRotation is: " + levelData.pokerInfo[j].fRotation);
                    }*/
                }
            }
            else
            {
                JsonReadWriteTest.LevelData data = new JsonReadWriteTest.LevelData();
                data.handPokerCount = 0;
                data.levelName = strLevelName;
                data.pokerInfo = new List<PokerInfo>();
                data.LockAreaCount = 0;
                data.unlockAreaCount = 0;

                levelDataList.Add(data);
            }
            
        }
        
    }

    //2021.7.16 this method is for saving the level data to file.
    public void SaveLevelData(int nLevel)
    {
        string strFileName = string.Format("Level_0001_{0:D3}.json", nLevel);

        JsonReadWriteTest.LevelData data = levelDataList[nLevel - 1];
        data.levelName = strFileName;

        //Debug.Log("ChapterInfo::SaveLevelData the rotation is: " + data.pokerInfo[2].fRotation);

        string jsonString = JsonUtility.ToJson(data);
        //Debug.Log(jsonString);

        string path = Application.dataPath + "/Resources/Configs/LevelInfo/Chapter_0001/" + strFileName;
        if (!File.Exists(path))
        {
            FileStream fStream = File.Create(path);
            fStream.Close();
        }

        File.WriteAllText(path, jsonString);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLevelInfos(int nLevel, List<GameObject> pokerInfos)
    {
        EraseOldData(nLevel);

        List<JsonReadWriteTest.PokerInfo> pokers = new List<PokerInfo>();

        //Debug.Log("before udpate, the poker count is: "+ pokerInfos.Count);

        foreach(GameObject go in pokerInfos)
        {
            //Debug.Log("---ChapterInfo::UpdateLevelInfos the poker game object is: " + go);
            JsonReadWriteTest.PokerInfo info = new JsonReadWriteTest.PokerInfo();
            info.fPosX = go.transform.position.x - PokerAreaMgr.Instance.trans.position.x;
            info.fPosY = go.transform.position.y - PokerAreaMgr.Instance.trans.position.y;

            DragButton dragBtn = go.GetComponent<DragButton>();
            info.nGroupID = dragBtn.nGroupID;
            //Debug.Log("ChapterInfo::UpdateLevelInfos store the old level. the group id is: " + info.nGroupID);
            
            float fAngle;
            Vector3 vecDir;
            go.transform.rotation.ToAngleAxis(out fAngle, out vecDir);
            info.fRotation = fAngle * vecDir.z;

            info.nItemType = (int)dragBtn.pokerItemType;
            info.strItemInfo = dragBtn.strItemInfo;

            //Debug.Log("---ChapterInfo::UpdateLevelInfos the poker game object's rotation is: " + info.fRotation + " vecdir is: " + vecDir);

            pokers.Add(info);
        }

        levelDataList[nLevel - 1].pokerInfo = pokers;
    }

    public void UpdateOnePokerInfo(int nLevel, int nIndex, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        levelDataList[nLevel - 1].pokerInfo[nIndex].fPosX = pokerInfo.fPosX;
        levelDataList[nLevel - 1].pokerInfo[nIndex].fPosY = pokerInfo.fPosY;
        levelDataList[nLevel - 1].pokerInfo[nIndex].fRotation = pokerInfo.fRotation;
        levelDataList[nLevel - 1].pokerInfo[nIndex].nGroupID = pokerInfo.nGroupID;
        levelDataList[nLevel - 1].pokerInfo[nIndex].nItemType = (int)pokerInfo.nItemType;
        levelDataList[nLevel - 1].pokerInfo[nIndex].strItemInfo = pokerInfo.strItemInfo;

        Debug.Log("ChapterInfo::UpdateOnePokerInfo fRotation set to: " + pokerInfo.fRotation);
    }
    
    void EraseOldData(int nLevel)
    {
        if (levelDataList[nLevel - 1].pokerInfo.Count == 0)
            return;

        levelDataList[nLevel - 1].pokerInfo.Clear();
    }

    //2021.7.19, here we fill data for the newly added level;
    public int AddOneLevel()
    {
        JsonReadWriteTest.LevelData data = new JsonReadWriteTest.LevelData();
        data.handPokerCount = 0;
        int nNewLevel = levelDataList.Count + 1;
        string strLevelName = string.Format("Level_{0:D4}_{1:D3}.json", nChapterID, nNewLevel);
        data.levelName = strLevelName;
        data.pokerInfo = new List<JsonReadWriteTest.PokerInfo>();

        //Debug.Log("AddOneLevel strLevelName is: " + strLevelName);
        //Debug.Log("AddOneLevel poker count is: " + data.pokerInfo.Count);

        levelDataList.Add(data);

        //todo: 这里的代码后面还需要根据具体的章的信息来修改
        chapterData.chapterInfos[0].LevelCount++;
        SaveChapterData();

        return nNewLevel;
    }
}
