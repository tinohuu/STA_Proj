using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerAreaMgr : MonoBehaviour
{
    public static PokerAreaMgr Instance;

    public DragButton selectedPoker { get; set; } = null;

    public LockEdit selectedLock { get; set; } = null;

    public ScalableImage selectdArea { get; set; } = null;

    public ScalableImage selectedUnlockArea { get; set; } = null;

    public bool bLeftCtrlDown { get; set; } = false;

    //the following are for poker group
    public GameObject pokerPrefab;
    public Transform trans;
    public RectTransform rectTransform;

    public const float POKER_WIDTH = 105.0f;
    public const float POKER_HEIGHT = 165.0f;

    public RectTransform rectTrans
    {
        get
        {
            return GetComponent<RectTransform>();
        }
    }

    public int nCurrentLevel = 0;
    List<GameObject> pokerInfos = new List<GameObject>();

    //2021.7.15 the following are for hand card
    public GameObject handCardPrefab;
    List<GameObject> handCardInfos = new List<GameObject>();

    //right-up menu can display chapter, level, hand card count and desk card count.
    RightUpMenuUI rightUpMenu;

    //2021.7.20 this list is used to store all selected poker.
    List<DragButton> allSelectPoker = new List<DragButton>();

    public GameObject lockPrefab;
    List<GameObject> lockInstances = new List<GameObject>();

    public GameObject scalableImagePrefab;
    List<GameObject> lockAreas = new List<GameObject>();

    List<GameObject> unlockAreas = new List<GameObject>();

    //2021.8.30 added by pengyuan, to edit ascending card and descending card
    public GameObject ascDesPrefab;

    public GameObject bombPrefab;

    private void Awake()
    {
        Instance = this;

        pokerPrefab = (GameObject)Resources.Load("LevelEditor/DragButton");

        handCardPrefab = (GameObject)Resources.Load("LevelEditor/Image");

        ascDesPrefab = (GameObject)Resources.Load("LevelEditor/AscDes");

        bombPrefab = (GameObject)Resources.Load("LevelEditor/BombEdit");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("--------------------here we call the PokerAreaMgr::Start funciton to init ... ...");
        trans = GetComponent<Transform>();
        rectTransform = GetComponent<RectTransform>();

        //Debug.Log("the transformation of PokerAreaMgr is: " + trans.position);
        //Debug.Log("the rect transform of PokerAreaMgr is: " + rectTransform.position);

        if (pokerPrefab == null)
            pokerPrefab = (GameObject)Resources.Load("LevelEditor/DragButton");

        if (handCardPrefab == null)
            handCardPrefab = (GameObject)Resources.Load("LevelEditor/Image");

        if (lockPrefab == null)
            lockPrefab = (GameObject)Resources.Load("LevelEditor/LockEditor");

        if (scalableImagePrefab == null)
            scalableImagePrefab = (GameObject)Resources.Load("LevelEditor/ScalableImage");

        rightUpMenu = Object.FindObjectOfType<RightUpMenuUI>();
        if (rightUpMenu == null)
            Debug.Log("PokerAreaMgr::Start() can not init right-up menu. check your code.");

        nCurrentLevel = 1;

        //TODO: 1. get poker info; 2. init images dynamically
        InitPokerInfo();

        InitHandCardInfo();

        //2021.8.17 added by pengyuan for lock item
        InitLockInfo();

        //2021.8.17 added by pengyuan for lock area
        InitLockAreaInfo();

        //2021.8.19 added by pengyuan for unlock area
        InitUnlockAreaInfo();
    }

    public void InitPokerInfo()
    {
        //Debug.Log("public void InitPokerInfo()... ...");
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[0];

        for (int i=0; i<data.pokerInfo.Count; ++i)
        {
            Vector3 posOffset = new Vector3(data.pokerInfo[i].fPosX, data.pokerInfo[i].fPosY, 0.0f);
            GameObject pokerInstance = (GameObject)Instantiate(pokerPrefab, trans.position + posOffset, trans.rotation);
            
            pokerInstance.transform.rotation = Quaternion.AngleAxis(data.pokerInfo[i].fRotation, transform.forward);
            pokerInstance.transform.SetParent(trans);
            pokerInstance.tag = "poker";

            DragButton btnDrag = pokerInstance.GetComponent<DragButton>();
            btnDrag.nIndex = i;
            btnDrag.nGroupID = data.pokerInfo[i].nGroupID;

            Vector2 relativePos = ToRelativePos(pokerInstance.transform.position, rectTransform.rect);
            //string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", pokerInstance.transform.position.x, pokerInstance.transform.position.y);
            string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", relativePos.x, relativePos.y);
            Text text = pokerInstance.GetComponentInChildren<Text>();
            text.text = strDisplay;

            string strRotation = string.Format("{0:F1}", data.pokerInfo[i].fRotation);
            InputField rotationInput = pokerInstance.GetComponentInChildren<InputField>();
            rotationInput.text = strRotation;

            //2021.8.31 added by pengyuan for poker items
            if(data.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.None)
            {
                InitPokerItemInfo(pokerInstance, btnDrag, data.pokerInfo[i]);
            }

            pokerInfos.Add(pokerInstance);
        }

    }

    //init the hand card area according to level data.
    public void InitHandCardInfo()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[0];

        for(int i = 0; i < data.handPokerCount; ++i)
        {
            Vector3 posOffset = new Vector3(-400.0f + i * 30.0f, -300.0f, 0.0f);

            GameObject handCardInstance = (GameObject)Instantiate(handCardPrefab, trans.position + posOffset, trans.rotation);
            handCardInstance.transform.rotation = Quaternion.AngleAxis(0.0f, transform.forward);
            handCardInstance.transform.SetParent(trans);
            handCardInstance.tag = "handCard";

            handCardInfos.Add(handCardInstance);
        }
    }

    public void InitLockInfo()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel-1];

        Debug.Log("the lock group count is: " + data.lockGroup.nLockGroupCount);
        for (int i = 0; i < data.lockGroup.nLockGroupCount; ++i)
        {
            JsonReadWriteTest.LockGroup lockGroup = data.lockGroup.lockGroups[i];

            for(int j = 0; j < lockGroup.nLockCount; ++j)
            {
                //Vector3 posOffset = new Vector3(-550.0f, 250.0f, 0.0f);
                Vector3 posOffset = new Vector3(lockGroup.lockInfos[j].fPosX, lockGroup.lockInfos[j].fPosY, 0.0f);
                GameObject lockInstance = (GameObject)Instantiate(lockPrefab, trans.position + posOffset, trans.rotation);
                
                lockInstance.transform.SetParent(trans);
                lockInstance.tag = "lock";

                lockInstances.Add(lockInstance);

                LockEdit lockEdit = lockInstance.GetComponent<LockEdit>();
                lockEdit.Init(lockGroup.lockInfos[j]);
            }
        }
    }

    public void InitLockAreaInfo()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel-1];
        Debug.Log("the lock area count is: " + data.LockAreaCount);

        for(int i = 0; i < data.LockAreaCount; ++i)
        {
            Vector3 posOffset = new Vector3(data.lockAreas[i].fPosX, data.lockAreas[i].fPosY, 0.0f);
            GameObject lockArea = (GameObject)Instantiate(scalableImagePrefab, trans.position, trans.rotation);

            lockArea.transform.SetParent(trans);
            lockArea.tag = "lockArea";

            lockAreas.Add(lockArea);

            ScalableImage lockAreaScript = lockArea.GetComponent<ScalableImage>();
            lockAreaScript.Init(data.lockAreas[i]);
        }
    }

    public void InitUnlockAreaInfo()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        Debug.Log("the unlock area count is: " + data.unlockAreaCount);

        for (int i = 0; i < data.unlockAreaCount; ++i)
        {
            Vector3 posOffset = new Vector3(data.unlockAreas[i].fPosX, data.unlockAreas[i].fPosY, 0.0f);
            GameObject unlockArea = (GameObject)Instantiate(scalableImagePrefab, trans.position + posOffset, trans.rotation);

            unlockArea.transform.SetParent(trans);

            unlockArea.tag = "unlockArea";
            unlockArea.GetComponent<Image>().color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.4f);

            unlockAreas.Add(unlockArea);

            ScalableImage unlockAreaScript = unlockArea.GetComponent<ScalableImage>();
            unlockAreaScript.InitUnlockArea(data.unlockAreas[i]);
        }
    }

    void InitPokerItemInfo(GameObject pokerInst, DragButton drag, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        switch((GameDefines.PokerItemType)pokerInfo.nItemType)
        {
            case GameDefines.PokerItemType.Ascending_Poker:
                InstantiateAscendPoker(pokerInst, pokerInfo);
                break;
            case GameDefines.PokerItemType.Descending_Poker:
                InstantiateDescendPoker(pokerInst, pokerInfo);
                break;
            default:break;
        }
    }

    public void SetPokerInfo(int nLevel)
    {
        trans = GetComponent<Transform>();

        ClearPokerInfos();

        nCurrentLevel = nLevel;

        if (pokerPrefab == null)
            pokerPrefab = (GameObject)Resources.Load("DragButton");

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nLevel-1];

        for (int i = 0; i < data.pokerInfo.Count; ++i)
        {
            Vector3 posOffset = new Vector3(data.pokerInfo[i].fPosX, data.pokerInfo[i].fPosY, 0.0f);
            GameObject pokerInstance = (GameObject)Instantiate(pokerPrefab, trans.position + posOffset, trans.rotation);
            pokerInstance.transform.rotation = Quaternion.AngleAxis(data.pokerInfo[i].fRotation, transform.forward);
            pokerInstance.transform.SetParent(trans);
            pokerInstance.tag = "poker";

            DragButton btnDrag = pokerInstance.GetComponent<DragButton>();
            btnDrag.nIndex = i;
            btnDrag.nGroupID = data.pokerInfo[i].nGroupID;

            Vector2 displayPos = ToRelativePos(pokerInstance.transform.position, rectTrans.rect);
            string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", displayPos.x, displayPos.y);
            //string strDisplay = string.Format("X: {0:F1}, Y: {1:F1}", pokerInstance.transform.position.x, pokerInstance.transform.position.y);
            Text text = pokerInstance.GetComponentInChildren<Text>();
            text.text = strDisplay;

            string strRotation = string.Format("{0:F1}", data.pokerInfo[i].fRotation);
            //Debug.Log("the poker rotation is: " + data.pokerInfo[i].fRotation + " display string is: " + strRotation);
            InputField rotationInput = pokerInstance.GetComponentInChildren<InputField>();
            rotationInput.text = strRotation;

            //2021.8.31 added by pengyuan for poker items
            if (data.pokerInfo[i].nItemType != (int)GameDefines.PokerItemType.None)
            {
                InitPokerItemInfo(pokerInstance, btnDrag, data.pokerInfo[i]);
            }

            pokerInfos.Add(pokerInstance);
        }
    }

    //set the hand card area according to level data. like the SetPokerInfo method
    public void SetHandCardInfo(int nLevel)
    {
        ClearHandCardInfos();

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nLevel - 1];

        for (int i = 0; i < data.handPokerCount; ++i)
        {
            Vector3 posOffset = new Vector3(-400.0f + i * 30.0f, -300.0f, 0.0f);

            GameObject handCardInstance = (GameObject)Instantiate(handCardPrefab, trans.position + posOffset, trans.rotation);
            handCardInstance.transform.rotation = Quaternion.AngleAxis(0.0f, transform.forward);
            handCardInstance.transform.SetParent(trans);
            handCardInstance.tag = "handCard";

            handCardInfos.Add(handCardInstance);
        }
    }

    //2021.8.16 add one lock to scene.
    public void AddOneLock()
    {
        Vector3 posOffset = new Vector3(-550.0f, 250.0f, 0.0f);
        GameObject lockInstance = (GameObject)Instantiate(lockPrefab, trans.position + posOffset, trans.rotation);
        //lockInstance = Vector3.one;
        lockInstance.transform.SetParent(trans);
        lockInstance.tag = "lock";

        lockInstances.Add(lockInstance);
    }

    //2021.8.16 add one lock area to scene.
    public void AddLockArea()
    {
        Vector3 posOffset = new Vector3(-650.0f, 250.0f, 0.0f);
        GameObject unlockArea = (GameObject)Instantiate(scalableImagePrefab, trans.position + posOffset, trans.rotation);
        //lockInstance = Vector3.one;
        unlockArea.transform.SetParent(trans);
        unlockArea.tag = "unlockArea";
        unlockArea.GetComponent<Image>().color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.4f);

        unlockAreas.Add(unlockArea);

        posOffset = new Vector3(-550.0f, 250.0f, 0.0f);
        GameObject scalableImg = (GameObject)Instantiate(scalableImagePrefab, trans.position + posOffset, trans.rotation);
        //lockInstance = Vector3.one;
        scalableImg.transform.SetParent(trans);
        scalableImg.tag = "lockArea";

        lockAreas.Add(scalableImg);

    }

    public void AddAscendingCard()
    {
        GameObject _obj = AddOnePoker();

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        data.pokerInfo[data.pokerInfo.Count - 1].nItemType = (int)GameDefines.PokerItemType.Ascending_Poker;
        data.pokerInfo[data.pokerInfo.Count - 1].strItemInfo = "";

        InstantiateAscendPoker(_obj, data.pokerInfo[data.pokerInfo.Count - 1]);
    }

    public void AddDescendingCard()
    {
        GameObject _obj = AddOnePoker();

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        data.pokerInfo[data.pokerInfo.Count - 1].nItemType = (int)GameDefines.PokerItemType.Descending_Poker;
        data.pokerInfo[data.pokerInfo.Count - 1].strItemInfo = "";

        InstantiateDescendPoker(_obj, data.pokerInfo[data.pokerInfo.Count - 1]);

    }

    void InstantiateAscendPoker(GameObject pokerObj, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        Vector3 posOffset = new Vector3(-30.0f, -60.0f, 0.0f);
        GameObject ascendEdit = (GameObject)Instantiate(ascDesPrefab, pokerObj.transform.position, Quaternion.identity);
        ascendEdit.transform.SetParent(pokerObj.transform);
        ascendEdit.transform.localRotation = Quaternion.identity ; ;// Quaternion.AngleAxis(pokerInfo.fRotation, transform.forward);

        DragButton btnDrag = pokerObj.GetComponent<DragButton>();
        btnDrag.SetPokerItemEditInfo(ascendEdit, pokerInfo);
    }

    void InstantiateDescendPoker(GameObject pokerObj, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        Vector3 posOffset = new Vector3(-50.0f, -60.0f, 0.0f);
        GameObject descendEdit = (GameObject)Instantiate(ascDesPrefab, pokerObj.transform.position , Quaternion.identity);
        descendEdit.transform.SetParent(pokerObj.transform);
        descendEdit.transform.localRotation = Quaternion.identity;// Quaternion.AngleAxis(pokerInfo.fRotation, transform.forward);

        DragButton btnDrag = pokerObj.GetComponent<DragButton>();
        btnDrag.SetPokerItemEditInfo(descendEdit, pokerInfo);
    }

    public void AddBombCard()
    {
        GameObject _obj = AddOnePoker();

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        data.pokerInfo[data.pokerInfo.Count - 1].nItemType = (int)GameDefines.PokerItemType.Bomb;
        data.pokerInfo[data.pokerInfo.Count - 1].strItemInfo = "";

        InstantiateBombPoker(_obj, data.pokerInfo[data.pokerInfo.Count - 1]);
    }

    void InstantiateBombPoker(GameObject pokerObj, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        //todo ...
        GameObject bombEdit = (GameObject)Instantiate(bombPrefab, pokerObj.transform.position, Quaternion.identity);
        bombEdit.transform.SetParent(pokerObj.transform);
        bombEdit.transform.localRotation = Quaternion.identity;

        DragButton btnDrag = pokerObj.GetComponent<DragButton>();
        btnDrag.SetPokerItemEditInfo(bombEdit, pokerInfo);
    }

    //this function is used for add a new card to current poker group
    public GameObject AddOnePoker()
    {
        Vector3 posOffset = new Vector3(-550.0f, 250.0f, 0.0f);
        GameObject pokerInstance = (GameObject)Instantiate(pokerPrefab, trans.position + posOffset, trans.rotation);
        pokerInstance.transform.SetParent(trans);
        pokerInstance.tag = "poker";

        //test code ...
        RectTransform _rectTrans = pokerInstance.GetComponent<RectTransform>();
        Debug.Log("PokerAreaMgr::AddOnePoker... the SiblingIndex is: " + _rectTrans.GetSiblingIndex());

        DragButton btnDrag = pokerInstance.GetComponent<DragButton>();
        btnDrag.nIndex = pokerInfos.Count;
        btnDrag.nGroupID = 1;


        Vector2 displayPos = ToRelativePos(pokerInstance.transform.position, rectTrans.rect);
        string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", displayPos.x, displayPos.y);
        //string strDisplay = string.Format("X: {0:F1}, Y: {1:F1}", pokerInstance.transform.position.x, pokerInstance.transform.position.y);
        Text text = pokerInstance.GetComponentInChildren<Text>();
        text.text = strDisplay;

        string strRotation = string.Format("{0:F1}", 0.0f);
        //Debug.Log("the poker rotation is: " + data.pokerInfo[i].fRotation + " display string is: " + strRotation);
        InputField rotationInput = pokerInstance.GetComponentInChildren<InputField>();
        rotationInput.text = strRotation;

        pokerInfos.Add(pokerInstance);

        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        JsonReadWriteTest.PokerInfo pokerInfo = new JsonReadWriteTest.PokerInfo();
        pokerInfo.fPosX = pokerInstance.transform.position.x - trans.position.x;
        pokerInfo.fPosY = pokerInstance.transform.position.y - trans.position.y;
        pokerInfo.fRotation = 0.0f;
        pokerInfo.nGroupID = 1;
        pokerInfo.nItemType = (int)GameDefines.PokerItemType.None;
        pokerInfo.strItemInfo = "";
        data.pokerInfo.Add(pokerInfo);

        rightUpMenu.UpdateDeskCardCount(data.pokerInfo.Count);

        return pokerInstance;

    }

    public void DeleteSelectedPoker()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        //if we have a selected poker, we delete this poker
        if(selectedPoker != null)
        {
            int nIndex = selectedPoker.nIndex;

            //Debug.Log("DeleteSelectedPoker ... Index is: " + nIndex);

            GameObject go = pokerInfos[nIndex];

            GameObject.Destroy(go);
            pokerInfos.RemoveAt(nIndex);
            data.pokerInfo.RemoveAt(nIndex);

            //here we should update the index info for the rest drag button and poker info
            for(int i = nIndex; i < data.pokerInfo.Count; ++i)
            {
                GameObject poker = pokerInfos[i];
                DragButton btn = poker.GetComponent<DragButton>();
                btn.nIndex = i;
                btn.nGroupID = data.pokerInfo[i].nGroupID;
            }

            selectedPoker = null;
        }

    }

    public void DeleteSelectedLock()
    {
        GameObject go = selectedLock.gameObject;
        lockInstances.Remove(go);

        GameObject.Destroy(go);

        selectedLock = null;
    }

    public void DeleteSelectedLockArea()
    {
        GameObject go = selectdArea.gameObject;

        ScalableImage areaScript = go.GetComponent<ScalableImage>();
        Debug.Log("here we delete a lock area, the id is: " + areaScript.nGroupID);

        lockAreas.Remove(go);

        DeleteUnlockAreaByGroupID(areaScript.nGroupID);

        GameObject.Destroy(go);

        selectdArea = null;
    }

    public void DeleteSelectedUnlockArea()
    {
        GameObject go = selectedUnlockArea.gameObject;

        ScalableImage unlockScript = go.GetComponent<ScalableImage>();
        Debug.Log("here we delete a unlock area, the id is: " + unlockScript.nGroupID);

        unlockAreas.Remove(go);
        GameObject.Destroy(go);

        selectedUnlockArea = null;
    }

    //this method is used to update poker infos before switch to a new level
    public void UpdatePokerInfos(int nChapter, int nLevel)
    {
        //debug test
        //Debug.Log("PokerAreaMgr::UpdatePokerInfos... pokerInfos count is: " + pokerInfos.Count);
        EditorScriptMgr.Instance.UpdateLevelInfos(nChapter, nLevel, pokerInfos);

        PokerAreaMgr.Instance.PrepareLockDataForSave();
        PokerAreaMgr.Instance.PrepareLockAreaDataForSave();
        PokerAreaMgr.Instance.PrepareUnlockAreaDataForSave();

        ClearPokerInfos();
        ClearHandCardInfos();

        ClearLockInfos();
        ClearLockAreaInfos();
        ClearUnlockAreaInfos();
    }

    //2021.7.16 this method is used to update one single card's information
    public void UpdateOnePokerInfo(int nIndex, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        EditorScriptMgr.Instance.UpdateOnePokerInfo(nIndex, pokerInfo);
    }

    public void UpdateHandCardInfos(int nCount)
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        if(data.handPokerCount != nCount)
        {
            data.handPokerCount = nCount;

            ClearHandCardInfos();

            for (int i = 0; i < data.handPokerCount; ++i)
            {
                Vector3 posOffset = new Vector3(-400.0f + i * 30.0f, -300.0f, 0.0f);

                GameObject handCardInstance = (GameObject)Instantiate(handCardPrefab, trans.position + posOffset, trans.rotation);
                handCardInstance.transform.rotation = Quaternion.AngleAxis(0.0f, transform.forward);
                handCardInstance.transform.SetParent(trans);
                handCardInstance.tag = "handCard";

                handCardInfos.Add(handCardInstance);
            }

            //2021.7.16 actually we should't add this code here, but for convenient, we do so.
            rightUpMenu.UpdateHandCardCount(data.handPokerCount);
        }
        
    }

    public Vector2 ToRelativePos(Vector3 pos, Rect rect)
    {
        Vector2 relativePos;
        relativePos.x = pos.x - rect.x - rect.width;
        relativePos.y = pos.y - rect.y - rect.height;

        return relativePos;
    }

    void ClearPokerInfos()
    {
        //Debug.Log("---PokerAreaMgr::ClearPokerInfos here we clear the poker images...");
        Transform transform;
        for (int i=0; i<trans.childCount; ++i)
        {
            transform = trans.GetChild(i);
            if(transform.tag == "poker")
            {
                GameObject.Destroy(transform.gameObject);
            }
        }

        pokerInfos.Clear();
        //Debug.Log("---PokerAreaMgr::ClearPokerInfos end... the array length is: "+pokerInfos.Count);
    }

    //clear the hand card area.
    void ClearHandCardInfos()
    {
        Transform transform;
        for (int i = 0; i < trans.childCount; ++i)
        {
            transform = trans.GetChild(i);
            if (transform.tag == "handCard")
            {
                GameObject.Destroy(transform.gameObject);
            }
        }

        handCardInfos.Clear();
    }

    void ClearLockInfos()
    {
        Transform transform;
        for (int i = 0; i < trans.childCount; ++i)
        {
            transform = trans.GetChild(i);
            if (transform.tag == "lock")
            {
                GameObject.Destroy(transform.gameObject);
            }
        }

        lockInstances.Clear();
    }

    void ClearLockAreaInfos()
    {
        Transform transform;
        for (int i = 0; i < trans.childCount; ++i)
        {
            transform = trans.GetChild(i);
            if (transform.tag == "lockArea")
            {
                GameObject.Destroy(transform.gameObject);
            }
        }

        lockAreas.Clear();
    }

    void ClearUnlockAreaInfos()
    {
        Transform transform;
        for (int i = 0; i < trans.childCount; ++i)
        {
            transform = trans.GetChild(i);
            if (transform.tag == "unlockArea")
            {
                GameObject.Destroy(transform.gameObject);
            }
        }

        unlockAreas.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            Debug.Log("PokerAreaMgr::Update... W Key pressed!");
            if(selectedPoker != null)//找到和该卡牌重叠的所有卡牌，然后找到目标卡牌，即排序比它大的最低排序卡牌，交换顺序，并交换两者的内容
            {
                MoveSelectPokerUp();
            }
        }

        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            Debug.Log("PokerAreaMgr::Update... S Key pressed!");
            if (selectedPoker != null)//找到和该卡牌重叠的所有卡牌，然后找到目标卡牌，即排序比它大的最低排序卡牌，交换顺序，并交换两者的内容
            {
                MoveSelectPokerDown();
            }
        }
    }

    /**********************************************************************************************
     1. 找到和当前选中扑克牌重叠的所有扑克牌
     2. 找到所有重叠扑克牌中排序比当前选中扑克牌大，并且排序值最小的那个
     3. 交换两个扑克牌的顺序和相关信息，包括牌区显示信息和关卡存储信息
    ***********************************************************************************************/
    private void MoveSelectPokerUp()
    {
        DragButton btnA = selectedPoker.GetComponent<DragButton>();

        List<GameObject> overlapPokers = GetAllOverlapPokers(true);

        if (overlapPokers.Count == 0)
        {
            Debug.Log("MoveSelectPokerUp... the overlapPokers' size is 0. something is wrong!");
            return;
        }

        //find the smallest index in overlapPokers
        int minIndex = int.MaxValue;
        foreach(GameObject go in overlapPokers)
        {
            DragButton dragBtn = go.GetComponent<DragButton>();
            if(dragBtn.nIndex < minIndex)
            {
                minIndex = dragBtn.nIndex;
            }
        }

        DragButton btnB = pokerInfos[minIndex].GetComponent<DragButton>();

        //exchange two poker by index, including the display index and the storage information
        ExchangeTwoPoker(btnA.nIndex, btnB.nIndex);
        int nTempIndex = btnA.nIndex;
        Debug.Log("MoveSelectPokerUp... btnA Index is: " + nTempIndex + "  btnB Index is: " + btnB.nIndex);
        btnA.OnLayerIndexChanged(btnB.nIndex);
        btnB.OnLayerIndexChanged(nTempIndex);
        Debug.Log("MoveSelectPokerUp... btnA Index is: " + btnA.nIndex + "  btnB Index is: " + btnB.nIndex);
    }

    private void MoveSelectPokerDown()
    {
        DragButton btnA = selectedPoker.GetComponent<DragButton>();

        List<GameObject> overlapPokers = GetAllOverlapPokers(false);

        if (overlapPokers.Count == 0)
        {
            return;
        }

        //find the smallest index in overlapPokers
        int maxIndex = int.MinValue;
        foreach (GameObject go in overlapPokers)
        {
            DragButton dragBtn = go.GetComponent<DragButton>();
            if (dragBtn.nIndex > maxIndex)
            {
                maxIndex = dragBtn.nIndex;
            }
        }

        DragButton btnB = pokerInfos[maxIndex].GetComponent<DragButton>();

        //exchange two poker by index, including the display index and the storage information
        ExchangeTwoPoker(btnA.nIndex, btnB.nIndex);
        int nTempIndex = btnA.nIndex;
        Debug.Log("MoveSelectPokerDown... btnA Index is: " + nTempIndex + "  btnB Index is: " + btnB.nIndex);
        btnA.OnLayerIndexChanged(btnB.nIndex);
        btnB.OnLayerIndexChanged(nTempIndex);
    }

    //bGreater is used to find the index that is bigger than the selected poker, or vise versa.
    private List<GameObject> GetAllOverlapPokers(bool bGreater)
    {
        DragButton btnA = selectedPoker.GetComponent<DragButton>();
        RectTransform rectTransA = selectedPoker.GetComponent<RectTransform>();
        List<GameObject> overlapPokers = new List<GameObject>();
        Rect rectA = new Rect(rectTransA.rect.x + rectTransA.position.x, rectTransA.rect.y + rectTransA.position.y, rectTransA.rect.width, rectTransA.rect.height);

        //Debug.Log("rectTransA is: " + rectA + " index is: " + btnA.nIndex +" Sibling is: " + rectTransA.GetSiblingIndex());

        for(int i = 0; i < pokerInfos.Count; ++i)
        {
            RectTransform rectTransB = pokerInfos[i].GetComponent<RectTransform>();
            DragButton BtnB = pokerInfos[i].GetComponent<DragButton>();
            if (btnA.nIndex == BtnB.nIndex)
            {
                //Debug.Log("PokerAreaMgr::GetAlloverlapPokers we find self, no overlap! the index is: " + BtnB.nIndex);
                continue;
            }
            
            Rect rectB = new Rect(rectTransB.rect.x + rectTransB.position.x, rectTransB.rect.y + rectTransB.position.y, rectTransB.rect.width, rectTransB.rect.height);
            //Debug.Log("rectTransB is: " + rectB + " index is: " + BtnB.nIndex +"Sibling is: "+ rectTransB.GetSiblingIndex());

            if (rectA.Overlaps(rectB))
            {
                //Debug.Log("GetAllOverlapPokers... rectA overlaps rectB...");
                if ((rectTransB.GetSiblingIndex() > rectTransA.GetSiblingIndex()) && bGreater)
                {
                    overlapPokers.Add(pokerInfos[i]);
                }

                //if ((BtnB.nIndex < btnA.nIndex) && !bGreater)
                if ((rectTransB.GetSiblingIndex() < rectTransA.GetSiblingIndex()) && !bGreater)
                {
                    overlapPokers.Add(pokerInfos[i]);
                    //Debug.Log("PokerAreaMgr::GetAlloverlapPokers we find a smaller overlap! the index is: " + BtnB.nIndex);
                }
            }
        }

        return overlapPokers;
    }

    string GetAllCoveredPokers(JsonReadWriteTest.UnlockArea area)
    {
        string strRet = "";

        Rect rectArea = new Rect(area.fPosX - area.fWidth / 2.0f, area.fPosY - area.fHeight / 2.0f, area.fWidth, area.fHeight);
        JsonReadWriteTest.LevelData levelData = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        for (int i = 0; i < levelData.pokerInfo.Count; ++i)
        {
            JsonReadWriteTest.PokerInfo Info = levelData.pokerInfo[i];
            Rect rectPoker = new Rect(Info.fPosX - 140 / 2.0f, Info.fPosY - 220 / 2.0f, 140.0f, 220.0f);

            if (rectArea.Overlaps(rectPoker))
            {
                if(strRet == "")
                {
                    strRet = string.Format("{0}", i);
                }
                else
                {
                    strRet += string.Format("_{0}", i);
                }
            }
        }

        return strRet;
    }

    private void ExchangeTwoPoker(int nIndexA, int nIndexB)
    {
        RectTransform rectTransA = pokerInfos[nIndexA].GetComponent<RectTransform>();
        RectTransform rectTransB = pokerInfos[nIndexB].GetComponent<RectTransform>();

        //交换A和B的button的数据
        DragButton btnA = pokerInfos[nIndexA].GetComponent<DragButton>();
        DragButton btnB = pokerInfos[nIndexB].GetComponent<DragButton>();

        int nTempGoupID = btnB.nGroupID;
        btnB.nGroupID = btnA.nGroupID;
        btnA.nGroupID = nTempGoupID;

        //2021.8.31 added by pengyuan for poker items...
        GameObject temObj = btnB.pokerItemEdit;
        GameDefines.PokerItemType tempItemType = btnB.pokerItemType;
        string tempItemInfo = btnB.strItemInfo;
        btnB.pokerItemEdit = btnA.pokerItemEdit;
        btnB.pokerItemType = btnA.pokerItemType;
        btnB.strItemInfo = btnA.strItemInfo;
        btnA.pokerItemEdit = temObj;
        btnA.pokerItemType = tempItemType;
        btnA.strItemInfo = tempItemInfo;

        //交换levelData中A和B的数据
        JsonReadWriteTest.LevelData levelData = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        JsonReadWriteTest.PokerInfo tempInfo = new JsonReadWriteTest.PokerInfo();
        tempInfo.fPosX = levelData.pokerInfo[nIndexB].fPosX;
        tempInfo.fPosY = levelData.pokerInfo[nIndexB].fPosY;
        tempInfo.fRotation = levelData.pokerInfo[nIndexB].fRotation;
        tempInfo.nGroupID = levelData.pokerInfo[nIndexB].nGroupID;
        tempInfo.nItemType = levelData.pokerInfo[nIndexB].nItemType;
        tempInfo.strItemInfo = levelData.pokerInfo[nIndexB].strItemInfo;

        levelData.pokerInfo[nIndexB] = levelData.pokerInfo[nIndexA];
        levelData.pokerInfo[nIndexA] = tempInfo;

        int nTemp = rectTransB.GetSiblingIndex();
        rectTransB.SetSiblingIndex(rectTransA.GetSiblingIndex());
        rectTransA.SetSiblingIndex(nTemp);

    }

    private void OnGUI()
    {
        if(Input.GetKeyUp(KeyCode.Delete))
        {
            //Debug.Log("PokerAreaMgr::OnGUI... delete key pressed!!!");
            if(selectedPoker != null)
            {
                DeleteSelectedPoker();
            }
            else if(selectedLock != null)
            {
                DeleteSelectedLock();
            }
            else if(selectdArea != null)
            {
                DeleteSelectedLockArea();
            }
            else if(selectedUnlockArea != null)
            {
                DeleteSelectedUnlockArea();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            //Debug.Log("PokerAreaMgr::OnGUI...   here we capture the left control down!");
            bLeftCtrlDown = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            //Debug.Log("PokerAreaMgr::OnGUI...   here we capture the left control up!");
            bLeftCtrlDown = false;
        }

        if(Input.GetMouseButtonUp(0))
        {
            //Debug.Log("PokerAreaMgr::OnGUI...  here we can disselect all poker.");
        }

        if(Input.GetMouseButtonDown(1))
        {
            //Debug.Log("PokerAreaMgr::OnGUI...  here we show a right button." + Input.mousePosition);
            
            if(GUI.Button(new Rect(Input.mousePosition.x, 800f - Input.mousePosition.y, 100f, 50f), "OJB K"))
            {
                Debug.Log("PokerAreaMgr::OnGUI...  you clicked knock button.");
            }
        }

        /*if (GUI.Button(new Rect(Input.mousePosition.x, 800f - Input.mousePosition.y, 100f, 50f), "OK"))
        {
            Debug.Log("PokerAreaMgr::OnGUI...  you clicked knock button.");
        }*/

    }

    //添加一个poker到选择队列中，如果队列中已经存在，就删除之（反选）
    public bool AddToSelectPokerList(DragButton poker)
    {
        for(int i=0; i< allSelectPoker.Count; ++i)
        {
            if(allSelectPoker[i] == poker)
            {
                allSelectPoker.RemoveAt(i);
                //Debug.Log("PokerAreaMgr::AddToSelectPokerList delete. count is: " + allSelectPoker.Count);
                return false;
            }
        }

        allSelectPoker.Add(poker);
        //Debug.Log("PokerAreaMgr::AddToSelectPokerList add. count is: " + allSelectPoker.Count);

        return true;
    }

    public bool InSelectPokerList(DragButton poker)
    {
        for (int i = 0; i < allSelectPoker.Count; ++i)
        {
            if (allSelectPoker[i] == poker)
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateAllSelectPokerDisplayPosition(Vector3 delta)
    {
        foreach( DragButton dragBtn in allSelectPoker )
        {
            Transform rectTransform = dragBtn.GetComponent<RectTransform>();
            rectTransform.position += delta;

            Vector2 displayPos = ToRelativePos(rectTransform.position, rectTrans.rect);
            string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", displayPos.x, displayPos.y);
            //string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", rectTransform.position.x, rectTransform.position.y);
            Text text = dragBtn.GetComponentInChildren<Text>();
            text.text = strDisplay;
        }
        
    }

    public void UpdateAllSelectPokerStoreInfo(Vector3 delta, float fRotation)
    {
        foreach(DragButton dragBtn in allSelectPoker)
        {
            JsonReadWriteTest.PokerInfo info = new JsonReadWriteTest.PokerInfo();
            Transform rectTransform = dragBtn.GetComponent<RectTransform>();

            info.fPosX = rectTransform.position.x - PokerAreaMgr.Instance.trans.position.x;
            info.fPosY = rectTransform.position.y - PokerAreaMgr.Instance.trans.position.y;
            info.fRotation = fRotation;
            info.nGroupID = dragBtn.nGroupID;
            info.nItemType = (int)dragBtn.pokerItemType;
            info.strItemInfo = dragBtn.strItemInfo;

            PokerAreaMgr.Instance.UpdateOnePokerInfo(dragBtn.nIndex, info);
        }
    }

    public void ClearAllSelectPoker()
    {
        //Debug.Log("PokerAreaMgr::ClearAllSelectPoker... here we clear allSelectPoker, the count is: " + allSelectPoker.Count);
        allSelectPoker.Clear();
    }

    public void setSelectPokerPosX(float fPosX)
    {
        if(selectedPoker == null)
        {
            Debug.Log("PokerAreaMgr::setSelectPokerPosX... fPosX is: " + fPosX);
            return;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;
        Vector3 pos = selectedPoker.transform.position;
        JsonReadWriteTest.LevelData levelInfo = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        pos.x = fPosX + rect.x + rect.width;
        //pos.y = levelInfo.pokerInfo[selectedPoker.nIndex].fPosY + rect.y + rect.height;
        selectedPoker.transform.position = pos;

        levelInfo.pokerInfo[selectedPoker.nIndex].fPosX = fPosX;

        string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fPosX, levelInfo.pokerInfo[selectedPoker.nIndex].fPosY);
        Text text = selectedPoker.GetComponentInChildren<Text>();
        text.text = strDisplay;

        string strRotation = string.Format("{0:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fRotation);
        InputField rotationInput = selectedPoker.GetComponentInChildren<InputField>();
        rotationInput.text = strRotation;
    }

    public void setSelectPokerPosY(float fPosY)
    {
        if (selectedPoker == null)
        {
            Debug.Log("PokerAreaMgr::setSelectPokerPosY... fPosY is: " + fPosY);
            return;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;
        Vector3 pos = selectedPoker.transform.position;
        pos.y = fPosY + rect.y + rect.height;
        selectedPoker.transform.position = pos;

        JsonReadWriteTest.LevelData levelInfo = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        levelInfo.pokerInfo[selectedPoker.nIndex].fPosY= fPosY;

        string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fPosX, levelInfo.pokerInfo[selectedPoker.nIndex].fPosY);
        Text text = selectedPoker.GetComponentInChildren<Text>();
        text.text = strDisplay;

        string strRotation = string.Format("{0:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fRotation);
        InputField rotationInput = selectedPoker.GetComponentInChildren<InputField>();
        rotationInput.text = strRotation;
    }

    public void setSelectPokerRotation(float fRotation)
    {
        if (selectedPoker == null)
        {
            Debug.Log("PokerAreaMgr::setSelectPokerRotation... fRotation is: " + fRotation);
            return;
        }

        RectTransform rectTransform = selectedPoker.GetComponent<RectTransform>();
        rectTransform.rotation = Quaternion.AngleAxis(fRotation, transform.forward);

        JsonReadWriteTest.LevelData levelInfo = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        levelInfo.pokerInfo[selectedPoker.nIndex].fRotation = fRotation;

        string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fPosX, levelInfo.pokerInfo[selectedPoker.nIndex].fPosY);
        Text text = selectedPoker.GetComponentInChildren<Text>();
        text.text = strDisplay;

        string strRotation = string.Format("{0:F1}", levelInfo.pokerInfo[selectedPoker.nIndex].fRotation);
        InputField rotationInput = selectedPoker.GetComponentInChildren<InputField>();
        rotationInput.text = strRotation;
    }

    public void setSelectPokerGroupID(int nGroupID)
    {
        JsonReadWriteTest.LevelData levelInfo = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        //set all GrofudID to the same value
        if (allSelectPoker.Count > 0)
        {
            for(int i = 0; i < allSelectPoker.Count; ++i)
            {
                int nIndex = allSelectPoker[i].nIndex;
                DragButton dragBtn = allSelectPoker[i].GetComponent<DragButton>();
                dragBtn.nGroupID = nGroupID;
                levelInfo.pokerInfo[nIndex].nGroupID = nGroupID;
            }
        }
        else
        {
            if (selectedPoker == null)
            {
                Debug.Log("PokerAreaMgr::setSelectPokerGroupID Fail! ... nGroupID is: " + nGroupID);
                return;
            }

            Debug.Log("PokerAreaMgr::setSelectPokerGroupID... nGroupID is: " + nGroupID);
            DragButton dragBtn = selectedPoker.GetComponent<DragButton>();
            dragBtn.nGroupID = nGroupID;
            levelInfo.pokerInfo[selectedPoker.nIndex].nGroupID = nGroupID;
        }
    }

    public void updateSelectPokerItemInfo(DragButton dragBtn)
    {
        if (dragBtn.nIndex < 0 )
        {
            Debug.Log("PokerAreaMgr::updateSelectPokerItemInfo... DragButton is invalid!!! ");
            return;
        }

        JsonReadWriteTest.LevelData levelInfo = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];

        levelInfo.pokerInfo[dragBtn.nIndex].nItemType = (int)dragBtn.pokerItemType;
        levelInfo.pokerInfo[dragBtn.nIndex].strItemInfo = dragBtn.strItemInfo;
    }

    public void setCurrentSelectPoker(DragButton selectBtn)
    {
        selectedPoker = selectBtn;
        selectedLock = null;
        selectdArea = null;
        selectedUnlockArea = null;
    }

    public void setCurrentSelectLock(LockEdit lockEdit)
    {
        selectedLock = lockEdit;
        selectedPoker = null;
        selectdArea = null;
        selectedUnlockArea = null;
    }

    public void setCurrentSelectLockArea(ScalableImage lockArea)
    {
        selectdArea = lockArea;
        selectedLock = null;
        selectedPoker = null;
        selectedUnlockArea = null;
    }

    public void setCurrentSelectUnlockArea(ScalableImage unlockArea)
    {
        selectedUnlockArea = unlockArea;
        selectdArea = null;
        selectedLock = null;
        selectedPoker = null;
        
    }

    public void onDisplayLockAreaChanged(bool bValue)
    {
        if(bValue)
        {
            foreach(GameObject go in lockAreas)
            {
                CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            foreach (GameObject go in unlockAreas)
            {
                CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            foreach (GameObject go in lockAreas)
            {
                CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            foreach (GameObject go in unlockAreas)
            {
                CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void PrepareLockAreaDataForSave()
    {
        //we first get the scalable image and sort by the scalable image index, then we store it into chapter data info
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        data.LockAreaCount = lockAreas.Count;
        data.lockAreas.Clear();

        List<ScalableImage> lockEdits = new List<ScalableImage>();
        for(int i = 0; i < lockAreas.Count; ++i)
        {
            lockEdits.Add(lockAreas[i].GetComponent<ScalableImage>());
        }

        lockEdits.Sort(delegate (ScalableImage lockArea1, ScalableImage lockArea2) { return lockArea1.nGroupID.CompareTo(lockArea2.nGroupID); });

        for (int i = 0; i < lockAreas.Count; ++i)
        {
            JsonReadWriteTest.LockArea lockArea = new JsonReadWriteTest.LockArea();

            lockArea.nAreaID = lockEdits[i].nGroupID;
            RectTransform _rectTrans = lockEdits[i].GetComponent<RectTransform>();
            //Debug.Log("the lock area transformation position is: " + _rectTrans.localPosition);
            //Debug.Log("the editor position is: " + trans.position);
            //Debug.Log("the transformation scale is: " + _rectTrans.localScale);
            /*lockArea.fPosX = (_rectTrans.position.x - trans.position.x) * _rectTrans.localScale.x;
            lockArea.fPosY = _rectTrans.position.y - trans.position.y;*/
            lockArea.fPosX = _rectTrans.localPosition.x;
            lockArea.fPosY = _rectTrans.localPosition.y;
            lockArea.fWidth = _rectTrans.rect.width;
            lockArea.fHeight = _rectTrans.rect.height;

            data.lockAreas.Add(lockArea);
        }
    }

    public void PrepareUnlockAreaDataForSave()
    {
        //we first get the scalable image and sort by the scalable image index, then we store it into chapter data info
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        data.unlockAreaCount = unlockAreas.Count;
        data.unlockAreas.Clear();

        List<ScalableImage> lockEdits = new List<ScalableImage>();
        for (int i = 0; i < unlockAreas.Count; ++i)
        {
            lockEdits.Add(unlockAreas[i].GetComponent<ScalableImage>());
        }

        lockEdits.Sort(delegate (ScalableImage lockArea1, ScalableImage lockArea2) { return lockArea1.nGroupID.CompareTo(lockArea2.nGroupID); });

        for (int i = 0; i < unlockAreas.Count; ++i)
        {
            JsonReadWriteTest.UnlockArea unlockArea = new JsonReadWriteTest.UnlockArea();

            unlockArea.nAreaID = lockEdits[i].nGroupID;
            RectTransform _rectTrans = lockEdits[i].GetComponent<RectTransform>();
            unlockArea.fPosX = _rectTrans.position.x - trans.position.x;
            unlockArea.fPosY = _rectTrans.position.y - trans.position.y;
            unlockArea.fWidth = _rectTrans.rect.width;
            unlockArea.fHeight = _rectTrans.rect.height;

            unlockArea.strUnlockPokers = GetAllCoveredPokers(unlockArea);

            data.unlockAreas.Add(unlockArea);
        }
    }

    //todo:2021.8.17 this method prepares lock data for save, we should sort the lock data by group, and store them in a nested class.
    public void PrepareLockDataForSave()
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nCurrentLevel - 1];
        //data.LockAreaCount = lockInstances;

        List<JsonReadWriteTest.LockGroup> lockGroups = new List<JsonReadWriteTest.LockGroup>();

        foreach (GameObject go in lockInstances)
        {
            LockEdit lockEdit = go.GetComponent<LockEdit>();

            InsertToLockGroupByGroupID(ref lockGroups, lockEdit);
        }

        //sort the locks by fPosY, descending
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            lockGroups[i].lockInfos.Sort((x, y) => { return -x.fPosY.CompareTo(y.fPosY); });
        }

        //store it to LevelData
        data.lockGroup.nLockGroupCount = lockGroups.Count;
        data.lockGroup.lockGroups = lockGroups;
    }

    void InsertToLockGroupByGroupID(ref List<JsonReadWriteTest.LockGroup>  lockGroups, LockEdit lockEdit)
    {
        for(int i = 0; i < lockGroups.Count; ++i)
        {
            if(lockGroups[i].nGroupID == lockEdit.nGroupID)
            {
                JsonReadWriteTest.LockInfo lockInfo = new JsonReadWriteTest.LockInfo();
                lockInfo.nGroupID = lockEdit.nGroupID;

                lockInfo.fPosX = lockEdit.transform.position.x - trans.position.x;
                lockInfo.fPosY = lockEdit.transform.position.y - trans.position.y;

                lockInfo.nSuit = (int)lockEdit.suitSelection;
                lockInfo.nColor = (int)lockEdit.colorSelection;
                lockInfo.nNumber = (int)lockEdit.numberSelection;

                lockGroups[i].lockInfos.Add(lockInfo);
                lockGroups[i].nLockCount++;

                return;
            }
        }

        JsonReadWriteTest.LockGroup groupInfo = new JsonReadWriteTest.LockGroup();
        groupInfo.nGroupID = lockEdit.nGroupID;

        JsonReadWriteTest.LockInfo newLock = new JsonReadWriteTest.LockInfo();
        newLock.nGroupID = lockEdit.nGroupID;

        newLock.fPosX = lockEdit.transform.position.x - trans.position.x;
        newLock.fPosY = lockEdit.transform.position.y - trans.position.y;

        newLock.nSuit = (int)lockEdit.suitSelection;
        newLock.nColor = (int)lockEdit.colorSelection;
        newLock.nNumber = (int)lockEdit.numberSelection;

        groupInfo.lockInfos.Add(newLock);
        groupInfo.nLockCount = groupInfo.lockInfos.Count;

        lockGroups.Add(groupInfo);
    }

    void DeleteUnlockAreaByGroupID(int nID)
    {
        if(nID == 0)
        {
            return;
        }

        for (int i = 0; i < unlockAreas.Count; ++i)
        {
            GameObject go = unlockAreas[i];
            ScalableImage unlockAreaScript = go.GetComponent<ScalableImage>();

            if (unlockAreaScript.nGroupID != 0 && unlockAreaScript.nGroupID == nID)
            {
                unlockAreas.Remove(go);

                GameObject.Destroy(go);
            }
        }
    }

    public string FormatPokerItemInfo(JsonReadWriteTest.PokerInfo pokerInfo)
    {
        if(pokerInfo.nItemType == (int)GameDefines.PokerItemType.None)
        {
            return "";
        }

        /*if(pokerInfo.nItemType == (int)GameDefines.GameItemType.Ascending_Poker)
        {
            string strInfo = string.Format("{0}_{1}", pokerInfo.)
        }*/

        return "";
    }

}
