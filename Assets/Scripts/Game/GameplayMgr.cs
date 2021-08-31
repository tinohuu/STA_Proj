using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.IO;

public class GameplayMgr : MonoBehaviour
{
    public enum GameStatus : int
    {
        GameStatus_Before = 1,
        GameStatus_Gaming,
        GameStatus_Settle,
        GameStatus_After_Settle
    }
    public enum GameResult : int
    {
        GameResult_Win = 1,
        GameResult_Loss
    }

    public enum PokerType : int
    {
        PublicPoker,
        HandPoker,
        WildPoker
    }

    public enum PokerColor : int
    {
        None,
        Black,
        Red
    }

    public enum StreakType : int
    {
        Add_Poker = 1,
        Add_Wildcard = 2,
        Add_Gold = 3
    }

    public static int Max_StreakBonus_Type = 3;
    public static int Max_StreakBonus_Count = 6;

    public enum PokerFacing : int
    {
        Facing,
        Backing
    }

    public enum PokerSuit : int
    {
        Suit_None,
        Suit_Spade,     //
        Suit_Heart,
        Suit_Club,
        Suit_Diamond
    }

    public enum HandPokerSource : int
    {
        Hand,
        StreakBonus
    }

    public enum WildCardSource : int
    {
        Gold,
        Item,
        StreakBonus
    }

    public enum LockState : int
    {
        LockState_None,      //no state
        LockState_Sleeping,  //the lock is coved with clouds
        LockState_Working,   //if the lock is open, set it's state to working state,
        LockState_Dying      //if the lock is cleared, then set it to dying state, don't destroy.
    }

    static int nIndexDelta = 0;

    public static GameplayMgr Instance;

    public Transform Trans;
    public MeshRenderer renderer;

    public Vector3 rendererSize {
        get
        {
            return renderer.bounds.size;
        }
    }

    public class OpStepInfo
    {
        public int nIndex;
        public int nStreakCount;
        public int nStreakBonus;    //010101, 6 bits store 6 color type
        public int nScore;
        public int nGold;
        public int nHasStreak;      //whether this step has a streak completed
        public int nStreakType;     //if so, store the streak(completed) type
        public int nComboCount;     //like the nStreakCount, but don't reset to 0 if a StreakBonus is got.
        public string strCardName;  //pengyuan 2021.8.4 added for withdraw bonus, such as added card and added wildcard
        public string strIDs;       //strIDs is for double streak bonus.
        public string strLockInfo;  //this string records lock's clearing: groupID_lockIndex    
        public int nLockAreaCleared;//this records lock area's clearing: groupID
    }

    //store a lock group's information, including group id and lock object information
    public class LockGroup
    {
        public int nGroupID;
        public List<GameObject> locks = new List<GameObject>();
    }

    public class LockAreaInfo
    {
        public int nGroupID;
        public GameObject Area;
    }

    public class UnlockAreaPokerIDs
    {
        public int nGroupID;
        public List<int> IDs = new List<int>();           //this id is the id for the public poker, used in editor and InitPoker()
        public List<int> pokerNumbers = new List<int>();  //this stores all generated poker's number, 1-52, that can unlock a lock
    }

    public int nGold = 0;
    public int nScore = 0;

    public GameObject pokerPrefab;

    public GameObject wildcardPrefab;    //2021.8.2 added by pengyuan for wild card
    //public GameSceneBG gameSceneBG;

    public GameObject scoreStarPrefab;

    public GameObject lockPrefab;    //2021.8.17 added by pengyuan for lock in the game

    public GameObject lockAreaPrefab; //2021.8.18 added by pengyuan for lock area in the game

    public Sprite lockTopSprite;
    public Sprite lockBottomSprite;
    //public SpriteAtlas lockSprites;
    public Sprite[] lockSprites;

    //use this to store all the poker.
    JsonReadWriteTest.LevelData levelData = new JsonReadWriteTest.LevelData();
    
    List<GameObject> publicPokers = new List<GameObject>();

    List<GameObject> handPokers = new List<GameObject>();

    Stack<GameObject> foldPoker = new Stack<GameObject>();     //the pokers being folded 


    //the folling is for storing information about locks
    List<LockGroup> lockGroups = new List<LockGroup>();    //2021.8.17 added by pengyuan, to store all the lock information for the locks in one game.

    Dictionary<string, GameObject> lockGroupDict1 = new Dictionary<string, GameObject>();   //one condition to clear this lock
    Dictionary<string, GameObject> lockGroupDict2 = new Dictionary<string, GameObject>();   //two conditions to clear this lock

    List<LockAreaInfo> lockAreas = new List<LockAreaInfo>();       //2021.8.18 added by pengyuan, 

    List<UnlockAreaPokerIDs> unlockAreaPokerIDs = new List<UnlockAreaPokerIDs>();


    GameObject currentFlippingPoker = null;    //store the current flipping card

    float fGameTime;

    bool bAutoFlipHandPoker = false;

    GameObject testModel;
    Vector3 testDir;
    float testT;

    List<GameObject> testCards = new List<GameObject>();

    public int cardIndex = 0;
    public int[] cardsArray = new int[52];//store the poker number... from 1 to 52.
    public List<int> cardsArray2nd = new List<int>();
    //public List<int> 

    public int streakCardIndex = 0;
    public int[] streakCardsArray = new int[52];

    //2021.8.3 added by pengyuan for streak, streak bonus and gold
    Stack<OpStepInfo> opStepInfos = new Stack<OpStepInfo>();     //the pokers being folded 

    public int nStreakCount = 0;    //streak count for collecting poker.    
    public int nStreakBonusCount = 0;    //this is for streak bonus only
    public int[] nStreakBonusColor = new int[6];    //this is for streak bonus only
    public int nCollectGold = 0;
    public int nCollectScore = 0;
    public int nClearGold = 0;

    public int nTotalComboCount = 0;

    int nCurrentStreakIndex = 0;
    StreakType streakType;
    int[] nStreakIDs;    //test, we temporarily store the config information here.
    int[] nStreakBonusAmounts;  //store the Bonus amounts for each streak
    int[] nStreakBonusScore;  //store the Streak Bonus score for each type of streak

    GameResult gameResult = GameResult.GameResult_Win;

    GameStatus gameStatus = GameStatus.GameStatus_Before;

    int nCurrentChapter { get; set; } = 0;
    int nCurrentLevel { get; set; } = 0;

    //pengyuan 2021.8.13 temporarily store poker's texture and back
    public Texture2D textureBack;
    public  Texture2D[] pokerTexture = new Texture2D[52];
    public Texture2D pokerAtlas;// = new Texture2D(8192, 256);
    public Rect[] pokerRects = new Rect[52];

    //the following is about some ui
    public GamePlayUI gameplayUI;

    public class StageConfig
    {
        public int ID = 0;
        public int ChapterNum = 0;
        public int Cost = 0;
        public int DoubleCost = 0;
        public int QuadrupleCost = 0;
        public int Deck = 0;
        public int HandCards = 0;
        
        public int WildCost = 0;
        public int DoubleWildCost = 0;
        public int QuadrupleWildCost = 0;
        public string CancelCost = "";
        public string DoubleCancelCost = "";
        public string QuadrupleCancelCost = "";
        public string BuyCard = "";
        public string DoubleBuyCard = "";
        public string QuadrupleBuyCard = "";
        public string CardScore = "";
        public int EndCardScore = 0;
        public string Star = "";
        public int CardCoin = 0;
        public int DoubleCardCoin = 0;
        public int QuadrupleCardCoin = 0;
        public int RestCardCoin = 0;
        public int DoubleRestCardCoin = 0;
        public int QuadrupleRestCardCoin = 0;
        public string StreakBonus = "";    //2021.8.9 added by pengyuan for streak bonus amounts
        public int DoubleStreakBonus = 0;
        public int QuadrupleStreakBonus = 0;
        public string StreakBonusScore = "";
        public string StreakType = "";
        //public string StreakType = "";    //2021.8.9 added by pengyuan for streak bonus amounts
        public int StageIcon = 0;
    }

    public class PublicConfig
    {
        public string CardCoinFunctionParm = "";
        public string StreakBonus = "";
        public string StageClearBonus = "";
    }

    List<StageConfig> stageConfigs;// = JsonExtensions.JsonToList<CropConfig>(ConfigsAsset.GetConfig("StageConfig"));
    List<PublicConfig> publicConfigs;

    private void Awake()
    {
        if (!Instance) Instance = this;

        Debug.Log("GameplayMgr::Awake()... ");

        pokerPrefab = (GameObject)Resources.Load("Poker/Poker_Club_10"); 
        //pokerPrefab = (GameObject)Resources.Load("Red_PlayingCards_Heart01_00");
        if (pokerPrefab == null)
            Debug.Log("GameplayMgr::Start()... PokerTest is null....");

        wildcardPrefab = (GameObject)Resources.Load("Poker/WildCard");
        // wildcardPrefab = (GameObject)Resources.Load("Poker_Club_10");
        if (wildcardPrefab == null)
            Debug.Log("GameplayMgr::Start()... WildCard is null....");

        scoreStarPrefab = (GameObject)Resources.Load("UI/ScoreStar");
        if (scoreStarPrefab == null)
            Debug.Log("GameplayMgr::Start()... ScoreStar is null....");

        lockPrefab = (GameObject)Resources.Load("Lock/Lock");
        if(lockPrefab == null)
            Debug.Log("GameplayMgr::Start()... Lock is null....");

        lockAreaPrefab = (GameObject)Resources.Load("Lock/LockArea");
        if(lockAreaPrefab == null)
            Debug.Log("GameplayMgr::Start()... LockArea is null....");

        lockTopSprite = Resources.Load<Sprite>("Lock/LockTop");
        lockBottomSprite = Resources.Load<Sprite>("Lock/LockBottom");

        //lockSprites = Resources.Load<SpriteAtlas>("Lock/LockSprites");
        lockSprites = Resources.LoadAll<Sprite>("Lock/LockMark");
        Debug.Log("the lock sprites length is: " + lockSprites.Length);

        for (int i = 0; i < 52; ++i)
        {
            cardsArray[i] = i + 1;
            cardsArray2nd.Add(i);
        }

        for (int i = 0; i < 52; ++i)
        {
            streakCardsArray[i] = i;
        }

        textureBack = Resources.Load("Poker/back1") as Texture2D;
        for(int i = 0; i < 52; ++i)
        {
            string strPokerName = string.Format("Poker/Poker_{0:D3}", i+1);
            pokerTexture[i] = Resources.Load(strPokerName) as Texture2D;
            if (pokerTexture[i] == null)
                Debug.Log("@@@@@@@@@@@@@@@ init poker texture error!!! please check your code! @@@@@@@@@@@@@@" + strPokerName);
            
        }

        //pokerAtlas = new Texture2D(8192, 256, TextureFormat.DXT5, false);
        pokerAtlas = new Texture2D(8192, 256);
        
        pokerRects = pokerAtlas.PackTextures(pokerTexture, 0, 8192, false);

        /*byte[] btes = DeCompress(pokerAtlas).EncodeToPNG();
        FileStream file = File.Open("Pokers.png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(btes);
        file.Close();*/
        //Texture2D.DestroyImmediate(png);

    }

    public Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameplayMgr::Start()... ");

        stageConfigs = JsonExtensions.JsonToList<StageConfig>(ConfigsAsset.GetConfig("StageConfig"));

        publicConfigs = JsonExtensions.JsonToList<PublicConfig>(ConfigsAsset.GetConfig("PublicConfig"));

        InitConfigInformation();

        Trans = GetComponent<Transform>();
        if (Trans == null)
            Debug.Log("GameplayMgr::Start()... rectTrans is null....");

        renderer = GetComponent<MeshRenderer>();
        Debug.Log("GameplayMgr::Start()... bounds is: " + renderer.bounds.size.y);

        Test_Shuffle();

        Test_Shuffle_StreakPoker();

        InitLockAreaInfo();//lock area should before the lock init

        InitLockInfo();

        InitPoker();

        InitHandPokerInfo();

        fGameTime = 0.0f;

        gameplayUI = Object.FindObjectOfType<GamePlayUI>();
        if(gameplayUI == null)
            Debug.Log("GameplayMgr::Start()... gameplayUI is null....");

        streakType = GetCurrentStreakType();
        gameplayUI.InitStreakBonus(streakType, GetNextStreakType());

        nGold = 0;
        nScore = 0;

        //2021.8.6 added by pengyaun for display gold and score in game playing.
        gameplayUI.goldAndScoreUI.nGold = 0;
        gameplayUI.goldAndScoreUI.nScore = 0;
    }

    void InitConfigInformation()
    {
        //todo: here we should change this call to some '...getLevelDataFrom...' call, to get the leve data from some place
        nCurrentChapter = 1;
        ReadLevelData(nCurrentChapter);

        InitStreakConfig();
    }

    public void RestartGame()
    {
        EndGame();

        InitConfigInformation();

        Test_Shuffle();

        Test_Shuffle_StreakPoker();

        InitLockAreaInfo();//lock area should before the lock init

        InitLockInfo();

        InitPoker();

        InitHandPokerInfo();

        fGameTime = 0.0f;

        streakType = GetCurrentStreakType();
        gameplayUI.InitStreakBonus(streakType, GetNextStreakType());

        //2021.8.6 added by pengyaun for display gold and score in game playing.
        gameplayUI.goldAndScoreUI.nGold = 0;
        gameplayUI.goldAndScoreUI.nScore = 0;

        gameplayUI.Reset();
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("MapTest");
    }

    //used for post process some data used in game.
    void EndGame()
    {
        foreach(GameObject go in publicPokers)
        {
            Destroy(go);
        }

        foreach (GameObject go in handPokers)
        {
            Destroy(go);
        }

        foreach (GameObject go in foldPoker)
        {
            Destroy(go);
        }

        publicPokers.Clear();

        handPokers.Clear();

        foldPoker.Clear();

        opStepInfos.Clear();

        //todo: we should clear the lock's information and the lock area's information
        //...
        //...
        foreach(LockGroup lockGroup in lockGroups)
        {
            foreach(GameObject go in lockGroup.locks)
            {
                Destroy(go);
            }
        }
        lockGroups.Clear();


        lockGroupDict1.Clear();
        lockGroupDict2.Clear();

        foreach(LockAreaInfo areaInfo in lockAreas)
        {
            Destroy(areaInfo.Area);
        }
        lockAreas.Clear();

        unlockAreaPokerIDs.Clear();

        currentFlippingPoker = null;
        bAutoFlipHandPoker = false;
        cardIndex = 0;
        streakCardIndex = 0;

        nStreakCount = 0;
        nStreakBonusCount = 0;
        nStreakBonusColor = new int[6];
        nCollectGold = 0;
        nCollectScore = 0;
        nClearGold = 0;
        nTotalComboCount = 0;

        nCurrentStreakIndex = 0;

        gameResult = GameResult.GameResult_Win;

        gameStatus = GameStatus.GameStatus_After_Settle;

    }

    public void InitPoker()
    {
        
        Debug.Log("GameplayMgr::InitPoker...");

        //2021.8.20 added by pengyuan,
        GeneratePokerForUnlockArea();

        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);

        int i = 0;
        foreach (JsonReadWriteTest.PokerInfo info in levelData.pokerInfo)
        {
            GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            
            //go.transform.SetParent(Trans);
            go.tag = "poker";

            //Debug.Log(go.GetComponent<MeshFilter>().mesh.bounds.size);

            Vector3 targetPos;
            GetTargetPosition(0, out targetPos);
            //testDir = Vector3.Normalize(targetPos - testModel.transform.position) * 3.0f;
            testT = 0.0f;

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            pokerScript.Init(pokerPrefab, levelData.pokerInfo[i], i, Trans.position, renderer.bounds.size, 0.0f);

            int nGroupID;
            int nGroupIndex;

            PokerSuit suit;
            int nNumber;

            if (IsInUnlockArea(i, out nGroupID, out nGroupIndex))
            {
                if (Test_GetPreAllocatedPoker(i, nGroupID, nGroupIndex, out suit, out nNumber) == 1)
                {
                    Test_GetNextPoker(out suit, out nNumber);
                }
            }
            else
            {
                //test code, set suit and number for display
                Test_GetNextPoker(out suit, out nNumber);
            }
            
            pokerScript.Test_SetSuitNumber(suit, nNumber);

            HandPoker handScript = go.GetComponent<HandPoker>();
            Destroy(handScript);

            //go.AddComponent<BoxCollider2D>();
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            //collider.offset = new Vector2(0.0f, 0.0f);

            //test code
            string strName = string.Format("poker_{0}", i);
            pokerScript.Test_SetName(strName);
            go.name = strName;

            publicPokers.Add(go);

            i++;
        }
    }

    void InitHandPokerInfo()
    {
        Debug.Log("GameplayMgr::InitHandPokerInfo...");
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);

        for (int i = 0; i < levelData.handPokerCount; ++i)
        {
            GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            //go.transform.SetParent(Trans);
            go.tag = "handCard";

            HandPoker handScript = go.GetComponent<HandPoker>();
            handScript.Init(pokerPrefab, levelData, i, Trans.position + posOffset, renderer.bounds.size, 0.0f);

            //test code, set suit and number for display
            PokerSuit suit;
            int nNumber;
            Test_GetNextPoker(out suit, out nNumber);
            handScript.Test_SetSuitNumber(suit, nNumber);

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            Destroy(pokerScript);

            //test code
            string strName = string.Format("hand_{0}", i);
            pokerScript.Test_SetName(strName);
            go.name = strName;

            handPokers.Add(go);

        }
    }

    void InitLockInfo()
    {
        Debug.Log("GameplayMgr::InitLockInfo... the lock count is: " + levelData.lockGroup.nLockGroupCount);
        //1. init the lock prefab, set position, tag, and set the data for the lock prefab script, set name... ,then add it to a list

        for(int i = 0; i < levelData.lockGroup.nLockGroupCount; ++i)
        {
            LockGroup lockGroup = new LockGroup();
            lockGroup.nGroupID = levelData.lockGroup.lockGroups[i].nGroupID;

            //int nLastIndex = levelData.lockGroup.lockGroups[i].lockInfos.Count - 1;
            //Vector3 posOffset = new Vector3(levelData.lockGroup.lockGroups[i].lockInfos[nLastIndex].fPosX, levelData.lockGroup.lockGroups[i].lockInfos[nLastIndex].fPosY, -1.0f) * 0.01f; 
            for (int j = 0; j < levelData.lockGroup.lockGroups[i].lockInfos.Count; ++j)
            {
                Vector3 posOffset = new Vector3(levelData.lockGroup.lockGroups[i].lockInfos[0].fPosX, levelData.lockGroup.lockGroups[i].lockInfos[j].fPosY, -1.0f) * 0.01f; ;
                //posOffset += Vector3.up * 1.88f;
                GameObject go = (GameObject)Instantiate(lockPrefab, Trans.position + posOffset, Quaternion.identity);

                go.tag = "lock";

                GameLock lockScript = go.GetComponent<GameLock>();
                lockScript.Init(levelData.lockGroup.lockGroups[i].lockInfos[j], j, Trans.position, renderer.bounds.size, 0.0f);

                lockGroup.locks.Add(go);

                //todo: we should init the data of the gameobject here ...
                string strKey = string.Format("{0}_{1}", i, j);
                if(lockScript.HasTwoClearConditions())
                {
                    lockGroupDict2.Add(strKey, go);
                }
                else
                {
                    lockGroupDict1.Add(strKey, go);
                }
                
            }

            lockGroups.Add(lockGroup);
        }
    }

    void InitLockAreaInfo()
    {
        Debug.Log("GameplayMgr::InitLockInfo... the lock area count is: " + levelData.LockAreaCount + "  " + renderer.bounds.size + "    " + Trans.position);

        for(int i = 0; i < levelData.LockAreaCount; ++i)
        {
            LockAreaInfo lockArea = new LockAreaInfo();
            lockArea.nGroupID = levelData.lockAreas[i].nAreaID;

            Vector3 posOffset = new Vector3(levelData.lockAreas[i].fPosX, levelData.lockAreas[i].fPosY, -1.0f) * 0.01f;
            //posOffset += new Vector3(renderer.bounds.size.x * 0.0f, -renderer.bounds.size.y * 0.5f, 0.0f);
            GameObject go = (GameObject)Instantiate(lockAreaPrefab, Trans.position + posOffset, Quaternion.identity);
            go.tag = "lockArea";

            lockArea.Area = go;

            //init the lock area infomation...
            LockArea lockAreaScript = go.GetComponent<LockArea>();

            lockAreaScript.Init(levelData.lockAreas[i], i, Trans.position, renderer.bounds.size, 0.0f);
            lockAreaScript.SetUnlockPokerIDs(levelData.unlockAreas[i].strUnlockPokers);

            lockAreas.Add(lockArea);
        }
    }

    public bool PointOverlapLockArea(Vector3 pos)
    {
        Vector2 pos2D = new Vector2(pos.x, pos.y) * 0.01f;

        for(int i = 0; i < lockAreas.Count; ++i)
        {
            //Rect rectArea = lockAreas[i].Area.GetComponent<RectTransform>().rect;
            Rect tempRect = lockAreas[i].Area.GetComponent<RectTransform>().rect;
            //Rect rectArea =  new Rect(tempRect.x * 0.01f - Trans.position.x - tempRect.width * 0.005f, tempRect.y * 0.01f - Trans.position.y - tempRect.height * 0.005f, tempRect.width * 0.01f, tempRect.height * 0.01f);
            Rect rectArea = new Rect(Trans.position.x + tempRect.x * 0.01f - tempRect.width * 0.005f, Trans.position.y + tempRect.y * 0.01f - tempRect.height * 0.005f, tempRect.width * 0.01f, tempRect.height * 0.01f);

            Debug.Log("PointOverlapLockArea ... click is in lock area, so we do nothing..." + rectArea + "the pos2D is: " + pos2D + "  the temp Rect is: " + tempRect);
            if (rectArea.Contains(pos2D))
            {
                Debug.Log("PointOverlapLockArea ... click is in lock area, so we do nothing..." + rectArea + "the pos2D is: " + pos2D + "  the area pos is: " + lockAreas[i].Area.GetComponent<RectTransform>().transform.position);
                return true;
            }
        }

        return false;
    }

    bool IsClickingLockArea(RaycastHit2D[] hitResults)
    {
        for(int i = 0; i < hitResults.Length; ++i)
        {
            if(hitResults[i].collider.tag == "lockArea")
            {
                return true;
            }
        }

        return false;
    }


    public bool LockOverlapLockArea(Vector3 pos, Vector3 size)
    {
        Vector2 pos2D = new Vector2(pos.x, pos.y);
        Vector2 size2D = new Vector2(size.x, size.y);

        for(int i = 0; i < lockAreas.Count; ++i)
        {
            Vector3 areaPos = lockAreas[i].Area.transform.position;
            Vector3 areaSize = lockAreas[i].Area.GetComponent<SpriteRenderer>().bounds.size;
            Vector2 areaPos2D = new Vector2(areaPos.x, areaPos.y);
            Vector2 areaSize2D = new Vector2(areaSize.x, areaSize.y);

            if ( Mathf.Abs(areaPos2D.x - pos2D.x) < (size2D.x + areaSize2D.x) * 0.5f
                && Mathf.Abs(areaPos2D.y - pos2D.y) < (size2D.y + areaSize2D.y) * 0.5f )
            {
                //Debug.Log("LockOverlapLockArea... ... the aresPos2d is: " + areaPos2D + "the areaSize2D is: " + areaSize2D);
                //Debug.Log("LockOverlapLockArea... ... the pos2D is: " + pos2D + "the size2D is: " + size2D);
                return true;
            }
        }

        return false;
    }

    bool CheckAndClearLockArea(int groupID)
    {
        bool bCanClear = true;
             
        for (int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == groupID)
            {
                for(int j = 0; j < lockGroups[i].locks.Count; ++j)
                {
                    GameObject go = lockGroups[i].locks[j];
                    GameLock lockScript = go.GetComponent<GameLock>();

                    bCanClear &= (lockScript.lockState != LockState.LockState_Working);
                    Debug.Log(lockScript.lockState);
                }
            }
        }

        //Debug.Log("CheckAndClearLockArea... we can clear a lock area, the id is: " + groupID);

        //here we clear a lock area
        if (bCanClear)
        {
            for (int i = 0; i < lockAreas.Count; ++i)
            {
                if (lockAreas[i].nGroupID == groupID)
                {
                    LockArea lockAreaScript = lockAreas[i].Area.GetComponent<LockArea>();
                    lockAreaScript.ClearArea();

                    return true;
                }
            }
        }

        return false;
    }


    void AdjustLockGroupPosition(int nID)
    {
        List<GameObject> tempLockList = new List<GameObject>();
        int nGroupIndex = 0;

        for(int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == nID)
            {
                nGroupIndex = i;
                for (int j = 0; j < lockGroups[i].locks.Count; ++j)
                {
                    GameLock lockScript = lockGroups[i].locks[j].GetComponent<GameLock>();

                    if(lockScript.lockState == LockState.LockState_Working)
                    {
                        tempLockList.Add(lockGroups[i].locks[j]);
                    }
                }
                break;
            }
        }

        //we adjust the lock position
        int nLastIndex = lockGroups[nGroupIndex].locks.Count - 1;
        Vector3 newPos = lockGroups[nGroupIndex].locks[nLastIndex].transform.position;

        int nLastIndex2 = tempLockList.Count - 1;
        for (int i = nLastIndex2, j = 0; i >= 0; --i, ++j)
        {
            GameLock lockScript = tempLockList[i].GetComponent<GameLock>();
            GameLock lockScript2 = lockGroups[nGroupIndex].locks[nLastIndex - j].GetComponent<GameLock>();
            tempLockList[i].transform.position = lockScript2.oriPosition;
            lockScript.AdjustPosition(nLastIndex2 - j);
        }
        /*for (int i = 0; i < tempLockList.Count; ++i)
        {
            GameLock lockScript = tempLockList[i].GetComponent<GameLock>();
            GameLock lockScript2 = lockGroups[nGroupIndex].locks[nLastIndex - i].GetComponent<GameLock>();
            tempLockList[i].transform.position = lockScript2.oriPosition;
            lockScript.AdjustPosition(nLastIndex2 - i);
        }*/
    }

    public bool CanClearOneLock(GamePoker pokerScript, ref Vector3 targetPos)
    {
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            for(int j = 0; j < lockGroups[i].locks.Count; ++j)
            {
                GameLock lockScript = lockGroups[i].locks[j].GetComponent<GameLock>();

                if(lockScript.CanLockBeCleared(pokerScript))
                {
                    targetPos = lockScript.GetComponent<Transform>().position;
                    //Debug.Log("here we ")
                    return true;
                }
            }
        }

        return false;
    }

    /*public bool CanClearOneLock(GamePoker pokerScript, out GameObject lockObj)
    {
        for (int i = 0; i < lockGroups.Count; ++i)
        {
            for (int j = 0; j < lockGroups[i].locks.Count; ++j)
            {
                GameLock lockScript = lockGroups[i].locks[j].GetComponent<GameLock>();

                if (lockScript.CanLockBeCleared_2nd(pokerScript))
                {
                    lockObj = lockGroups[i].locks[j];
                    
                    return true;
                }
            }
        }

        lockObj = null;
        return false;
    }*/
    public bool CanClearOneLock(GamePoker pokerScript, out GameObject lockObj)
    {
        List<GameObject> compoundLock;
        List<GameObject> singleLock;

        if (HasActiveCompoundLock(out compoundLock))
        {
            compoundLock.Sort(delegate (GameObject Obj1, GameObject Obj2) 
            {
                int suit1 = (int)Obj1.GetComponent<GameLock>().pokerSuit;//这里排序有问题，应该检查是否有suit，然后有赋值1，没有赋值0，没问题的（2021.8.19但是要注意）
                int suit2 = (int)Obj2.GetComponent<GameLock>().pokerSuit;

                return -suit1.CompareTo(suit2);
            });
            //sorting algorithm reference:
            //list = list.OrderBy(o => o.Id).ThenBy(o => o.Name).ToList();
            //list = list.OrderByDescending(o => o.Id).ThenByDescending(o => o.Name).ToList();//降序

            for(int i = 0; i < compoundLock.Count; ++i)
            {
                GameLock lockScript = compoundLock[i].GetComponent<GameLock>();
                if(lockScript.CanLockBeCleared_2nd(pokerScript))
                {
                    lockObj = compoundLock[i];
                    return true;
                }
            }
        }

        if(HasActiveSingleLock(out singleLock))
        {
            singleLock.Sort(delegate (GameObject Obj1, GameObject Obj2)//这里排序先降序排数字，如果相同就排序花色
            {
                int nNumber1 = Obj1.GetComponent<GameLock>().nNumber;
                int nNumber2 = Obj2.GetComponent<GameLock>().nNumber;

                int nResult = -nNumber1.CompareTo(nNumber2);

                if(nResult == 0)
                {
                    int suit1 = (int)Obj1.GetComponent<GameLock>().pokerSuit;
                    int suit2 = (int)Obj2.GetComponent<GameLock>().pokerSuit;

                    return -suit1.CompareTo(suit2);
                }

                return nResult;
            });

            for (int i = 0; i < singleLock.Count; ++i)
            {
                GameLock lockScript = singleLock[i].GetComponent<GameLock>();
                if (lockScript.CanLockBeCleared_2nd(pokerScript))
                {
                    lockObj = singleLock[i];
                    return true;
                }
            }
        }
        
        lockObj = null;
        return false;
    }

    bool HasActiveCompoundLock(out List<GameObject> compoundLock)
    {
        //List<GameObject> compoundLock = new List<GameObject>();
        compoundLock = new List<GameObject>();

        foreach (var item in lockGroupDict2 )
        {
            GameObject go = item.Value;
            GameLock lockScript = go.GetComponent<GameLock>();

            if(lockScript.lockState == LockState.LockState_Working 
                && lockScript.HasTwoClearConditions())
            {
                compoundLock.Add(go);
            }
        }

        if(compoundLock.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool HasActiveSingleLock(out List<GameObject> singleLock)
    {
        singleLock = new List<GameObject>();

        foreach (var item in lockGroupDict1)
        {
            GameObject go = item.Value;
            GameLock lockScript = go.GetComponent<GameLock>();

            if (lockScript.lockState == LockState.LockState_Working
                && lockScript.HasOneClearConditions())
            {
                singleLock.Add(go);
            }
        }

        if (singleLock.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void WithdrawLock(int nGroup, int nIndex)
    {
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == nGroup)
            {
                for(int j = 0; j < lockGroups[i].locks.Count; ++j)
                {
                    GameLock lockScript = lockGroups[i].locks[j].GetComponent<GameLock>();

                    if(lockScript.nGroupID == nGroup && lockScript.nIndex == nIndex)
                    {
                        lockScript.WithdrawClear();
                        break;
                    }
                }
            }
        }

        AdjustLockGroupPosition(nGroup);
    }

    void WithdrawLockArea(int nGroup)
    {
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            if (lockAreas[i].nGroupID == nGroup)
            {
                LockArea lockAreaScript = lockAreas[i].Area.GetComponent<LockArea>();

                lockAreaScript.WithdrawClear();
            }
        }
    }

    void ReadLevelData(int nChapter)
    {
        //string strLevel = string.Format("/Resources/Configs/LevelInfo/Chapter_{0:D4}/", nChapter);
        string strLevel = string.Format("Configs/LevelInfo/Chapter_{0:D4}/", nChapter);

        //todo: this level should come from a call from map ...
        int nTempLevel = Random.Range(1, 6);
        nTempLevel = STAGameManager.Instance.nLevelID;
        nCurrentLevel = nTempLevel;
        JsonReadWriteTest.Test_ReadLevelData(strLevel, 1, nTempLevel, out levelData);

        Debug.Log("GameplayMgr::ReadLevelData... card count is: " + levelData.pokerInfo.Count);
    }

    public void Test_InitPoker()
    {
        Debug.Log("GameplayMgr::Test_InitPoker()... ");

        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);
        Debug.Log("test model position is: " + (Trans.position + posOffset));

        testModel = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
        testModel.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        testModel.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        testModel.transform.SetParent(Trans);
        testModel.tag = "poker";

        Vector3 targetPos;
        GetTargetPosition(0, out targetPos);
        testDir = Vector3.Normalize( targetPos - testModel.transform.position ) * 3.0f;
        testT = 0.0f;

        for (int i=1; i<=0; ++i)
        {
            GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            go.transform.SetParent(Trans);
            go.tag = "poker";

            testCards.Add(go);
        }
    }


    // Update is called once per frame
    void Update()
    {
        fGameTime += Time.deltaTime;

        if(gameStatus != GameStatus.GameStatus_Gaming)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            //Ray ray;
            Vector2 mousePos = new Vector3();
            mousePos.x = Input.mousePosition.x;
            mousePos.y = Input.mousePosition.y;
            //mousePos.z = 0.0f;

            //2021.8.18 pengyuan , if click position overelapes with lock area, we do nothing
            //todo...
            //LockOverlapLockArea
            Vector3 clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit hit = Physics.Raycast(ray, hit);
            /*if(PointOverlapLockArea(Input.mousePosition))
            {
                return;
            }*/


            //Debug.Log("GameplayMgr::Update... we have to compute the hit results " + Input.mousePosition);

            RaycastHit2D[] hitResults = Physics2D.RaycastAll(clickWorldPos, Vector2.zero);

            GameObject topHandPoker = GetTopHandPoker();

            if (hitResults.Length > 0 && !IsClickingLockArea(hitResults))
            {
                //Debug.Log("@@@@@@@@@@@@@@@ here we get a ray, the hitResult length is: " + hitResults.Length);

                if (topHandPoker != null && IsClickHandPoker(hitResults))
                {
                    if (IsTopHandPokerClicked(topHandPoker, hitResults))
                    {
                        OnTopHandPokerClicked(topHandPoker);
                    }
                }

                if (IsClickGamePoker(hitResults) && foldPoker.Count > 0)
                {
                    List<GameObject> topPokers = new List<GameObject>();
                    int nIndex = -1;
                    bool bRet = GetTopPokerInfos(ref topPokers);

                    //Debug.Log("here we get a ray, the hitResult length is: " + hitResults.Length);

                    if ( IsTopPublicPokerClicked(ref nIndex, ref topPokers, ref hitResults) )
                    {
                        //here we process the top public poker clicking result.
                        OnTopGamePokerClicked(topPokers[nIndex]);
                    }
                }

            }
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 50, Color.red);
        }
    }

    public Vector3 GetFoldPokerPosition()
    {
        Vector3 pos = new Vector3();

        pos.x = renderer.bounds.size.x * 0.1f;
        pos.y = Trans.position.y + renderer.bounds.size.y * -0.4f;
        pos.z = foldPoker.Peek().GetComponent<Transform>().position.z - 0.05f;

        return pos;
    }

    public float GetFoldPokerPosition_Z()
    {
        if (foldPoker.Count <= 1)
        {
            return Trans.position.z - 2.0f;
        }

        //float fZ = foldPoker.Peek().GetComponent<Transform>().position.z - 0.05f;
        float fZ = Trans.position.z - 2.0f;// - foldPoker.Count * 0.05f;

        return fZ;
    }

    //withdraw one poker from the foldPokers
    public void WithdrawOnePoker()
    {
        if(foldPoker.Count < 2)
        {
            Debug.Log("nothing to do!");
            return;
        }

        //Debug.Log("withdraw one poker. the foldpoker count is: " + foldPoker.Count);

        //todo here we withdraw one poker to its original place 
        GameObject poker = foldPoker.Peek();
        Debug.Log("poker name is: " + poker.name + "  tag is: " + poker.tag);

        OpStepInfo stepInfo = opStepInfos.Peek();

        switch (poker.tag)
        {
            case "poker":
                WithdrawGamePoker(poker);
                gameplayUI.goldAndScoreUI.AddGold(-stepInfo.nGold);
                gameplayUI.goldAndScoreUI.AddScore(-stepInfo.nScore);
                break;

            case "handCard":
                WithdrawHandPoker(poker);
                AdjustAllHandPokerPosition();
                break;

            case "wildCard":
                WithdrawWildCard(poker);
                AdjustAllHandPokerPosition();
                break;
            default:break;
        }

        
        streakType = (StreakType)stepInfo.nStreakType;
        nStreakCount = stepInfo.nStreakCount;
        nStreakBonusCount = stepInfo.nStreakCount;
        StreakBonusDecode(stepInfo.nStreakBonus);
        nCollectScore -= stepInfo.nScore;

        nTotalComboCount = stepInfo.nComboCount;

        if(poker.tag == "poker")
        {
            int nStreakFinishCount = GetStreakFinishCount((StreakType)stepInfo.nStreakType);
            if (stepInfo.nStreakCount > 0 && (nStreakFinishCount - stepInfo.nStreakCount) == 1)
            {
                nCurrentStreakIndex--;
            }
        }

        //Debug.Log("withdraw one hand poker, the OpStepInfo is: " + stepInfo);

        //if (stepInfo.strCardName != "")//represents this is a bonus card
        if (stepInfo.strIDs.Length != 0)//represents this is a bonus card
        {
            //Debug.Log("we withdraw two pokers, the id is: " + stepInfo.strIDs);
            string[] strIDs = stepInfo.strIDs.Split('_');

            int nMultiple = strIDs.Length;

            for(int i = 0; i < nMultiple; ++i)
            {
                //Debug.Log("here we withdraw a card, and its bonus name is: " + stepInfo.strCardName);

                if(stepInfo.nStreakType == (int)StreakType.Add_Poker)
                {
                    string strCardName = string.Format("handpoker_bonus_{0}", int.Parse(strIDs[i])); 
                    GameObject insertPoker = handPokers.Find(target => target.name.Equals(strCardName));

                    if(insertPoker != null)
                    {
                        WithdrawHandCardFromHand(insertPoker);
                    }
                }

                if (stepInfo.nStreakType == (int)StreakType.Add_Wildcard)
                {
                    string strCardName = string.Format("wildcard_bonus_{0}", int.Parse(strIDs[i])); 
                    GameObject insertPoker = handPokers.Find(target => target.name.Equals(strCardName));

                    if (insertPoker != null)
                    {
                        WithdrawWildCardFromHand(insertPoker);
                    }
                }

            }
        }

        //2021.8.20 added by pengyuan to withdraw a lock
        if(stepInfo.strLockInfo.Length != 0)
        {
            string[] strParams = stepInfo.strLockInfo.Split('_');
            int nGroupID = int.Parse(strParams[0]);
            int nIndex = int.Parse(strParams[1]);

            WithdrawLock(nGroupID, nIndex);
        }
        if(stepInfo.nLockAreaCleared >= 0)
        {
            WithdrawLockArea(stepInfo.nLockAreaCleared);
        }

        gameplayUI.InitStreakBonus(streakType, GetNextStreakType());
        gameplayUI.SetStreakBonusStatus(nStreakCount, stepInfo.nStreakBonus);

        Debug.Log("Withdraw a poker, and the last streakType is: " + streakType + "  streak count is: " + nStreakCount);

        opStepInfos.Pop();

        if (foldPoker.Count < 2)
        {
            Debug.Log("Withdraw Button disabled! foldPoker count is: " + foldPoker.Count);
            gameplayUI.DisableWithdrawBtn();
        }
        else//this case if for test only...
        {
            Test_EnableTopFoldPokerText();
        }

        //determine whether we should show Add5Btn
        if (handPokers.Count == 0)
        {
            gameplayUI.ShowAdd5Btn();
            gameplayUI.ShowEndGameBtn();
        }
    }

    //when click "Add5Btn", add 5 pokers to the hand poker
    public void Add5HandPoker()
    {
        PokerSuit[] suits = new PokerSuit[5];
        int[] nNumbers = new int[5];

        Test_Get5MoreHandPoker(ref suits, ref nNumbers);

        int nBegin = levelData.handPokerCount;
        levelData.handPokerCount += 5;
        //Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);
        for (int i = 0; i < 5; ++i)
        {
            GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            //go.transform.SetParent(Trans);
            go.tag = "handCard";

            HandPoker handScript = go.GetComponent<HandPoker>();
            handScript.Init(pokerPrefab, levelData, nBegin + i, Trans.position + posOffset, renderer.bounds.size, 0.0f);

            //test code, set suit and number for display
            handScript.Test_SetSuitNumber(suits[i], nNumbers[i]);

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            Destroy(pokerScript);

            //test code
            string strName = string.Format("hand_{0}", nBegin + i);
            pokerScript.Test_SetName(strName);
            go.name = strName;

            handPokers.Add(go);
        }
    }

    //when click "Wild Card" button, we call this method
    public void OnClickWildCardBtn()
    {
        if(foldPoker.Count > 0)
        {
            GameObject top = foldPoker.Peek();

            if(top.tag == "wildCard")
            {
                return;
            }
        }

        //Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * -0.35f, -1.0f);

        GameObject go = (GameObject)Instantiate(wildcardPrefab, Trans.position + posOffset, Quaternion.identity);
        go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
        //go.transform.SetParent(Trans);
        go.tag = "wildCard";

        WildCard wildcardScript = go.GetComponent<WildCard>();
        GameObject topPoker = foldPoker.Peek();
        float fZ = topPoker.GetComponent<Transform>().position.z;
        wildcardScript.Init(wildcardPrefab, foldPoker.Count, Trans.position, renderer.bounds.size, 0.0f);//the second parameter should be the top foldPoker's index

        //
        int nColors = GetStreakBonusEncode();
        int nHasStreak = nStreakCount > 0 ? 1 : 0;
        //AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, nHasStreak, StreakType.Add_Gold);
        AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, 0, nHasStreak, streakType, nTotalComboCount);

        foldPoker.Push(go);
        gameplayUI.HideEndGameBtn();
        gameplayUI.EnableWithdrawBtn();
    }

    void WithdrawGamePoker(GameObject poker)
    {
        UnflipAllCoveredGamePoker(poker);

        GamePoker gamePokerScript = poker.GetComponent<GamePoker>();
        gamePokerScript.Withdraw();

        //2021.8.20 if this step includes a lock's clear, we need to withdraw this lock's clearing
        //todo:...

        nStreakCount--;
        nStreakBonusCount--;
        nCollectScore -= 1;

        //nStreakBonusColor[nStreakBonusCount % 6] = (int)PokerColor.Red;
        //nCollectGold += 1;//todo:here we should get the gold from the config file
        //nClearGold = 0;
        //nTotalComboCount -= 1;

        foldPoker.Pop();
        publicPokers.Add(poker);

        Test_EnableTopFoldPokerText();

        //Debug.Log("one game poker withdrawed!!! name is: " + poker.name);
    }

    void WithdrawHandPoker(GameObject poker)
    {
        HandPoker handPokerScript = poker.GetComponent<HandPoker>();
        handPokerScript.Withdraw();

        nStreakCount--;
        nStreakBonusCount--;
        nCollectScore -= 1;

        //nStreakBonusColor[nStreakBonusCount % 6] = (int)PokerColor.Red;
        //nCollectGold += 1;//todo:here we should get the gold from the config file
        //nClearGold = 0;
        //nTotalComboCount -= 1;

        foldPoker.Pop();
        handPokers.Add(poker);

        Test_EnableTopFoldPokerText();

        //Debug.Log("one hand poker withdrawed!!! name is: " + poker.name);
    }

    void WithdrawWildCard(GameObject poker)
    {
        Debug.Log("one WildCard withdrawed!!! name is: " + poker.name);
        WildCard wildCard = poker.GetComponent<WildCard>();
        wildCard.Withdraw();

        foldPoker.Pop();
        if(wildCard.wildcardSource == WildCardSource.StreakBonus)
        {
            handPokers.Add(poker);
        }
        
        Test_EnableTopFoldPokerText();

        //todo: here we should give the money or item back to player...
    }

    void WithdrawWildCardFromHand(GameObject poker)
    {
        Debug.Log("one wild card withdrawed from hand pokers!!! name is: " + poker.name);
        WildCard wildCard = poker.GetComponent<WildCard>();
        //wildCard.Withdraw();
        wildCard.Cancel();

        handPokers.Remove(poker);
        AdjustAllHandPokerPosition();

        //foldPoker.Pop();
        //Test_EnableTopFoldPokerText();

        //todo: here we should give the money or item back to player...
    }

    void WithdrawHandCardFromHand(GameObject poker)
    {
        Debug.Log("one hand card withdrawed from hand pokers!!! name is: " + poker.name);

        HandPoker handPokerScript = poker.GetComponent<HandPoker>();
        handPokerScript.Cancel();

        handPokers.Remove(poker);
        AdjustAllHandPokerPosition();
    }

    void UnflipAllCoveredGamePoker(GameObject withdrawedPoker)
    {
        //get all facing game poker, detect the withdrawed poker can cover it,
        //if so, unflip this poker
        List<GameObject> facingPokers = new List<GameObject>();
        foreach(GameObject go in publicPokers)
        {
            GamePoker gamePokerScript = go.GetComponent<GamePoker>();
            if(gamePokerScript.pokerFacing == PokerFacing.Facing)
            {
                facingPokers.Add(go);
            }
        }

        //Debug.Log("UnflipAllCoveredGamePoker... the withdraw go is: " + withdrawedPoker.name);
        //precompute the target pos of the withdrawed poker
        Vector3 targetPos = new Vector3();
        int nIndex = withdrawedPoker.GetComponent<GamePoker>().Index;
        targetPos.x = Trans.position.x + levelData.pokerInfo[nIndex].fPosX * 0.01f;
        targetPos.y = Trans.position.y + levelData.pokerInfo[nIndex].fPosY * 0.01f;
        targetPos.z = Trans.position.z - 1.0f - nIndex * 0.05f;

        //detect all facing poker to see whether the withdrawed poker can cover it.
        foreach(GameObject go in facingPokers)
        {
            //Debug.Log("UnflipAllCoveredGamePoker... the overdraw go is: " + go.name);
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            GamePoker pokerData = go.GetComponent<GamePoker>();

            Vector2 newPos;
            newPos.x = targetPos.x;
            newPos.y = targetPos.y;
            
            Collider2D[] hitResults = Physics2D.OverlapBoxAll(newPos, collider.size, levelData.pokerInfo[nIndex].fRotation);

            if(hitResults.Length > 0)
            {
                //Debug.Log("UnflipAllCoveredGamePoker... the overlap box count is: " + hitResults.Length);
                for(int i = 0; i < hitResults.Length; ++i)
                {
                    if(go.name == hitResults[i].name)
                    {
                        //Debug.Log("UnflipAllCoveredGamePoker... the unflip poker name is: " + hitResults[i].name);
                        pokerData.UnflipPoker();
                        break;
                    }
                }
            }
        }
    }

    //we assume that we can only click hand poker or public poker at one time, 
    //we can't click both.
    private bool IsClickHandPoker(RaycastHit2D[] hitResults)
    {
        if(hitResults[0].collider.tag == "handCard")
        {
            HandPoker handPokerScript = hitResults[0].collider.GetComponent<HandPoker>();

            if (handPokerScript != null && handPokerScript.pokerType == PokerType.HandPoker)
            {
                return true;
            }
        }
        
        //2021.8.5 added by pengyuan, to process wildcard click
        if(hitResults[0].collider.tag == "wildCard")
        {
            WildCard wildcardScript = hitResults[0].collider.GetComponent<WildCard>();

            if(wildcardScript != null)
            {
                return true;
            }
        }

        return false;
    }

    //we assume that we can only click hand poker or public poker at one time, 
    //we can't click both.
    private bool IsClickGamePoker(RaycastHit2D[] hitResults)
    {
        GamePoker gamePokerScript = hitResults[0].collider.GetComponent<GamePoker>();

        if (gamePokerScript != null && gamePokerScript.pokerType == PokerType.PublicPoker)
        {
            return true;
        }

        return false;
    }

    bool IsTopHandPokerClicked(GameObject topHandPoker, RaycastHit2D[] hitResults)
    {
        bool bFind = false;
        for (int i = 0; i < hitResults.Length; ++i)
        {
            //Debug.Log("here we get a ray, the hitResults[i].collider.name is: " + hitResults[i].collider.name);
            if (hitResults[i].collider.name == topHandPoker.name)
            {
                bFind = true;
                break;
            }
        }

        return bFind;
    }

    void OnTopGamePokerClicked(GameObject topGamePoker)
    {
        GamePoker gamePokerScript = topGamePoker.GetComponent<GamePoker>();
        GameObject handPoker = foldPoker.Peek();

        string strLockInfo = "";
        int nLockAreaID = -1;

        if(handPoker != null)
        {
            //HandPoker handPokerScript = handPoker.GetComponent<HandPoker>();

            Debug.Log("OnTopGamePokerClicked... the game poker is: " + gamePokerScript.nPokerNumber + "  the hand poker is: " + handPoker.name);

            if(CanFoldGamePoker(gamePokerScript, handPoker))
            {
                Vector3 lockPos = Vector3.zero;
                GameObject lockObject;
                if (CanClearOneLock(gamePokerScript, out lockObject))
                {
                    GameLock lockScript = lockObject.GetComponent<GameLock>();
                    //Debug.Log("here we clear a lock, the color is: " + lockScript.pokerColor);
                    lockScript.ClearLock();
                    strLockInfo = string.Format("{0}_{1}", lockScript.nGroupID, lockScript.nIndex);

                    gamePokerScript.FoldPokerWithLock(foldPoker.Count, lockObject.transform.position);

                    //check whether a lock area can be open
                    if (CheckAndClearLockArea(lockScript.nGroupID))
                    {
                        nLockAreaID = lockScript.nGroupID;
                    }

                    //reset the lock group's position
                    AdjustLockGroupPosition(lockScript.nGroupID);
                }
                else
                {
                    gamePokerScript.FoldPoker(foldPoker.Count);
                }
                

                int nColors = GetStreakBonusEncode();

                int nLastStreakCount = nStreakCount;
                nStreakCount++;

                int nStepScore = ComputeOperationScore(nStreakCount);
                int nStepGold = ComputeOperationGold();

                AddOpStepInfo(opStepInfos.Count, nLastStreakCount, nColors, nStepScore, nStepGold, 1, streakType, nTotalComboCount);
                nTotalComboCount++;

                if(strLockInfo != null)
                {
                    OpStepInfo stepInfo = opStepInfos.Peek();
                    stepInfo.strLockInfo = strLockInfo;
                }
                if(nLockAreaID >= 0)
                {
                    OpStepInfo stepInfo = opStepInfos.Peek();
                    stepInfo.nLockAreaCleared = nLockAreaID;
                }
                
                nStreakBonusColor[nStreakBonusCount % Max_StreakBonus_Count] = (int)GetPokerColor(gamePokerScript);
                nStreakBonusCount++;
                nCollectGold += nStepGold;//todo:here we should get the gold from the config file
                nCollectScore += nStepScore;
                //nClearGold = 0;

                gameplayUI.goldAndScoreUI.AddGold(nStepGold);
                gameplayUI.goldAndScoreUI.AddScore(nStepScore);

                //AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, GetCollectScore(), 1, StreakType.Add_Gold);

                //gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);
                nColors = GetStreakBonusEncode();
                UpdateStreakStatus(nStreakCount, nColors);

                currentFlippingPoker = topGamePoker;

                publicPokers.Remove(topGamePoker);

                Test_DisableAllFoldPokerText();
                foldPoker.Push(topGamePoker);
                gameplayUI.EnableWithdrawBtn();

                //2021.8.12 added by pengyuan
                CheckGameEnd();
            }
        }
        
    }

    void OnTopHandPokerClicked(GameObject topHandPoker)
    {
        Debug.Log("OnTopHandPokerClicked ... ...");
        if (!CanClickTopHandPoker())
        {
            return;
        }

        if (topHandPoker.tag == "handCard")
        {
            HandPoker handPokerScript = topHandPoker.GetComponent<HandPoker>();
            handPokerScript.FlipPoker(foldPoker.Count);

            float fZ = GetFoldPokerPosition_Z();

            handPokerScript.SetFoldIndex(foldPoker.Count);

            //Debug.Log("OnTopHandPokerClicked ... ...  11111111111111111111111111");
        }
        else if(topHandPoker.tag == "wildCard")//if hand poker is wildcard, we still need to interrupt the streak
        {
            WildCard wildcardScript = topHandPoker.GetComponent<WildCard>();
            wildcardScript.FlipPoker(foldPoker.Count);

            wildcardScript.SetFoldIndex(foldPoker.Count);
        }

        int nColors = GetStreakBonusEncode();
        AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, 0, 0, streakType, nTotalComboCount);

        nStreakCount = 0;
        nStreakBonusCount = 0;
        nTotalComboCount = 0;
        ClearStreakBonus();

        nColors = GetStreakBonusEncode();
        UpdateStreakStatus(0, nColors);

        currentFlippingPoker = topHandPoker;

        handPokers.Remove(topHandPoker);

        Test_DisableAllFoldPokerText();

        
        foldPoker.Push(topHandPoker);



        //update withdraw button
        gameplayUI.EnableWithdrawBtn();

        AdjustAllHandPokerPosition();

        //determine whether we should show Add5Btn
        if(handPokers.Count == 0)
        {
            gameplayUI.ShowAdd5Btn();
            gameplayUI.ShowEndGameBtn();
        }
    }

    void AdjustAllHandPokerPosition()
    {
        int nIndex = 0;
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);

        foreach (GameObject go in handPokers)
        {
            if(go.tag == "handCard")
            {
                HandPoker handPokerScript = go.GetComponent<HandPoker>();
                if (handPokerScript != null)
                {
                    handPokerScript.AdjustHandPokerPosition(levelData, handPokers.Count, nIndex, Trans.position + posOffset, renderer.bounds.size);
                }
            }
            
            if(go.tag == "wildCard")
            {
                WildCard wildcardScript = go.GetComponent<WildCard>();
                if(wildcardScript != null)
                {
                    wildcardScript.AdjustWildcardPosition(handPokers.Count, nIndex, Trans.position + posOffset, renderer.bounds.size);
                }
            }
            
            nIndex++;
        }
    }

    bool IsTopPublicPokerClicked(ref int nIndex, ref List<GameObject> topPokers, ref RaycastHit2D[] hitResults)
    {
        nIndex = 0;
        foreach (GameObject go in topPokers)
        {
            for(int i = 0; i < hitResults.Length; ++i)
            {
                if (go.name == hitResults[i].collider.name)
                {
                    return true;
                }
            }

            nIndex++;
        }

        return false;
    }

//    public class OpStepInfo
//    {
//      int nIndex;
//      int nStreakCount;
//      int nStreakBonus;//010101, 6 bits store 6 color type
//      int nScore;      //the score for this step only
//      int nHasStreak;   //whether this step has a streak completed
//      int nStreakType;  //if so, store the streak(completed) type
//    }
    void AddOpStepInfo(int nIndex, int nStreakCount, int nStreakBonus, int nScore, int nGold, int nHasStreak, StreakType streak, int nComboCount)
    {
        //Debug.Log("AddOpStepInfo... nStreakCount is: " + nStreakCount);
        OpStepInfo info = new OpStepInfo();

        info.nIndex = nIndex;
        info.nStreakCount = nStreakCount;
        info.nStreakBonus = nStreakBonus;
        info.nScore = nScore;
        info.nGold = nGold;
        info.nHasStreak = nHasStreak;
        info.nStreakType = (int)streak;
        info.nComboCount = nComboCount;
        info.strCardName = "";
        info.strIDs = "";
        info.strLockInfo = "";
        info.nLockAreaCleared = -1;

        opStepInfos.Push(info);
    }

    int GetStreakBonusEncode()
    {
        int nResult = 0;

        for (int i=0; i<Max_StreakBonus_Count; ++i)
        {
            if (nStreakBonusColor[i] > 0)
            {
                nResult |= (1 << i);
            }
        }

        return nResult;
    }

    //code streak bonus for OpStepInfo.nStreakBonus, result is in nStreakBonusColor[].
    void StreakBonusDecode(int nBonus)
    {
        for(int i = 0; i < Max_StreakBonus_Count; ++i)
        {
            nStreakBonusColor[i] = (nBonus & (1 << i) >> i );
        }
    }

    public void StreakBonusDecode(int nBonus, ref PokerColor[] streakBonus)
    {
        for (int i = 0; i < Max_StreakBonus_Count; ++i)
        {
            streakBonus[i] = (PokerColor)((nBonus & (1 << i)) >> i);
        }
    }

    public StreakType GetCurrentStreakType()
    {
        int totalStreakCount = nStreakIDs.Length;
        if (nCurrentStreakIndex >= (totalStreakCount - 1) )
        {   
            return (StreakType)nStreakIDs[totalStreakCount - 1];//todo: here we should return the 3rd streak type from the config file.
        }

        return (StreakType)(nStreakIDs[nCurrentStreakIndex]);
    }

    public StreakType GetNextStreakType()
    {
        int totalStreakCount = nStreakIDs.Length;
        if ((nCurrentStreakIndex + 1) >= (totalStreakCount - 1))
        {
            return (StreakType)nStreakIDs[totalStreakCount - 1];
        }

        return (StreakType)(nStreakIDs[nCurrentStreakIndex + 1]);
    }

    //if the poker is the same color, then give double bonus to player
    bool IsDoubleStreak(int nCount, int nStreakBonus)
    {
        PokerColor[] streakBonus = new PokerColor[Max_StreakBonus_Count];

        StreakBonusDecode(nStreakBonus, ref streakBonus);

        bool bRed = true;
        bool bBlack = true;
        for (int i = 0; i < nCount; ++i)
        {
            bRed &= (streakBonus[i] == PokerColor.Red) ? true : false;
            bBlack &= (streakBonus[i] == PokerColor.Black) ? true : false;
        }

        return bRed | bBlack;
    }

    void UpdateStreakStatus(int nCount, int nStreakBonus)
    {
        gameplayUI.SetStreakBonusStatus(nCount, nStreakBonus);
        Debug.Log("UpdateStreakStatus... nCount is: " + nCount + "  type is: " + streakType);
        
        if (nCount >= GetStreakFinishCount(streakType))
        {
            //todo: here we should compute the streak bonus, and add them to player
            bool bDouble = IsDoubleStreak(nCount, nStreakBonus);
            AddStreakBonusToPlayer(bDouble);

            nStreakCount = 0;
            nStreakBonusCount = 0;
            ClearStreakBonus();

            nCurrentStreakIndex++;
            streakType = GetCurrentStreakType();
            gameplayUI.InitStreakBonus(streakType, GetNextStreakType());
            gameplayUI.SetStreakBonusStatus(nStreakCount, 0);

            Debug.Log("UpdateStreakStatus.. here we switch to next streak is: " + streakType);
        }
    }

    public PokerColor GetPokerColor(GamePoker gamePokerScript)
    {
        if(gamePokerScript.pokerSuit == PokerSuit.Suit_Heart
            || gamePokerScript.pokerSuit == PokerSuit.Suit_Diamond)
        {
            return PokerColor.Red;
        }

        return PokerColor.Black;
    }

    public PokerColor GetPokerColor(PokerSuit suit)
    {
        if (suit == PokerSuit.Suit_Heart
            || suit == PokerSuit.Suit_Diamond)
        {
            return PokerColor.Red;
        }

        return PokerColor.Black;
    }

    void ClearStreakBonus()
    {
        for(int i = 0; i < Max_StreakBonus_Count; ++i)
        {
            nStreakBonusColor[i] = 0;
        }
    }

    int GetCollectGold()
    {
        return 1;
    }

    int GetCollectScore()
    {
        return 1;
    }

    void AddStreakBonusToPlayer(bool bDouble = false)
    {
        switch(streakType)
        {
            case StreakType.Add_Gold:
                AddStreakBonusGold(bDouble);
                break;
            case StreakType.Add_Poker:
                AddStreakBonusPoker(bDouble);
                break;
            case StreakType.Add_Wildcard:
                AddStreakBonusWildcard(bDouble);
                break;
            default:break;

        }
    }

    //2021.8.9 added by pengyuan
    void AddStreakBonusGold(bool bDouble = false)
    {
        int nMultiple = bDouble ? 2 : 1;

        int nBonusGold = GetStreakBonusGold() * 2;
        int nBonusScore = GetStreakBonusScore(streakType) * 2;

        //Debug.Log("AddStreakBonusGold ... nBonusGold is:  " + nBonusGold + "  nBonusScore is: " + nBonusScore);

        OpStepInfo stepInfo = opStepInfos.Peek();
        stepInfo.nGold += nBonusGold;
        stepInfo.nScore += nBonusScore;

        nCollectGold += nBonusGold;
        nCollectScore += nBonusScore;

        gameplayUI.goldAndScoreUI.AddGold(nBonusGold);
        gameplayUI.goldAndScoreUI.AddScore(nBonusScore);

        return;
    }

    //todo:2021.8.9-2021.8.10
    void AddStreakBonusPoker(bool bDouble = false)
    {
        int nMultiple = bDouble ? 2 : 1;
        int[] bonusIDs = new int[2];

        OpStepInfo stepInfo = opStepInfos.Peek();

        for (int i = 0; i < nMultiple; ++i)
        {
            //instantiate a hand poker , find a position in handpoker, insert it into handpoker.
            Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * 0.4f, -1.0f);

            GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            //go.transform.SetParent(Trans);
            go.tag = "handCard";

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            Destroy(pokerScript);

            HandPoker handPokerScript = go.GetComponent<HandPoker>();

            int insertIndex = GetInsertPositionInHandPoker();
            GameObject insertGameObject = handPokers[insertIndex];
            handPokerScript.InitStreakBonusHandPoker(insertIndex, insertGameObject.transform.position, renderer.bounds.size, 0.0f);

            PokerSuit suit;
            int nNumber;
            Test_GetNextStreakPoker(out suit, out nNumber);
            //Debug.Log("AddStreakBonusPoker... the suit is:  " + suit + "  the number is: " + nNumber);
            handPokerScript.Test_SetSuitNumber(suit, nNumber);

            string strName = string.Format("handpoker_bonus_{0}", insertIndex);
            go.name = strName;

            //OpStepInfo stepInfo = opStepInfos.Peek();
            stepInfo.strCardName = strName;

            bonusIDs[i] = insertIndex;

            handPokers.Insert(insertIndex, go);
        }

        if(bDouble)
        {
            stepInfo.strIDs = string.Format("{0}_{1}", bonusIDs[0], bonusIDs[1]);
        }
        else
        {
            stepInfo.strIDs = string.Format("{0}", bonusIDs[0]);
        }

        gameplayUI.HideAdd5Btn();
        gameplayUI.HideEndGameBtn();

        AdjustAllHandPokerPosition();
    }
    
    void AddStreakBonusWildcard(bool bDouble = false)
    {
        int nMultiple = bDouble ? 2 : 1;
        int[] bonusIDs = new int[2];

        OpStepInfo stepInfo = opStepInfos.Peek();

        for (int i = 0; i < nMultiple; ++i)
        {
            //instantiate a wild card , find a position in handpoker, insert it into handpoker.
            Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * 0.4f, -1.0f);

            GameObject go = (GameObject)Instantiate(wildcardPrefab, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            //go.transform.SetParent(Trans);
            go.tag = "wildCard";

            WildCard wildcardScript = go.GetComponent<WildCard>();
            //GameObject topPoker = foldPoker.Peek();
            //float fZ = topPoker.GetComponent<Transform>().position.z;

            int insertIndex = GetInsertPositionInHandPoker();
            GameObject insertGameObject = handPokers[insertIndex];
            wildcardScript.InitStreakBonusWildcard(insertIndex, insertGameObject.transform.position, renderer.bounds.size, 0.0f);

            string strName = string.Format("wildcard_bonus_{0}", insertIndex);
            go.name = strName;

            //OpStepInfo stepInfo = opStepInfos.Peek();
            stepInfo.strCardName = strName;

            bonusIDs[i] = insertIndex;

            handPokers.Insert(insertIndex, go);
        }

        if(bDouble)
        {
            stepInfo.strIDs = string.Format("{0}_{1}", bonusIDs[0], bonusIDs[1]);
        }
        else
        {
            stepInfo.strIDs = string.Format("{0}", bonusIDs[0]);
        }

        gameplayUI.HideAdd5Btn();
        gameplayUI.HideEndGameBtn();

        AdjustAllHandPokerPosition();

    }

    int GetInsertPositionInHandPoker()
    {
        if(handPokers.Count < 2)
        {
            return 0;
        }

        Random.InitState((int)Time.time);

        return Random.Range(1, handPokers.Count);
    }

    private void FixedUpdate()
    {
        //here we can update the poker's position and rotation to see if they stop translate and rotate, and set a bool value.
        //这个可以牌形变化的时候再检测
        List<GameObject> topPokers = new List<GameObject>();
        if (fGameTime > 3.0f)
        {
            if (!IsPublicPokerFlipping())
            {
                if (GetTopPokerInfos(ref topPokers))
                {
                    for (int i = 0; i < topPokers.Count; ++i)
                    {
                        //todo: flip the top pokers
                        GamePoker gamePokerScript = topPokers[i].GetComponent<GamePoker>();
                        gamePokerScript.FlipPoker();

                        gameStatus = GameStatus.GameStatus_Gaming;
                    }
                    /*Debug.Log("we found on top pokers count is: " + topPokers.Count);
                    for(int i=0; i<topPokers.Count; ++i)
                    {
                        Debug.Log("thee poker's name is: " + topPokers[i].name);
                    }*/
                }

                if(!bAutoFlipHandPoker)
                {
                    GameObject handPoker = GetTopHandPoker();
                    if(handPoker != null)
                    {
                        OnTopHandPokerClicked(handPoker);
                        bAutoFlipHandPoker = true;
                    }
                }
            }
        }
    }

    void GetTargetPosition(int nPokerIndex, out Vector3 targetPos)
    {
        targetPos.x = Trans.position.x - 5.0f + nPokerIndex * 2.0f;
        targetPos.y = Trans.position.y;
        targetPos.z = Trans.position.z;
    }

    bool GetTopPokerInfos(ref List<GameObject> topPokers)
    {
        //Debug.Log("enter fixed update.. get top poker infos...");
        foreach (GameObject go in publicPokers)
        {
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();

            //Vector2 size;
            GamePoker pokerData = go.GetComponent<GamePoker>();
            RaycastHit2D[] hitResults = Physics2D.BoxCastAll(collider.bounds.center, collider.size, pokerData.pokerInfo.fRotation, Vector2.zero);

            if(hitResults.Length > 0)
            {
                //2021.8.19 modified by pengyuan for lockarea block the top poker. so we use the second method
                //int nMinIndex = GetMinDepthIndex(ref hitResults);
                int nMinIndex = GetMinDepthIndexWithoutLockArea(ref hitResults);

                if (hitResults[nMinIndex].collider.name == collider.name)// && !pokerData.bHasWithdrawed)
                {
                    topPokers.Add(go);
                }
            }
        }

        if(topPokers.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    int GetMinDepthIndex(ref RaycastHit2D[] hitResults)
    {
        float fMinDepth = float.MaxValue;
        int nMinIndex = -1;

        for(int i = 0; i < hitResults.Length; ++i)
        {
            if(hitResults[i].collider.transform.position.z < fMinDepth)
            {
                fMinDepth = hitResults[i].collider.transform.position.z;
                nMinIndex = i;
            }
        }

        return nMinIndex;
    }

    //2021.8.19 added by pengyuan for lockarea block the top poker...
    int GetMinDepthIndexWithoutLockArea(ref RaycastHit2D[] hitResults)
    {
        float fMinDepth = float.MaxValue;
        int nMinIndex = -1;

        for (int i = 0; i < hitResults.Length; ++i)
        {
            if(hitResults[i].collider.tag == "lockArea")
            {
                continue;
            }

            if (hitResults[i].collider.transform.position.z < fMinDepth)
            {
                fMinDepth = hitResults[i].collider.transform.position.z;
                nMinIndex = i;
            }
        }

        return nMinIndex;
    }

    GameObject GetTopHandPoker()
    {
        if(handPokers.Count == 0)
        {
            //Debug.Log("GetTopHandPoker... the handPokers.Count is 0.");
            return null;
        }

        //Debug.Log("GetTopHandPoker... the handPokers.Count is: " + handPokers.Count);
        return handPokers[handPokers.Count - 1];
    }

    private bool IsPublicPokerFlipping()
    {
        bool bRet = false;

        for (int i=0; i<publicPokers.Count; ++i)
        {
            GamePoker gamePoker = publicPokers[i].GetComponent<GamePoker>();
            bRet |= gamePoker.bIsFlipping;
        }

        return bRet;
    }

    public string Test_GetSuitDisplayString(PokerSuit suit, int nNumber)
    {
        string[] suitStrings = { "黑桃 ", "红桃 ", "草花 ", "方片 " };

        //Debug.Log("Test_GetSuitDisplayString... the suit is: " + suit + "  the number is: " + nNumber); 

        string strSuit = string.Format("{0}{1}", suitStrings[(int)suit - 1], nNumber);

        return strSuit;
    }

    //a shuffle function, return a number from 0-51
    int Test_Shuffle()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        for (int i=0; i<cardsArray.Length; ++i)
        {
            int temp = cardsArray[i];
            int nRandomIndex = Random.Range(0, cardsArray.Length);
            cardsArray[i] = cardsArray[nRandomIndex];
            cardsArray[nRandomIndex] = temp;
        }

        cardsArray2nd.Clear();
        for(int i = 0; i < cardsArray.Length; ++i)
        {
            cardsArray2nd.Add(cardsArray[i]);
        }

        return 0;
    }

    void Test_Shuffle_StreakPoker()
    {
        //Random.InitState((int)Time.realtimeSinceStartup);
        Random.InitState((int)System.DateTime.Now.Ticks);

        for (int i = 0; i < streakCardsArray.Length; ++i)
        {
            int temp = streakCardsArray[i];
            int nRandomIndex = Random.Range(0, streakCardsArray.Length);
            streakCardsArray[i] = streakCardsArray[nRandomIndex];
            streakCardsArray[nRandomIndex] = temp;
        }
    }

    public int Test_GetPreAllocatedPoker(int nPokerIndex, int nGroupID, int nGroupIndex, out PokerSuit suit, out int nNumber)
    {
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            if(nGroupID == unlockAreaPokerIDs[i].nGroupID)
            {
                for(int j = 0; j < unlockAreaPokerIDs[i].IDs.Count; ++j)
                {
                    if(unlockAreaPokerIDs[i].IDs[j] == nPokerIndex)
                    {
                        suit = (PokerSuit)(unlockAreaPokerIDs[i].pokerNumbers[j] / 13 + 1);
                        nNumber = unlockAreaPokerIDs[i].pokerNumbers[j] % 13;

                        return 0;
                    }
                }
            }
        }

        suit = PokerSuit.Suit_None;
        nNumber = 0;

        return 1;
    }

    public int Test_GetNextPoker(out PokerSuit suit, out int nNumber)
    {
        /*if(cardIndex >= cardsArray.Length)
        {
            Test_Shuffle();
            cardIndex -= cardsArray.Length;
        }

        int nSuit = (int)cardsArray[cardIndex] / 13 + 1;
        suit = (PokerSuit)nSuit;
        nNumber = cardsArray[cardIndex] % 13 + 1;
        cardIndex++;*/

        if(cardIndex >= cardsArray2nd.Count)
        {
            Debug.Log("_________这里需要洗牌了。。。." + cardsArray2nd.Count);
            Test_Shuffle();
            cardIndex -= cardsArray2nd.Count;
        }

        int nSuit = (int)(cardsArray2nd[cardIndex] - 1) / 13 + 1;
        suit = (PokerSuit)nSuit;
        nNumber = cardsArray2nd[cardIndex] % 13;
        cardIndex++;

        //Debug.Log("Test_GetSuitDisplayString... the indexx is: " + cardsArray[cardIndex] + "  the suit is: " + nSuit + "  the number is: " + nNumber);

        return 0;
    }

    public int Test_GetNextStreakPoker(out PokerSuit suit, out int nNumber)
    {
        if (streakCardIndex >= streakCardsArray.Length)
        {
            Test_Shuffle_StreakPoker();
            streakCardIndex -= streakCardsArray.Length;
        }

        int nSuit = (int)streakCardsArray[streakCardIndex] / 13 + 1;
        suit = (PokerSuit)nSuit;
        nNumber = streakCardsArray[streakCardIndex] % 13 + 1;
        streakCardIndex++;

        return 0;
    }

    public void Test_Get5MoreHandPoker( ref PokerSuit[] suits, ref int[] nNumbers )
    {
        for(int i = 0; i < 5; ++i)
        {
            /*if(cardIndex >= cardsArray.Length)
            {
                Test_Shuffle();
                cardIndex -= cardsArray.Length;
            }

            int nSuit = (int)cardsArray[cardIndex] / 13 + 1;
            suits[i] = (PokerSuit)nSuit;
            nNumbers[i] = cardsArray[cardIndex] % 13 + 1;

            cardIndex++;*/

            if (cardIndex >= cardsArray2nd.Count)
            {
                Test_Shuffle();
                cardIndex -= cardsArray2nd.Count;
            }

            int nSuit = (int)(cardsArray2nd[cardIndex] - 1)/ 13 + 1;
            suits[i] = (PokerSuit)nSuit;
            nNumbers[i] = cardsArray2nd[cardIndex] % 13;

            cardIndex++;
        }
    }

    void GeneratePokerForUnlockArea()
    {
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            UnlockAreaPokerIDs unlockAreaPokers = new UnlockAreaPokerIDs();
            unlockAreaPokers.nGroupID = lockAreas[i].nGroupID;
            unlockAreaPokerIDs.Add(unlockAreaPokers);
        }
        
        //first we find all the poker indices that exist in an unlock area
        for (int i = 0; i < levelData.pokerInfo.Count; ++i)
        {
            for (int j = 0; j < lockAreas.Count; ++j)
            {
                LockArea lockAreaScript = lockAreas[j].Area.GetComponent<LockArea>();

                int groupID;
                int groupIndex;
                if (lockAreaScript.IsInUnlockArea(i, out groupID, out groupIndex))
                {
                    InsertToUnlockPokerArray(i, groupID);
                }
            }
        }

        //second we generate the all the poker for the unlockAreaPokerIDs, according to the lockgroup information.
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            GeneratePokerInfoForLockGroup(lockGroups[i]);
        }

        //Debug.Log("在补牌之前一共有的牌张数：" + cardsArray2nd.Count);

        //maybe we should random the pre-allocated poker for each lock group
        //todo...
        for (int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            int totalCount = unlockAreaPokerIDs[i].IDs.Count;
            if (unlockAreaPokerIDs[i].pokerNumbers.Count < totalCount)
            {
                for(int j = unlockAreaPokerIDs[i].pokerNumbers.Count; j < totalCount; ++j)
                {
                    PokerSuit suit;
                    int nNumber;

                    Test_GetNextPoker(out suit, out nNumber);
                    int pokerNumber = ((int)suit - 1) * 13 + nNumber;

                    /*string strDebug = Test_GetSuitDisplayString(suit, nNumber);
                    strDebug += string.Format("__{0}", pokerNumber);
                    Debug.Log("补上一张牌，" + strDebug);*/
                    //Debug.Log("pre generate unlock area, suit is: " + suit + "  number is: " + nNumber);
                    

                    unlockAreaPokerIDs[i].pokerNumbers.Add(pokerNumber);
                    cardsArray2nd.Remove(pokerNumber);
                }
            }
        }

        //pengyuan 2021.8.23 shuffle this group, so the unlock poker can appear in random position, not in the beginning.
        for (int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            for (int j = 0; j < unlockAreaPokerIDs[i].pokerNumbers.Count; ++j)
            {
                int temp = unlockAreaPokerIDs[i].pokerNumbers[j];
                int nRandomIndex = Random.Range(0, unlockAreaPokerIDs[i].pokerNumbers.Count);
                unlockAreaPokerIDs[i].pokerNumbers[j] = unlockAreaPokerIDs[i].pokerNumbers[nRandomIndex];
                unlockAreaPokerIDs[i].pokerNumbers[nRandomIndex] = temp;
            }
        }

        /*string strDebugString = "";
        for(int i = 0; i < unlockAreaPokerIDs[0].pokerNumbers.Count; ++i)
        {
            //strDebugString += string.Format("_{0}_", unlockAreaPokerIDs[0].pokerNumbers[i]);
            PokerSuit suit =(PokerSuit)((unlockAreaPokerIDs[0].pokerNumbers[i] - 1) / 13 + 1 );
            int nDigit = unlockAreaPokerIDs[0].pokerNumbers[i] % 13;
            strDebugString += string.Format("_{0}_", Test_GetSuitDisplayString(suit, nDigit));
        }
        Debug.Log("pre generate unlock area's poker is: " + strDebugString);*/
    }

    bool IsInUnlockArea(int nPokerID, out int groupID, out int groupIndex)
    {
        for (int i = 0; i < lockAreas.Count; ++i)
        {
            LockArea lockAreaScript = lockAreas[i].Area.GetComponent<LockArea>();

            if (lockAreaScript.IsInUnlockArea(nPokerID, out groupID, out groupIndex))
            {
                return true;
            }
        }

        groupID = 0;
        groupIndex = 0;
        return false;
    }

    void GeneratePokerInfoForLockGroup(LockGroup groupInfo)
    {
        for(int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            if(unlockAreaPokerIDs[i].nGroupID == groupInfo.nGroupID)
            {
                for(int j = 0; j < groupInfo.locks.Count; ++j)
                {
                    //here we generate a poker that fit the condition of the lock info
                    GameLock lockScript = groupInfo.locks[j].GetComponent<GameLock>();

                    int nAllocatedPoker = GenerateUnlockPoker(lockScript);
                    unlockAreaPokerIDs[i].pokerNumbers.Add(nAllocatedPoker);

                    cardsArray2nd.Remove(nAllocatedPoker);

                    //todo: here maybe we should add the removed poker to another list.
                    //...    
                    PokerSuit suit = (PokerSuit)(nAllocatedPoker / 13 + 1);
                    int nDigit = nAllocatedPoker % 13;
                    string strDebugString = string.Format("_{0}_{1}", Test_GetSuitDisplayString(suit, nDigit), nAllocatedPoker);
                    Debug.Log("预生成扑克，移除：" + strDebugString);
                }

                break ;
            }
        }
    }

    int GenerateUnlockPoker(GameLock lockScript)
    {
        int[] nDelatIndexs = { 2, 3, 5, 7 };

        int nRet = 0;
        //suit & number
        if (lockScript.nNumber != 0 && lockScript.pokerSuit != PokerSuit.Suit_None)
        {
            nRet = ((int)lockScript.pokerSuit - 1) * 13 + lockScript.nNumber;
            
            return nRet;
        }

        //color & number
        if(lockScript.nNumber != 0 && lockScript.pokerColor != PokerColor.None)
        {
            int nRandom = Random.Range(1, 100);
            nRandom %= 2;
            nRet = ((int)lockScript.pokerColor - 1) * 13 + nRandom * 26 + lockScript.nNumber;

            if (!cardsArray2nd.Exists(t => t == nRet))
            {
                nRandom = (nRandom + 1) % 2;
                nRet = ((int)lockScript.pokerColor - 1) * 13 + nRandom * 26 + lockScript.nNumber;
            }

            return nRet;
        }

        //number only
        if(lockScript.nNumber != 0)
        {
            int nSuit = Random.Range(1, 4);
            nRet = (nSuit - 1) * 13 + lockScript.nNumber;

            if (!cardsArray2nd.Exists(t => t == nRet))//nRet already be used
            {
                nSuit = (nSuit + 1) % 4 + 1;
                nRet = (nSuit - 1) * 13 + lockScript.nNumber;
            }

            return nRet;
        }

        //suit only
        if (lockScript.pokerSuit != PokerSuit.Suit_None)
        {
            int nNumber = Random.Range(1, 13);
            nRet = ((int)lockScript.pokerSuit - 1) * 13 + nNumber;

            if (!cardsArray2nd.Exists(t => t == nRet))
            {
                //nNumber = (nNumber + 1) % 13 + 1;
                nNumber = (int)lockScript.pokerSuit * 13 - nNumber;
                nRet = ((int)lockScript.pokerSuit - 1) * 13 + nNumber;
            }

            return nRet;
        }

        //color only
        if(lockScript.pokerColor != PokerColor.None)
        {
            int nRandom = Random.Range(1, 100);
            nRandom %= 2;

            int nRandomNumber = Random.Range(1, 13);

            nRet = ((int)lockScript.pokerColor - 1) * 13 + nRandom * 26 + nRandomNumber;

            if (!cardsArray2nd.Exists(t => t == nRet))
            {
                nRandom = (nRandom + 1) % 2;

                int nDelta = ((int)Time.time + nIndexDelta) % 4;
                nRandomNumber = (nRandomNumber + nDelatIndexs[nDelta]) % 13 + 1;
                //nRandomNumber = (((int)Time.time % 3) == 0) ? nRandomNumber : (13 - nRandomNumber);
                nRet = ((int)lockScript.pokerColor - 1) * 13 + nRandom * 26 + nRandomNumber;

                nIndexDelta++;
            }

            return nRet;
        }

        return 0;
    }

    void InsertToUnlockPokerArray(int pokerIndex, int groupID)
    {
        bool bFind = false;

        for(int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            if(unlockAreaPokerIDs[i].IDs.Exists(t => t == pokerIndex))
            {
                bFind = true;
                break;
            }
        }

        if(bFind)
        {
            return;
        }

        for(int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            if(unlockAreaPokerIDs[i].nGroupID == groupID)
            {
                unlockAreaPokerIDs[i].IDs.Add(pokerIndex);
                break;
            }
        }
    }

    void Test_DisableAllFoldPokerText()
    {
        foreach(GameObject go in foldPoker)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();
            if(gamePoker)
            {
                gamePoker.Test_DisableDisplayText();
            }
            else
            {
                HandPoker handPoker = go.GetComponent<HandPoker>();
                if(handPoker)
                {
                    handPoker.Test_DisableDisplayText();
                }
            }
        }
    }

    void Test_EnableTopFoldPokerText()
    {
        GameObject go = foldPoker.Peek();

        GamePoker gamePoker = go.GetComponent<GamePoker>();
        if (gamePoker)
        {
            gamePoker.Test_EnableDisplayText();
        }
        else
        {
            HandPoker handPoker = go.GetComponent<HandPoker>();
            if (handPoker)
            {
                handPoker.Test_EnableDisplayText();
            }
        }
    }

    bool CanFoldGamePoker(GamePoker gamePoker, GameObject handPoker)
    {
        if(TopFoldPokerIsWildcard(handPoker))
        {
            return true;
        }

        int nGamePokerIndex = gamePoker.nPokerNumber;
        int nHandPokerIndex = -1;

        //
        HandPoker handPokerScript = handPoker.GetComponent<HandPoker>();
        if(handPokerScript != null)
        {
            nHandPokerIndex = handPokerScript.nPokerNumber;
        }
        else
        {
            GamePoker gamePokerScript = handPoker.GetComponent<GamePoker>();
            if(gamePokerScript != null)
            {
                nHandPokerIndex = gamePokerScript.nPokerNumber;
            }
        }

        if((nGamePokerIndex == 1 && nHandPokerIndex == 13) 
            || (nGamePokerIndex == 13 && nHandPokerIndex == 1))
        {
            return true;
        }
        else
        {
            if (Mathf.Abs(nGamePokerIndex - nHandPokerIndex) == 1)
            {
                return true;
            }
        }

        return false;
    }

    bool CanClickTopHandPoker()
    {
        if(foldPoker.Count == 0)
        {
            return true;
        }

        GameObject topFold = foldPoker.Peek();

        if (TopFoldPokerIsWildcard(topFold))
        {
            return false;
        }

        return true;
    }

    bool TopFoldPokerIsWildcard(GameObject handPoker)
    {
        if(handPoker.tag == "wildCard")
        {
            return true;
        }

        return false;
    }

    int ComputeOperationGold(bool bWithdraw = false)
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel];
        PublicConfig publicConfig = publicConfigs[0];

        float x = stageConfig.CardCoin / 1000.0f;

        string strCardCoinFunctionParam = publicConfig.CardCoinFunctionParm;
        string[] strParams = strCardCoinFunctionParam.Split('_');
        float a = float.Parse(strParams[0]);
        float b = float.Parse(strParams[1]);
        float c = float.Parse(strParams[2]);

        double dIntern = Mathf.Log( b * nStreakCount + c );

        return Mathf.RoundToInt((float)(a * x * dIntern));
    }

    int ComputeOperationScore(int nCount, bool bWithdraw = false)
    {
        if(nCount == 0)
        {
            return 0;
        }

        StageConfig stageConfig = stageConfigs[nCurrentLevel];

        string strScoreConfig = stageConfig.CardScore;
        string[] strParams = strScoreConfig.Split('_');

        int nBase = int.Parse(strParams[0]);
        int nDiff = int.Parse(strParams[1]);

        int nScore = nBase + (nDiff * (nCount - 1));

        Debug.Log("ComputeOperationScore base is: " + nBase + "  Diff is: " + nDiff);
        Debug.Log("the result score is: " + nScore);

        return nScore;
    }

    public void Test_ComputeStars(int nScore, ref float[] fillAmounts)
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel];

        string strStarConfig = stageConfig.Star;
        string[] strParams = strStarConfig.Split('_');

        int oneStar = int.Parse(strParams[0]);
        int twoStar = int.Parse(strParams[1]);
        int threeStar = int.Parse(strParams[2]);

        if(nScore > threeStar)
        {
            fillAmounts[2] = 1.0f;
            fillAmounts[1] = 1.0f;
            fillAmounts[0] = 1.0f;
        }
        else if(nScore > twoStar)
        {
            fillAmounts[2] = (nScore - twoStar) / (float)(threeStar - twoStar); 
            fillAmounts[1] = 1.0f;
            fillAmounts[0] = 1.0f;
        }
        else if(nScore > oneStar)
        {
            fillAmounts[2] = 0.0f;
            fillAmounts[1] = (nScore - oneStar) / (float)(twoStar - oneStar);
            fillAmounts[0] = 1.0f;
        }
        else
        {
            fillAmounts[2] = 0.0f;
            fillAmounts[1] = 0.0f;
            fillAmounts[0] = nScore / (float)oneStar;
        }
    }

    void InitStreakConfig()
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel];

        string strStreakConfig = stageConfig.StreakType;
        string strBonuses = stageConfig.StreakBonus;
        string strBonusScores = stageConfig.StreakBonusScore;

        string[] strParams = strStreakConfig.Split('_');
        string[] strBonusParams = strBonuses.Split('_');
        string[] strBonusScoreParams = strBonusScores.Split('_');

        nStreakIDs = new int[strParams.Length];
        nStreakBonusAmounts = new int[strBonusParams.Length];
        nStreakBonusScore = new int[Max_StreakBonus_Type];

        for (int i = 0; i < strParams.Length; ++i)
        {
            nStreakIDs[i] = int.Parse(strParams[i]);
            nStreakBonusAmounts[i] = int.Parse(strBonusParams[i]);
        }

        for(int i = 0; i < Max_StreakBonus_Type; ++i)
        {
            nStreakBonusScore[i] = int.Parse(strBonusScoreParams[i]);
        }
    }

    int GetStreakBonusGold()
    {
        if(nCurrentStreakIndex >= (nStreakBonusAmounts.Length - 1))
        {
            return nStreakBonusAmounts[nStreakBonusAmounts.Length - 1];
        }

        return nStreakBonusAmounts[nCurrentStreakIndex];
    }

    int GetStreakBonusScore(StreakType streak)
    {
        return nStreakBonusScore[(int)streak - 1];
    }

    public int GetStreakFinishCount(StreakType type)
    {
        PublicConfig publicConfig = publicConfigs[0];
        string strStreakBonus = publicConfig.StreakBonus;

        string[] strStreakBonusFinishCount = strStreakBonus.Split('_');
        int[] nStreakFinsihCounts = new int[3];

        nStreakFinsihCounts[0] = int.Parse(strStreakBonusFinishCount[1]);
        nStreakFinsihCounts[1] = int.Parse(strStreakBonusFinishCount[3]);
        nStreakFinsihCounts[2] = int.Parse(strStreakBonusFinishCount[5]);

        return nStreakFinsihCounts[(int)type - 1];
    }

    void CheckGameEnd()
    {
        if(publicPokers.Count == 0)
        {
            Debug.Log("Contratulations!  you win the game!!!");

            gameResult = GameResult.GameResult_Win;

            WinGame();
        }
    }

    void WinGame()
    {
        gameStatus = GameStatus.GameStatus_Settle;

        gameplayUI.HideAdd5Btn();
        gameplayUI.HideEndGameBtn();
        gameplayUI.DisableAllGameButton();

        StartCoroutine(SettleAllHandPokers());

    }

    public void LoseGame()
    {
        gameResult = GameResult.GameResult_Loss;

        gameStatus = GameStatus.GameStatus_Settle;

        gameplayUI.LoseGame(nCollectGold, 0);

        EndGame();
    }

    IEnumerator SettleAllHandPokers()
    {
        //Debug.Log("enter SettleAllHandPokers ... ...");
        int nIndex = 0;
        int nPokerOrder = 1;

        while(handPokers.Count > 0)
        {
            nIndex = handPokers.Count - 1;
            //Debug.Log("1 SettleAllHandPokers ... ...  " + nIndex);
            GameObject go = handPokers[nIndex];
            
            //HandPoker handPokerScript = go.GetComponent<HandPoker>();
            go.transform.position = go.transform.position + Vector3.up * 2;
            nIndex++;

            //add gold and score for the rest hand pokers
            int nGold = stageConfigs[nCurrentLevel].RestCardCoin * nPokerOrder;
            if( nPokerOrder > 1)
            {
                nGold = (int)Mathf.RoundToInt(nGold / 10.0f) * 10;
            }
            if(go.tag == "wildcard")
            {
                nGold *= 2;
            }

            nCollectGold += nGold;
            gameplayUI.goldAndScoreUI.AddGold(nGold);

            int nScore = stageConfigs[nCurrentLevel].EndCardScore * nPokerOrder;
            if (go.tag == "wildcard")
            {
                nScore *= 2;
            }
            nCollectScore += nScore;
            gameplayUI.goldAndScoreUI.AddScore(nScore);

            //star
            /*Image scoreStar = ((GameObject)Instantiate(scoreStarPrefab, transform)).GetComponent<Image>();
            scoreStar.transform.position = new Vector3(0f, -2f, -2f);
            Hashtable args = new Hashtable();
            args.Add("time", 1.8f);
            args.Add("speed", 0.2f);
            args.Add("position", new Vector3(-2f, 4f, -2f));
            iTween.MoveTo(scoreStar.gameObject, args);
            
            Object.Destroy(scoreStar, 2.0f);*/

            nPokerOrder++;

            yield return new WaitForSeconds(1.0f);

            handPokers.Remove(go);
            Destroy(go);
        }

        string strStageClearBonus = publicConfigs[0].StageClearBonus;
        string[] strParams = strStageClearBonus.Split('_');

        Random.InitState((int)System.DateTime.Now.Ticks);
        Debug.Log(System.DateTime.Now.Second);

        float fX = float.Parse(strParams[0]);
        float fY = float.Parse(strParams[1]);
        float fZ = float.Parse(strParams[2]);

        int nMinimalGold = (int)(stageConfigs[nCurrentLevel].Cost * fX * 0.0001f);
        float a = Random.Range(fY, fZ) * 0.0001f;
        nClearGold =(int)(stageConfigs[nCurrentLevel].Cost * a - nCollectGold );
        Debug.Log("a is: " + a + "  collect gold is: " + nCollectGold + "  clear gold is: " + nClearGold);
        if (nClearGold < nMinimalGold)
            nClearGold = nMinimalGold;

        Debug.Log("collect gold is: " + nCollectGold + "  clear gold is: " + nClearGold);

        gameplayUI.goldAndScoreUI.SetGold(nCollectGold + nClearGold);
        gameplayUI.WinGame(nCollectGold, nClearGold);

        //gameStatus = GameStatus.GameStatus_After_Settle;
        EndGame();

        StopCoroutine(SettleAllHandPokers());
       
    }
}
