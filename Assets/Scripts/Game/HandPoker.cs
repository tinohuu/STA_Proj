using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandPoker : MonoBehaviour
{
    int nIndex;

    public GameplayMgr.PokerType pokerType { get; set; }

    public GameplayMgr.HandPokerSource handPokerSource { get; set; }

    Vector3 originPos;

    Vector3 targetPos;
    int nFoldIndex = -1;

    float fTime;

    static float fRotateTime = 1.5f;
    static float fFlipTotalTime = 1.5f;

    public bool bIsFlipping { get; set; } = false;
    bool bFlip { get; set; } = false;

    float fFlipTime = 0.0f;

    bool bCancel { get; set; } = false;
    float fCancelTime = 0.0f;

    string strName;//this is for test
    float screenX = 0.0f;
    TextMesh textName;

    //data
    GameplayMgr.PokerSuit pokerSuit;
    public int nPokerNumber { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(GameObject goPrefab, JsonReadWriteTest.LevelData leveInfo, int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        pokerType = GameplayMgr.PokerType.HandPoker;
        handPokerSource = GameplayMgr.HandPokerSource.Hand;

        nIndex = index;
        nFoldIndex = nIndex;

        targetPos.x = pos.x - (leveInfo.handPokerCount - index) * 0.2f;
        targetPos.y = pos.y + rendererSize.y * 0.3f;// + pokerInfo.fPosY * 0.01f;
        targetPos.z = pos.z - index * 0.05f;

        screenX = rendererSize.x;

        //Debug.Log("HandPoker:;Init... the target pos is: " + targetPos + "  pos.y is: " + pos.y);

        fTime = fBeginTime;

        Transform textTrans = gameObject.transform.Find("Text");
        textName = textTrans.GetComponent<TextMesh>();
        textName.text = "手牌";
    }

    public void InitStreakBonusHandPoker(int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        pokerType = GameplayMgr.PokerType.HandPoker;
        handPokerSource = GameplayMgr.HandPokerSource.StreakBonus;

        nIndex = index;
        originPos = transform.position;
        targetPos = pos;

        screenX = rendererSize.x;

        fTime = fBeginTime;

        Transform textTrans = gameObject.transform.Find("Text");
        textName = textTrans.GetComponent<TextMesh>();
        textName.text = "奖励牌";

    }

    // Update is called once per frame
    void Update()
    {
        if(!bFlip && !bCancel)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.1f);

            fTime += Time.deltaTime;
            Quaternion quatFrom = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fTime / fRotateTime);

        }

        if (bFlip)
        {
            Vector3 pos = Vector3.zero;
            pos.x = screenX * 0.1f;
            pos.y = targetPos.y;
            //pos.z = targetPos.z;
            pos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.05f;
            //Debug.Log("the hand poker flip position is; " + pos);

            //2021.8.10 added by pengyuan for testing the z value
            //transform.position = Vector3.MoveTowards(transform.position, pos, 0.1f);
            //transform.position -= Vector3.forward;
            transform.position = Vector3.MoveTowards(transform.position, pos, 0.1f);

            fFlipTime += Time.deltaTime;
            Quaternion quatTo = Quaternion.Euler(0.0f, 180.0f, 0.0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);

            if (fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;
            }
        }

        if(bCancel)
        {
            fCancelTime += Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, originPos + Vector3.right * 2.0f, 0.1f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fCancelTime / fFlipTotalTime);

            if (fCancelTime > fFlipTotalTime)
            {
                bCancel = false;
                Destroy(gameObject);
            }
        }
    }

    public void Test_SetName(string name)
    {
        strName = name;
    }

    public void Test_SetSuitNumber(GameplayMgr.PokerSuit suit, int nNumber)
    {
        pokerSuit = suit;
        if(nNumber == 0)
        {
            nPokerNumber = 13;
        }
        else
        {
            nPokerNumber = nNumber;
        }
        
        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);

        int textureIndex = ((int)suit - 1) * 13 + nPokerNumber - 1;
        GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
    }

    public void Test_EnableDisplayText()
    {
        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);
    }

    public void Test_DisableDisplayText()
    {
        textName.text = "";
    }

    public void SetFoldIndex(int nIndex)
    {
        nFoldIndex = nIndex;
    }

    public void FlipPoker(int nIndex)
    {
        if (bFlip)
        {
            return;
        }

        Debug.Log("here we flip the poker, name is: " + gameObject.name + "  number is: " + nPokerNumber);
        
        bFlip = true;
        bIsFlipping = true;
        fFlipTime = 0.0f;

        nFoldIndex = nIndex;

        //transform.position -= Vector3.forward * 5;

        //targetPos.z = fZ;
    }

    public void Withdraw()
    {
        bFlip = false;
    }

    //this is a streak bonus, and when we withdraw the operation step, the bonus should be canceled
    public void Cancel()
    {
        bCancel = true;

        fCancelTime = 0.0f;
    }

    public void AdjustHandPokerPosition(JsonReadWriteTest.LevelData leveInfo, int nTotalCount, int index, Vector3 pos, Vector3 rendererSize)
    {
        if(index > 20)
        {
            index = 20;
        }

        Vector3 newPos = new Vector3();
        
        newPos.x = pos.x - (nTotalCount - index) * 0.2f;
        newPos.y = pos.y + rendererSize.y * 0.3f;
        newPos.z = pos.z - index * 0.05f;

        targetPos = newPos;
    }

}
