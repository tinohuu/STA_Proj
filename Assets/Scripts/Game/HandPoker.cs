using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    bool bIsAddN { get; set; } = false;
    float fAddNTime = 0.0f;

    bool bIsJumping { get; set; } = false;

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

    public void InitAddNHandPoker(int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        pokerType = GameplayMgr.PokerType.HandPoker;
        handPokerSource = GameplayMgr.HandPokerSource.AddNPoker;

        nIndex = index;
        originPos = transform.position;
        //targetPos = pos;
        targetPos.x = pos.x - 0.2f;
        targetPos.y = pos.y;
        targetPos.z = pos.z - index * 0.05f;

        screenX = rendererSize.x;

        fTime = fBeginTime;

        Transform textTrans = gameObject.transform.Find("Text");
        textName = textTrans.GetComponent<TextMesh>();
        textName.text = "加N牌";

        bIsAddN = true;
        fAddNTime = 0.0f;

        //Debug.Log("InitAddNHandPoker...inde x is:  " + index + "  the target pos is: " + targetPos + " init pos is: " + transform.position);

        //StartCoroutine(CardJump(gameObject.GetComponent<MeshFilter>().transform, gameObject.GetComponent<Renderer>().bounds.size.x, targetPos));
        StartCoroutine(CardJump(gameObject.transform, gameObject.GetComponent<Renderer>().bounds.size.x, targetPos));

        //StartCoroutine(adjustPosition());

    }

    IEnumerator CardJumpIEnumerator()
    {
        yield return StartCoroutine(CardJump(GetComponent<MeshFilter>().transform, gameObject.GetComponent<Renderer>().bounds.size.x, targetPos));
    }


    // Update is called once per frame
    void Update()
    {
        if (!bFlip && !bCancel && !bIsAddN)
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

        if (bCancel)
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

        if (bIsAddN)
        {
            fAddNTime += Time.deltaTime;
            
            if(!bFlip)
            {
                ;
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
        if (nNumber == 0)
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

        if(handPokerSource == GameplayMgr.HandPokerSource.AddNPoker)
        {
            transform.DORotate(new Vector3(0.0f, 180.0f, 0.0f), 0.2f, RotateMode.WorldAxisAdd);
            Vector3 newPos = GameplayMgr.Instance.GetTopHandPoker().transform.position;
            newPos.z = newPos.z - 0.05f;
            transform.DOMove(newPos, 0.25f);
        }
    }

    public void WithdrawAddNPoker(GamePoker gamePoker)
    {
        StartCoroutine(WithdrawAndDestroy(gamePoker));
    }

    IEnumerator WithdrawAndDestroy(GamePoker gamePoker)
    {
        transform.DOMove(gamePoker.targetPos, 0.5f);

        float fTime = 0.0f;

        while(fTime < 0.5f)
        {
            fTime += Time.deltaTime;
            yield return null;
        }

        GameplayMgr.Instance.RemoveOneHandPoker(gameObject);
        Destroy(gameObject);

        GameplayMgr.Instance.AdjustAllHandPokerPosition();
    }

    //this is a streak bonus, and when we withdraw the operation step, the bonus should be canceled
    public void Cancel()
    {
        bCancel = true;

        fCancelTime = 0.0f;
    }

    public void AdjustHandPokerPosition(JsonReadWriteTest.LevelData leveInfo, int nTotalCount, int index, Vector3 pos, Vector3 rendererSize)
    {
        if (index > 20)
        {
            index = 20;
        }

        Vector3 newPos = new Vector3();

        //todo: in the if statement, we should adjust the first nTotalCount-20 poker to the leftmost position, but we can do it later.
        if (nTotalCount > 20)
        {
            newPos.x = pos.x - (20 - index) * 0.2f;
            newPos.y = pos.y + rendererSize.y * 0.3f;
            newPos.z = pos.z - index * 0.05f;
        }
        else
        {
            newPos.x = pos.x - (nTotalCount - index) * 0.2f;
            newPos.y = pos.y + rendererSize.y * 0.3f;
            newPos.z = pos.z - index * 0.05f;
        }

        //Debug.Log("AdjustHandPokerPosition... the newPos is: " + newPos + ", the total count is: " + nTotalCount + ", the idnex is : " + index + "  name is: " + gameObject.name);

        targetPos = newPos;

        if(handPokerSource == GameplayMgr.HandPokerSource.AddNPoker && !bIsJumping)
        {
            transform.position = targetPos;
        }
    }

    IEnumerator CardJump(Transform card, float cardWidth, Vector3 target)
    {
        bIsJumping = true;

        float _Width = target.x - card.position.x;
        float _Height = card.transform.position.y - target.y;
        float _xSpeed = 5f / 3 * _Width;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
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

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            card.position = newPosZ;

            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height));

        // Stage 3
        Vector3 oriPos = card.position;
        float dis = (oriPos - target).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            card.position = Vector3.Lerp(oriPos, target, (Time.time - _StartTime - 0.7f) / 0.1f);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            card.position = newPosZ;
            //Debug.Log("hand card jump coroutine  "  + " the index is: " + nIndex);
            yield return null;
        }

        card.position = target;

        bIsJumping = false;

        GameplayMgr.Instance.AdjustAllHandPokerPosition();

        /*Debug.Log("hand card jump coroutine end time is: " + Time.time + "the target Pos is: " + targetPos + " the index is: " + nIndex);
        Debug.Log("hand card jump coroutine end we set the target pos is: " + targetPos + " the index is: " + nIndex);
        GameplayMgr.Instance.AdjustAllHandPokerPosition2nd(nIndex);
        transform.DOMove(targetPos, 0.1f);*/
    }

    IEnumerator adjustPosition()
    {
        Debug.Log("here we enter coroutine adjustPosition, the index is: " + nIndex);

        yield return new WaitForEndOfFrame();

        GameplayMgr.Instance.AdjustAllHandPokerPosition();

        //transform.DOMove(targetPos, 0.1f);
        //transform.position = targetPos;
    }

}
