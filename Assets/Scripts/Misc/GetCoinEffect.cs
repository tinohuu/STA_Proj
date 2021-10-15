using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GetCoinEffect : MonoBehaviour
{
    int nCoin = 0;

    static float fLifeTime = 1.0f;
    float fStartTime = 0.0f;

    bool bAlive = false;

    SpriteRenderer imgCoin;
    SpriteRenderer imgHundred;
    SpriteRenderer imgDigit;
    SpriteRenderer imgUnit;

    private void Awake()
    {
        imgCoin = transform.Find("IconCoin").GetComponent<SpriteRenderer>();
        if (imgCoin == null)
            Debug.Log("----------------------------------------------------------------the imgCoin is null...");

        imgHundred = transform.Find("Number/Hundred").GetComponent<SpriteRenderer>();
        if (imgHundred == null)
            Debug.Log("----------------------------------------------------------------the imgHundred is null...");

        imgDigit = transform.Find("Number/Digit").GetComponent<SpriteRenderer>();
        if (imgDigit == null)
            Debug.Log("----------------------------------------------------------------the imgDigit is null...");

        imgUnit = transform.Find("Number/Unit").GetComponent<SpriteRenderer>();
        if (imgUnit == null)
            Debug.Log("----------------------------------------------------------------the imgUnit is null...");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init(int nGetCoin)
    {
        nCoin = nGetCoin;
        
        fStartTime = Time.time;

        Debug.Log("here we get coin is : " + nCoin + "  start time is: " + fStartTime);

        bAlive = true;
        
        int nHundred = nCoin / 100;
        if (nHundred > 0)
            imgHundred.sprite = GameplayMgr.Instance.bombNumbers[nHundred];
        else
            imgHundred.sprite = null;

        int nDigit = (nCoin - nHundred * 100) / 10;
        if ((nDigit > 0) || (nHundred > 0 && nDigit == 0))
            imgDigit.sprite = GameplayMgr.Instance.bombNumbers[nDigit];
        else
            imgDigit.sprite = null;

        int nUnit = nCoin - nHundred * 100 - nDigit * 10;
        imgUnit.sprite = GameplayMgr.Instance.bombNumbers[nUnit];

        AdjustSpritePosition(nHundred, nDigit, nUnit);

        //Debug.Log("here we get coin'S UNIT is : " + nUnit);

        Vector3 newPos = gameObject.transform.position + Vector3.up * 0.4f;
        gameObject.transform.DOMove(newPos, fLifeTime);

        imgCoin.material.DOFade(0.0f, fLifeTime);
        imgHundred.material.DOFade(0.0f, fLifeTime);
        imgDigit.material.DOFade(0.0f, fLifeTime);
        imgUnit.material.DOFade(0.0f, fLifeTime);

    }

    void AdjustSpritePosition(int nHundred, int nDigit, int nUnit)
    {
        if(nHundred == 0 && nDigit != 0)
        {
            Vector3 tempPos = imgDigit.transform.position;
            imgDigit.transform.position = imgHundred.transform.position;
            imgUnit.transform.position = tempPos;
        }

        if(nHundred == 0 && nDigit == 0)
        {
            imgUnit.transform.position = imgHundred.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!bAlive)
            return;

        if (Time.time - fStartTime > fLifeTime)
        {
            Destroy(gameObject, 0.5f);
            Debug.Log("here we finish the get coin effect is : " + nCoin + "  current time is: " + Time.time);

            bAlive = false;
        }
    }
}
