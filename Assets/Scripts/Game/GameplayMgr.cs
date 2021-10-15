using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.IO;
using DG.Tweening;

public class GameplayMgr : MonoBehaviour
{
    public enum GameStatus : int
    {
        GameStatus_Before = 1,
        GameStatus_Gaming,
        GameStatus_WaitForSettle, //2021.9.2 added by pengyuan, bomb and lock fail, wait for end.
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

    public static int Invalid_Poker_Digit = -1;
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
        StreakBonus,
        AddNPoker,
        ClearAll
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
        public string strClearAllStreakInfo;  //2021.9.17 for clear all info, the format is: count_SteakType1_StreakValueEncode1_Bonus11_Bonus12_SteakType2_......
        public string strClearAllPokerInfo;   //2021.9.17 for clear all public poker info, the format is: Index1_Index2_...
        public string strClearAllLockInfos;   //2021.9.22 for clear all PowerUP, record all the locks and lockarea ids that been cleared, the format is: Count_GroupID1_GroupID2_...
    }

    public class StreakBonusInfo
    {
        public int StreakType;
        public int SteakValueEncode;
        public int[] Bonus = new int[2];
        //public int Bonus2;
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

    //this class is used to store all the ascend & descend poker that has been assigned suit and number;
    //like the class UnlockAreaPokerIDs
    public class AscDesPokerIDs
    {
        public List<int> IDs = new List<int>();
        public List<int> pokerNumbers = new List<int>();
    }

    public int nGold = 0;
    public int nScore = 0;

    public GameObject pokerPrefab;
    public GameObject pokerCard;

    public GameObject wildcardPrefab;    //2021.8.2 added by pengyuan for wild card
    //public GameSceneBG gameSceneBG;

    public GameObject scoreStarPrefab;

    int ScoreStar = 1;

    public GameObject lockPrefab;    //2021.8.17 added by pengyuan for lock in the game

    public GameObject lockAreaPrefab; //2021.8.18 added by pengyuan for lock area in the game

    public Sprite lockTopSprite;
    public Sprite lockBottomSprite;
    //public SpriteAtlas lockSprites;
    public Sprite[] lockSprites;

    //pengyuan 2021.8.31 added for ascending and descending poker
    public GameObject ascendingPrefab;
    public GameObject descendingPrefab;
    public GameObject bombPrefab;
    public Sprite[] bombNumbers;

    public GameObject addNPrefab;
    public Sprite[] addNNumbers;

    public GameObject[] removeThreePrefabs;
    List<GameObject> removeThreePokers = new List<GameObject>();
    public int removeThreeCurrentIndex { get; set; } = 0;
    public int nRemoveThreeLeftCount = 3;
    public int nRemoveThreeFinishCount = 0;
    public bool bIsRemovingThree = false;    //here we are removing one or more cards, and haven't finished yet, so we should wait it to finish.

    //use this to store all the poker.
    JsonReadWriteTest.LevelData levelData = new JsonReadWriteTest.LevelData();

    //store all the pokers in the game area
    List<GameObject> publicPokers = new List<GameObject>();

    //store all the pokers in the hand
    List<GameObject> handPokers = new List<GameObject>();

    //store all the  pokers that has been folded
    Stack<GameObject> foldPoker = new Stack<GameObject>();     //the pokers being folded 


    //the folling is for storing information about locks
    List<LockGroup> lockGroups = new List<LockGroup>();    //2021.8.17 added by pengyuan, to store all the lock information for the locks in one game.

    Dictionary<string, GameObject> lockGroupDict1 = new Dictionary<string, GameObject>();   //one condition to clear this lock
    Dictionary<string, GameObject> lockGroupDict2 = new Dictionary<string, GameObject>();   //two conditions to clear this lock

    List<LockAreaInfo> lockAreas = new List<LockAreaInfo>();       //2021.8.18 added by pengyuan, 

    List<UnlockAreaPokerIDs> unlockAreaPokerIDs = new List<UnlockAreaPokerIDs>();

    List<GameObject> wildDropPokers = new List<GameObject>();

    //store all the ascend and descend poker id and number, that has been assigned suit and number.
    AscDesPokerIDs ascDesPokerIDs = new AscDesPokerIDs();

    GameObject currentFlippingPoker = null;    //store the current flipping card

    float fGameTime;

    public bool bAutoFlipHandPoker = false;

    public bool bFirstCheckPowerUPs = false;

    //2021.9.10 added by pengyuan, to record whether once powerups has been used.
    bool bHasUsedOncePowerUPs = false;
    bool bHasFinishedOncePowerUPs = false;

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
    bool bHasSetToGaming = false;

    int nCurrentChapter { get; set; } = 0;
    int nCurrentLevel { get; set; } = 1;

    //pengyuan 2021.8.13 temporarily store poker's texture and back
    public Texture2D textureBack;
    public  Texture2D[] pokerTexture = new Texture2D[52];
    public Texture2D pokerAtlas;// = new Texture2D(8192, 256);
    public Rect[] pokerRects = new Rect[52];
    public Texture2D ascendTexture;
    public Texture2D addNTexture;
    public Texture2D[] clearAllTexture = new Texture2D[2];
    public Texture2D wildCardTexture;

    //the following is about some ui
    public GamePlayUI gameplayUI;

    public PowerUPProcess powerUpProcess = new PowerUPProcess();

    int[] nItemCounts = new int[(int)GameDefines.PowerUPType.Max];
    int nSingleClearScore = 0;

    //2021.9.18 added by pengyuan, to record withdraw and add5_poker buy times.
    int nWithdrawBuyTimes = 0;
    int nAdd5BuyTimes = 0;

    public GameObject FXScoreStar;
    public GameObject FXCoin;
    public GameObject FXCardStar;

    public GameObject ParticleTest;

    public Material oneSideMaterial;
    
    public GameObject getCoinEffect;

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
        public int PowerUpScore = 0;
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

    public ParticleSystemForceField[] forceFields;
    public GameObject starDieBox;

    //pengyuan 2021.8.27 added for test
    //public int nWithdrawCount { get; set; } = 0;

    private void Awake()
    {
        Instance = this;

        //Screen.SetResolution(1920, 1080, false);

        Debug.Log("GameplayMgr::Awake()... ");

        pokerPrefab = (GameObject)Resources.Load("Poker/Poker_Club_10"); 
        if (pokerPrefab == null)
            Debug.Log("GameplayMgr::Awake()... PokerTest is null....");

        pokerCard = (GameObject)Resources.Load("Poker/Poker_Card");
        if (pokerCard == null)
            Debug.Log("GameplayMgr::Awake()... Poker_Card is null....");

        wildcardPrefab = (GameObject)Resources.Load("Poker/Wild_Card");
        //wildcardPrefab = (GameObject)Resources.Load("Poker/WildCard");
        // wildcardPrefab = (GameObject)Resources.Load("Poker_Club_10");
        if (wildcardPrefab == null)
            Debug.Log("GameplayMgr::Awake()... WildCard is null....");

        scoreStarPrefab = (GameObject)Resources.Load("UI/ScoreStar");
        if (scoreStarPrefab == null)
            Debug.Log("GameplayMgr::Awake()... ScoreStar is null....");

        lockPrefab = (GameObject)Resources.Load("Lock/Lock");
        if(lockPrefab == null)
            Debug.Log("GameplayMgr::Awake()... Lock is null....");

        lockAreaPrefab = (GameObject)Resources.Load("Lock/LockArea");
        if(lockAreaPrefab == null)
            Debug.Log("GameplayMgr::Awake()... LockArea is null....");

        lockTopSprite = Resources.Load<Sprite>("Lock/LockTop");
        lockBottomSprite = Resources.Load<Sprite>("Lock/LockBottom");

        lockSprites = Resources.LoadAll<Sprite>("Lock/LockMark");
        Debug.Log("the lock sprites length is: " + lockSprites.Length);

        ascendingPrefab = (GameObject)Resources.Load("Poker/FXChangeUp");
        if (ascendingPrefab == null)
            Debug.Log("GameplayMgr::Awake()... ChangeUp is null....");

        descendingPrefab = (GameObject)Resources.Load("Poker/FXChangeDown");
        if (descendingPrefab == null)
            Debug.Log("GameplayMgr::Awake()... ChangeDown is null....");

        bombPrefab = (GameObject)Resources.Load("Poker/FXPropBomb");
        if (bombPrefab == null)
            Debug.Log("GameplayMgr::Awake()... Bomb is null....");

        bombNumbers = Resources.LoadAll<Sprite>("UI/BombNumber");
        Debug.Log("the bomb number sprites length is: " + bombNumbers.Length);

        addNPrefab = (GameObject)Resources.Load("Poker/FXItemPlus");
        if (addNPrefab == null)
            Debug.Log("GameplayMgr::Awake()... FXItemPlus is null....");

        addNNumbers = Resources.LoadAll<Sprite>("UI/ItemPlusNumber");
        if (addNNumbers == null)
            Debug.Log("GameplayMgr::Awake()... ItemPlusNumber is null....");

        addNTexture = Resources.Load("Poker/ItemPlusFront") as Texture2D;
        if (addNTexture == null)
            Debug.Log("GameplayMgr::Awake()... addNTexture is null....");
        
        clearAllTexture[0] = Resources.Load("Poker/ItemClearBack") as Texture2D;
        if (clearAllTexture[0] == null)
            Debug.Log("GameplayMgr::Awake()... ItemClear is null....");

        clearAllTexture[1] = Resources.Load("Poker/ItemClear") as Texture2D;
        if (clearAllTexture[1] == null)
            Debug.Log("GameplayMgr::Awake()... ItemClearBack is null....");

        wildCardTexture = Resources.Load("Poker/ItemWild") as Texture2D;
        if (wildCardTexture == null)
            Debug.Log("GameplayMgr::Awake()... ItemWild is null....");

        removeThreePrefabs = new GameObject[4];
        removeThreePrefabs[0] = (GameObject)Resources.Load("Poker/FXCardCutLeftBlack");
        if (removeThreePrefabs[0] == null)
            Debug.Log("GameplayMgr::Awake()... FXCardCutLeftBlack is null....");

        removeThreePrefabs[1] = (GameObject)Resources.Load("Poker/FXCardCutLeftRed");
        if (removeThreePrefabs[1] == null)
            Debug.Log("GameplayMgr::Awake()... FXCardCutLeftRed is null....");

        removeThreePrefabs[2] = (GameObject)Resources.Load("Poker/FXCardCutRightBlack");
        if (removeThreePrefabs[2] == null)
            Debug.Log("GameplayMgr::Awake()... FXCardCutRightBlack is null....");

        removeThreePrefabs[3] = (GameObject)Resources.Load("Poker/FXCardCutRightRed");
        if (removeThreePrefabs[3] == null)
            Debug.Log("GameplayMgr::Awake()... FXCardCutRightRed is null....");

        //the following three ara used for streak bonus effects.
        FXScoreStar = (GameObject)Resources.Load("UI/FXScoreStar");
        if (FXScoreStar == null)
            Debug.Log("GameplayMgr::Awake()... FXScoreStar is null....");

        FXCoin = (GameObject)Resources.Load("UI/FXCoin");
        if (FXCoin == null)
            Debug.Log("GameplayMgr::Awake()... FXCoin is null....");

        FXCardStar = (GameObject)Resources.Load("UI/FXCardStar");
        if (FXCardStar == null)
            Debug.Log("GameplayMgr::Awake()... FXCardStar is null....");

        ParticleTest = (GameObject)Resources.Load("Crops/HarvestParticles/FXHarvestCabbage");
        if (ParticleTest == null)
            Debug.Log("GameplayMgr::Awake()... ParticleTest is null....");
        
        oneSideMaterial = Resources.Load("Materials/OneSide") as Material;
        if (oneSideMaterial == null)
            Debug.Log("GameplayMgr::Awake()... oneSideMaterial is null....");

        getCoinEffect = (GameObject)Resources.Load("Prefabs/GetCoinEffect");
        if (getCoinEffect == null)
            Debug.Log("GameplayMgr::Awake()... getCoinEffect is null....");

        /*oneSideMesh = (Mesh)Resources.Load("Poker/CardMesh1");
        GameObject cardMesh1 = (GameObject)Resources.Load("Poker/CardMesh1");
        if (oneSideMesh == null)
            Debug.Log("GameplayMgr::Awake()... oneSideMesh is null....");

        Debug.Log(cardMesh1.GetType());*/

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

        /*ascendTexture = Resources.Load("Poker/ChangeArrow") as Texture2D;
        if (ascendTexture == null)
            Debug.Log("---------------------- init ascend texture error!!! please check your code! -------------" + "Poker / ChangeArrow");*/

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

        //2021.9.10 added by pengyuan, to process powerups.
        powerUpProcess.Init();
        nRemoveThreeLeftCount = 3;
        nRemoveThreeFinishCount = 0;
        bIsRemovingThree = false;

        wildDropPokers.Clear();

        InitConfigInformation();

        nWithdrawBuyTimes = 0;
        nAdd5BuyTimes = 0;

        Trans = GetComponent<Transform>();
        if (Trans == null)
            Debug.Log("GameplayMgr::Start()... rectTrans is null....");

        renderer = GetComponent<MeshRenderer>();
        //Debug.Log("GameplayMgr::Start()... bounds is: " + renderer.bounds.size.y);

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
        gameplayUI.DisableWildCardBtn();

        nGold = 0;
        nScore = 0;

        ScoreStar = 1;

        //2021.8.6 added by pengyaun for display gold and score in game playing.
        //gameplayUI.goldAndScoreUI.nGold = 0;
        //gameplayUI.goldAndScoreUI.nGold = Reward.Coin;
        gameplayUI.goldAndScoreUI.SetGold(Reward.Coin);
        gameplayUI.goldAndScoreUI.nScore = 0;

        //Debug.Log("GameplayMgr::Start()... nGold is: " + Reward.Coin);
    }

    void InitConfigInformation()
    {
        //todo: here we should change this call to some '...getLevelDataFrom...' call, to get the leve data from some place
        nCurrentChapter = 1;
        ReadLevelData(nCurrentChapter);

        InitStreakConfig();

        InitItemCount();
    }

    public void RestartGame()
    {
        EndGame();

        wildDropPokers.Clear();

        //2021.9.10 added by pengyuan, to process powerups.
        powerUpProcess.Reset();
        nRemoveThreeLeftCount = 3;
        nRemoveThreeFinishCount = 0;
        bIsRemovingThree = false;


        InitConfigInformation();
        
        nWithdrawBuyTimes = 0;
        nAdd5BuyTimes = 0;

        Test_Shuffle();

        Test_Shuffle_StreakPoker();

        InitLockAreaInfo();//lock area should before the lock init

        InitLockInfo();

        InitPoker();

        InitHandPokerInfo();

        fGameTime = 0.0f;
        gameStatus = GameStatus.GameStatus_Before;
        bHasSetToGaming = false;
        bAutoFlipHandPoker = false;
        bHasUsedOncePowerUPs = false;
        bHasFinishedOncePowerUPs = false;

        streakType = GetCurrentStreakType();
        gameplayUI.InitStreakBonus(streakType, GetNextStreakType());

        //2021.8.6 added by pengyaun for display gold and score in game playing.
        //gameplayUI.goldAndScoreUI.nGold = 0;
        //gameplayUI.goldAndScoreUI.nGold = Reward.Coin;

        ScoreStar = 1;

        gameplayUI.goldAndScoreUI.SetGold(Reward.Coin);
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

        ascDesPokerIDs.IDs.Clear();
        ascDesPokerIDs.pokerNumbers.Clear();

        currentFlippingPoker = null;
        bAutoFlipHandPoker = false;
        bHasUsedOncePowerUPs = false;
        bHasFinishedOncePowerUPs = false;

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

        GeneratePokerForAscendDescendPoker();

        //2021.8.20 added by pengyuan, to pre-generate the pokers in unlock area, so that we can unlock the locks.
        GeneratePokerForUnlockArea();

        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);

        int i = 0;
        foreach (JsonReadWriteTest.PokerInfo info in levelData.pokerInfo)
        {
            //GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            GameObject go = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);

            go.tag = "poker";

            //Debug.Log(go.GetComponent<MeshFilter>().mesh.bounds.size);

            Vector3 targetPos;
            GetTargetPosition(0, out targetPos);
            //testDir = Vector3.Normalize(targetPos - testModel.transform.position) * 3.0f;
            testT = 0.0f;

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            pokerScript.Init(pokerCard, levelData.pokerInfo[i], i, Trans.position, renderer.bounds.size, 0.0f);

            int nGroupID;
            int nGroupIndex;

            PokerSuit suit = PokerSuit.Suit_None;
            int nNumber = 0 ;

            if (IsInUnlockArea(i, out nGroupID, out nGroupIndex) 
                && levelData.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.Ascending_Poker 
                && levelData.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.Descending_Poker
                && levelData.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.Add_N_Poker)
            {
                int nResult = Test_GetPreAllocatedPoker(i, nGroupID, nGroupIndex, out suit, out nNumber);

                if (nResult == 1)
                {
                    Test_GetNextPoker(ref suit, ref nNumber);
                }
                
            }
            else
            {
                if(levelData.pokerInfo[i].nItemType == (int)GameDefines.PokerItemType.Ascending_Poker
                   || levelData.pokerInfo[i].nItemType == (int)GameDefines.PokerItemType.Descending_Poker)
                {
                    bool bFind = false;
                    
                    for (int j = 0; j < ascDesPokerIDs.IDs.Count; ++j)
                    {
                        if(ascDesPokerIDs.IDs[j] == i)
                        {
                            suit = (PokerSuit)(ascDesPokerIDs.pokerNumbers[j] / 13 + 1); ;
                            nNumber = ascDesPokerIDs.pokerNumbers[j] % 13;
                            //Test_GetPreallocateAscendDescendPoker(j, ref suit, ref nNumber);
                            bFind = true;
                            break;
                        }
                        
                    }

                    if(!bFind)
                    {
                        Test_GetNextPoker(ref suit, ref nNumber);
                    }
                }
                else
                {
                    if (levelData.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.Add_N_Poker)
                    {
                        //test code, set suit and number for display
                        Test_GetNextPoker(ref suit, ref nNumber);
                    }
                }
            }

            if(levelData.pokerInfo[i].nItemType == (int)GameDefines.PokerItemType.Add_N_Poker)
            {
                pokerScript.Test_SetSuitNumberAddNPoker();
            }
            else
            {
                pokerScript.Test_SetSuitNumber(suit, nNumber);
            }
            
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

            Debug.Log("the poker's render bounds is: " + pokerScript.pokerInst.GetComponent<Renderer>().bounds.size);
        }
    }

    void InitHandPokerInfo()
    {
        Debug.Log("GameplayMgr::InitHandPokerInfo... count is : " + levelData.handPokerCount);

        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);

        for (int i = 0; i < levelData.handPokerCount; ++i)
        {
            //GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            GameObject go = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            go.tag = "handCard";

            HandPoker handScript = go.GetComponent<HandPoker>();
            handScript.Init(pokerCard, levelData, i, Trans.position + posOffset, renderer.bounds.size, 0.0f);

            //test code, set suit and number for display
            PokerSuit suit = PokerSuit.Suit_None;
            int nNumber = 0;
            Test_GetNextPoker(ref suit, ref nNumber);
            handScript.Test_SetSuitNumber(suit, nNumber);

            GamePoker pokerScript = go.GetComponent<GamePoker>();
            Destroy(pokerScript);

            //test code
            string strName = string.Format("hand_{0}", i);
            pokerScript.Test_SetName(strName);
            go.name = strName;

            handPokers.Add(go);

        }

        Debug.Log("GameplayMgr::InitHandPokerInfo... ... end... ... ");
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
            go.GetComponent<SpriteRenderer>().sortingLayerName = "Environment";
            go.GetComponent<SpriteRenderer>().sortingOrder = 10;

            lockArea.Area = go;
            //lockArea.Area.gameObject.layer = 5;

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
            tempLockList[i].transform.position = lockScript2.originPos;
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

    int HasActiveCompoundLock()
    {
        foreach (var item in lockGroupDict2)
        {
            GameObject go = item.Value;
            GameLock lockScript = go.GetComponent<GameLock>();

            if (lockScript.lockState == LockState.LockState_Working
                && lockScript.HasTwoClearConditions())
            {
                return lockScript.nGroupID;
            }
        }

        return -1;
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

    int HasActiveSingleLock()
    {
        foreach (var item in lockGroupDict1)
        {
            GameObject go = item.Value;
            if (go == null)
                continue;

            GameLock lockScript = go.GetComponent<GameLock>();

            if (lockScript.lockState == LockState.LockState_Working
                && lockScript.HasOneClearConditions())
            {
                return lockScript.nGroupID;
            }
        }

        return -1;
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

    void WithdrawClearAllPokers(OpStepInfo stepInfo)
    {
        string[] strParams = stepInfo.strClearAllPokerInfo.Split('_');

        for(int i = 1; i < strParams.Length; ++i)
        {
            int nPokerIndex = int.Parse(strParams[i]);

            GameObject gamePoker = GetPublicPokerByIndex(nPokerIndex);

            if(gamePoker != null)
            {
                GamePoker gamePokerScript = gamePoker.GetComponent<GamePoker>();
                gamePokerScript.Withdraw();

                //AddToPublicPoker(gamePoker);

                UnflipAllCoveredGamePoker(gamePoker);
            }
        }
        
    }

    void WithdrawClearAllStreakBonus(OpStepInfo stepInfo)
    {
        List<StreakBonusInfo> bonusInfos = new List<StreakBonusInfo>();

        string[] strParams = stepInfo.strClearAllStreakInfo.Split('_');

        int nCount = int.Parse(strParams[0]);
        for(int i = 0; i < nCount; ++i)
        {
            StreakBonusInfo info = new StreakBonusInfo();

            info.StreakType = int.Parse(strParams[1 + i * 4]);
            info.SteakValueEncode = int.Parse(strParams[2 + i * 4]);
            info.Bonus[0] = int.Parse(strParams[3 + i * 4]);
            info.Bonus[1] = int.Parse(strParams[4 + i * 4]);

            bonusInfos.Add(info);
        }

        gameplayUI.WithDrawClearAllStreakBonus(bonusInfos);

        //todo: 这里需要撤回相应的每个奖励数据
        for(int i = bonusInfos.Count - 1; i >= 0; --i)
        {
            StreakBonusInfo info = bonusInfos[i];

            switch ((StreakType)info.StreakType)
            {
                case StreakType.Add_Gold:
                    break;
                case StreakType.Add_Poker:
                    for(int j = 0; j < 2; ++j)
                    {
                        if(info.Bonus[j] != 0)
                        {
                            string strCardName = string.Format("handpoker_bonus_{0}", info.Bonus[j]);
                            GameObject insertPoker = handPokers.Find(target => target.name.Equals(strCardName));

                            if (insertPoker != null)
                            {
                                WithdrawHandCardFromHand(insertPoker);
                            }
                        }
                    }
                    break;
                case StreakType.Add_Wildcard:
                    for(int j = 0; j < 2; ++j)
                    {
                        if(info.Bonus[j] != 0)
                        {
                            string strCardName = string.Format("wildcard_bonus_{0}", info.Bonus[j]);
                            GameObject insertPoker = handPokers.Find(target => target.name.Equals(strCardName));

                            if (insertPoker != null)
                            {
                                WithdrawWildCardFromHand(insertPoker);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            nCurrentStreakIndex--;
        }
    }

    void WithdrawClearAllLockInfos(OpStepInfo stepInfo)
    {
        string[] strParams = stepInfo.strClearAllLockInfos.Split('_');

        int nCount = int.Parse(strParams[0]);

        Debug.Log("WithdrawClearAllLockInfos... ... ... the lock group info is: " + stepInfo.strClearAllLockInfos);

        for(int i = 0; i < nCount; ++i)
        {
            int nGroupID = int.Parse(strParams[i+1]);
            Cancel_ClearAll_ClearLockGroup(nGroupID);
            Cancel_ClearAll_ClearLockArea(nGroupID);
        }
    }


    void ReadLevelData(int nChapter)
    {
        //string strLevel = string.Format("/Resources/Configs/LevelInfo/Chapter_{0:D4}/", nChapter);
        string strLevel = string.Format("Configs/LevelInfo/Chapter_{0:D4}/", nChapter);

        //todo: this level should come from a call from map ...
        int nTempLevel = Random.Range(1, 6);
        nTempLevel = 9;// STAGameManager.Instance.nLevelID;
        nCurrentLevel = nTempLevel;
        JsonReadWriteTest.Test_ReadLevelData(strLevel, 1, nTempLevel, out levelData);

        Debug.Log("GameplayMgr::ReadLevelData... card count is: " + levelData.pokerInfo.Count);
    }

    public void Test_InitPoker()
    {
        Debug.Log("GameplayMgr::Test_InitPoker()... ");

        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);
        Debug.Log("test model position is: " + (Trans.position + posOffset));

        testModel = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
        testModel.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        testModel.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        testModel.tag = "poker";

        Vector3 targetPos;
        GetTargetPosition(0, out targetPos);
        testDir = Vector3.Normalize( targetPos - testModel.transform.position ) * 3.0f;
        testT = 0.0f;

        for (int i=1; i<=0; ++i)
        {
            GameObject go = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
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
            Vector3 clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           
            //Debug.Log("GameplayMgr::Update... we have to compute the hit results " + Input.mousePosition);

            RaycastHit2D[] hitResults = Physics2D.RaycastAll(clickWorldPos, Vector2.zero);

            GameObject topHandPoker = GetTopHandPoker();

            int nAddingNPokerIndex = HasPokerIsAddN();

            if (hitResults.Length > 0 && !IsClickingLockArea(hitResults) && nAddingNPokerIndex < 0)
            {
                //Debug.Log("@@@@@@@@@@@@@@@ here we get a ray, the hitResult length is: " + hitResults.Length);

                if (topHandPoker != null && IsClickHandPoker(hitResults) )
                {
                    if (IsTopHandPokerClicked(topHandPoker, hitResults))
                    {
                        OnTopHandPokerClicked(topHandPoker, false);
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

            if(nAddingNPokerIndex > 0)
            {
                SetAddingNAccelerating(nAddingNPokerIndex);
            }
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 50, Color.red);
        }
    }

    public Vector3 GetFoldPokerPosition()
    {
        Vector3 pos = new Vector3();

        pos.x = renderer.bounds.size.x * 0.1f;
        pos.y = Trans.position.y + renderer.bounds.size.y * -0.35f;
        if(foldPoker.Count == 0)
            pos.z = -0.05f;
        else
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

    //when click withdraw button, we call this method.
    public void WithDrawClicked()
    {
        if(opStepInfos.Count == 0)
        {
            return;
        }

        if (foldPoker.Count < 2)
        {
            Debug.Log("WithDrawClicked, but nothing to do!");
            return;
        }

        if(HasPokerIsAddN() > 0)
        {
            return;
        }

        int nBuyCost = GetWithdrawCost();
        if (Reward.Coin < nBuyCost)
        {
            Debug.Log("WithDrawClicked, but money is not enough!");
            return;
        }
            
        nWithdrawBuyTimes++;
        gameplayUI.AddGold(-nBuyCost);
        Reward.Coin -= nBuyCost;

        GameObject topHandPoker = foldPoker.Peek();
        HandPoker handPokerScript = topHandPoker.GetComponent<HandPoker>();

        //Debug.Log("-------------  here the top hand poker source is: " + handPokerScript.handPokerSource);

        while(opStepInfos.Peek().strCardName == "handpoker_add_n" 
            && (( handPokerScript != null && handPokerScript.handPokerSource != HandPokerSource.AddNPoker) 
               || handPokerScript == null) )
        {
            OpStepInfo stepInfo = opStepInfos.Peek();
            Debug.Log("-------------  here we withdraw a handpoker to the public poker 1111111 the name is: " + topHandPoker.name);
            WithdrawAddNPoker();

            opStepInfos.Pop();
        }

        if(opStepInfos.Count > 0)
        {
            Debug.Log("-------------  here we withdraw a handpoker to the hand poker " + topHandPoker.name);
            WithdrawOnePoker();
        }
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

        //here we withdraw one poker to its original place 
        bool bNeedAscDesStatusChange = true;
        bool bNeedBombStatusChange = true;

        GameObject poker = foldPoker.Peek();
        if(poker.tag == "wildDrop")
        {
            AddToPublicPoker(poker);

            GamePoker gamePoker = poker.GetComponent<GamePoker>();
            if(gamePoker.wildSource == GameDefines.WildCardSource.WildDrop)
            {
                gameplayUI.IncWildCard();
                Reward.Data[RewardType.WildCard]++;
            }

            Debug.Log("Withdraw poker name is: " + poker.name + "  wild source is: " + gamePoker.wildSource);

            foldPoker.Pop();
            poker = foldPoker.Peek();
        }
        Debug.Log("Withdraw poker name is: " + poker.name + "  tag is: " + poker.tag);

        OpStepInfo stepInfo = opStepInfos.Peek();

        switch (poker.tag)
        {
            case "poker":
                WithdrawGamePoker(poker);
                gameplayUI.goldAndScoreUI.AddGold(-stepInfo.nGold);
                gameplayUI.goldAndScoreUI.AddScore(-stepInfo.nScore);
                //bNeedBombStatusChange = false;
                break;

            case "handCard":
                WithdrawHandPoker(poker);
                gameplayUI.goldAndScoreUI.AddScore(-stepInfo.nScore);
                //AdjustAllHandPokerPosition();
                break;

            case "wildCard":
                WithdrawWildCard(poker, ref bNeedAscDesStatusChange, ref bNeedBombStatusChange);
                AdjustAllHandPokerPosition();
                break;
            default:break;
        }

        if(bNeedAscDesStatusChange)
        {
            UpdateAllAscDesPokerStatus(poker.name, true);
        }
        if(bNeedBombStatusChange)
        {
            UpdateAllBombStatus(poker.name, true);
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
            Debug.Log("-----------------------------\n here we output the strIDs is: " + stepInfo.strIDs + "the strName is: " + stepInfo.strCardName);
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

        //this statement is used to withdraw all pokers that has been 
        if(stepInfo.strClearAllPokerInfo.Length != 0)
        {
            WithdrawClearAllPokers(stepInfo);
        }

        if(stepInfo.strClearAllStreakInfo.Length != 0)
        {
            WithdrawClearAllStreakBonus(stepInfo);
        }

        if(stepInfo.strClearAllLockInfos.Length != 0)
        {
            WithdrawClearAllLockInfos(stepInfo);
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

        //determine whether we should show Add5Btn and end game btn.
        if (handPokers.Count == 0)
        {
            gameplayUI.ShowAdd5Btn();
            gameplayUI.ShowEndGameBtn();
        }
        else
        {
            gameplayUI.HideAdd5Btn();
            gameplayUI.HideEndGameBtn();
        }

        //todo: here we check opStepInfos's peek info, to see whether we should withdraw the add n poker from the handpokers
        while(CheckCanWithdrawAddNPoker())
        {
            Debug.Log("-----------------------------\n here we should withdraw a handpoker that is add_n, the ids are: " + opStepInfos.Peek().strIDs);
            WithdrawAddNPoker();

            opStepInfos.Pop();
        }
    }

    bool CheckCanWithdrawAddNPoker()
    {
        if (foldPoker.Count < 2)
        {
            Debug.Log("CheckCanWithdrawAddNPoker, but nothing to do!");
            return false;
        }

        GameObject topHandPoker = foldPoker.Peek();
        HandPoker handPokerScript = topHandPoker.GetComponent<HandPoker>();

        OpStepInfo stepInfo = opStepInfos.Peek();

        if (stepInfo.strCardName == "handpoker_add_n"
            && handPokerScript != null
            && handPokerScript.handPokerSource != HandPokerSource.AddNPoker)
        {
            return true;
        }

        return false;
    }

    //when click "Add5Btn", add 5 pokers to the hand poker
    public void Add5HandPoker()
    {
        //first check if we have enough gold
        int nBuyCost = GetBuy5PokerCost();
        if (Reward.Coin < nBuyCost)
            return;

        nAdd5BuyTimes++;
        gameplayUI.AddGold(-nBuyCost);
        Reward.Coin -= nBuyCost;

        PokerSuit[] suits = new PokerSuit[5];
        int[] nNumbers = new int[5];

        Test_Get5MoreHandPoker(ref suits, ref nNumbers);

        int nBegin = levelData.handPokerCount;
        levelData.handPokerCount += 5;
        //Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, 0.0f);
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.7f, -1.0f);
        for (int i = 0; i < 5; ++i)
        {
            //GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            GameObject go = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            go.tag = "handCard";

            HandPoker handScript = go.GetComponent<HandPoker>();
            handScript.Init(pokerCard, levelData, nBegin + i, Trans.position + posOffset, renderer.bounds.size, 0.0f);

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

        AdjustAllHandPokerPosition();
    }

    public void OnClickReplayButton()
    {
        MapDataManager.SetRetry(nCurrentLevel);
        SceneManager.LoadScene("MapTest");
    }

    public void OnClickNextLevelButton()
    {
        MapDataManager.SetRetry(nCurrentLevel+1);
        SceneManager.LoadScene("MapTest");
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

        if(Reward.Data[RewardType.WildCard] > 0)
        {
            gameplayUI.DecWildCard();
            Reward.Data[RewardType.WildCard]--;

            GameObject wildDrop = FindInActiveWildDrop();
            if(wildDrop != null)
            {
                GamePoker gamePoker = wildDrop.GetComponent<GamePoker>();
                gamePoker.UnFoldWildDropPoker();

                foldPoker.Push(wildDrop);
            }
            else
            {
                Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * -0.35f, -1.0f);

                GameObject go = (GameObject)Instantiate(wildcardPrefab, Trans.position + posOffset, Quaternion.identity);
                //go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
                go.tag = "wildCard";

                WildCard wildcardScript = go.GetComponent<WildCard>();
                GameObject topPoker = foldPoker.Peek();
                float fZ = topPoker.GetComponent<Transform>().position.z;
                wildcardScript.Init(wildcardPrefab, foldPoker.Count, Trans.position, renderer.bounds.size, 0.0f);//the second parameter should be the top foldPoker's index

                foldPoker.Push(go);
            }
        }
        else
        {
            int nBuyCost = GetWildCardCost();
            if (Reward.Coin < nBuyCost)
                return;

            gameplayUI.AddGold(-nBuyCost);
            Reward.Coin -= nBuyCost;

            Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * -0.35f, -1.0f);

            GameObject go = (GameObject)Instantiate(wildcardPrefab, Trans.position + posOffset, Quaternion.identity);
            //go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            go.tag = "wildCard";

            WildCard wildcardScript = go.GetComponent<WildCard>();
            GameObject topPoker = foldPoker.Peek();
            float fZ = topPoker.GetComponent<Transform>().position.z;
            wildcardScript.Init(wildcardPrefab, foldPoker.Count, Trans.position, renderer.bounds.size, 0.0f);//the second parameter should be the top foldPoker's index

            foldPoker.Push(go);
        }

        int nColors = GetStreakBonusEncode();
        int nHasStreak = nStreakCount > 0 ? 1 : 0;
        
        AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, 0, nHasStreak, streakType, nTotalComboCount);

        gameplayUI.HideEndGameBtn();
        gameplayUI.EnableWithdrawBtn();
    }

    GameObject FindInActiveWildDrop()
    {
        foreach(GameObject go in publicPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();
            if (gamePoker.wildSource == GameDefines.WildCardSource.WildDrop)
            {
                if(gamePoker.pokerInst.GetComponent<MeshRenderer>().enabled == false)
                {
                    return go;
                }
            }
        }

        return null;
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

        AddToPublicPoker(poker);
        //publicPokers.Add(poker);

        Test_EnableTopFoldPokerText();

        //Debug.Log("one game poker withdrawed!!! name is: " + poker.name);
    }

    //2021.9.8 added by pengyuan, to withdraw the add n pokers from handpoker
    void WithdrawAddNPoker()
    {
        OpStepInfo stepInfo = opStepInfos.Peek();

        string[] strIDs = stepInfo.strIDs.Split('_');
        int nGamePokerIndex = int.Parse(strIDs[0]);

        GameObject gamePoker = GetPublicPokerByIndex(nGamePokerIndex);

        if(gamePoker != null)
        {
            GamePoker gamePokerScript = gamePoker.GetComponent<GamePoker>();
            gamePokerScript.Withdraw();

            for (int i = 1; i < strIDs.Length; ++i)
            {
                int nAddNPokerID = int.Parse(strIDs[i]);

                string strPokerName = string.Format("handpoker_add_n_{0}_{1}", nGamePokerIndex, nAddNPokerID);

                GameObject addNPoker = GetHandPokerByName(strPokerName);
                if(addNPoker != null)
                {
                    HandPoker handPokerScript = addNPoker.GetComponent<HandPoker>();
                    handPokerScript.WithdrawAddNPoker(gamePokerScript);
                }
            }

            UnflipAllCoveredGamePoker(gamePoker);
        }
        else
        {
            Debug.Log("_____________________________we can not find a game poker--------------------------------");
        }
    }

    GameObject GetPublicPokerByIndex(int nIndex)
    {
        for(int i = 0; i < publicPokers.Count; ++i)
        {
            GamePoker pokerScript = publicPokers[i].GetComponent<GamePoker>();
            if (pokerScript.Index == nIndex)
            {
                return publicPokers[i];
            }
        }

        return null;
    }

    GameObject GetHandPokerByName(string strName)
    {
        return handPokers.Find(target => target.name.Equals(strName));
    }

    void WithdrawHandPoker(GameObject poker)
    {
        HandPoker handPokerScript = poker.GetComponent<HandPoker>();
        handPokerScript.Withdraw();

        foldPoker.Pop();
        handPokers.Add(poker);

        nStreakCount--;
        nStreakBonusCount--;
        nCollectScore -= 1;

        //nStreakBonusColor[nStreakBonusCount % 6] = (int)PokerColor.Red;
        //nCollectGold += 1;//todo:here we should get the gold from the config file
        //nClearGold = 0;
        //nTotalComboCount -= 1;

        
        Test_EnableTopFoldPokerText();

        //AdjustAllHandPokerPosition();

        Debug.Log("one hand poker withdrawed!!! name is: " + poker.name);
    }

    void WithdrawWildCard(GameObject poker, ref bool needChangeAseDesStatus, ref bool needChangeBombStatus)
    {
        Debug.Log("one WildCard withdrawed!!! name is: " + poker.name);
        WildCard wildCard = poker.GetComponent<WildCard>();
        wildCard.Withdraw();

        foldPoker.Pop();
        if(wildCard.wildcardSource == GameDefines.WildCardSource.StreakBonus)
        {
            handPokers.Add(poker);
        }
        else
        {
            needChangeAseDesStatus = false;
            needChangeBombStatus = false;

            int nBuyCost = GetWildCardCost();
            gameplayUI.AddGold(nBuyCost);
            Reward.Coin += nBuyCost;
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

    public void UnflipAllCoveredGamePoker(GameObject withdrawedPoker)
    {
        //get all facing game poker, detect the withdrawed poker can cover it,
        //if so, unflip this poker
        List<GameObject> facingPokers = new List<GameObject>();
        foreach(GameObject go in publicPokers)
        {
            GamePoker gamePokerScript = go.GetComponent<GamePoker>();
            if(gamePokerScript.pokerFacing == PokerFacing.Facing && withdrawedPoker.name != go.name
                && gamePokerScript.itemType != GameDefines.PokerItemType.Add_N_Poker)
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
        //targetPos = withdrawedPoker.GetComponent<GamePoker>().targetPos;

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
                //Debug.Log("UnflipAllCoveredGamePoker... the overlap box count is: " + hitResults.Length + " withdraw poker new pos is: " + newPos);
                for(int i = 0; i < hitResults.Length; ++i)
                {
                    //Debug.Log("UnflipAllCoveredGamePoker... the go name is: " + go.name + " the hitResults[i] name is: " + hitResults[i].name + " pos is: " + hitResults[i].transform.position);
                    if (go.name == hitResults[i].name)
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
        GameObject newFoldPoker = null;

        HandPoker handPokerScript = handPoker.GetComponent<HandPoker>();
        if(handPokerScript != null && handPokerScript.handPokerSource == HandPokerSource.ClearAll)
        {
            newFoldPoker = foldPoker.Pop();
            handPoker = foldPoker.Peek();
            foldPoker.Push(newFoldPoker);
        }

        string strLockInfo = "";
        int nLockAreaID = -1;

        if(handPoker != null)
        {
            //HandPoker handPokerScript = handPoker.GetComponent<HandPoker>();

            Debug.Log("OnTopGamePokerClicked... the game poker is: " + gamePokerScript.nPokerNumber + "  the fold poker is: " + handPoker.name + "game status is: " + gameStatus);

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

                UpdateAllAscDesPokerStatus(gamePokerScript.name);
                UpdateAllBombStatus(gamePokerScript.name);

                //2021.9.24 added by pengyuan
                //CheckCanFoldWildDrop(gamePokerScript);

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

                PlayGetCoinEffect(topGamePoker.transform.position - Vector3.forward * 0.5f, nStepGold);

                Debug.Log("*****************************here we fold a game poker, and we got gold number is: " + nStepGold);

                //AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, GetCollectScore(), 1, StreakType.Add_Gold);

                //gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);
                nColors = GetStreakBonusEncode();
                UpdateStreakStatus(nStreakCount, nColors);

                currentFlippingPoker = topGamePoker;

                publicPokers.Remove(topGamePoker);

                //Test_DisableAllFoldPokerText();
                foldPoker.Push(topGamePoker);
                gameplayUI.EnableWithdrawBtn();

                //2021.8.12 added by pengyuan
                CheckGameEnd();
            }
        }
        
    }

    void OnTopHandPokerClicked(GameObject topHandPoker, bool bFirstFlip)
    {
        Debug.Log("OnTopHandPokerClicked ... ...");
        if (!CanClickTopHandPoker())
        {
            return;
        }

        if (topHandPoker.tag == "handCard")
        {
            HandPoker handPokerScript = topHandPoker.GetComponent<HandPoker>();
            handPokerScript.FlipPoker(foldPoker.Count, bFirstFlip);

            float fZ = GetFoldPokerPosition_Z();

            handPokerScript.SetFoldIndex(foldPoker.Count);

        }
        else if (topHandPoker.tag == "wildCard")//if hand poker is wildcard, we still need to interrupt the streak
        {
            WildCard wildcardScript = topHandPoker.GetComponent<WildCard>();
            wildcardScript.FlipPoker(foldPoker.Count, bFirstFlip);

            wildcardScript.SetFoldIndex(foldPoker.Count);
        }

        if(!bFirstFlip)
        {
            UpdateAllAscDesPokerStatus(topHandPoker.name);
            UpdateAllBombStatus(topHandPoker.name);

            int nColors = GetStreakBonusEncode();
            AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, 0, 0, streakType, nTotalComboCount);

            nStreakCount = 0;
            nStreakBonusCount = 0;
            nTotalComboCount = 0;
            ClearStreakBonus();

            nColors = GetStreakBonusEncode();
            UpdateStreakStatus(0, nColors);

            OpStepInfo stepInfo = opStepInfos.Peek();
            stepInfo.strCardName = topHandPoker.name;  //2021.9.17 added by pengyuan, to store the clear all hand poker's name , so when the clear all animation has finished, we can store the information to opStepInfos.
        }

        currentFlippingPoker = topHandPoker;
        handPokers.Remove(topHandPoker);
        foldPoker.Push(topHandPoker);

        //Test_DisableAllFoldPokerText();

        //update withdraw button
        if (foldPoker.Count <= 1)
        {
            /*gameplayUI.ShowWithDrawBtn();
            gameplayUI.DisableWithdrawBtn();*/
            gameplayUI.HideWithDrawBtn();
        }
        else
        {
            gameplayUI.ShowWithDrawBtn();
            gameplayUI.EnableWithdrawBtn();
        }
        gameplayUI.EnableWildCardBtn();
        
        AdjustAllHandPokerPosition();

        //determine whether we should show Add5Btn
        if(handPokers.Count == 0)
        {
            gameplayUI.ShowAdd5Btn();
            gameplayUI.ShowEndGameBtn();
        }
    }

    public void AdjustAllHandPokerPosition()
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

    public void AdjustAllHandPokerPosition2nd(int nIndex)
    {
        for(int i = 0; i < nIndex; ++i)
        {
            GameObject go = handPokers[i];
            go.transform.position -= Vector3.right * 0.2f;
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
        info.strClearAllStreakInfo = "";
        info.strClearAllPokerInfo = "";
        info.strClearAllLockInfos = "";

        opStepInfos.Push(info);
    }

    int GetStreakBonusEncode()
    {
        int nResult = 0;

        for (int i = 0; i < Max_StreakBonus_Count; ++i)
        {
            if (nStreakBonusColor[i] > 1)
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
            nStreakBonusColor[i] = (nBonus & (1 << i) >> i ) + 1;
        }
    }

    public void StreakBonusDecode(int nBonus, ref PokerColor[] streakBonus)
    {
        for (int i = 0; i < Max_StreakBonus_Count; ++i)
        {
            streakBonus[i] = (PokerColor)(((nBonus & (1 << i)) >> i) + 1);
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

    public StreakType GetPreviousStreakType()
    {
        //int totalStreakCount = nStreakIDs.Length;
        if ((nCurrentStreakIndex - 1) >= 0)
        {
            return (StreakType)nStreakIDs[nCurrentStreakIndex - 1];
        }

        return (StreakType)(nStreakIDs[nCurrentStreakIndex]);
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
        //Debug.Log("UpdateStreakStatus... nCount is: " + nCount + "  type is: " + streakType);
        
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

            //Debug.Log("UpdateStreakStatus.. here we switch to next streak is: " + streakType);
        }
    }

    //2021.9.17 added by pengyuan , to compute the clear all's streak bonus at one time.
    void QuickUpdateSteakStatus(List<GameObject> pokerList)
    {
        /*nStreakCount++;
        int nStepScore = ComputeOperationScore(nStreakCount);
        int nStepGold = ComputeOperationGold();

        nTotalComboCount++;

        //nStreakBonusColor[nStreakBonusCount % Max_StreakBonus_Count] = (int)GetPokerColor(gamePoker);
        nStreakBonusCount++;
        nCollectGold += nStepGold;
        nCollectScore += nStepScore;
        //nClearGold = 0;

        gameplayUI.goldAndScoreUI.AddGold(nStepGold);
        gameplayUI.goldAndScoreUI.AddScore(nStepScore);

        //gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);
        int nColors = GetStreakBonusEncode();

        UpdateStreakStatus(nStreakCount, nColors);
        */
        //the following code are used,

        List<StreakBonusInfo> streakBonusInfos = new List<StreakBonusInfo>();

        int nTotalStepScore = 0;
        foreach(GameObject go in pokerList)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();
            nStreakCount++;

            int nStepScore = ComputeOperationScore(nStreakCount);
            int nStepGold = ComputeOperationGold();
            nTotalStepScore += nStepScore;

            Debug.Log("ComputeOperationGold is: " + nStepGold + "  name is: " + go.name);
            nTotalComboCount++;

            nStreakBonusColor[nStreakBonusCount % Max_StreakBonus_Count] = (int)GetPokerColor(gamePoker);

            nStreakBonusCount++;
            nCollectGold += nStepGold;
            nCollectScore += nStepScore;

            gameplayUI.goldAndScoreUI.AddGold(nStepGold);
            gameplayUI.goldAndScoreUI.AddScore(nStepScore);

            int nColors = GetStreakBonusEncode();
            gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);

            //gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);

            if (nStreakCount >= GetStreakFinishCount(streakType))
            {
                bool bDouble = IsDoubleStreak(nStreakCount, nColors);
                AddStreakBonusToPlayer(bDouble);

                //this is used to store one streak bonus
                StreakBonusInfo bonusInfo = new StreakBonusInfo();
                bonusInfo.StreakType = (int)streakType;
                bonusInfo.SteakValueEncode = nColors;

                int nBonus1 = 0, nBonus2 = 0;
                DecodeStreakBonus(opStepInfos.Peek().strIDs, ref nBonus1, ref nBonus2);
                bonusInfo.Bonus[0] = nBonus1;
                bonusInfo.Bonus[1] = nBonus2;

                streakBonusInfos.Add(bonusInfo);

                nStreakCount = 0;
                nStreakBonusCount = 0;
                ClearStreakBonus();

                nCurrentStreakIndex++;
                streakType = GetCurrentStreakType();
                gameplayUI.InitStreakBonus(streakType, GetNextStreakType());
                gameplayUI.SetStreakBonusStatus(nStreakCount, 0);
            }
        }

        //first set streak bonus ui, second format the information in streakBonusInfo to a string and store it in strClearAllStreakInfo
        gameplayUI.SetClearAllStreakBonus(streakBonusInfos);
        opStepInfos.Peek().strClearAllStreakInfo = EncodeClearAllStreakBonusInfo(streakBonusInfos);
        opStepInfos.Peek().strIDs = "";
        opStepInfos.Peek().nScore = nTotalStepScore;

        Debug.Log("QuickUpdateSteakStatus... opStepInfos.Peek().strClearAllStreakInfo is: " + opStepInfos.Peek().strClearAllStreakInfo + "  the total score is: " + nTotalStepScore);
    }

    void DecodeStreakBonus(string strBonus, ref int nBonus1, ref int nBonus2)
    {
        //if strBonus.Length == 0, this means that the bonus is add money.
        if (strBonus.Length == 0)
        {
            return;
        }

        if (strBonus.IndexOf('_') == -1 )
        {
            nBonus1 = int.Parse(strBonus);
        }
        else
        {
            string[] strParams = strBonus.Split('_');

            nBonus1 = int.Parse(strParams[0]);
            nBonus2 = int.Parse(strParams[1]);
        }
    }

    string EncodeClearAllStreakBonusInfo(List<StreakBonusInfo> bonusInfos)
    {
        string strBonusInfo = "";

        if (bonusInfos.Count == 0)
            return strBonusInfo;

        strBonusInfo = string.Format("{0}", bonusInfos.Count);

        foreach(StreakBonusInfo info in bonusInfos)
        {
            strBonusInfo += string.Format("_{0}", info.StreakType);
            strBonusInfo += string.Format("_{0}", info.SteakValueEncode);
            strBonusInfo += string.Format("_{0}", info.Bonus[0]);
            strBonusInfo += string.Format("_{0}", info.Bonus[1]);
        }

        return strBonusInfo;
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

        Debug.Log("AddStreakBonusGold ... nBonusGold is:  " + nBonusGold + "  nBonusScore is: " + nBonusScore);

        OpStepInfo stepInfo = opStepInfos.Peek();
        stepInfo.nGold += nBonusGold;
        stepInfo.nScore += nBonusScore;

        nCollectGold += nBonusGold;
        nCollectScore += nBonusScore;

        gameplayUI.goldAndScoreUI.AddGold(nBonusGold);
        gameplayUI.goldAndScoreUI.AddScore(nBonusScore);

        gameplayUI.streakBonusUI.ShowCoinEffect();
        //gameplayUI.goldAndScoreUI.ShowCoinEffectTest();
        //GameObject streakCoin = Instantiate(FXCoin, gameplayUI.streakBonusUI.transform);

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
            Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * 0.35f, -1.0f);

            //GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position + posOffset, Quaternion.identity);
            GameObject go = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
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
            Debug.Log("AddStreakBonusPoker... the suit is:  " + suit + "  the number is: " + nNumber + "  the pos is: " + go.transform.position);
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

        //AdjustAllHandPokerPosition();
    }
    
    void AddStreakBonusWildcard(bool bDouble = false)
    {
        int nMultiple = bDouble ? 2 : 1;
        int[] bonusIDs = new int[2];

        OpStepInfo stepInfo = opStepInfos.Peek();

        for (int i = 0; i < nMultiple; ++i)
        {
            //instantiate a wild card , find a position in handpoker, insert it into handpoker.
            Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.4f, renderer.bounds.size.y * 0.35f, -2.0f);

            GameObject go = (GameObject)Instantiate(wildcardPrefab, Trans.position + posOffset, Quaternion.identity);
            //go.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            go.transform.rotation = Quaternion.identity;
            go.tag = "wildCard";

            WildCard wildcardScript = go.GetComponent<WildCard>();
            //GameObject topPoker = foldPoker.Peek();
            //float fZ = topPoker.GetComponent<Transform>().position.z;

            int insertIndex = GetInsertPositionInHandPoker();
            GameObject insertGameObject = handPokers[insertIndex];
            wildcardScript.InitStreakBonusWildcard(insertIndex, insertGameObject.transform.position, renderer.bounds.size, 0.0f);

            string strName = string.Format("wildcard_bonus_{0}", insertIndex);
            go.name = strName;

            Debug.Log("AddStreakBonusWildcard... the name is:  " + strName);

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

        //AdjustAllHandPokerPosition();

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

    int GetTopInsertPositionInHandPoker()
    {
        return handPokers.Count;
    }

    //static int nframeINdex = 0;
    private void FixedUpdate()
    {
        if (fGameTime > 3.0f && !bHasSetToGaming)
        {
            gameStatus = GameStatus.GameStatus_Gaming;
            bHasSetToGaming = true;
        }

        if (gameStatus != GameStatus.GameStatus_Gaming)
        {
            return;
        }

        //here we can update the poker's position and rotation to see if they stop translate and rotate, and set a bool value.
        //这个可以牌形变化的时候再检测
        List<GameObject> topPokers = new List<GameObject>();

        if (!IsPublicPokerFlipping())//when we have a flipping poker, we wait until the poker has finished flipping...
        {
            if (GetTopPokerInfos(ref topPokers))
            {
                //Debug.Log("thee topPokers's couunt is: " + topPokers.Count);
                //nframeINdex++;
                for (int i = 0; i < topPokers.Count; ++i)
                {
                    //todo: flip the top pokers
                    GamePoker gamePokerScript = topPokers[i].GetComponent<GamePoker>();
                    if (gamePokerScript.itemType != GameDefines.PokerItemType.Add_N_Poker 
                        && gamePokerScript.itemType != GameDefines.PokerItemType.WildDrop)
                    {
                        gamePokerScript.FlipPoker();
                        //Debug.Log("fixedupdate flip the poker, the frame index is: " + nframeINdex);
                    }
                    else
                    {
                        if(HasPokerIsAddN() < 0 && powerUpProcess.HasFinishedOncePowerUPUsage()
                            && gamePokerScript.itemType == GameDefines.PokerItemType.Add_N_Poker)
                        {
                            if (gamePokerScript.AddNPoker())
                            {
                                Add_AddNPokerStep(gamePokerScript);
                            }
                        }

                        /*if(gamePokerScript.itemType == GameDefines.PokerItemType.WildDrop)
                        { }*/
                    }
                }

                //2021.9.24 added by pengyuan, to process all wild drops
                //ProcessWildDropUpdate();
                /*Debug.Log("we found on top pokers count is: " + topPokers.Count);
                for(int i=0; i<topPokers.Count; ++i)
                {
                    Debug.Log("thee poker's name is: " + topPokers[i].name);
                }*/
            }

            //2021.9.10 added by pengyuan, we use once powerups here.
            if(!IsPublicPokerFlipping() && !bHasUsedOncePowerUPs)
            {
                powerUpProcess.BeginUsePowerUPs();
                //bHasUsedOncePowerUPs = true;
            }

            //if (!bAutoFlipHandPoker && bHasUsedOncePowerUPs)bHasFinishedOncePowerUPs
            if (!bAutoFlipHandPoker && bHasFinishedOncePowerUPs) 
            {
                GameObject handPoker = GetTopHandPoker();
                if (handPoker != null)
                {
                    OnTopHandPokerClicked(handPoker, true);
                    bAutoFlipHandPoker = true;
                }

                if(!bFirstCheckPowerUPs)
                {
                    CheckPowerUP_Clear();

                    //CheckPowerUP_ClearLock();

                    bFirstCheckPowerUPs = true;
                }
            }

            CheckGameEnd();
        }
    }

    void GetTargetPosition(int nPokerIndex, out Vector3 targetPos)
    {
        targetPos.x = Trans.position.x - 5.0f + nPokerIndex * 2.0f;
        targetPos.y = Trans.position.y;
        targetPos.z = Trans.position.z;
    }

    void GetAllFacingPoker(ref List<GameObject> topPokers)
    {
        foreach(GameObject go in publicPokers)
        {
            GamePoker pokerScript = go.GetComponent<GamePoker>();
            if (pokerScript.pokerFacing == PokerFacing.Facing)
                topPokers.Add(go);
        }

        for(int i = topPokers.Count - 1; i >= 0; --i)
        {
            GamePoker pokerScript = topPokers[i].GetComponent<GamePoker>();
            BoxCollider2D collider = topPokers[i].GetComponent<BoxCollider2D>();

            if (pokerScript.pokerInst.GetComponent<MeshRenderer>().enabled == false)
            {
                topPokers.Remove(topPokers[i]);
                break;
            }

            RaycastHit2D[] hitResults = Physics2D.BoxCastAll(collider.bounds.center, collider.size, pokerScript.pokerInfo.fRotation, Vector2.zero);
            if (hitResults.Length > 0)
            {
                int nMinIndex;
                if (pokerScript.itemType == GameDefines.PokerItemType.Add_N_Poker)
                {
                    nMinIndex = GetMinDepthIndex(ref hitResults);
                }
                else
                {
                    nMinIndex = GetMinDepthIndexOutsideLockArea(ref hitResults);
                }

                //if (nMinIndex >= 0 && hitResults[nMinIndex].collider.name == collider.name && !pokerScript.bAddNPoker)
                if (nMinIndex < 0 || hitResults[nMinIndex].collider.name != collider.name || pokerScript.bAddNPoker)
                {
                    topPokers.Remove(topPokers[i]);
                }
            }
        }
    }

    bool GetTopPokerInfos(ref List<GameObject> topPokers)
    {
        //Debug.Log("enter fixed update.. get top poker infos...");
        foreach (GameObject go in publicPokers)
        {
            //Vector2 size;
            GamePoker pokerScript = go.GetComponent<GamePoker>();
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();

            if (pokerScript.pokerInst.GetComponent<MeshRenderer>().enabled == false)
                continue;

            RaycastHit2D[] hitResults = Physics2D.BoxCastAll(collider.bounds.center, collider.size, pokerScript.pokerInfo.fRotation, Vector2.zero);
            //Debug.Log("enter fixed update.. get top poker infos... the hit result length is: " + hitResults.Length + ", the collider is: " + collider.name);
            if (hitResults.Length > 0)
            {
                //this is for debug:
                //OutputHitResultDebugString(hitResults);

                int nMinIndex;
                if (pokerScript.itemType == GameDefines.PokerItemType.Add_N_Poker)
                {
                    nMinIndex = GetMinDepthIndex(ref hitResults);
                }
                else
                {
                    nMinIndex = GetMinDepthIndexWithoutLockArea(ref hitResults);
                }

                if (nMinIndex >= 0 && hitResults[nMinIndex].collider.name == collider.name && !pokerScript.bAddNPoker)// && !pokerData.bHasWithdrawed)
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

    void OutputHitResultDebugString(RaycastHit2D[] hitResults)
    {
        string strDebugOutput = ";";
        for (int i = 0; i < hitResults.Length; ++i)
        {
            strDebugOutput += hitResults[i].collider.name;
            strDebugOutput += "    ";
        }
        Debug.Log(strDebugOutput);
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

            GamePoker gamePoker = hitResults[i].collider.GetComponent<GamePoker>();
            if (gamePoker == null)
            {
                //Debug.Log("--------------------------------------------- ---------------\n the gamepoker is null , hti result name is: " + hitResults[i].collider.name);
                continue;
            }

            if (gamePoker.bIsFolding)
                continue;

            GameObject go = gamePoker.pokerInst;
            if ( (hitResults[i].collider.transform.position.z < fMinDepth) 
                && (go.GetComponent<MeshRenderer>().enabled) )
            {
                fMinDepth = hitResults[i].collider.transform.position.z;
                nMinIndex = i;
            }
        }

        return nMinIndex;
    }

    int GetMinDepthIndexOutsideLockArea(ref RaycastHit2D[] hitResults)
    {
        float fMinDepth = float.MaxValue;
        int nMinIndex = -1;

        for (int i = 0; i < hitResults.Length; ++i)
        {
            if (hitResults[i].collider.tag == "lockArea")
            {
                return -1;
            }

            GamePoker gamePoker = hitResults[i].collider.GetComponent<GamePoker>();
            if (gamePoker == null)
            {
                continue;
            }

            GameObject go = gamePoker.pokerInst;
            if ((hitResults[i].collider.transform.position.z < fMinDepth)
                && (go.GetComponent<MeshRenderer>().enabled))
            {
                fMinDepth = hitResults[i].collider.transform.position.z;
                nMinIndex = i;
            }
        }

        return nMinIndex;
    }

    public GameObject GetTopHandPoker()
    {
        if(handPokers.Count == 0)
        {
            return null;
        }

        return handPokers[handPokers.Count - 1];
    }

    public void RemoveOneHandPoker(GameObject handPoker)
    {
        handPokers.Remove(handPoker);
    }

    private bool IsPublicPokerFlipping()
    {
        bool bRet = false;

        for (int i=0; i<publicPokers.Count; ++i)
        {
            GamePoker gamePoker = publicPokers[i].GetComponent<GamePoker>();
            bRet |= gamePoker.bIsFlipping;
        }

        /*if (bRet)
            Debug.Log("~~~~~~~~~~~~~~~~~~~~~public poker is flipping......");*/

        return bRet;
    }

    public void AddToPublicPoker(GameObject go)
    {
        Debug.Log("AddToPublicPoker ... name is: " + go.name + "  tag is: " + go.tag);
        publicPokers.Add(go);
    }

    public void RemoveOnePublicPoker(GameObject go)
    {
        publicPokers.Remove(go);
    }

    public void DestroyOnePublicPoker(GameObject go)
    {
        Destroy(go);
        publicPokers.Remove(go);
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

        for (int i = 0; i < cardsArray.Length; ++i)
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
        nNumber = Invalid_Poker_Digit;

        return 1;
    }

    public int Test_GetNextPoker(ref PokerSuit suit, ref int nNumber)
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
            //Debug.Log("_________这里需要洗牌了。。。." + cardsArray2nd.Count);
            cardIndex -= cardsArray2nd.Count;
            Test_Shuffle();
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
                cardIndex -= cardsArray2nd.Count;
                Test_Shuffle();
            }

            int nSuit = (int)(cardsArray2nd[cardIndex] - 1)/ 13 + 1;
            suits[i] = (PokerSuit)nSuit;
            nNumbers[i] = cardsArray2nd[cardIndex] % 13;

            cardIndex++;
        }
    }

    void Test_GetPreallocateAscendDescendPoker(int nIndex, ref PokerSuit suit, ref int nNumber)
    {
        suit = (PokerSuit)(ascDesPokerIDs.pokerNumbers[nIndex] / 13 + 1); ;
        nNumber = ascDesPokerIDs.pokerNumbers[nIndex] % 13;
    }

    //2021.9.1 added to generate ascend and descend poker, and then remove from the cardsArray2nd
    void GeneratePokerForAscendDescendPoker()
    {
        for(int i = 0; i < levelData.pokerInfo.Count; ++i)
        {
            if( (levelData.pokerInfo[i].nItemType == (int)GameDefines.PokerItemType.Ascending_Poker
                || levelData.pokerInfo[i].nItemType == (int)GameDefines.PokerItemType.Descending_Poker)
                && levelData.pokerInfo[i].strItemInfo.Length > 0)
            {
                string[] strParams = levelData.pokerInfo[i].strItemInfo.Split('_');

                PokerSuit suit = (PokerSuit)int.Parse(strParams[0]);
                int nNumber = int.Parse(strParams[1]);

                int nPokerNumber = ((int)suit - 1) * 13 + nNumber;

                ascDesPokerIDs.IDs.Add(i);
                ascDesPokerIDs.pokerNumbers.Add(nPokerNumber);

                cardsArray2nd.Remove(nPokerNumber);
            }
        }
    }

    void UpdateAllAscDesPokerStatus(string strFoldName, bool bWithdraw = false)
    {
        foreach(GameObject go in publicPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            //if(gamePoker.itemType != GameDefines.PokerItemType.None)
            if (gamePoker.itemType == GameDefines.PokerItemType.Ascending_Poker
                || gamePoker.itemType == GameDefines.PokerItemType.Descending_Poker)
            {
                gamePoker.UpdateAscendDescendStatus(strFoldName, bWithdraw);
            }
        }
    }

    //todo: 
    void UpdateAllBombStatus(string strFoldName, bool bWithdraw = false)
    {
        if(!bAutoFlipHandPoker)
        {
            return;
        }

        foreach (GameObject go in publicPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            if(gamePoker.itemType == GameDefines.PokerItemType.Bomb)
            {
                gamePoker.UpdateBombStatus(strFoldName, bWithdraw);
            }
        }
    }

    public void OnBombEndGame()
    {
        Debug.Log("---------------------OnBombEndGame-----------------");

        gameStatus = GameStatus.GameStatus_WaitForSettle;

        gameplayUI.ShowBombEndGameUI();
    }

    int HasPokerIsAddN()
    {
        for(int i = 0; i < publicPokers.Count; ++i)
        {
            GamePoker gamePoker = publicPokers[i].GetComponent<GamePoker>();
            if(gamePoker.itemType == GameDefines.PokerItemType.Add_N_Poker 
                && gamePoker.bIsAddingNPoker)
            {
                return gamePoker.Index;
            }
        }

        return -1;
    }

    void SetAddingNAccelerating(int nIndex)
    {
        
        foreach (GameObject go in publicPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            if (gamePoker.Index == nIndex && gamePoker.bIsAddingNPoker)
            {
                if (!gamePoker.bAccelAddingNPoker)
                {
                    gamePoker.bAccelAddingNPoker = true;
                }

                return;
            }
        }
    }


    //2021.9.7 added by pengyaun add one poker
    public void OnAddNPoker_One(GamePoker gamePokerScript)
    {
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.35f, -1.0f);

        //GameObject go = (GameObject)Instantiate(pokerPrefab, Trans.position, Quaternion.identity);
        GameObject go = (GameObject)Instantiate(pokerCard, Trans.position, Quaternion.identity);
        go.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        go.tag = "handCard";

        GamePoker pokerScript = go.GetComponent<GamePoker>();
        Destroy(pokerScript);

        HandPoker handPokerScript = go.GetComponent<HandPoker>();

        //int insertIndex = GetInsertPositionInHandPoker();
        int insertIndex = GetTopInsertPositionInHandPoker();
        handPokerScript.InitAddNHandPoker(insertIndex, Trans.position + posOffset, renderer.bounds.size, 0.0f);

        PokerSuit suit = PokerSuit.Suit_None;
        int nNumber = Invalid_Poker_Digit;
        
        Test_GetNextPoker(ref suit, ref nNumber);
        handPokerScript.Test_SetSuitNumber(suit, nNumber);

        gamePokerScript.AddNPokerOneID(insertIndex);
        string strName = string.Format("handpoker_add_n_{0}_{1}", gamePokerScript.Index, insertIndex);
        go.name = strName;

        handPokers.Insert(insertIndex, go);
    }

    public void Add_AddNPokerStep(GamePoker pokerScript)
    {
        int nColors = GetStreakBonusEncode();
        int nHasStreak = nStreakCount > 0 ? 1 : 0;

        AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, 0, 0, nHasStreak, streakType, nTotalComboCount);

        OpStepInfo stepInfo = opStepInfos.Peek();

        Debug.Log("----------------------------Add_AddNPokerStep the stack count is: " + opStepInfos.Count);
        stepInfo.strCardName = "handpoker_add_n";
        stepInfo.strIDs = pokerScript.strHandPokerIDs;
    }

    public void OnAddNPokerExit(GamePoker gamePoker)
    {
        OpStepInfo stepInfo = opStepInfos.Peek();
        stepInfo.strIDs = gamePoker.strHandPokerIDs;

        //Debug.Log("---------------OnAddNPokerExit-------------the strIDs is: " + gamePoker.strHandPokerIDs);

        gamePoker.OnAddNPokerExit();
    }

    void GeneratePokerForUnlockArea()
    {
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            UnlockAreaPokerIDs unlockAreaPokers = new UnlockAreaPokerIDs();
            unlockAreaPokers.nGroupID = lockAreas[i].nGroupID;
            unlockAreaPokerIDs.Add(unlockAreaPokers);
        }

        //first we find all the poker indices that exist in an unlock area, and don't include GameDefines.PokerItemType.Add_N_Poker
        for (int i = 0; i < levelData.pokerInfo.Count; ++i)
        {
            for (int j = 0; j < lockAreas.Count; ++j)
            {
                LockArea lockAreaScript = lockAreas[j].Area.GetComponent<LockArea>();

                int groupID;
                int groupIndex;
                if (lockAreaScript.IsInUnlockArea(i, out groupID, out groupIndex)
                    && levelData.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.Add_N_Poker)
                    //pengyuan 2021.9.6, if this poker is a Add_N_Poker, we should exclude it from the unlock area, this means that we shouldn't preallocate this poker.
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
                    PokerSuit suit = PokerSuit.Suit_None;
                    int nNumber = Invalid_Poker_Digit;

                    Test_GetNextPoker(ref suit, ref nNumber);
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
                    //Debug.Log("预生成扑克ID：" + nAllocatedPoker);
                    PokerSuit suit = (PokerSuit)(nAllocatedPoker / 13 + 1);
                    int nDigit = nAllocatedPoker % 13;
                    string strDebugString = string.Format("_{0}_{1}", Test_GetSuitDisplayString(suit, nDigit), nAllocatedPoker);
                    //Debug.Log("预生成扑克，移除：" + strDebugString);
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
                //nNumber = (int)lockScript.pokerSuit * 13 - nNumber;
                nNumber = 13 - nNumber;
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
    
    int GetUnlockAreaPokerCount(int groupID)
    {
        for(int i = 0; i < unlockAreaPokerIDs.Count; ++i)
        {
            if(unlockAreaPokerIDs[i].nGroupID == groupID)
            {
                for(int j = 0; j < lockGroups.Count; ++j)
                {
                    if(lockGroups[j].nGroupID == groupID)
                    {
                        for(int k = 0; k < lockGroups[j].locks.Count; k++)
                        {
                            GameLock gameLock = lockGroups[j].locks[k].GetComponent<GameLock>();

                            if(gameLock.lockState == LockState.LockState_Working)
                            {
                                return 1;
                            }
                        }
                        
                    }
                }

                break;
            }
        }

        return 0;
    }

    //2021.9.22 added by pengyuan, to get all lock groups that is working.
    void GetWorkingLockGroups(ref List<int> lockGroupIDs)
    {
        lockGroupIDs.Clear();

        foreach(LockGroup lockGroup in lockGroups)
        {
            bool bWorking = true;

            for (int i = 0; i < lockGroup.locks.Count; ++i)
            {
                GameLock gameLock = lockGroup.locks[i].GetComponent<GameLock>();
                bWorking &= (gameLock.lockState == LockState.LockState_Working || gameLock.lockState == LockState.LockState_Dying) ? true : false;
            }

            if (bWorking)
                lockGroupIDs.Add(lockGroup.nGroupID);
        }
    }

    void ClearAll_ClearLockGroup(int nID, bool bLeft)
    {
        for (int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == nID)
            {
                for(int j = 0; j < lockGroups[i].locks.Count; ++j)
                {
                    GameLock gameLock = lockGroups[i].locks[j].GetComponent<GameLock>();
                    gameLock.ClearAll_ClearLock(bLeft);
                }
                
                break;
            }
        }
    }

    void Cancel_ClearAll_ClearLockGroup(int nID)
    {
        for (int i = 0; i < lockGroups.Count; ++i)
        {
            if (lockGroups[i].nGroupID == nID)
            {
                for (int j = 0; j < lockGroups[i].locks.Count; ++j)
                {
                    GameLock gameLock = lockGroups[i].locks[j].GetComponent<GameLock>();
                    gameLock.Cancel_ClearAll_ClearLock();
                }

                break;
            }
        }
    }

    bool ClearAll_ClearLockArea(int nID)
    {
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            if (lockAreas[i].nGroupID == nID)
            {
                LockArea lockArea = lockAreas[i].Area.GetComponent<LockArea>();
                return lockArea.ClearAll_ClearLock();
                //break;
            }
        }

        return false;
    }

    void Cancel_ClearAll_ClearLockArea(int nID)
    {
        for (int i = 0; i < lockAreas.Count; ++i)
        {
            if (lockAreas[i].nGroupID == nID)
            {
                LockArea lockArea = lockAreas[i].Area.GetComponent<LockArea>();
                lockArea.Cancel_ClearAll_ClearLock();
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
        return;
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
        if(handPoker.tag == "wildCard" || handPoker.tag == "wildDrop")
        {
            return true;
        }

        return false;
    }

    int ComputeOperationGold(bool bWithdraw = false)
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];
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

        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        string strScoreConfig = stageConfig.CardScore;
        string[] strParams = strScoreConfig.Split('_');

        int nBase = int.Parse(strParams[0]);
        int nDiff = int.Parse(strParams[1]);

        int nScore = nBase + (nDiff * (nCount - 1));

        Debug.Log("ComputeOperationScore base is: " + nBase + "  Diff is: " + nDiff + "  the result score is: " + nScore);
        //Debug.Log("the result score is: " + nScore);

        return nScore;
    }

    int GetWildCardCost()
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        return stageConfig.WildCost;
    }

    int GetWithdrawCost()
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        string[] strParams = stageConfig.CancelCost.Split('_');

        if(nWithdrawBuyTimes >= 2)
        {
            return int.Parse(strParams[2]);
        }
        else
        {
            return int.Parse(strParams[nWithdrawBuyTimes]);
        }
    }

    int GetBuy5PokerCost()
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        string[] strParams = stageConfig.BuyCard.Split('_');

        if (nAdd5BuyTimes >= 2)
        {
            return int.Parse(strParams[2]);
        }
        else
        {
            return int.Parse(strParams[nAdd5BuyTimes]);
        }
    }

    public int ComputeStars(int nScore)
    {
        int nStar = 0;

        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        string strStarConfig = stageConfig.Star;
        string[] strParams = strStarConfig.Split('_');

        int oneStar = int.Parse(strParams[0]);
        int twoStar = int.Parse(strParams[1]);
        int threeStar = int.Parse(strParams[2]);

        if (nScore > threeStar)
            return 3;
        else if (nScore > twoStar)
            return 2;
        else
            return 1;

        return nStar;
    }

    public void Test_ComputeStars(int nScore, ref float[] fillAmounts)
    {
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

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
        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

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

    void InitItemCount()
    {
        nSingleClearScore = 0;
        for(int i = 0; i < nItemCounts.Length; ++i)
        {
            nItemCounts[i] = 0;
        }

        foreach(JsonReadWriteTest.PokerInfo info in levelData.pokerInfo)
        {
            if (info.nItemType == (int)GameDefines.PokerItemType.Bomb)
                nItemCounts[(int)GameDefines.PowerUPType.Remove_Bomb]++;

            if (info.nItemType == (int)GameDefines.PokerItemType.Ascending_Poker
                || info.nItemType == (int)GameDefines.PokerItemType.Descending_Poker)
                nItemCounts[(int)GameDefines.PowerUPType.Remove_Ascend_Descend]++;
        }

        nItemCounts[(int)GameDefines.PowerUPType.Remove_Lock] = levelData.lockGroup.nLockGroupCount;

        StageConfig stageConfig = stageConfigs[nCurrentLevel-1];

        //compute each step's score
        if (powerUpProcess.HasPowerUP_ClearBomb())
            nSingleClearScore = stageConfig.PowerUpScore / nItemCounts[(int)GameDefines.PowerUPType.Remove_Bomb];

        if (powerUpProcess.HasPowerUP_ClearAscDes())
            nSingleClearScore = stageConfig.PowerUpScore / nItemCounts[(int)GameDefines.PowerUPType.Remove_Ascend_Descend];

        if(powerUpProcess.HasPowerUP_ClearLock())
            nSingleClearScore = stageConfig.PowerUpScore / nItemCounts[(int)GameDefines.PowerUPType.Remove_Lock];
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
        //todo: if we have cleared all the pokers in current unlock area, but we still have a lock that don't clear, then we lose the game.
        //...
        int nSingleLockGroupID = HasActiveSingleLock();
        int nCompoundLockGroupdID = HasActiveCompoundLock();
        if ( nSingleLockGroupID >= 0 || nCompoundLockGroupdID >= 0)
        {
            if(nSingleLockGroupID >= 0)
            {
                int nCount = GetUnlockAreaPokerCount(nSingleLockGroupID);
                if(nCount == 0)
                {
                    gameStatus = GameStatus.GameStatus_WaitForSettle;
                    gameplayUI.ShowLockEndGameUI();
                    return;
                }
            }

            if(nCompoundLockGroupdID >= 0)
            {
                int nCount = GetUnlockAreaPokerCount(nCompoundLockGroupdID);
                if(nCount == 0)
                {
                    gameStatus = GameStatus.GameStatus_WaitForSettle;
                    gameplayUI.ShowLockEndGameUI();
                    return;
                }
            }
            
        }

        if (publicPokers.Count == 0)
        {
            Debug.Log("Contratulations!  you win the game!!!");

            WinGame();
        }
        else
        {
            bool bAllDiabled = true;
            for (int i = 0; i < publicPokers.Count; ++i)
            {
                GamePoker gamePoker = publicPokers[i].GetComponent<GamePoker>();

                if(gamePoker.pokerInst.gameObject.GetComponent<MeshRenderer>().enabled && (gamePoker.pokerStatus != GameDefines.PokerStatus.Clearing))
                {
                    bAllDiabled = false;
                    continue;
                }
            }

            if(bAllDiabled)
            {
                Debug.Log("Contratulations!  you win the game!!! -----111---");

                WinGame();
            }
        }
    }

    void WinGame()
    {
        gameResult = GameResult.GameResult_Win;
        gameStatus = GameStatus.GameStatus_Settle;

        gameplayUI.HideAdd5Btn();
        gameplayUI.HideEndGameBtn();
        gameplayUI.DisableAllGameButton();

        Reward.Coin += nCollectGold;
        gameplayUI.goldAndScoreUI.SetGold(Reward.Coin);

        StartCoroutine(SettleAllHandPokers());

    }

    //this method is used to settle gold, score and other game result data.
    public void LoseGame()
    {
        gameStatus = GameStatus.GameStatus_WaitForSettle;

        //DismissAllGamePoker();
        //return;

        gameResult = GameResult.GameResult_Loss;

        gameStatus = GameStatus.GameStatus_Settle;

        gameplayUI.LoseGame(nCollectGold, 0);

        //Reward.Coin += nCollectGold;
        //Reward.Data[RewardType.FreeRound] += 1;

        EndGame();
    }

    public void DismissAllGamePoker()
    {
        foreach(GameObject go in publicPokers)
        {
            GamePoker pokerScript = go.GetComponent<GamePoker>();
            pokerScript.Dismiss();
        }
    }

    IEnumerator SettleAllHandPokers()
    {
        //Debug.Log("enter SettleAllHandPokers ... ...");
        int nIndex = 0;
        int nPokerOrder = 1;

        while(handPokers.Count > 0)
        {
            nIndex = handPokers.Count - 1;
            
            GameObject go = handPokers[nIndex];

            HandPoker handPoker = go.GetComponent<HandPoker>();
            go.transform.DOMove(go.transform.position + Vector3.up * 1, 0.5f).SetEase(Ease.InSine);
            handPoker.pokerInst.GetComponent<MeshRenderer>().material = oneSideMaterial;
            Material material = handPoker.pokerInst.GetComponent<MeshRenderer>().material;

            yield return new WaitForSeconds(0.5f);

            material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.4f), 0.5f);

            nIndex++;

            //add gold and score for the rest hand pokers
            int nGold = stageConfigs[nCurrentLevel-1].RestCardCoin * nPokerOrder;
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

            int nScore = stageConfigs[nCurrentLevel-1].EndCardScore * nPokerOrder;
            if (go.tag == "wildcard")
            {
                nScore *= 2;
            }
            nCollectScore += nScore;
            gameplayUI.goldAndScoreUI.AddScore(nScore);

            //2021.10.9 here we play the add coin effect.
            /*GameObject coinEffect = Instantiate(getCoinEffect, go.transform.position + Vector3.up * 1.5f + Vector3.right * -0.4f, Quaternion.identity);
            GetCoinEffect getCoinScript = coinEffect.GetComponent<GetCoinEffect>();
            getCoinScript.Init(nGold);*/

            Vector3 coinEffectPos = go.transform.position + Vector3.up * 1.5f + Vector3.right * -0.4f;
            PlayGetCoinEffect(coinEffectPos, nGold);

            //todo: we should add score and gold effect here.
            //Vector3 newPos = coinEffect.transform.position;
            //GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCardStar, coinEffect.transform.position, Quaternion.identity);
            GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCardStar, coinEffectPos, Quaternion.identity);

            PlaySettleStarEffect(streakCoin);

            nPokerOrder++;

            //yield return new WaitForSeconds(0.3f);

            handPokers.Remove(go);
            Destroy(go, 0.5f);
            Destroy(streakCoin, 1.0f);
        }

        string strStageClearBonus = publicConfigs[0].StageClearBonus;
        string[] strParams = strStageClearBonus.Split('_');

        Random.InitState((int)System.DateTime.Now.Ticks);
        Debug.Log(System.DateTime.Now.Second);

        float fX = float.Parse(strParams[0]);
        float fY = float.Parse(strParams[1]);
        float fZ = float.Parse(strParams[2]);

        int nMinimalGold = (int)(stageConfigs[nCurrentLevel-1].Cost * fX * 0.0001f);
        float a = Random.Range(fY, fZ) * 0.0001f;
        nClearGold =(int)(stageConfigs[nCurrentLevel-1].Cost * a - nCollectGold );
        //Debug.Log("a is: " + a + "  collect gold is: " + nCollectGold + "  clear gold is: " + nClearGold);
        if (nClearGold < nMinimalGold)
            nClearGold = nMinimalGold;

        Debug.Log("collect gold is: " + nCollectGold + "  clear gold is: " + nClearGold);

        //gameplayUI.goldAndScoreUI.SetGold(nCollectGold + nClearGold);

        ScoreStar = ComputeStars(nCollectScore);
        MapDataManager.SetLevelRating(nCurrentLevel, ScoreStar);  //pengyuan 2021.9.28 added, to record stars in chapter data.

        gameplayUI.WinGame(nCollectGold, nClearGold, ScoreStar);

        //gameStatus = GameStatus.GameStatus_After_Settle;
        EndGame();

        StopCoroutine(SettleAllHandPokers());
       
    }

    void PlayGetCoinEffect(Vector3 basePos, int nGold)
    {
        //Vector3 coinEffectPos = basePos + Vector3.up * 1.5f + Vector3.right * -0.4f;

        GameObject coinEffect = Instantiate(getCoinEffect, basePos, Quaternion.identity);
        GetCoinEffect getCoinScript = coinEffect.GetComponent<GetCoinEffect>();
        getCoinScript.Init(nGold);
    }

    void PlaySettleStarEffect(GameObject cardStarPrefab)
    {
        ParticleSystem particleSystem;
        particleSystem = cardStarPrefab.transform.GetComponentInChildren<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

        ParticleSystem.ExternalForcesModule forcesModule = particleSystem.externalForces;
        ParticleSystem.TriggerModule triggerModule = particleSystem.trigger;
        //forcesModule.AddInfluence()

        foreach (ParticleSystemForceField forceField in forceFields)
            forcesModule.AddInfluence(forceField);

        Collider2D starDieCollider = starDieBox.GetComponent<BoxCollider2D>();
        triggerModule.AddCollider(starDieCollider);

        //Debug.Log("the star die box is: " + starDieBox);
        //Debug.Log("the star die collider is: " + starDieCollider);
    }

    //////////////////////////////////////////////////////
    // the following are functions for use PowerUPs
    //////////////////////////////////////////////////////

    public void UsePowerUP_ClearAll(GameDefines.PowerUPInfo powerUPInfo)
    {
        //1. instantiate the prefab,
        //2. set the origin position, then move to the center of the screen.
        //3. insert to handPokers
        //return;
        //GameObject clearAll = Instantiate();
        Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * 0.5f, -2.0f);
        //Vector3 posOffset = new Vector3(renderer.bounds.size.x * 0.0f, renderer.bounds.size.y * -0.4f, -1.0f);

        GameObject clearAll = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);
        //clearAll.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);

        //GameObject pokerInst = clearAll.transform.Find("Poker_Club_10").gameObject;
        clearAll.tag = "handCard";
        //int i = 0;

        int insertIndex = GetInsertPositionInHandPoker();
        GameObject insertGameObject = handPokers[insertIndex];
        HandPoker handScript = clearAll.GetComponent<HandPoker>();
        handScript.InitClearAllHandPoker(insertIndex, Trans.position, insertGameObject.transform.position, renderer.bounds.size, 0.0f);

        //test code, set suit and number for display
        //  PokerSuit suit = PokerSuit.Suit_Club;
        //int nNumber = 0;
        //handScript.Test_SetSuitNumber(suit, nNumber);

        GamePoker pokerScript = clearAll.GetComponent<GamePoker>();
        Destroy(pokerScript);

        //test code
        string strName = string.Format("hand_clear_all_{0}", insertIndex);
        pokerScript.Test_SetName(strName);
        clearAll.name = strName;

        //handPokers.Add(clearAll);
        handPokers.Insert(insertIndex, clearAll);
    }

    public bool UsePowerUP_RemoveThree(GameDefines.PowerUPInfo powerUPInfo)
    {
        //Debug.Log("-------------here we remove three cards... .........................");
        //1. get three poker, first choose the public poker that has been flipped, then choose the unflipped public poker
        //2. cut three poker.
        //List<GameObject> topPokers = new List<GameObject>();
        if (nRemoveThreeFinishCount >= 3)
            return true ;

        if (bIsRemovingThree)
            return false;

        bool bResult = true;
        removeThreePokers.Clear();
        List<GameObject> pokersWithItem = new List<GameObject>();

        bool bRet = GetTopPokerInfos(ref removeThreePokers);

        for(int i = removeThreePokers.Count-1; i >= 0; --i)
        {
            GamePoker gamePokerScript = removeThreePokers[i].GetComponent<GamePoker>();
            if(gamePokerScript.itemType != GameDefines.PokerItemType.None)
            {
                pokersWithItem.Add(removeThreePokers[i]);
                removeThreePokers.Remove(removeThreePokers[i]);
            }
        }

        if(removeThreePokers.Count == 0)
        {
            List<GameObject> pokersToRemove = new List<GameObject>();
            GetRemoveThreeFromUnflippedPoker(pokersWithItem, ref pokersToRemove, nRemoveThreeLeftCount);

            removeThreeCurrentIndex = 0;
            removeThreePokers = pokersToRemove;
            GamePoker gamePoker = removeThreePokers[removeThreeCurrentIndex].GetComponent<GamePoker>();
            bIsRemovingThree = true;
            gamePoker.RemoveThree();

            UpdateRemoveThreeScore(gamePoker);

            bResult = true;
        }
        else
        {
            if (removeThreePokers.Count >= nRemoveThreeLeftCount)
            {
                removeThreePokers.RemoveRange(nRemoveThreeLeftCount, removeThreePokers.Count - nRemoveThreeLeftCount);
                bResult = true;
                //nRemoveThreeLeftCount = 0;
            }
            else
            {
                bResult = false;
                nRemoveThreeLeftCount = nRemoveThreeLeftCount - removeThreePokers.Count;
            }

            removeThreePokers.Sort(delegate (GameObject go1, GameObject go2)
            {
                return go1.transform.position.x.CompareTo(go2.transform.position.x);
            });

            removeThreeCurrentIndex = 0;
            GamePoker gamePoker = removeThreePokers[removeThreeCurrentIndex].GetComponent<GamePoker>();
            bIsRemovingThree = true;
            gamePoker.RemoveThree();

            UpdateRemoveThreeScore(gamePoker);
        }

        return bResult;
    }

    public bool UsePowerUP_WildDrop(GameDefines.PowerUPInfo powerUPInfo)
    {
        for(int i = 0; i < 3; ++i)
        {
            Vector3 posOffset = Vector3.right * (i - 1) * 5f - Vector3.forward * 3.0f;
            Debug.Log("UsePowerUP_WildDrop   the offset is: " + posOffset);
            GameObject wildDrop = (GameObject)Instantiate(pokerCard, Trans.position + posOffset, Quaternion.identity);

            wildDrop.tag = "wildDrop";
            wildDrop.name = string.Format("wilddrop_{0}", i);

            GamePoker gamePoker = wildDrop.GetComponent<GamePoker>();
            gamePoker.InitWildDrop(i);

            HandPoker handPoker = wildDrop.GetComponent<HandPoker>();
            Destroy(handPoker);

            //todo: set the texture to wildcard

            //todo: set the animation controller to wildcard animation controller
            //GameObject
            //wildDrop.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(obj);
            wildDrop.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animations/FXWildCardAnim") ;

            //todo: set the animator trigger, "appear"

            wildDropPokers.Add(wildDrop);
        }

        return true;
    }

    public void UseClearAllPoker(HandPoker handPoker)
    {
        //1. get all top public poker
        //2. remove them outside of the screen.
        List<GameObject> topPokers = new List<GameObject>();
        //int nIndex = -1;
        GetAllFacingPoker(ref topPokers);
        //GetTopPokerInfos(ref topPokers);

        Debug.Log("here we clear all facing poker... the facing poker count is: " + topPokers.Count);

        handPoker.strClearAllPokerIDs = string.Format("{0}", handPoker.Index);

        bool bFirst = true;
        foreach (GameObject go in topPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            handPoker.strClearAllPokerIDs += string.Format("_{0}", gamePoker.Index);

            gamePoker.ClearAll(bFirst);

            bFirst = false;
        }

        //CheckCanFoldWildDrop(topPokers[0].GetComponent<GamePoker>());

        OpStepInfo stepInfo = opStepInfos.Peek(); 
        stepInfo.strClearAllPokerInfo = handPoker.strClearAllPokerIDs;

        QuickUpdateSteakStatus(topPokers);
    }

    void GetRemoveThreeFromUnflippedPoker(List<GameObject> pokerWithItem, ref List<GameObject> removePokers, int nCount)
    {
        //1. get all pokers that collapse with pokerWithItem
        //2. cull the pokers that has special item function
        //3. find nCount poker with max z value and return

        foreach (GameObject go in pokerWithItem)
        {
            GamePoker pokerScript = go.GetComponent<GamePoker>();
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();

            RaycastHit2D[] hitResults = Physics2D.BoxCastAll(collider.bounds.center, collider.size, pokerScript.pokerInfo.fRotation, Vector2.zero);

            for(int i = 0; i < hitResults.Length; ++i)
            {
                //GameObject hitObject = hitResults[i].collider.GetComponent<GameObject>();
                GameObject hitObject = hitResults[i].collider.gameObject;
                GamePoker hitScript = hitObject.GetComponent<GamePoker>();

                if(hitScript.itemType != GameDefines.PokerItemType.None
                    || hitResults[i].collider.tag == "lockArea")
                {
                    continue;
                }

                if(removePokers.Exists(t => t == hitObject))
                {
                    continue;
                }

                removePokers.Add(hitObject);
            }
        }

        removePokers.Sort(delegate (GameObject obj1, GameObject obj2) 
        {
            float fZ1 = obj1.GetComponent<Transform>().position.z;
            float fZ2 = obj2.GetComponent<Transform>().position.z;

            return fZ1.CompareTo(fZ2);
        });

        if(removePokers.Count > nCount)
        {
            removePokers.RemoveRange(nCount, removePokers.Count - nCount);
        }

    }

    void UpdateRemoveThreeScore(GamePoker gamePoker)
    {
        //nRemoveThreeLeftCount--;
        Debug.Log("------------------------------------------------\n UpdateRemoveThreeScore ... we remove gamepoker: " + gamePoker.name + "  left count is:  " + nRemoveThreeLeftCount);
        Debug.Log("------------------------------------------------");

        nStreakCount++;
        int nStepScore = ComputeOperationScore(nStreakCount);
        //int nStepGold = ComputeOperationGold();

        //AddOpStepInfo(opStepInfos.Count, nLastStreakCount, nColors, nStepScore, nStepGold, 1, streakType, nTotalComboCount);
        nTotalComboCount++;

        /*if (strLockInfo != null)
        {
            OpStepInfo stepInfo = opStepInfos.Peek();
            stepInfo.strLockInfo = strLockInfo;
        }
        if (nLockAreaID >= 0)
        {
            OpStepInfo stepInfo = opStepInfos.Peek();
            stepInfo.nLockAreaCleared = nLockAreaID;
        }*/

        nStreakBonusColor[nStreakBonusCount % Max_StreakBonus_Count] = (int)GetPokerColor(gamePoker);
        nStreakBonusCount++;
        //nCollectGold += nStepGold;
        nCollectScore += nStepScore;
        //nClearGold = 0;

        //gameplayUI.goldAndScoreUI.AddGold(nStepGold);
        gameplayUI.goldAndScoreUI.AddScore(nStepScore);

        //AddOpStepInfo(opStepInfos.Count, nStreakCount, nColors, GetCollectScore(), 1, StreakType.Add_Gold);

        //gameplayUI.SetStreakBonusStatus(nStreakCount, nColors);
        int nColors = GetStreakBonusEncode();
        //pengyuan 2021.9.15. here we need this statement, but we first comment it out.
        UpdateStreakStatus(nStreakCount, nColors);

        Debug.Log("UpdateRemoveThreeScore ... the score is: " + nStepScore + "  the streak count is: " + nStreakCount);
    }

    void GetMoreUnflipPublicPoker(ref List<GameObject> flipPokers, int nCount)
    {
        for(int i = 0; i < nCount; ++i)
        {
            foreach(GameObject go in publicPokers)
            {
                if(!flipPokers.Find(t => t.GetComponent<GamePoker>().Index == go.GetComponent<GamePoker>().Index))
                {
                    if (go.GetComponent<GamePoker>().itemType == GameDefines.PokerItemType.None)
                    {
                        flipPokers.Add(go);
                        break;
                    }
                }
            }
        }
    }

    public bool MoveToNextRemovePoker()
    {
        removeThreeCurrentIndex++;

        if(removeThreeCurrentIndex < removeThreePokers.Count)
        {
            GamePoker gamePoker = removeThreePokers[removeThreeCurrentIndex].GetComponent<GamePoker>();
            gamePoker.RemoveThree();

            UpdateRemoveThreeScore(gamePoker);

            return true;
        }
        else
        {
            //bHasFinishedOncePowerUPs = true;

            return false;
        }
    }

    public void FinishUsingOncePowerUPs()
    {
        //Debug.Log("------------------------------ 111111111 ---------------FinishUsingOncePowerUPs----------------");
        bHasFinishedOncePowerUPs = true;
    }

    /// <summary>
    /// 清除所有的激活中的trigger缓存
    /// </summary>
    public void ResetAllTriggers(Animator animator)
    {
        AnimatorControllerParameter[] aps = animator.parameters;
        for (int i = 0; i < aps.Length; i++)
        {
            AnimatorControllerParameter paramItem = aps[i];
            if (paramItem.type == AnimatorControllerParameterType.Trigger)
            {
                string triggerName = paramItem.name;
                bool isActive = animator.GetBool(triggerName);
                if (isActive)
                {
                    animator.ResetTrigger(triggerName);
                }
            }
        }
    }

    //pengyuan 2021.9.22 added, to process clearing the locks and lockareas...
    //get active locks groups, then clear them temporarily.
    public void UseClearAllSecondStage(HandPoker handPoker)
    {
        List<int> workingLockGroups = new List<int>();

        GetWorkingLockGroups(ref workingLockGroups);

        if (workingLockGroups.Count > 0)
        {
            string strInfo = string.Format("{0}", workingLockGroups.Count);

            for (int i = 0; i < workingLockGroups.Count; ++i)
            {
                bool bLeft = ClearAll_ClearLockArea(workingLockGroups[i]);
                ClearAll_ClearLockGroup(workingLockGroups[i], bLeft);
                
                strInfo += string.Format("_{0}", workingLockGroups[i]);
            }

            opStepInfos.Peek().strClearAllLockInfos = strInfo;
        }
    }

    public void UpdateUseClearAllStepInfo(HandPoker handPoker)
    {
        foreach(OpStepInfo stepInfo in opStepInfos)
        {
            if(stepInfo.strCardName == handPoker.name)
            {
                stepInfo.strIDs = handPoker.strClearAllPokerIDs;
                break;
            }
        }
    }

    public void InsertWildDrop()
    {
        //1. select 3 random position(3 random public poker), then sort the position by x coordinate, then 
        Random.InitState((int)System.DateTime.Now.Ticks);

        int nInitRandom = Random.Range(0, publicPokers.Count);
        int nOffset = (int)publicPokers.Count / 3;

        for(int i = 0; i < 3; ++i)
        {
            int nPokerIndex = (nInitRandom + nOffset * i) % publicPokers.Count;

            GameObject insertObj = publicPokers[nPokerIndex];
            GamePoker gamePoker = insertObj.GetComponent<GamePoker>();

            Vector3 topPosition = insertObj.transform.position;
            bool bUP = (topPosition.x - wildDropPokers[i].transform.position.x) > 0.0 ? true : false;
            topPosition.y = GetWildDropInsertPosY(gamePoker, bUP);
            topPosition.z = insertObj.transform.position.z + 0.025f;

            Vector3 endPos;// = insertObj.transform.position;

            GameObject collapseObj = GetInsertWildDropBackPoker(insertObj);
            if(collapseObj == null)
            {
                endPos = insertObj.transform.position + Vector3.forward * 0.0125f;
                wildDropPokers[i].GetComponent<GamePoker>().pokerInfo.fRotation = insertObj.GetComponent<GamePoker>().pokerInfo.fRotation + 10.0f;
            }
            else
            {
                endPos = (insertObj.transform.position + collapseObj.transform.position) / 2;// - Vector3.forward * 0.0125f ;
                wildDropPokers[i].GetComponent<GamePoker>().pokerInfo.fRotation = (insertObj.GetComponent<GamePoker>().pokerInfo.fRotation + collapseObj.GetComponent<GamePoker>().pokerInfo.fRotation) / 2;
            }

            wildDropPokers[i].GetComponent<GamePoker>().WildDropInsertFirstStage(topPosition, endPos, bUP);
        }
    }

    float GetWildDropInsertPosY(GamePoker insertObject, bool bUP)
    {
        if (bUP)
            return insertObject.pokerInst.GetComponent<Renderer>().bounds.max.y + 0.5f;
        else
            return insertObject.pokerInst.GetComponent<Renderer>().bounds.min.y - 0.5f;
    }

    GameObject GetInsertWildDropBackPoker(GameObject insertObject)
    {
        GamePoker pokerScript = insertObject.GetComponent<GamePoker>();
        BoxCollider2D collider = insertObject.GetComponent<BoxCollider2D>();

        if (pokerScript.pokerInst.GetComponent<MeshRenderer>().enabled == false)
        {
            return null;
        }

        GameObject collapseObject = null;
        RaycastHit2D[] hitResults = Physics2D.BoxCastAll(collider.bounds.center, collider.size, pokerScript.pokerInfo.fRotation, Vector2.zero);
        if (hitResults.Length > 0)
        {
            float fMaxZ = insertObject.transform.position.z;

            for(int i = 0; i < hitResults.Length; ++i)
            {
                if (hitResults[i].transform.tag != "poker")
                    continue;

                if (hitResults[i].transform.position.z > fMaxZ)
                {
                    collapseObject = hitResults[i].collider.gameObject;
                    fMaxZ = hitResults[i].transform.position.z;
                }
            }
        }

        return collapseObject;
    }

    public void CheckCanFoldWildDrop(GamePoker gamePokerScript)
    {
        Debug.Log("CheckCanFoldWildDrop... the wilddrop poker count is: " + wildDropPokers.Count);

        //Reward.Coin += 10000;
        List<GameObject> topPokers = new List<GameObject>();
        List<GameObject> wildDrops = new List<GameObject>();

        if (GetTopPokerInfos(ref topPokers))
        {
            foreach (GameObject go in topPokers)
            {
                GamePoker gamePoker = go.GetComponent<GamePoker>();
                if(gamePoker.itemType == GameDefines.PokerItemType.WildDrop)
                {
                    wildDrops.Add(go);
                    Debug.Log("CheckCanFoldWildDrop... the wilddrop poker name is: " + go.name);
                }
            }

            StartCoroutine(FoldWildDropPoker(wildDrops, gamePokerScript));

            /*for(int i = 0; i < wildDrops.Count; ++i)
            {
                GamePoker gamePoker = wildDrops[i].GetComponent<GamePoker>();
                if (i == 0)
                {
                    gamePoker.FoldPoker(foldPoker.Count);
                    foldPoker.Push(wildDrops[i]);    
                }
                else
                {
                    gamePoker.FoldWildDropPoker(foldPoker.Count);
                }

                gamePokerScript.AddAttachedWildDrop(wildDrops[i]);
                RemoveOnePublicPoker(wildDrops[i]);
            }*/
        }
        
    }

    IEnumerator FoldWildDropPoker(List<GameObject> pokerList, GamePoker gamePokerScript)
    {
        float fStartTime = Time.time;

        for (int i = 0; i < pokerList.Count; ++i)
        {
            GamePoker gamePoker = pokerList[i].GetComponent<GamePoker>();
            if (i == 0)
            {
                gamePoker.FoldPoker(foldPoker.Count);
                foldPoker.Push(pokerList[i]);

                RemoveOnePublicPoker(pokerList[i]);

                gamePoker.wildSource = GameDefines.WildCardSource.None;
            }
            else
            {
                gamePoker.FoldWildDropPoker(foldPoker.Count);

                gamePoker.wildSource = GameDefines.WildCardSource.WildDrop;
            }

            gamePokerScript.AddAttachedWildDrop(pokerList[i]);
            //RemoveOnePublicPoker(pokerList[i]);

            while( Time.time - fStartTime < 0.8f * (i+1))
            {
                yield return null;
            }

            if(i >= 1)
            {
                gameplayUI.IncWildCard();
                Reward.Data[RewardType.WildCard]++;
                //gamePoker.wildSource = GameDefines.WildCardSource.WildDrop;
            }
            else
            {
                ;//gamePoker.wildSource = GameDefines.WildCardSource.None;
            }
        }
    }

    public void AddPowerUP_ClearScore()
    {
        AddCollectScore(nSingleClearScore);
    }

    public void AddCollectScore(int nScore)
    {
        nCollectScore += nScore;
        gameplayUI.AddScore(nScore);
    }

    /*public void CheckCanFoldWildDrop(List<GameObject> pokerList)
    {
        Debug.Log("CheckCanFoldWildDrop... input a list... the wilddrop poker count is: " + wildDropPokers.Count);

        
        List<GameObject> wildDrops = new List<GameObject>();

        foreach(GameObject go in pokerList)
        {
            List<GameObject> topPokers = new List<GameObject>();
            if (GetTopPokerInfos(ref topPokers))
            {
                foreach (GameObject go1 in topPokers)
                {
                    GamePoker gamePoker = go1.GetComponent<GamePoker>();
                    if (gamePoker.itemType == GameDefines.PokerItemType.WildDrop 
                        && !wildDrops.Exists( t => t.name == go1.name ))
                    {
                        wildDrops.Add(go1);
                    }
                }
            }
        }
    }*/

    void CheckPowerUP_Clear()
    {
        if (!powerUpProcess.HasPowerUP_ClearAscDes() && !powerUpProcess.HasPowerUP_ClearBomb())
            return;

        foreach (GameObject go in publicPokers)
        {
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            if(gamePoker && gamePoker.pokerFacing == PokerFacing.Facing)
            {
                gamePoker.CheckClearPowerUPs();
            }
        }
    }

    public void CheckPowerUP_ClearLock()
    {
        if (!powerUpProcess.HasPowerUP_ClearLock())
            return;

        if (lockGroups.Count == 0)
            return;

        List<int> workingLockGroups = new List<int>();

        GetWorkingLockGroups(ref workingLockGroups);

        if (workingLockGroups.Count > 0)
        {
            //string strInfo = string.Format("{0}", workingLockGroups.Count);

            for (int i = 0; i < workingLockGroups.Count; ++i)
            {
                bool bLeft = true;

                for (int j = 0; j < lockAreas.Count; ++j)
                {
                    if (lockAreas[j].nGroupID == workingLockGroups[i])
                    {
                        LockArea lockArea = lockAreas[j].Area.GetComponent<LockArea>();
                        bLeft = lockArea.PowerUP_ClearLockArea();
                        break;
                    }
                }

                for (int j = 0; j < lockGroups.Count; ++j)
                {
                    if (lockGroups[j].nGroupID == workingLockGroups[i])
                    {
                        for (int k = 0; k < lockGroups[j].locks.Count; ++k)
                        {
                            GameLock gameLock = lockGroups[j].locks[k].GetComponent<GameLock>();
                            gameLock.PowerUP_ClearLock(bLeft);
                        }

                        lockGroups.RemoveAt(j);

                        break;
                    }
                }
                //bool bLeft = ClearAll_ClearLockArea(workingLockGroups[i]);
                //ClearAll_ClearLockGroup(workingLockGroups[i], bLeft);

                //strInfo += string.Format("_{0}", workingLockGroups[i]);
            }

            //opStepInfos.Peek().strClearAllLockInfos = strInfo;
        }
    }

    public void PowerUP_ClearOneLock(int nGroupID, GameObject lockObject)
    {
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == nGroupID)
            {
                lockGroups[i].locks.Remove(lockObject);
                break;
            }
        }
    }

    public void PowerUP_ClearLockArea(int nGroupID)
    {
        for (int i = 0; i < lockAreas.Count; ++i)
        {
            if (lockAreas[i].nGroupID == nGroupID)
            {
                lockAreas.RemoveAt(i);
                
                break;
            }
        }
    }

}

