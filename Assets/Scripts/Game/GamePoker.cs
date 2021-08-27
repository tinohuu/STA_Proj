using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePoker : MonoBehaviour
{
    public int Index { get; set; }
    public JsonReadWriteTest.PokerInfo pokerInfo { get; set; }
    public GameplayMgr.PokerType pokerType { get; set; }

    //when withdraw a poker, use this to see whether this poker is facing or backing
    public GameplayMgr.PokerFacing pokerFacing { get; set; }

    //Vector3 originPos;

    public Vector3 targetPos;
    int nFoldIndex = -1;
    float fTime;

    float fFlipTime = 0.0f;

    static float fRotateTime = 1.5f;
    static float fFlipTotalTime = 0.3f;
    static float fFoldTotalTime = 1.5f;

    public string strName { get; set; }//this is for test
    float screenX = 0.0f;
    TextMesh textName;

    public bool bHasWithdrawed { get; set; } = false;

    bool bFlip { get; set; } = false;
    public bool bIsFlipping { get; set; } = false;

    float fFoldTime = 0.0f;
    bool bFold { get; set; } = false;
    public bool bIsFolding { get; set; } = false;

    //data
    public GameplayMgr.PokerSuit pokerSuit;

    public GameplayMgr.PokerColor pokerColor;

    public int nPokerNumber { get; set; }

    //2021.8.11 added for foldinga game poker, translate and rotate in detail
    Vector3 foldPeekPoint;
    Vector3 foldSecondPoint;
    Vector3 secondVelocity;
    float fSecondSpeed = 0.1f;
    float fFoldFirstStage = 0.5f;
    float fFoldSecondStage = 0.2f;

    int testIdnex = 0;
    public GameObject scoreStarPrefab;

    //SimpleCurveHelper curveHelper;

    //Collider2D

    Transform trans;

    // Start is called before the first frame update
    void Start()
    {
        scoreStarPrefab = (GameObject)Resources.Load("ScoreStar");

        //transform = GetComponent<Transform>();
        /*curveHelper = gameObject.AddComponent<SimpleCurveHelper>();

        if (curveHelper == null)
            Debug.Log("@@@@ we init curveHelper error!!! please check your code!");*/

        trans = GetComponent<MeshFilter>().transform;
    }

    public void Init(GameObject goPrefab, JsonReadWriteTest.PokerInfo info, int nIndex, Vector3 pos, Vector3 rendererSize, float fBeginTime)
    {
        Index = nIndex;
        nFoldIndex = nIndex;

        pokerType = GameplayMgr.PokerType.PublicPoker;
        pokerFacing = GameplayMgr.PokerFacing.Backing;

        pokerInfo = info;
        targetPos.x = pos.x + pokerInfo.fPosX * 0.01f;
        targetPos.y = pos.y + pokerInfo.fPosY * 0.01f;
        targetPos.z = pos.z - 1.0f - nIndex * 0.05f;

        //Debug.Log("GamePoker:;Init... the target pos z is: " + targetPos.z);

        fTime = fBeginTime;

        Transform textTrans = gameObject.transform.Find("Text");
        textName = textTrans.GetComponent<TextMesh>();
        textName.text = "测试代码";

        //originPos = transform.position;

        foldPeekPoint = Vector3.zero;
        foldSecondPoint = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        fTime += Time.deltaTime;

        if (!bFlip && !bFold)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.08f);

            Quaternion quatFrom = Quaternion.Euler(-90.0f, 0.0f, -90.0f);
            Quaternion quatTo = Quaternion.Euler(0.0f, 0.0f, pokerInfo.fRotation);

            if (!bHasWithdrawed)
            {
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

            fFlipTime += Time.deltaTime;

            /*if (fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;
            }*/
        }

        if(bFlip && bIsFlipping)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.08f);

            fFlipTime += Time.deltaTime;
            Quaternion quatTo = Quaternion.Euler(0.0f, 180.0f, -pokerInfo.fRotation);

            transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);

            if(fFlipTime >= fFlipTotalTime)
            {
                bIsFlipping = false;

                //2021.8.26 added by pengyaun to adjust the rotation of the poker.
                /*if(Mathf.Abs(pokerInfo.fRotation) > 0.1f)
                {
                    quatTo = Quaternion.Euler(0.0f, 180.0f, -pokerInfo.fRotation);
                    transform.rotation = Quaternion.Lerp(transform.rotation, quatTo, fFlipTime / fFlipTotalTime);
                }*/
            }
        }

        if(bFold)
        {
            fFoldTime += Time.deltaTime;

            Vector3 pos = GameplayMgr.Instance.GetFoldPokerPosition();

            Vector3 oldPos = trans.position;
            //transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

            if (fFoldTime >= fFoldTotalTime)
            {
                bIsFolding = false;
                transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
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
        //nPokerNumber = nNumber;

        //Debug.Log("GamePoker::Test_SetSuitNumber... the suit is: " + suit + "the name is: " + gameObject.name);

        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);

        //Texture2D texture2D = Resources.Load<Texture2D>();
        //Texture2D texture2D = Resources.Load("HeartA") as Texture2D;
        //Texture2D texture2D = Resources.Load("back1") as Texture2D;
        //GetComponent<Renderer>().material.SetTexture("_MainTex", texture2D);
        //string strPokerName = string.Format("Poker/Poker_{0:3D}", nNumber + 1);

        int textureIndex = ((int)suit - 1) * 13 + nPokerNumber - 1;
        //int offsetX = textureIndex * 142;
        GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerTexture[textureIndex]);
        //GetComponent<Renderer>().material.SetTexture("_MainTex2", GameplayMgr.Instance.pokerAtlas);
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(textureIndex * 142, 0));
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(0.0625f, 0f));
        //GetComponent<Renderer>().material.SetTextureOffset("_MainTex2", new Vector2(GameplayMgr.Instance.pokerRects[textureIndex].xMin, GameplayMgr.Instance.pokerRects[textureIndex].yMin));

    }

    public void Test_DisableDisplayText()
    {
        textName.text = "";
    }

    public void Test_EnableDisplayText()
    {
        textName.text = GameplayMgr.Instance.Test_GetSuitDisplayString(pokerSuit, nPokerNumber);
    }

    //we call this method to flip one poker
    //public void FlipPoker() => gameObject.transform.RotateAround(Vector3.up, 180.0f);
    public void FlipPoker()
    {
        if(bFlip)
        {
            return;
        }

        //Debug.Log("here we flip the poker, name is: " + gameObject.name);
        
        bFlip = true;
        bIsFlipping = true;
        fFlipTime = 0.0f;

        //2021.8.10 added by pengyuan for testing the z value
        transform.position -= Vector3.forward * 1.5f;

        pokerFacing = GameplayMgr.PokerFacing.Facing;
    }

    public void UnflipPoker()
    {
        Debug.Log("GamePoker::UnflipPoker... we UnflipPoker, the name is: " + name + "  number is: " + nPokerNumber);

        bFlip = false;
        bIsFlipping = true;
        fFlipTime = 0.0f;

        pokerFacing = GameplayMgr.PokerFacing.Backing;
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

        nFoldIndex = nIndex;

        //2021.8.11 added by pengyuan 
        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();

        /*foldPeekPoint.x = transform.position.x + (GameplayMgr.Instance.GetFoldPokerPosition().x - transform.position.x) * 0.5f;
        foldPeekPoint.y = transform.position.y + gameObject.GetComponent<Renderer>().bounds.size.x * 2;
        foldPeekPoint.z = transform.position.z;

        foldSecondPoint.x = GameplayMgr.Instance.GetFoldPokerPosition().x;
        foldSecondPoint.y = targetPos.y;
        foldSecondPoint.z = transform.position.z;

        secondVelocity.x = (GameplayMgr.Instance.GetFoldPokerPosition().x - foldSecondPoint.x) / 0.2f;
        secondVelocity.y = (GameplayMgr.Instance.GetFoldPokerPosition().y - foldSecondPoint.y) / 0.2f;
        secondVelocity.z = (GameplayMgr.Instance.GetFoldPokerPosition().z - foldSecondPoint.z) / 0.2f;*///GameplayMgr.Instance.GetFoldPokerPosition_Z() - nFoldIndex * 0.05f;
        /*secondVelocity.Normalize();

        fSecondSpeed = Vector3.Distance(GameplayMgr.Instance.GetFoldPokerPosition() , foldSecondPoint) / fFoldSecondStage;

        curveHelper.SetCurveParams(transform.position, foldPeekPoint, foldPos, 0.4f, 0.8f);

        Debug.Log("we fold a poker, and the origin pos is: " + transform.position + "  mid point is: " + foldPeekPoint + "  end point is: " + foldPos);*/

        StartCoroutine(CardJump(trans, gameObject.GetComponent<Renderer>().bounds.size.x, foldPos));
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

        nFoldIndex = nIndex;

        Vector3 foldPos = GameplayMgr.Instance.GetFoldPokerPosition();

        Debug.Log("game card jump second coroutine... the target1 is: " + lockPos);

        //StartCoroutine(CardJump(trans, gameObject.GetComponent<Renderer>().bounds.size.x, lockPos));
        
        StartCoroutine(CardJumpSecond(trans, gameObject.GetComponent<Renderer>().bounds.size.x, lockPos, foldPos));

        //Debug.Log("game card jump second coroutine... the time is: " + Time.time);

    }

    public void Withdraw()
    {
        bFlip = false;
        bFold = false;

        bHasWithdrawed = true;

        transform.DOMove(targetPos, 0.8f);
    }

    IEnumerator CardJump(Transform card, float cardWidth, Vector3 target)
    {
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

            //Debug.Log("the old z is: " + oriPos.z + "  the new z is: " + target.z);

            Vector3 newPosZ = new Vector3(card.position.x, card.position.y, target.z);
            card.position = newPosZ;

            yield return null;
        }

        Debug.Log("game card jump coroutine end time is: " + Time.time);
    }

    IEnumerator CardJumpSecond(Transform card, float cardWidth, Vector3 target1, Vector3 target2)
    {
        float _Width1 = target1.x - card.position.x;
        float _Height1 = card.transform.position.y - target1.y;
        
        float _xSpeed = 5f / 3 * _Width1;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width1 < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);
        //transform.DORotate(new Vector3(80, 90, 0), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 0;

        // Stage 2
        while (Time.time - _StartTime < 0.7f)
        {
            _xSpeed -= (25f / 6 * _Width1) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height1))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height1));

        // Stage 3
        Vector3 oriPos = card.position;
        float dis = (oriPos - target1).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            card.position = Vector3.Lerp(oriPos, target1, (Time.time - _StartTime - 0.7f) / 0.1f);
            yield return null;
        }

        card.position = target1;
        float _Width2 = target2.x - card.position.x;
        float _Height2 = card.transform.position.y - target2.y;
        _xSpeed = 5f / 3 * _Width2;
        _ySpeed = 10 * cardWidth;

        //transform.DORotate(new Vector3(0, 0, _Width2 < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        //2021.8.18 added by pengyuan 
        // Stage 4
        while (Time.time - _StartTime < 1.2f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 0;

        // Stage 5
        while (Time.time - _StartTime < 1.5f)
        {
            _xSpeed -= (25f / 6 * _Width2) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height2))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height2));

        // Stage 6
        oriPos = card.position;
        dis = (oriPos - target2).magnitude;
        while (Time.time - _StartTime < 1.6f)
        {
            card.position = Vector3.Lerp(oriPos, target2, (Time.time - _StartTime - 0.7f) / 0.1f);
            yield return null;
        }
    }
}
