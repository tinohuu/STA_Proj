using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameLock : MonoBehaviour
{
    public int nIndex;

    public int nGroupID;

    public GameplayMgr.LockState lockState = GameplayMgr.LockState.LockState_None;

    public Vector3 originPos;

    //public float fPosX = 0.0f;
    //public float fPosY = 0.0f;

    public GameplayMgr.PokerSuit pokerSuit;
    public GameplayMgr.PokerColor pokerColor;
    public int nNumber = 0;

    SpriteRenderer lockBGImage;
    SpriteRenderer suitImage;
    SpriteRenderer colorImage;
    SpriteRenderer numberImage;

    public float Height { get { return lockBGImage.bounds.size.y; } }

    private void Awake()
    {
        lockBGImage = GetComponent<SpriteRenderer>();

        suitImage = transform.Find("Suit").GetComponent<SpriteRenderer>();

        colorImage = transform.Find("Color").GetComponent<SpriteRenderer>();

        numberImage = transform.Find("Number").GetComponent<SpriteRenderer>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(JsonReadWriteTest.LockInfo info, int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        nIndex = index;

        nGroupID = info.nGroupID;

        Vector3 posOffset = new Vector3(info.fPosX * 1920.0f / 1440.0f, info.fPosY, -1.0f) * 0.01f;// - Vector3.forward * 2.0f;
        transform.position = pos + posOffset;

        pokerSuit = (GameplayMgr.PokerSuit)info.nSuit;
        pokerColor = (GameplayMgr.PokerColor)info.nColor;

        nNumber = info.nNumber;

        //Debug.Log("GameLock::Init ... we init a lock in the game, the index is: " + nIndex);
        if (nIndex == 0)
        {
            lockBGImage.sprite = GameplayMgr.Instance.lockTopSprite;
        }

        //todo: init the color and suit and nubmer.
        if(pokerColor != GameplayMgr.PokerColor.None)
        {
            //Debug.Log("set the color image... ...");
            colorImage.sprite = GameplayMgr.Instance.lockSprites[12 + (int)pokerColor];
        }
        else
        {
            colorImage.sprite = null;
        }

        if (pokerSuit != GameplayMgr.PokerSuit.Suit_None)
        {
            //Debug.Log("set the suit image... ...");
            suitImage.sprite = GameplayMgr.Instance.lockSprites[14 + (int)pokerSuit];
        }
        else
        {
            suitImage.sprite = null;
        }

        if(nNumber != 0)
        {
            //Debug.Log("set the number image... ...");
            numberImage.sprite = GameplayMgr.Instance.lockSprites[nNumber - 1];
        }
        else
        {
            numberImage.sprite = null;
        }

        //Debug.Log("the render size is: " + lockBGImage.bounds.size);
        if(GameplayMgr.Instance.LockOverlapLockArea(transform.position, lockBGImage.bounds.size))
        {
            lockState = GameplayMgr.LockState.LockState_Sleeping;
        }
        else
        {
            lockState = GameplayMgr.LockState.LockState_Working;
        }

        originPos = transform.position;
    }

    public bool CanLockBeCleared(GamePoker pokerScript)
    {
        bool bRet = true;

        if (lockState != GameplayMgr.LockState.LockState_Working)
        {
            bRet = false;
        }

        if(pokerSuit != GameplayMgr.PokerSuit.Suit_None)
        {
            if(pokerSuit != pokerScript.pokerSuit)
            {
                bRet = false;
            }
        }

        if(pokerColor != GameplayMgr.PokerColor.None)
        {
            if(pokerColor != pokerScript.pokerColor)
            {
                bRet = false;
            }
        }

        if(nNumber != 0)
        {
            if(nNumber != pokerScript.nPokerNumber)
            {
                bRet = false;
            }
        }

        return bRet;
    }

    //here we should follow the order:
    //1. check the composite condition, those including two conditions to clear the lock
    //2. check the single condition in the order: number > suit > color
    public bool CanLockBeCleared_2nd(GamePoker pokerScript)
    {
        bool[] bTwoConditions; //first is suit&number, second is color&number

        bool bRet = false;
        if (HasTwoClearConditions(out bTwoConditions))
        {
            if(bTwoConditions[0])//first is suit&number
            {
                if(CanClearSuit(pokerScript) && CanClearNumber(pokerScript))
                {
                    bRet = true;
                }
            }

            if (bTwoConditions[1])//second is color&number
            {
                if (CanClearColor(pokerScript) && CanClearNumber(pokerScript))
                {
                    bRet = true;
                }
            }
        }
        else
        {
            bRet = CanLockBeCleared(pokerScript);
        }

        return bRet;
    }


    bool CanClearSuit(GamePoker pokerScript) => (pokerSuit == pokerScript.pokerSuit) ?  true : false;
    
    bool CanClearColor(GamePoker pokerScript) => (pokerColor == pokerScript.pokerColor) ? true : false;

    bool CanClearNumber(GamePoker pokerScript) => (nNumber == pokerScript.nPokerNumber) ? true : false;

    public void ClearLock()
    {
        lockState = GameplayMgr.LockState.LockState_Dying;

        //hide self
        GetComponent<SpriteRenderer>().enabled = false;
        suitImage.GetComponent<SpriteRenderer>().enabled = false;
        colorImage.GetComponent<SpriteRenderer>().enabled = false;
        numberImage.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void ClearAll_ClearLock(bool bLeft)
    {
        lockState = GameplayMgr.LockState.LockState_Dying;

        Vector3 newPos = gameObject.transform.position;

        //if (gameObject.transform.position.x > 0.0f)
        if(bLeft)
            newPos.x -= 15.0f;
        else
            newPos.x += 15.0f;

        gameObject.transform.DOMove(newPos, 1.0f);//.OnComplete();
    }

    public void Cancel_ClearAll_ClearLock()
    {
        Debug.Log("GameLock::Cancel_ClearAll_ClearLock... the oriPosition is: " + originPos);

        lockState = GameplayMgr.LockState.LockState_Working;

        gameObject.transform.position = originPos;
        //gameObject.GetComponent<SpriteRenderer>().DOFade(0.3f, 1.0f);
    }

    public void PowerUP_ClearLock(bool bLeft)
    {
        lockState = GameplayMgr.LockState.LockState_None;

        Vector3 newPos = gameObject.transform.position;

        if (bLeft)
            newPos.x -= 15.0f;
        else
            newPos.x += 15.0f;

        gameObject.transform.DOMove(newPos, 2.0f).OnComplete(()=> { GameplayMgr.Instance.PowerUP_ClearOneLock(nGroupID, gameObject); Destroy(gameObject); });
    }

    public void AdjustPosition(int nNewIndex)
    {
        if(nNewIndex == 0)
        {
            lockBGImage.sprite = GameplayMgr.Instance.lockTopSprite;
        }
        else
        {
            lockBGImage.sprite = GameplayMgr.Instance.lockBottomSprite;
        }
    }

    public void WithdrawClear()
    {
        lockState = GameplayMgr.LockState.LockState_Working;

        GetComponent<SpriteRenderer>().enabled = true;
        suitImage.GetComponent<SpriteRenderer>().enabled = true;
        colorImage.GetComponent<SpriteRenderer>().enabled = true;
        numberImage.GetComponent<SpriteRenderer>().enabled = true;
    }

    bool HasTwoClearConditions(out bool[] bConditions)
    {
        bConditions = new bool[2];
        bConditions[0] = bConditions[1] = false;

        bool bSuit = (pokerSuit != GameplayMgr.PokerSuit.Suit_None) ? true : false;
        bool bColor = (pokerColor != GameplayMgr.PokerColor.None) ? true : false;
        bool bNumber = (nNumber > 0) ? true : false;

        if ( ( bSuit && bNumber ) ||( bColor && bNumber ) )
        {
            if(bSuit && bNumber)
            {
                bConditions[0] = true;
            }
            if(bColor && bNumber)
            {
                bConditions[1] = true;
            }

            return true;
        }

        return false;
    }

    public bool HasTwoClearConditions()
    {
        bool bSuit = (pokerSuit != GameplayMgr.PokerSuit.Suit_None) ? true : false;
        bool bColor = (pokerColor != GameplayMgr.PokerColor.None) ? true : false;
        bool bNumber = (nNumber > 0) ? true : false;

        if ((bSuit && bNumber) || (bColor && bNumber))
        {
            return true;
        }

        return false;
    }

    public bool HasOneClearConditions()
    {
        bool bSuit = (pokerSuit != GameplayMgr.PokerSuit.Suit_None) ? true : false;
        bool bColor = (pokerColor != GameplayMgr.PokerColor.None) ? true : false;
        bool bNumber = (nNumber > 0) ? true : false;

        if ( (bSuit && !bColor && !bNumber ) 
            || (bColor && !bSuit && !bNumber)
            || (bNumber && !bSuit && !bColor) )
        {
            return true;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
