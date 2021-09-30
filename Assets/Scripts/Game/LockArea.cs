using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LockArea : MonoBehaviour
{
    public int nIndex;//the lock area's index, 0 to total count.

    public int nGroupID;

    Vector3 originPos;

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
        //Graphics.DrawMesh(GetComponent<Mesh>(), rectTrans.transform.position, rectTrans.transform.rotation, GetComponent<Material>(), 0);
        Bounds displayBound = GetComponent<Renderer>().bounds;
        
        Debug.DrawLine(new Vector3(displayBound.min.x, displayBound.min.y, 0.0f), new Vector3(displayBound.min.x, displayBound.max.y, 0.0f), Color.red);
        Debug.DrawLine(new Vector3(displayBound.min.x, displayBound.max.y, 0.0f), new Vector3(displayBound.max.x, displayBound.max.y, 0.0f), Color.red);
        Debug.DrawLine(new Vector3(displayBound.max.x, displayBound.max.y, 0.0f), new Vector3(displayBound.max.x, displayBound.min.y, 0.0f), Color.red);
        Debug.DrawLine(new Vector3(displayBound.max.x, displayBound.min.y, 0.0f), new Vector3(displayBound.min.x, displayBound.min.y, 0.0f), Color.red);
    }

    public void Init(JsonReadWriteTest.LockArea info, int index, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        nIndex = index;

        nGroupID = info.nAreaID;

        areaInfo = info;

        rectTrans.sizeDelta = new Vector2(info.fWidth, info.fHeight);

        Sprite cloudSprite = GetComponent<SpriteRenderer>().sprite;
        GetComponent<SpriteRenderer>().sprite = Sprite.Create( ScaleTexture(cloudSprite.texture, info.fWidth, info.fHeight) , rectTrans.rect, new Vector2(0.5f, 0.5f));

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(info.fWidth * 0.01f, info.fHeight * 0.01f);

        //Vector3 posOffset = new Vector3(areaInfo.fPosX * 1920.0f/1440.0f, areaInfo.fPosY, -0.0f) * 0.01f - Vector3.forward * 2.0f;
        Vector3 posOffset = new Vector3(areaInfo.fPosX, areaInfo.fPosY, -0.0f) * 0.01f - Vector3.forward * 2.0f;

        rectTrans.transform.position = pos + posOffset;
        //gameObject.transform.position = pos + posOffset;
        originPos = rectTrans.transform.position;

        Debug.Log("lock area position is: " + rectTrans.transform.position + "  posOffset is:  " + posOffset + "  size Delta is: " + rectTrans.sizeDelta);
    }

    Texture2D ScaleTexture(Texture2D source, float targetWidth, float targetHeight)
    {
        Texture2D result = new Texture2D((int)targetWidth, (int)targetHeight);

        float fStepX = (1.0f / targetWidth);
        float fStepY = (1.0f / targetHeight);

        for(int i = 0; i < result.height; ++i)
        {
            for(int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j * fStepX, (float)i * fStepY);
                result.SetPixel(j, i, newColor);
            }
        }

        result.Apply();

        return result;
    }

    public void ClearArea()
    {
        GetComponent<SpriteRenderer>().enabled = false;

        GetComponent<Collider2D>().enabled = false;
    }

    public bool ClearAll_ClearLock()
    {
        bool bRet = false;
        Vector3 newPos = gameObject.transform.position;

        if (gameObject.transform.position.x > 0.0f)
        {
            newPos.x += 15.0f;
        }
        else
        {
            newPos.x -= 15.0f;
            bRet = true;
        }

        gameObject.transform.DOMove(newPos, 1.0f);

        return bRet;
    }

    public void Cancel_ClearAll_ClearLock()
    {
        Debug.Log("LockArea::Cancel_ClearAll_ClearLock... the originPos is: " + originPos);
        gameObject.transform.position = originPos;
        //gameObject.GetComponent<SpriteRenderer>().DOFade(0.3f, 1.0f);
    }

    public bool PowerUP_ClearLockArea()
    {
        bool bRet = false;
        Vector3 newPos = gameObject.transform.position;

        if (gameObject.transform.position.x > 0.0f)
        {
            newPos.x += 15.0f;
        }
        else
        {
            newPos.x -= 15.0f;
            bRet = true;
        }

        gameObject.transform.DOMove(newPos, 2.0f).OnComplete(() => { GameplayMgr.Instance.PowerUP_ClearLockArea(nGroupID); Destroy(gameObject); });

        return bRet;
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
