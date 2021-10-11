using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GetCoinEffect : MonoBehaviour
{
    int nCoin = 0;

    static float fLifeTime = 0.8f;
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
        Debug.Log("here we get coin is : " + nCoin);

        fStartTime = Time.time;

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

        imgCoin.material.DOFade(0.0f, fLifeTime);
        imgHundred.material.DOFade(0.0f, fLifeTime);
        imgDigit.material.DOFade(0.0f, fLifeTime);
        imgUnit.material.DOFade(0.0f, fLifeTime);

    }

    // Update is called once per frame
    void Update()
    {
        if (!bAlive)
            return;

        if (Time.time - fStartTime > fLifeTime)
            Destroy(gameObject);
    }
}
