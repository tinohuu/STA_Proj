using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class EditorScriptMgr : Singleton<EditorScriptMgr>
public class EditorScriptMgr : MonoBehaviour
{
    public static EditorScriptMgr Instance;
    public ChapterInfo chapterInfo{get; set;} = new ChapterInfo();
    public int nCurrentLevel { get; set; } = 1;

    // Start is called before the first frame update
    private void Awake() 
    {
        Instance = this;

        Screen.SetResolution(1920, 1080, false);

        Init();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init() 
    {
        //Debug.Log("here we init edit script mananger ... ... ... ");
        chapterInfo.Init(1);
        Debug.Log("EditorScriptMgr::init read chapter infomation successfully... ...");
    }

    //TODO: we update a level's information here.
    public void UpdateLevelInfos(int nChapter, int nLevel, List<GameObject> pokerInfos)
    {
        chapterInfo.UpdateLevelInfos(nLevel, pokerInfos);
    }

    public void UpdateOnePokerInfo(int nIndex, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        chapterInfo.UpdateOnePokerInfo(nCurrentLevel, nIndex, pokerInfo);
    }

    public void SaveCurrentLevel()
    {
        //Debug.Log("EditorScriptMgr::nCurrentLevel is: " + nCurrentLevel);
        chapterInfo.SaveLevelData(nCurrentLevel);
    }

    public int AddNewLevel()
    {
        return chapterInfo.AddOneLevel();
    }

}
