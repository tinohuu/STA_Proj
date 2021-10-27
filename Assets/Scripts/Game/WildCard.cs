using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WildCard : MonoBehaviour
{
    public GameplayMgr.PokerType pokerType { get; set; }

    public GameDefines.WildCardSource wildcardSource { get; set; }

    public GameObject pokerInst = null;

    Animator animator = null;

    Vector3 originPos;

    Vector3 targetPos;
    int nFoldIndex = -1;

    float fTime;
    static float fRotateTime = 1.5f;

    bool bWithdraw = false;

    float fWithdrawTime = 0.0f;
    static float fWithdrawTotalTime = 1.5f;
    static float fFlipTotalTime = 1.5f;

    bool bFlip { get; set; } = false;
    float fFlipTime = 0.0f;

    bool bFirstFlip = false;

    bool bCancel { get; set; } = false;
    float fCancelTime = 0.0f;

    //this is for test displays
    //TextMesh textName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(GameObject goPrefab, int nIndex, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        pokerInst = transform.Find("Poker_Club_10").gameObject;

        pokerType = GameplayMgr.PokerType.WildPoker;
        wildcardSource = GameDefines.WildCardSource.Gold;

        nFoldIndex = nIndex;

        originPos.x = pos.x + rendererSize.x * 0.4f;
        originPos.y = pos.y + rendererSize.y * -0.35f;
        originPos.z = pos.z - 1.0f;

        targetPos.x = pos.x + rendererSize.x * 0.1f;
        targetPos.y = pos.y - rendererSize.y * 0.35f;
        //targetPos.z = zValue - 0.05f ;
        targetPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;

        fTime = fBeginTime;

        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex", GameplayMgr.Instance.wildCardTexture);
        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.wildCardTexture);

        animator = GetComponent<Animator>();
        animator.SetTrigger("WildBuy");

        Sequence s = DOTween.Sequence();
        s.Append(transform.DOMoveX(targetPos.x, 1.0f));
        s.Insert(0.0f, transform.DOMoveY(targetPos.y + 3.0f, 0.5f));
        s.Insert(0.5f, transform.DOMoveY(targetPos.y, 0.5f));
        s.Insert(0.0f, transform.DOMoveZ(targetPos.z, 0.5f));
        s.Play();
        //transform.DOMove(targetPos, 1.0f);
        //transform.domo

        //Transform textTrans = gameObject.transform.Find("Text");
        //textName = textTrans.GetComponent<TextMesh>();
        //textName.text = "WildCard";
    }

    public void InitStreakBonusWildcard(int nIndex, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        pokerInst = transform.Find("Poker_Club_10").gameObject;

        pokerType = GameplayMgr.PokerType.WildPoker;
        wildcardSource = GameDefines.WildCardSource.StreakBonus;

        nFoldIndex = nIndex;
        originPos = transform.position;
        targetPos = pos;

        fTime = fBeginTime;

        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex", GameplayMgr.Instance.wildCardTexture);
        pokerInst.GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.wildCardTexture);

        animator = GetComponent<Animator>();
        //animator.SetTrigger("WildBuy");

        transform.DOMove(targetPos, 1.0f).OnComplete(() => { GameplayMgr.Instance.AdjustAllHandPokerPosition(); });
        transform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 1.0f);

        //transform.DOMove(targetPos, 1.0f).OnComplete(() => { GameplayMgr.Instance.AdjustAllHandPokerPosition(); });

    }

    // Update is called once per frame
    void Update()
    {
        fTime += Time.deltaTime;

        if (!bWithdraw && !bFlip && !bCancel)
        {
            //targetPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;

            //transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.01f);
            
            Quaternion quatFrom = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);// y=180.0

            //transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fTime / fRotateTime);
        }

        //the wild card should fly back to its original position and disappear
        if (bWithdraw)
        {
            //todo: if wildcard is from bonus, we should set a different originPos
            fWithdrawTime += Time.deltaTime;

            if (wildcardSource == GameDefines.WildCardSource.Gold || wildcardSource == GameDefines.WildCardSource.Item)
            {
                transform.position = Vector3.MoveTowards(transform.position, originPos + Vector3.right * 2.0f, 0.1f);

                Quaternion quatFrom = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fWithdrawTime / fWithdrawTotalTime);
            }
            else//from StreakBonus
            {
                targetPos.x = GameplayMgr.Instance.Trans.position.x;// + GameplayMgr.Instance.rendererSize.x * 0.1f;
                targetPos.y = GameplayMgr.Instance.Trans.position.y - GameplayMgr.Instance.rendererSize.y * 0.35f; 
                targetPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;

                transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.1f);

                Quaternion quatFrom = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fWithdrawTime / fWithdrawTotalTime);
            }

            if(fWithdrawTime > fWithdrawTotalTime)
            {
                bWithdraw = false;
                if (wildcardSource == GameDefines.WildCardSource.Gold || wildcardSource == GameDefines.WildCardSource.Item)
                {
                    Destroy(gameObject);
                }
            }
        }

        if(bFlip)
        {
            fFlipTime += Time.deltaTime;

            targetPos = GameplayMgr.Instance.GetFoldPokerPosition(); 
            targetPos.z = GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.01f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.1f);
            
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fTime / fRotateTime);
            if(fFlipTime > fFlipTotalTime)
            {
                bFlip = false;

                if(bFirstFlip)
                {
                    GameplayMgr.Instance.CheckPowerUP_ClearLock();
                }
            }
        }

        if(bCancel)
        {
            fCancelTime += Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, originPos + Vector3.right * 2.0f, 0.1f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            //transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fWithdrawTime / fWithdrawTotalTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fCancelTime / fWithdrawTotalTime);

            if (fCancelTime > fWithdrawTotalTime)
            {
                bCancel = false;
                Destroy(gameObject);
            }
        }
    }

    public void SetFoldIndex(int nIndex)
    {
        nFoldIndex = nIndex;
    }

    public void Withdraw()
    {
        bWithdraw = true;

        fWithdrawTime = 0.0f;

        animator.SetTrigger("Withdraw");
    }

    //this is a streak bonus, and when we withdraw the operation step, the bonus should be canceled
    public void Cancel()
    {
        bCancel = true;

        fCancelTime = 0.0f;
    }

    public void FlipPoker(int nIndex, bool bFirst = false)
    {
        if (bFlip)
        {
            return;
        }

        //Debug.Log("here we flip the wildcard, name is: " + gameObject.name + "  number is: " + nPokerNumber);
        Debug.Log("here we flip the wildcard, name is: " + gameObject.name);

        bFlip = true;
        //bIsFlipping = true;
        fFlipTime = 0.0f;

        bFirstFlip = bFirst;

        nFoldIndex = nIndex;

        animator.SetTrigger("Flip");

        //targetPos.z = fZ;
    }

    public void AdjustWildcardPosition(int nTotalCount, int index, Vector3 pos, Vector3 rendererSize)
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
            newPos.y = pos.y + rendererSize.y * 0.35f;
            newPos.z = pos.z - index * 0.01f;
        }
        else
        {
            newPos.x = pos.x - (nTotalCount - index) * 0.2f;
            newPos.y = pos.y + rendererSize.y * 0.35f;
            newPos.z = pos.z - index * 0.01f;
        }

        /*newPos.x = pos.x - (nTotalCount - index) * 0.3f;
        newPos.y = pos.y + rendererSize.y * 0.3f;
        newPos.z = pos.z - index * 0.01f;*/

        targetPos = newPos;
        transform.DOMove(targetPos, 0.2f);

        Debug.Log("AdjustWildcardPosition... ... ... the target Pos is: " + targetPos);
    }


}
