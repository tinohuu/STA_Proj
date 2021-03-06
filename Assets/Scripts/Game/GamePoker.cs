using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePoker : MonoBehaviour
{
    public int Index { get; set; }

    public GameObject pokerInst = null;

    Animator animator = null;

    public GameDefines.PokerStatus pokerStatus = GameDefines.PokerStatus.None;

    public GameDefines.WildCardSource wildSource = GameDefines.WildCardSource.None;

    public JsonReadWriteTest.PokerInfo pokerInfo { get; set; }
    public GameplayMgr.PokerType pokerType { get; set; }

    //when withdraw a poker, use this to see whether this poker is facing or backing
    public GameplayMgr.PokerFacing pokerFacing;// { get; set; }

    Vector3 _ascendPos;

    Vector3 _bombPos;

    Vector3 _addNPos;

    //pengyuan 2021.8.31 the ascend poker and descend poker effect
    GameObject ascendEffect = null;

    GameObject ascendEffectEntity = null;

    //pengyuan 2021.9.1, the following are used for bomb process
    GameObject bombEffect = null;
    int nBombStep = 1;
    Image bombDigitImage;
    Image bombUnitImage;
    Image bombSingleImage;
    Animator bombAnim;

    //pengyuan 2021.9.6, the following are used for add n poker process
    GameObject addNEffect = null;
    public int nAddNCount = 1;
    public int nAddNOriginCount = 1;
    Image addNPlusImage;    //+
    Image addNUnitImage;    //number
    Animator addNAnim;
    public string strHandPokerIDs = "";

    //pengyuan 2021.9.17 add for clear all poker
    bool bClearAll = false;

    //pengyuan 2021.9.23 added for wild drop
    bool bWildDrop = false;

    public Vector3 originPos;
    public Vector3 targetPos;
    int nFoldIndex = -1;
    float fTime;

    //pengyuan 2021.9.3 added for dismiss the poker
    public Vector3 dismissPos;

    float fFlipTime = 0.0f;

    static float fRotateTime = 1.5f;
    static float fFlipTotalTime = 0.4f;
    static float fFoldTotalTime = 0.8f;

    public string strName { get; set; }//this is for test
    float screenX = 0.0f;
    TextMesh textName;

    public bool bHasWithdrawed { get; set; } = false;
    bool bIsWithdrawing = false;
    float fWithdrawTime = 0.0f;
    static float fWithdrawTotalTime = 0.4f;

    bool bFlip { get; set; } = false;
    public bool bIsFlipping { get; set; } = false;

    bool bUnFlip { get; set; } = false;

    float fFoldTime = 0.0f;
    bool bFold { get; set; } = false;
    public bool bIsFolding { get; set; } = false;

    //2021.9.7 added by pengyuan
    public bool bAddNPoker { get; set; } = false;

    public bool bIsAddingNPoker = false;

    public bool bAccelAddingNPoker = false;

    bool bDismiss = false;
    float fDismissTime = 0.0f;
    static float fDismissTotalTime = 1.5f;

    //data
    public GameplayMgr.PokerSuit pokerSuit;

    public GameplayMgr.PokerColor pokerColor;

    public GameDefines.PokerItemType itemType = GameDefines.PokerItemType.None;

    public int nPokerNumber { get; set; }//this is the original poker number
    public int nOriginNumber { get; set; }

    //2021.8.11 added for foldinga game poker, translate and rotate in detail
    Vector3 foldPeekPoint;
    Vector3 foldSecondPoint;
    Vector3 secondVelocity;
    float fSecondSpeed = 0.1f;
    float fFoldFirstStage = 0.5f;
    float fFoldSecondStage = 0.2f;

    int testIdnex = 0;
    public GameObject scoreStarPrefab;

    int nWildDropIndex = -1;

    List<GameObject> wildDrops = new List<GameObject>();

    //SimpleCurveHelper curveHelper;

    //Collider2D

    //Transform trans;

    private void Awake()
    {
        //if (transform == null)
        //    Debug.Log("transform is null---------------------------");

        //pokerInst = transform.Find("Poker_Club_10").gameObject;
        //Transform childTrans = transform.Find("Poker_Club_10");

        //Debug.Log("child trans is: " + childTrans);
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreStarPrefab = (GameObject)Resources.Load("ScoreStar");

        //transform = GetComponent<Transform>();
        /*curveHelper = gameObject.AddComponent<SimpleCurveHelper>();

        if (curveHelper == null)
            Debug.Log("@@@@ we init curveHelper error!!! please check your code!");*/
        //trans = GetComponent<MeshFilter>().transform;
    }

    public void Init(GameObject goPrefab, JsonReadWriteTest.PokerInfo info, int nIndex, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        //Debug.Log("game poker init function ,  here we set the pokerinfo class... ... ... ");
        pokerInst = transform.Find("Poker_Club_10").gameObject;
        animator = GetComponent<Animator>();
        //trans = pokerInst.GetComponent<MeshFilter>().transform;

        Index = nIndex;
        nFoldIndex = nIndex;
        originPos = transform.position;

        _ascendPos = pos + new Vector3(0.0f, rendererSize.y * 0.5f, 0.0f);
        _bombPos = pos + new Vector3(0.0f, rendererSize.y * 0.5f, 0.0f) + new Vector3(-0.3f, -0.5f, 0.0f);
        _addNPos = pos + new Vector3(0.0f, rendererSize.y * 0.5f, 0.0f);

        pokerType = GameplayMgr.PokerType.PublicPoker;
        pokerFacing = GameplayMgr.PokerFacing.Backing;

        pokerInfo = info;
        targetPos.x = pos.x + pokerInfo.fPosX * 0.01f;
        targetPos.y = pos.y + pokerInfo.fPosY * 0.01f;
        targetPos.z = pos.z - 1.0f - nIndex * 0.01f;

        dismissPos = targetPos - Vector3.up * 8.0f - Vector3.right * 8.0f ;

        //Debug.Log("GamePoker:;Init... the target pos z is: " + targetPos.z);

        fTime = fBeginTime;

        Transform textTrans = pokerInst.gameObject.transform.Find("Text");
        textName = textTrans.GetComponent<TextMesh>();
        textName.text = "测试代码";

        foldPeekPoint = Vector3.zero;
        foldSecondPoint = Vector3.zero;

        //this canvas is used to display ui animation, ascending, descending poker.
        if(pokerInfo.nItemType != (int)GameDefines.PokerItemType.None)
        {
            //gameObject.AddComponent<Canvas>();
            pokerInst.gameObject.AddComponent<Canvas>();
        }
        
        itemType = (GameDefines.PokerItemType)info.nItemType;

        //2021.8.31 for ascending and descending poker ...
        InitItemEffect(info);

    }

    public void InitWildDrop(int dropIndex)
    {
        bWildDrop = true;

        itemType = GameDefines.PokerItemType.WildDrop;
        //wildSource = GameDefines.WildCardSource.WildDrop;

        pokerInst = transform.Find("Poker_Club_10").gameObject;
        animator = GetComponent<Animator>();

        nWildDropIndex = dropIndex;

        pokerInfo = new JsonReadWriteTest.PokerInfo();
        pokerInfo.fPosX = transform.position.x;
        pokerInfo.fPosY = transform.position.y;
        pokerInfo.fRotation = 0.0f;
        pokerInfo.nGroupID = 0;
        pokerInfo.nItemType = (int)GameDefines.PokerItemType.WildDrop;
        pokerInfo.strItemInfo = "";

        //targetPos = transform.position;

        pokerType = GameplayMgr.PokerType.PublicPoker;
        pokerFacing = GameplayMgr.PokerFacing.Backing;

        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex", GameplayMgr.Instance.wildCardTexture);
        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.wildCardTexture);

    }

    // Update is called once per frame
    void Update()
    {
        fTime += Time.deltaTime;

        if (!bFlip && !bFold && !bUnFlip && !bDismiss && !bAddNPoker && !bClearAll && !bWildDrop)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.08f);

            Quaternion quatFrom = Quaternion.Euler(-90.0f, 0.0f, -90.0f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, pokerInfo.fRotation);

            if (!bHasWithdrawed)
            {
                if(itemType == GameDefines.PokerItemType.Add_N_Poker)
                {
                    quatFrom = Quaternion.Euler(0.0f, 180.0f, 90.0f);
                    quatTo = Quaternion.Euler(0.0f, 180.0f, pokerInfo.fRotation);
                }
                transform.rotation = Quaternion.Lerp(quatFrom, quatTo, fTime / fRotateTime);

                if (fTime > fRotateTime && !bFlip && Mathf.Abs(pokerInfo.fRotation) > 0.1f)
                {
                    transform.rotation = Quaternion.Lerp(quatFrom, quatTo, fTime / fRotateTime);
                }
            }
            else
            {
                //Debug.Log(strName + " is withdrawed, the target pos is: " + targetPos);
                quatTo = Quaternion.Euler(0.0f, 180.0f, -pokerInfo.fRotation);
                
                transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fTime / fRotateTime);
            }

            if(fTime > fRotateTime)
            {
                if(itemType == GameDefines.PokerItemType.Add_N_Poker && addNEffect != null)
                {
                    addNEffect.SetActive(true);
                }
            }

            //the following code are for test alpha fade
            /*MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            Color meshColor = meshRenderer.material.color;
            int nTime = (int)Time.time;
            float alphaValue = Time.time - nTime;
            meshColor.a = alphaValue;
            if(gameObject.name == "poker_9")
            Debug.Log("the alpha value is: " + alphaValue);*/

            //fFlipTime += Time.deltaTime;

            /*if (fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;
            }*/
        }

        if(bFlip && bIsFlipping && !bAddNPoker)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.08f);

            fFlipTime += Time.deltaTime;
            Quaternion quatTo = Quaternion.Euler(0.0f, 180.0f, -pokerInfo.fRotation);

            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);

            if(fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;
                pokerFacing = GameplayMgr.PokerFacing.Facing;

                UpdatePokerItemDisplay(true);
                CheckClearPowerUPs();

                //2021.8.26 added by pengyaun to adjust the rotation of the poker.
                /*if(Mathf.Abs(pokerInfo.fRotation) > 0.1f)
                {
                    quatTo = Quaternion.Euler(0.0f, 180.0f, -pokerInfo.fRotation);
                    transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);
                }*/
            }
        }

        if(bUnFlip && bIsFlipping)
        {
            fFlipTime += Time.deltaTime;

            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, pokerInfo.fRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);

            if (fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;
                pokerFacing = GameplayMgr.PokerFacing.Backing;
                UpdatePokerItemDisplay(false);
            }
        }

        if(bFold)
        {
            fFoldTime += Time.deltaTime;

            Vector3 pos = GameplayMgr.Instance.GetFoldPokerPosition();

            //Vector3 oldPos = trans.position;
            Vector3 oldPos = transform.position;
            //transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

            if (fFoldTime >= fFoldTotalTime && bIsFolding)
            {
                bIsFolding = false;
                transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

                if (itemType != GameDefines.PokerItemType.WildDrop)
                    GameplayMgr.Instance.CheckCanFoldWildDrop(this);
                if (wildSource == GameDefines.WildCardSource.WildDrop)
                {
                    Debug.Log("---------------------pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = false... ... ... ");
                    pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = false;
                }

            }
        }

        if(bIsWithdrawing)
        {
            fWithdrawTime += Time.deltaTime;
            if(fWithdrawTime >= fWithdrawTotalTime)
            {
                bIsWithdrawing = false;

                //GameplayMgr.Instance.UnflipAllCoveredGamePoker(gameObject);
                //GameplayMgr.Instance.nWithdrawCount --;
            }
        }

        if(bDismiss)
        {
            fDismissTime += Time.deltaTime;
            if(fDismissTime >= fDismissTotalTime)
            {
                bDismiss = false;
                Destroy(gameObject);
            }
        }
    }

    public void SetFoldIndex(int nIndex)
    {
        nFoldIndex = nIndex;
    }

    public void Test_SetName(string name)
    {
        strName = name;
    }

    public void Test_SetSuitNumber(GameplayMgr.PokerSuit suit, int nNumber)
    {
        pokerSuit = suit;
        pokerColor = GameplayMgr.Instance.GetPokerColor(pokerSuit);

        if (nNumber == 0)
        {
            nPokerNumber = 13;
        }
        else
        {
            nPokerNumber = nNumber;
        }
        
        nOriginNumber = nPokerNumber;
        //Debug.Log("GamePoker::Test_SetSuitNumber... the suit is: " + suit + "the name is: " + gameObject.name);

        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);

        //Texture2D texture2D = Resources.Load<Texture2D>();
        //Texture2D texture2D = Resources.Load("HeartA") as Texture2D;
        //Texture2D texture2D = Resources.Load("back1") as Texture2D;
        //GetComponent<Renderer>().material.SetTexture("_MainTex", texture2D);
        //string strPokerName = string.Format("Poker/Poker_{0:3D}", nNumber + 1);

        int textureIndex = ((int)suit - 1) * 13 + nPokerNumber - 1;
        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
        //int offsetX = textureIndex * 142;
        //GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerAtlas);
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(textureIndex * 142, 0));
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(0.0625f, 0f));
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(GameplayMgr.Instance.pokerRects[textureIndex].xMin, GameplayMgr.Instance.pokerRects[textureIndex].yMin));
    }


    //pengyuan 2021.9.6 for init add n poker
    public void Test_SetSuitNumberAddNPoker()
    {
        pokerSuit = GameplayMgr.PokerSuit.Suit_Club;
        pokerColor = GameplayMgr.PokerColor.Black;

        nPokerNumber = 1;
        nOriginNumber = nPokerNumber;

        textName.text = "";

        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.addNTexture);
    }

    //pengyuan 2021.9.1 added for update ascend and descend poker status
    public void UpdateAscendDescendStatus(string foldPokerName, bool bWithdraw = false)
    {
        if(pokerFacing != GameplayMgr.PokerFacing.Facing)
        {
            return;
        }

        if(foldPokerName == gameObject.name)
        {
            return;
        }

        if(itemType == GameDefines.PokerItemType.Ascending_Poker)
        {
            int nDeltaIndex = 1;
            if (bWithdraw)
            {
                nDeltaIndex = 12;
            }
            nPokerNumber = (nPokerNumber + nDeltaIndex) % 13;
            if(nPokerNumber == 0)
            {
                nPokerNumber = 13;
            }

            int textureIndex = ((int)pokerSuit - 1) * 13 + nPokerNumber - 1;
            pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
        }
        if(itemType == GameDefines.PokerItemType.Descending_Poker)
        {
            int nDeltaIndex = 12;
            if (bWithdraw)
            {
                nDeltaIndex = 1;
            }

            nPokerNumber = (nPokerNumber + nDeltaIndex) % 13;
            if (nPokerNumber == 0)
            {
                nPokerNumber = 13;
            }

            int textureIndex = ((int)pokerSuit - 1) * 13 + nPokerNumber - 1;
            pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
        }
    }

    //pengyuan 2021.9.1 added for update bomb poker status
    public void UpdateBombStatus(string foldPokerName, bool bWithdraw = false)
    {
        if(bWithdraw)
            Debug.Log("here we update bomb status withdraw is true");

        if (pokerFacing != GameplayMgr.PokerFacing.Facing)
        {
            return;
        }

        if (foldPokerName == gameObject.name)
        {
            if (bWithdraw)
            {
                //Debug.Log("UpdateBombStatus... bombAnim.SetBool(Die, false);");
                bombAnim.SetBool("Die", false);
                //Debug.Log("---------UpdateBombStatus--------------bombAnim.SetBool(Die, false);----------------------");
            }

            return;
        }

        if(bWithdraw)
        {
            nBombStep += 1;
            bombAnim.SetTrigger("Swing");

            if(nBombStep > 5)
                bombAnim.SetBool("BlastIdle", false);

        }
        else
        {
            nBombStep = (nBombStep <= 1) ? 0 : (nBombStep - 1);

            Debug.Log("-------------------nBombStep is -------------------------------------: " + nBombStep);

            if (nBombStep >= 10)
                bombAnim.SetTrigger("StepDecDigit");
            else
                bombAnim.SetTrigger("StepDecUnit");

            if(nBombStep == 5)
                bombAnim.SetBool("BlastIdle", true);

            bombAnim.SetInteger("BombStep", nBombStep);

            if(nBombStep <= 0)
            {
                GameplayMgr.Instance.OnBombEndGame();
            }
            else
            {
                SoundManager.Instance.PlaySFX("gameBombCountdown");
            }
            /*if (nBombStep == 0)
                Debug.Log("-------------------nBombStep is 0-------------------------------------");*/
        }

        UpdateBombStepDisplay();

    }

    void InitItemEffect(JsonReadWriteTest.PokerInfo pokeInfo)
    {
        ascendEffect = null;
        ascendEffectEntity = null;
        bombEffect   = null;
        addNEffect   = null;

        switch (itemType)
        {
            case GameDefines.PokerItemType.Ascending_Poker:
                ascendEffect = Instantiate(GameplayMgr.Instance.ascendingPrefab, _ascendPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
                PostInitAscDesEffect(true);
                break;
            case GameDefines.PokerItemType.Descending_Poker:
                ascendEffect = Instantiate(GameplayMgr.Instance.descendingPrefab, _ascendPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
                PostInitAscDesEffect(false);
                break;
            case GameDefines.PokerItemType.Bomb:
                bombEffect = Instantiate(GameplayMgr.Instance.bombPrefab, _bombPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
                PostInitBombEffect(pokeInfo);
                break;
            case GameDefines.PokerItemType.Add_N_Poker:
                addNEffect = Instantiate(GameplayMgr.Instance.addNPrefab, _addNPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
                PostInitAddNEffect(pokeInfo);
                break;
            default:break;
        }
    }

    void PostInitAscDesEffect(bool bAscend)
    {
        ascendEffect.transform.SetParent(pokerInst.GetComponent<Canvas>().transform);
        ascendEffect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        ascendEffect.SetActive(false);

        //ascendEffectEntity = ascendEffect.transform.Find("ChangeUp").GetComponent<GameObject>();
        if(bAscend)
            ascendEffectEntity = ascendEffect.transform.Find("ChangeUp").gameObject;
        else
            ascendEffectEntity = ascendEffect.transform.Find("ChangeDown").gameObject;
    }

    void PostInitAddNEffect(JsonReadWriteTest.PokerInfo pokeInfo)
    {
        nAddNCount = int.Parse(pokerInfo.strItemInfo);
        nAddNOriginCount = nAddNCount;

        //addNEffect.gameObject.layer = 0;
        addNEffect.GetComponent<ItemPlusCallBack>().rootObject = gameObject;
        addNEffect.transform.SetParent(pokerInst.GetComponent<Canvas>().transform);
        addNEffect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        pokerInst.GetComponent<Canvas>().sortingLayerName = "Environment";

        Transform numberTrans = addNEffect.transform.Find("FXPlus/FXCard/Group/Plus");
        if (numberTrans == null)
            Debug.Log("addNEffect... we cann't find number object transform...");

        GameObject mountPlus = numberTrans.gameObject;
        addNPlusImage = mountPlus.GetComponent<Image>();

        numberTrans = addNEffect.transform.Find("FXPlus/FXCard/Group/unitNum");
        GameObject mountUnit = numberTrans.gameObject;
        addNUnitImage = mountUnit.GetComponent<Image>();

        addNEffect.SetActive(false);

        InitAddNEffectDisplay();
    }

    void InitAddNEffectDisplay()
    {
        SetAddNDigitDisplay();

        pokerFacing = GameplayMgr.PokerFacing.Facing;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, -pokerInfo.fRotation);

        addNAnim = addNEffect.GetComponent<Animator>();
    }

    public void UpdateAddNEffectDisplay()
    {
        SetAddNDigitDisplay();

        pokerFacing = GameplayMgr.PokerFacing.Facing;
        transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        addNAnim = addNEffect.GetComponent<Animator>();
    }

    void SetAddNDigitDisplay()
    {
        if (nAddNCount > 0)
        {
            addNPlusImage.sprite = GameplayMgr.Instance.addNNumbers[10];
            addNUnitImage.sprite = GameplayMgr.Instance.addNNumbers[nAddNCount];
        }
        else
        {
            //Color colorEmpty = addNPlusImage.color;
            //colorEmpty.a = 0.0f;
            addNPlusImage.enabled = false;
            addNUnitImage.enabled = false;
        }
    }

    void PostInitBombEffect(JsonReadWriteTest.PokerInfo pokeInfo)
    {
        //todo:...
        nBombStep = int.Parse(pokeInfo.strItemInfo);

        CheckCorrectBombStepCount();

        bombEffect.transform.SetParent(pokerInst.GetComponent<Canvas>().transform);
        bombEffect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        bombAnim = bombEffect.GetComponent<Animator>();
        //AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        //Debug.Log("the anim is: " + anim + " the clip info count is: " + anim.GetCurrentAnimatorStateInfo(0));

        
        Transform numberTrans = bombEffect.transform.Find("FXBomb/FXBomb/Group/digitNum");
        if (numberTrans == null)
            Debug.Log("we cann't find number object transform...");

        GameObject mountDigit = numberTrans.gameObject;
        bombDigitImage = mountDigit.GetComponent<Image>();

        numberTrans = bombEffect.transform.Find("FXBomb/FXBomb/Group/unitNum");
        GameObject mountUnit = numberTrans.gameObject;
        bombUnitImage = mountUnit.GetComponent<Image>();

        numberTrans = bombEffect.transform.Find("FXBomb/FXBomb/singleNum");
        GameObject mountSingle = numberTrans.gameObject;
        bombSingleImage = mountSingle.GetComponent<Image>();

        bombEffect.SetActive(false);

        UpdateBombStepDisplay();
    }

    void UpdateBombStepDisplay()
    {
        if(nBombStep >= 10)
        {
            int nDigitIndex = nBombStep / 10;
            int nUnitIndex = nBombStep % 10;

            bombDigitImage.sprite = GameplayMgr.Instance.bombNumbers[nDigitIndex];
            bombUnitImage.sprite = GameplayMgr.Instance.bombNumbers[nUnitIndex];

            bombDigitImage.enabled = true;
            bombUnitImage.enabled = true;
            bombSingleImage.enabled = false;
        }
        else
        {
            bombSingleImage.sprite = GameplayMgr.Instance.bombNumbers[nBombStep];

            bombSingleImage.enabled = true;
            bombDigitImage.enabled = false;
            bombUnitImage.enabled = false;
        }
        
    }

    void CheckCorrectBombStepCount()
    {
        nBombStep = nBombStep < 1 ? 1 : nBombStep;
        nBombStep = nBombStep > 50 ? 50 : nBombStep;
    }

    public void Test_DisableDisplayText()
    {
        textName.text = "";
    }

    public void Test_EnableDisplayText()
    {
        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);
    }

    //pengyuan 2021.9.7 this method is used to process the add n poker game logic, 
    public bool AddNPoker()
    {
        if (bIsWithdrawing)
            return false;

        bFlip = false;
        bFold = false;
        bUnFlip = false;

        bAddNPoker = true;
        strHandPokerIDs = "";

        transform.DOMove(GameplayMgr.Instance.Trans.position - Vector3.forward * 2.0f, 0.5f);

        GameplayMgr.Instance.ResetAllTriggers(addNAnim);
        addNAnim.SetTrigger("PlusMove");

        Debug.Log("----------------------here we add n poker , set the plusmove trigger...------------------the name is: " + gameObject.name + " the nAddNCount is: " + nAddNCount);

        bIsAddingNPoker = true;
        pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = false;

        return true;
    }

    public void OnAddNPokerExit()
    {
        bIsAddingNPoker = false;
        bAccelAddingNPoker = false;

        //gameObject.transform.position = originPos;
    }

    //public void Withdraw

    public void AddNPokerOneID(int nID)
    {
        if(strHandPokerIDs.Length == 0)
        {
            strHandPokerIDs = string.Format("{0}_{1}", Index, nID);
        }
        else
        {
            strHandPokerIDs += string.Format("_{0}", nID);
        }
    }

    //we call this method to flip one poker
    //public void FlipPoker() => gameObject.transform.RotateAround(Vector3.up, 180.0f);
    public void FlipPoker()
    {
        if(bFlip)
        {
            return;
        }

        //Debug.Log("here we flip the poker, name is: " + gameObject.name + "  depth is: " + transform.position.z + " time is : " + Time.time);
        
        bFlip = true;
        bUnFlip = false;
        bIsFlipping = true;
        fFlipTime = 0.0f;

        //2021.8.10 added by pengyuan for testing the z value
        //pokerInst.transform.position -= Vector3.forward * 1.0f;

        pokerFacing = GameplayMgr.PokerFacing.Facing;
        UpdatePokerItemDisplay(true);

        //CheckClearBomb();
    }

    public void UnflipPoker()
    {
        //Debug.Log("GamePoker::UnflipPoker... we UnflipPoker, the name is: " + name + "  number is: " + nPokerNumber);

        bFlip = false;
        bFold = false;
        bUnFlip = true;
        bIsFlipping = true;
        fFlipTime = 0.0f;

        //2021.8.10 added by pengyuan for testing the z value
        //pokerInst.transform.position += Vector3.forward * 1.0f;

        pokerFacing = GameplayMgr.PokerFacing.Backing;
        UpdatePokerItemDisplay(false);
    }

    public void FoldPoker(int nIndex)
    {
        if(bFold)
        {
            return;
        }

        Debug.Log("here we fold a poker, name is: " + gameObject.name + "  number is: " + nPokerNumber);
        
        bFold = true;
        bIsFolding = true;
        fFoldTime = 0.0f;
        fFoldTotalTime = 0.8f;

        nFoldIndex = nIndex;

        //2021.9.1 added by pengyuan to disable the ascend and descend display
        DisablePokerItemDisplay();
        
        //2021.8.11 added by pengyuan 
        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();
        foldPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;

        //StartCoroutine(CardJump(trans, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
        StartCoroutine(CardJump(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
    }

    public void FoldWildDropPoker(int nIndex)
    {
        if (bFold)
        {
            return;
        }

        //Debug.Log("here we fold a wilddrop poker, name is: " + gameObject.name + "  number is: " + nPokerNumber);

        bFold = true;
        bIsFolding = true;
        fFoldTime = 0.0f;
        fFoldTotalTime = 1.6f;

        //nFoldIndex = nIndex;

        //2021.8.11 added by pengyuan 
        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();
        foldPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;
        foldPos.y += 2.0f;

        Vector3 wildCardPos = new Vector3(5.6f, -4.0f, -1.0f);

        //StartCoroutine(CardJump(trans, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
        StartCoroutine(CardJumpSecond(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos, wildCardPos));
    }

    public void UnFoldWildDropPoker()
    {
        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();
        foldPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;
        //foldPos.y += 2.0f;

        pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = true;

        StartCoroutine(CardJump(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
    }

    public void FoldPokerWithLock(int nIndex, Vector3 lockPos)
    {
        if (bFold)
        {
            return;
        }

        Debug.Log("here we fold a poker with a lock, name is: " + gameObject.name + "  number is: " + nPokerNumber + "  the lock pos is: " + lockPos);

        bFold = true;
        bIsFolding = true;
        fFoldTime = 0.0f;
        fFoldTotalTime = 1.6f;

        nFoldIndex = nIndex;

        //2021.9.1 added by pengyuan to disable the ascend and descend display
        DisablePokerItemDisplay();

        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();
        foldPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;

        Debug.Log("game card jump second coroutine... the target1 is: " + lockPos);

        //StartCoroutine(CardJump(trans, gameObject.GetComponent<Renderer>().bounds.size.x, lockPos));

        //StartCoroutine(CardJumpSecond(trans, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, lockPos, foldPos));
        StartCoroutine(CardJumpSecond(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, lockPos, foldPos));

        //Debug.Log("game card jump second coroutine... the time is: " + Time.time);

    }

    public void Withdraw()
    {
        bFlip = false;
        bFold = false;
        bIsFolding = false;

        bIsWithdrawing = true;
        fWithdrawTime = 0.0f;

        bHasWithdrawed = true;

        pokerStatus = GameDefines.PokerStatus.None;

        if(itemType == GameDefines.PokerItemType.Add_N_Poker)
        {
            pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = true;
            bAddNPoker = false;
            transform.position = originPos;
            nAddNCount = nAddNOriginCount;

            addNPlusImage.enabled = true;
            addNUnitImage.enabled = true;

            addNAnim.Play("FXPlusIdle", 0, 0.0f);

            UpdateAddNEffectDisplay();
        }
        Debug.Log("----------the name is : " + gameObject.name + "   origin pos is: " + originPos + "the target pos is: " + targetPos + "  wildDrops count is: " + wildDrops.Count);

        if(wildSource == GameDefines.WildCardSource.WildDrop)
        {
            GameplayMgr.Instance.gameplayUI.DecWildCard();
            Reward.Data[RewardType.WildCard]--;
            wildSource = GameDefines.WildCardSource.None;
            pokerInst.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }

        StopAllCoroutines();
        //StopCoroutine(CardJump(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, targetPos))
        transform.DOMove(targetPos, 0.5f).OnComplete(() => { transform.DORotate(new Vector3(0.0f, 0.0f, pokerInfo.fRotation), 0.0f, RotateMode.WorldAxisAdd); }) ;

        //here we process the wild drop
        //todo: 
        foreach(GameObject go in wildDrops)
        {
            Debug.Log("the wild drop count is: " + wildDrops.Count);
            GamePoker gamePoker = go.GetComponent<GamePoker>();

            //if(gamePoker.wildSource != GameDefines.WildCardSource.WildDrop)
              //  GameplayMgr.Instance.AddToPublicPoker(go);

            gamePoker.Withdraw();
        }
        wildDrops.Clear();

        GameplayMgr.Instance.UnflipAllCoveredGamePoker(gameObject);
    }

    //when the game is end, and the player failed, we call this method to dismiss the poker.
    public void Dismiss()
    {
        bFlip = false;
        bFold = false;
        bUnFlip = false;

        bDismiss = true;
        fDismissTime = 0.0f;

        SetDismissPosition();

        transform.DOMove(dismissPos, fDismissTotalTime);
        transform.DORotate(new Vector3(90.0f, 180.0f, 90.0f), fDismissTotalTime, RotateMode.WorldAxisAdd);
    }

    public void Swing()
    {
        animator.SetTrigger("Swing");
    }

    //2021.9.13 added by pengyuan, to process remove three cards effect
    public void RemoveThree()
    {
        if (bFold)
        {
            return;
        }

        Debug.Log("here we process RemoveThree poker effect, name is: " + gameObject.name + "  number is: " + nPokerNumber);

        bFold = true;
        bIsFolding = true;
        fFoldTime = 0.0f;
        fFoldTotalTime = 0.8f;

        Vector3 foldPos = new Vector3(0.0f, -0.5f, -2.0f);

        //StartCoroutine(CardJumpRemoveThree(trans, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
        StartCoroutine(CardJumpRemoveThree(transform, pokerInst.gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
    }

    public void ClearAll(bool bFirst)
    {
        bClearAll = true;
        pokerStatus = GameDefines.PokerStatus.Clearing;

        bFlip = false;
        bFold = false;
        bUnFlip = false;
        bDismiss = false;

        //Debug.Log("here we process ClearAll poker effect, name is: " + gameObject.name + "  number is: " + nPokerNumber);

        animator.SetTrigger("Remove");

        StartCoroutine(OnRemoveAction(bFirst));
    }

    //2021.9.23 added by pengyuan, wilddrop appear end callback...
    public void WildDropAppearEnd()
    {
        if(nWildDropIndex == 2)
        {
            //todo: do next step, that is to select the position, and insert to the position.
            GameplayMgr.Instance.InsertWildDrop();
        }
    }

    public void WildDropInsertFirstStage(Vector3 pos1, Vector3 pos2, bool bUP)
    {
        //first set the trigger, then move to position
        if (bUP)
            animator.SetTrigger("GetUp");
        else
            animator.SetTrigger("GetDown");

        transform.DOMove(pos1, 0.5f).OnComplete(() => { WildDropInsertSecondStage(pos2); }); 
    }

    public void WildDropInsertSecondStage(Vector3 pos)
    {
        targetPos = pos;

        //first set the trigger, then move to position        
        transform.DORotate(new Vector3( 0.0f, 0.0f, pokerInfo.fRotation), 0.5f, RotateMode.WorldAxisAdd);
        transform.DOMove(pos, 0.5f).OnComplete(() => { GameplayMgr.Instance.powerUpProcess.FinishUsingWildDrop(); });

        GameplayMgr.Instance.AddToPublicPoker(gameObject);
    }

    public void AddAttachedWildDrop(GameObject go)
    {
        wildDrops.Add(go);
    }

    int GetCutEffectIndexBySuitAndIndex(GameplayMgr.PokerColor color, int nRemoveIndex)
    {
        int nRemainder = nRemoveIndex % 2;
        if (nRemainder == 0)
        {
            if (color == GameplayMgr.PokerColor.Black)
                return 0;
            else
                return 1;
        }
        else
        {
            if (color == GameplayMgr.PokerColor.Black)
                return 2;
            else
                return 3;
        }
    }

    void SetDismissPosition()
    {
        if (targetPos.x >= 0.0f && targetPos.y >= 0.0f)
            dismissPos = targetPos + Vector3.right * 8.0f + Vector3.up * 5.0f;

        if (targetPos.x >= 0.0f && targetPos.y < 0.0f)
            dismissPos = targetPos + Vector3.right * 8.0f - Vector3.up * 5.0f;

        if (targetPos.x < 0.0f && targetPos.y < 0.0f)
            dismissPos = targetPos - Vector3.right * 8.0f - Vector3.up * 5.0f;

        if (targetPos.x < 0.0f && targetPos.y >= 0.0f)
            dismissPos = targetPos - Vector3.right * 8.0f + Vector3.up * 5.0f;
    }

    void UpdatePokerItemDisplay(bool bDisplay)
    {
        if (ascendEffect != null)
        {
            ascendEffect.SetActive(bDisplay);
        }

        if(bombEffect != null)
        {
            bombEffect.SetActive(bDisplay);

            if (bDisplay)
            {
                bombAnim.SetBool("Die", false);
            }
        }
    }

    public void CheckClearPowerUPs()
    {
        if (!GameplayMgr.Instance.bAutoFlipHandPoker)
            return;

        CheckClearBomb();

        CheckClearAscDes();
    }

    void CheckClearBomb()
    {
        if(!GameplayMgr.Instance.powerUpProcess.HasPowerUP_ClearBomb())
        {
            return;
        }

        if (bombEffect == null)
        {
            return;
        }

        if (GameplayMgr.Instance.powerUpProcess.HasPowerUP_ClearBomb())
        {
            bombAnim.SetTrigger("Remove");
            itemType = GameDefines.PokerItemType.None;

            GameplayMgr.Instance.AddPowerUP_ClearScore();

            Destroy(bombEffect, 1.0f);
            bombEffect = null;

            SoundManager.Instance.PlaySFX("gamePUBombRemoved");
        }
    }

    void CheckClearAscDes()
    {
        if (!GameplayMgr.Instance.powerUpProcess.HasPowerUP_ClearAscDes())
        {
            return;
        }

        if (ascendEffect == null)
        {
            return;
        }

        if (GameplayMgr.Instance.powerUpProcess.HasPowerUP_ClearAscDes())
        {
            Animator ascDesAnim = ascendEffectEntity.GetComponent<Animator>();
            ascDesAnim.SetTrigger("Remove");

            //Debug.Log("------------------------CheckClearAscDes------------------name is: " + gameObject.name);

            itemType = GameDefines.PokerItemType.None;

            GameplayMgr.Instance.AddPowerUP_ClearScore();

            Destroy(ascendEffect, 1.25f);
            ascendEffect = null;

            SoundManager.Instance.PlaySFX("gamePUValueChangerRemoved");
        }
    }

    void DisablePokerItemDisplay()
    {
        if (itemType == GameDefines.PokerItemType.Ascending_Poker || itemType == GameDefines.PokerItemType.Descending_Poker)
        {
            ascendEffect.SetActive(false);
        }

        if (itemType == GameDefines.PokerItemType.Bomb)
        {
            //bombEffect.SetActive(false);
            bombAnim.SetBool("Die", true);
            //Debug.Log("-----------------------bombAnim.SetBool(Die, true);----------------------");
        }
    }

    IEnumerator OnRemoveAction(bool bFirst)
    {
        float fStartTime = Time.time;

        while (Time.time - fStartTime < 1.5f)
        {
            yield return null;
        }

        Vector3 newPos = transform.position;
        newPos.x = 12.0f;
        transform.DOMove(newPos, 2.0f);

        while (Time.time - fStartTime < 3.5f)
        {
            yield return null;
        }

        if(bFirst)
            GameplayMgr.Instance.CheckCanFoldWildDrop(this);
    }

    IEnumerator CardJump(Transform card, float cardWidth, Vector3 target)
    {
        float _Width = target.x - card.position.x;
        float _Height = card.transform.position.y - target.y;
        float _xSpeed = 5f / 3 * _Width;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        float fZ = -4.0f;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width < 0 ? 360 - pokerInfo.fRotation : -360 - pokerInfo.fRotation), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;

            //Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 0;

        // Stage 2
        while (Time.time - _StartTime < 0.7f)
        {
            _xSpeed -= (25f / 6 * _Width) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;

            //Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height));

        // Stage 3
        Vector3 oriPos = card.position;
        Vector3 midTarget = target;
        midTarget.z = fZ;

        float dis = (oriPos - target).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            //card.position = Vector3.Lerp(oriPos, target, (Time.time - _StartTime - 0.7f) / 0.1f);
            card.position = Vector3.Lerp(oriPos, midTarget, (Time.time - _StartTime - 0.7f) / 0.1f);

            //Debug.Log("the old z is: " + oriPos.z + "  the new z is: " + target.z);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }

        card.position = target; 

        Debug.Log("game card jump coroutine end time is: " + Time.time);
    }

    IEnumerator CardJumpSecond(Transform card, float cardWidth, Vector3 target1, Vector3 target2)
    {
        float _Width1 = target1.x - card.position.x;
        float _Height1 = card.transform.position.y - target1.y;
        
        float _xSpeed = 5f / 3 * _Width1;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        float fZ = -4.0f;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width1 < 0 ? 360 - pokerInfo.fRotation : -360 - pokerInfo.fRotation), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);
        //transform.DORotate(new Vector3(80, 90, 0), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 0;

        // Stage 2
        while (Time.time - _StartTime < 0.7f)
        {
            _xSpeed -= (25f / 6 * _Width1) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height1))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height1));

        // Stage 3
        Vector3 oriPos = card.position;
        float dis = (oriPos - target1).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            card.position = Vector3.Lerp(oriPos, target1, (Time.time - _StartTime - 0.7f) / 0.1f);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }

        card.position = target1;
        float _Width2 = target2.x - card.position.x;
        float _Height2 = card.transform.position.y - target2.y;
        _xSpeed = 5f / 3 * _Width2;
        _ySpeed = 10 * cardWidth;

        animator.SetTrigger("WildExtra");

        //transform.DORotate(new Vector3(0, 0, _Width2 < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        //2021.8.18 added by pengyuan 
        // Stage 4
        while (Time.time - _StartTime < 1.2f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 0;

        // Stage 5
        while (Time.time - _StartTime < 1.5f)
        {
            _xSpeed -= (25f / 6 * _Width2) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height2))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height2));

        // Stage 6
        oriPos = card.position;
        dis = (oriPos - target2).magnitude;
        while (Time.time - _StartTime < 1.6f)
        {
            card.position = Vector3.Lerp(oriPos, target2, (Time.time - _StartTime - 0.7f) / 0.1f);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, fZ);
            card.position = newPosZ;

            yield return null;
        }

        card.position = target2;
    }

    IEnumerator CardJumpRemoveThree(Transform card, float cardWidth, Vector3 target)
    {
        float _Width = target.x - card.position.x;
        float _Height = card.transform.position.y - target.y;
        float _xSpeed = 5f / 3 * _Width;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);
        transform.DOScale(new Vector3(1.4f, 1.4f, 1.0f), 0.8f);

        Vector3 midPoint = (target + card.position) / 2;
        midPoint.y = card.position.y + cardWidth;
        midPoint.z = -2.0f;
        transform.DOMove(midPoint, 0.4f).SetEase(Ease.InSine) ;
        _xSpeed = (target.x - card.position.x) / 0.8f;
        _ySpeed = 2 * cardWidth / 0.8f;
        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            //_ySpeed -= (25f * cardWidth) * Time.deltaTime;
            //card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;

            //Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            //card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 0;

        //_xSpeed = (target.x - card.position.x) / 0.8f;
        transform.DOMove(target, 0.4f).SetEase(Ease.InSine);
        _ySpeed = (target.y - card.position.y) / 0.4f;
        // Stage 2
        while (Time.time - _StartTime < 0.8f)
        {
            //_xSpeed -= (25f / 6 * _Width) * Time.deltaTime;
            //_ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height))) * Time.deltaTime;
            //card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;

            //Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            //card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height));

        // Stage 3
        /*Vector3 oriPos = card.position;
        float dis = (oriPos - target).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            card.position = Vector3.Lerp(oriPos, target, (Time.time - _StartTime - 0.7f) / 0.1f);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            card.position = newPosZ;

            yield return null;
        }*/

        //card.position = target;

        Debug.Log("game card jump remove three coroutine end time is: " + Time.time);

        //todo: here we play the remove cards effect
        GameplayMgr.Instance.DestroyOnePublicPoker(gameObject);
        int nPrefabIndex = GetCutEffectIndexBySuitAndIndex(pokerColor, GameplayMgr.Instance.removeThreeCurrentIndex);
        GameObject cutEffect = Instantiate(GameplayMgr.Instance.removeThreePrefabs[nPrefabIndex], target, Quaternion.identity);
        cutEffect.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f) ;

        int textureIndex = ((int)pokerSuit - 1) * 13 + nPokerNumber - 1;
        Transform cutUPTrans = cutEffect.transform.Find("FXCardUp");
        Transform cutDownTrans = cutEffect.transform.Find("FXCardDown");
        cutUPTrans.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
        cutDownTrans.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);

        if (!GameplayMgr.Instance.MoveToNextRemovePoker())
            GameplayMgr.Instance.bIsRemovingThree = false;

        Destroy(cutEffect, 1.0f);

        GameplayMgr.Instance.nRemoveThreeFinishCount++; 
        if (GameplayMgr.Instance.nRemoveThreeFinishCount >= 3)
        {
            //GameplayMgr.Instance.FinishUsingOncePowerUPs();
            GameplayMgr.Instance.powerUpProcess.FinishUsingClearThree();
        }
    }
}
