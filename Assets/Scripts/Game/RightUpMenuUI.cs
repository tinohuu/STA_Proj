using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//this file is to process the right-up menu
public class RightUpMenuUI : MonoBehaviour
{
    Transform trans;

    Text chapterInfo;
    Text levelInfo;
    Text handCardInfo;
    Text deskCardInfo;

    int nChapterIndex = 1;
    int nLevelIndex = 1;
    int nHandCardCount = 10;
    int nDeskCardCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        nHandCardCount = EditorScriptMgr.Instance.chapterInfo.levelDataList[0].handPokerCount;
        nDeskCardCount = EditorScriptMgr.Instance.chapterInfo.levelDataList[0].pokerInfo.Count;
        
        trans = GetComponent<Transform>();

        Transform chapterTrans = trans.Find("ChapterInfo");
        chapterInfo = chapterTrans.GetComponent<Text>();
        string strChapterInfo = string.Format("章节编号：{0}", nChapterIndex);
        chapterInfo.text = strChapterInfo;

        Transform levelTrans = trans.Find("LevelInfo");
        levelInfo = levelTrans.GetComponent<Text>();
        string strLevelInfo = string.Format("关卡编号：{0}", nLevelIndex);
        levelInfo.text = strLevelInfo;

        Transform handCardTrans = trans.Find("HandCardInfo");
        handCardInfo = handCardTrans.GetComponent<Text>();
        string strHandCardInfo = string.Format("当前手牌：{0}", nHandCardCount);
        handCardInfo.text = strHandCardInfo;

        Transform deskCardTrans = trans.Find("DeskCardInfo");
        deskCardInfo = deskCardTrans.GetComponent<Text>();
        string strDeskCardInfo = string.Format("牌堆区：{0}", nDeskCardCount);
        deskCardInfo.text = strDeskCardInfo;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateDisplayInfo(int nChapter, int nLevel, JsonReadWriteTest.LevelData data)
    {
        nChapterIndex = nChapter;
        string strChapterInfo = string.Format("章节编号：{0}", nChapterIndex);
        chapterInfo.text = strChapterInfo;

        nLevelIndex = nLevel;
        string strLevelInfo = string.Format("关卡编号：{0}", nLevelIndex);
        levelInfo.text = strLevelInfo;

        nHandCardCount = data.handPokerCount;
        string strHandCardInfo = string.Format("当前手牌：{0}", nHandCardCount);
        handCardInfo.text = strHandCardInfo;

        nDeskCardCount = data.pokerInfo.Count;
        string strDeskCardInfo = string.Format("牌堆区：{0}", nDeskCardCount);
        deskCardInfo.text = strDeskCardInfo;
    }

    public void UpdateHandCardCount(int nCount)
    {
        nHandCardCount = nCount;

        string strHandCardInfo = string.Format("当前手牌：{0}", nHandCardCount);
        handCardInfo.text = strHandCardInfo;
    }

    public void UpdateDeskCardCount(int nCount)
    {
        nDeskCardCount = nCount;

        string strDeskCardInfo = string.Format("牌堆区：{0}", nDeskCardCount);
        deskCardInfo.text = strDeskCardInfo;
    }
}
