using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockArea : MonoBehaviour
{
    public int nIndex;//the lock area's index, 0 to total count.

    public int nGroupID;

    JsonReadWriteTest.LockArea areaInfo;

    public int unlockPokerCount = 0;
    public int[] unlockPokerIDs;

    RectTransform rectTrans;

    private void Awake()
    {
        Debug.Log("hjere we get the RectTransform of a lock area....");
        rectTrans = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(JsonReadWriteTest.LockArea info, int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        nIndex = index;

        nGroupID = info.nAreaID;

        areaInfo = info;

        rectTrans.sizeDelta = new Vector2(info.fWidth, info.fHeight);

        Vector3 posOffset = new Vector3(areaInfo.fPosX * 1920.0f/1440.0f, areaInfo.fPosY, -0.0f) * 0.01f - Vector3.forward * 2.0f;

        rectTrans.transform.position = pos + posOffset;
        //gameObject.transform.position = pos + posOffset;

        Debug.Log("lock area position is: " + rectTrans.transform.position + "  posOffset is:  " + posOffset + "  size Delta is: " + rectTrans.sizeDelta);
    }

    public void ClearArea()
    {
        GetComponent<SpriteRenderer>().enabled = false;

        GetComponent<Collider2D>().enabled = false;
    }

    public void SetUnlockPokerIDs(string strPokerIDs)
    {
        if(strPokerIDs == "")
        {
            return;
        }

        string[] strIDs = strPokerIDs.Split('_');
        unlockPokerCount = strIDs.Length;

        unlockPokerIDs = new int[unlockPokerCount];
        for(int i = 0; i < unlockPokerCount; ++i)
        {
            unlockPokerIDs[i] = int.Parse(strIDs[i]);
        }
    }

    public bool IsInUnlockArea(int nPokerID, out int lockGroup, out int lockIndex)
    {
        for(int i = 0; i < unlockPokerIDs.Length; ++i)
        {
            if(unlockPokerIDs[i] == nPokerID)
            {
                lockGroup = nGroupID;
                lockIndex = nIndex;
                return true;
            }
        }

        lockGroup = 0;
        lockIndex = 0;

        return false;
    }

    public void WithdrawClear()
    {
        GetComponent<SpriteRenderer>().enabled = true;

        GetComponent<Collider2D>().enabled = true;

        //todo: here we still have to to something more, to withdraw a lock area's hiding
    }
}
