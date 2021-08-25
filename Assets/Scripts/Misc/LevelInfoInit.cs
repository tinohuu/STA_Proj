using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoInit : MonoBehaviour
{
    public GameObject levelInfoPrefab;

    public BottomMenuUI bottomMenu;

    //right-up menu can display chapter, level, hand card count and desk card count.
    public RightUpMenuUI rightUpMenu;

    Transform trans;
    int nCurrentChapter = 1;
    int nCurrentLevel = 1;
    string strCurrentLevel;

    List<GameObject> levelInfos = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("LevelInfoInit Start ... ... ...");
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        Transform transContent = transform.Find("Viewport/Content");
        trans = transContent.GetComponent<Transform>();

        bottomMenu = Object.FindObjectOfType<BottomMenuUI>();
        if (bottomMenu == null)
            Debug.Log("can not init bottom menu. check your code.");

        rightUpMenu = Object.FindObjectOfType<RightUpMenuUI>();
        if (rightUpMenu == null)
            Debug.Log("can not init right-up menu. check your code.");

        InitChapterInfo(nCurrentChapter);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitChapterInfo(int nChapterIndex)
    {
        int i = 0;
        foreach(JsonReadWriteTest.LevelData data in EditorScriptMgr.Instance.chapterInfo.levelDataList)
        {
            i ++;
            GameObject levelInfo = (GameObject)Instantiate(levelInfoPrefab, trans.position, trans.rotation);
            Text textInfo = levelInfo.GetComponentInChildren<Text>();
            string strDisplayName = string.Format("Level_{0:D4}_{1:D3}", nChapterIndex, i);
            textInfo.text = strDisplayName;
            levelInfo.transform.SetParent(trans);
            levelInfos.Add(levelInfo);

            Button button = levelInfo.GetComponent<Button>();
            button.name = strDisplayName;
            button.onClick.AddListener(delegate() {this.OnClickLevel(levelInfo);});

            //set the first button selected
            if(i == 1)
            {
                Debug.Log("LevelInfoInit::InitChapterInfo... set first button selected!");
                button.Select();
            }

            //Debug.Log("LevelInfoInit::InitChapterInfo... the position of button is: " + levelInfo.transform.position);
        }

        //select first level for default;
        strCurrentLevel = levelInfos[0].name;
        nCurrentChapter = nChapterIndex;
        nCurrentLevel = 1;
        EditorScriptMgr.Instance.nCurrentLevel = nCurrentLevel;
    }

    private void OnClickLevel(GameObject sender)
    {
        if(sender.name != strCurrentLevel)
        {
            //Debug.Log("_______________________you clicked anothoer button: " + sender.name);
            strCurrentLevel = sender.name;

            string[] strArray = strCurrentLevel.Split('_');
            string strLevel = strArray[2];
            int nSelectLevel = int.Parse(strLevel);

            if(nCurrentLevel != nSelectLevel)
            {
                //here we store the information of the level before we switch to a new level
                PokerAreaMgr.Instance.UpdatePokerInfos(nCurrentChapter, nCurrentLevel);

                nCurrentLevel = nSelectLevel;
                EditorScriptMgr.Instance.nCurrentLevel = nCurrentLevel;

                bottomMenu.SetHandCardDisplay(nCurrentLevel);
                rightUpMenu.UpdateDisplayInfo(nCurrentChapter, nCurrentLevel, EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel-1]);
            }

            PokerAreaMgr.Instance.SetPokerInfo(nSelectLevel);
            PokerAreaMgr.Instance.SetHandCardInfo(nSelectLevel);
            PokerAreaMgr.Instance.InitLockInfo();
            PokerAreaMgr.Instance.InitLockAreaInfo();
            PokerAreaMgr.Instance.InitUnlockAreaInfo();
            PokerAreaMgr.Instance.ClearAllSelectPoker();
        }
    }

    //2021.7.19 when add a new level, call this method to switch to new level, 
    //should be called after new level data has been added to the chapter info.
    public void SwitchToNewLevel(int nNewLevel)
    {
        //first we add a level button, and set its name and information.
        GameObject levelInfo = (GameObject)Instantiate(levelInfoPrefab, trans.position, trans.rotation);
        Text textInfo = levelInfo.GetComponentInChildren<Text>();
        string strDisplayName = string.Format("Level_{0:D4}_{1:D3}", nCurrentChapter, nNewLevel);
        textInfo.text = strDisplayName;
        levelInfo.transform.SetParent(trans);
        levelInfos.Add(levelInfo);

        Button button = levelInfo.GetComponent<Button>();
        button.name = strDisplayName;
        button.onClick.AddListener(delegate () { this.OnClickLevel(levelInfo); });

        nCurrentLevel = nNewLevel;
        if (nCurrentLevel > 1)
        {
            PokerAreaMgr.Instance.UpdatePokerInfos(nCurrentChapter, nCurrentLevel - 1);//这里不应该调用update poker info来初始化新关卡
        }
        EditorScriptMgr.Instance.nCurrentLevel = nCurrentLevel;

        bottomMenu.SetHandCardDisplay(nCurrentLevel);
        rightUpMenu.UpdateDisplayInfo(nCurrentChapter, nCurrentLevel, EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1]);

        PokerAreaMgr.Instance.SetPokerInfo(nNewLevel);
        PokerAreaMgr.Instance.SetHandCardInfo(nNewLevel);
        PokerAreaMgr.Instance.InitLockInfo();
        PokerAreaMgr.Instance.InitLockAreaInfo();
        PokerAreaMgr.Instance.InitUnlockAreaInfo();
        PokerAreaMgr.Instance.ClearAllSelectPoker();
    }

    //this function is for test only
    public void Test_InitChapterInfo(int nChapterIndex)
    {
        GameObject levelInfo = (GameObject)Instantiate(levelInfoPrefab, trans.position, trans.rotation);
        Text textInfo = levelInfo.GetComponentInChildren<Text>();
        textInfo.text = "Level_01";
        levelInfo.transform.SetParent(trans);
        
        GameObject levelInfo1 = (GameObject)Instantiate(levelInfoPrefab, trans.position, trans.rotation);
        Text textInfo1 = levelInfo1.GetComponentInChildren<Text>();
        textInfo1.text = "Level_02";
        levelInfo1.transform.SetParent(trans);
    }
}
