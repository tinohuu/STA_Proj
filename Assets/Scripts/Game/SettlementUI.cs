using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SettlementUI : MonoBehaviour
{
    int nCollectGold = 0;
    int nClearGold = 0;
    int nScoreStar = 1;

    RectTransform rectTrans;

    Image imgFirstStar;
    Image imgSecondStar;
    Image imgThirdStar;

    TMP_Text collectGold;
    TMP_Text clearGold;
    TMP_Text totalGold;

    Button replayBtn;
    Button nextBtn;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();

        imgFirstStar = rectTrans.transform.Find("FirstStar").GetComponent<Image>();
        imgSecondStar = rectTrans.transform.Find("SecondStar").GetComponent<Image>();
        imgThirdStar = rectTrans.transform.Find("ThirdStar").GetComponent<Image>();

        collectGold = rectTrans.transform.Find("CreditCollectText").GetComponent<TMP_Text>();
        collectGold.text = nCollectGold.ToString("N0");

        clearGold = rectTrans.transform.Find("LevelClearText").GetComponent<TMP_Text>();
        clearGold.text = nClearGold.ToString("N0");

        totalGold = rectTrans.transform.Find("TotalText").GetComponent<TMP_Text>();
        totalGold.text = (nCollectGold + nClearGold).ToString("N0");

        replayBtn = rectTrans.transform.Find("ReplayBtn").GetComponent<Button>();
        replayBtn.onClick.AddListener(delegate { this.OnClickReplayBtn(); });

        nextBtn = rectTrans.transform.Find("NextBtn").GetComponent<Button>();
        nextBtn.onClick.AddListener(delegate { this.OnClickNextBtn(); });

        ResetScoreStar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetScoreStar()
    {
        imgFirstStar.CrossFadeColor(Color.grey, 0.1f, true, true);
        imgSecondStar.CrossFadeColor(Color.grey, 0.1f, true, true);
        imgThirdStar.CrossFadeColor(Color.grey, 0.1f, true, true);
    }

    void InitScoreStar()
    {
        if (nScoreStar > 0)
            imgFirstStar.CrossFadeColor(Color.white, 0.1f, true, true);

        if (nScoreStar > 1)
            imgSecondStar.CrossFadeColor(Color.white, 0.1f, true, true);

        if (nScoreStar > 2)
            imgThirdStar.CrossFadeColor(Color.white, 0.1f, true, true);
    }

    public void SetSettlementData(int nCollect, int nClear, int nStar)
    {
        nCollectGold = nCollect;
        nClearGold = nClear;
        nScoreStar = nStar;

        collectGold.text = nCollectGold.ToString("N0");
        //clearGold.text = nClearGold.ToString("N0");
        clearGold.text = string.Format("{0}", 0);
        totalGold.text = (nCollectGold + nClearGold).ToString("N0");

        InitScoreStar();
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        gameObject.transform.DOMove(new Vector3(0.0f, 0.4f, 0.0f), 0.5f);

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(PlayGoldEffect(nClearGold));
    }

    IEnumerator PlayGoldEffect(int nAddValue)
    {
        int nGold = 0;

        int nStepCount = 20;
        int nStep = nAddValue / nStepCount;

        if (nAddValue > 0)
        {
            nStep = nStep >= 1 ? nStep : 1;
            nStepCount = nStep >= 1 ? nStepCount : nAddValue;
        }
        else
        {
            nStep = Mathf.Abs(nStep) >= 1 ? nStep : -1;
            nStepCount = Mathf.Abs(nStep) >= 1 ? nStepCount : Mathf.Abs(nAddValue);
        }

        for (int i = 0; i < nStepCount; ++i)
        {
            nGold += nStep;
            clearGold.text = nGold.ToString("N0");

            yield return new WaitForSeconds(0.05f);
        }

        StopCoroutine(PlayGoldEffect(nAddValue));
    }

    void OnClickReplayBtn()
    {
        Debug.Log("you clicked Retry Button!");

        //GameplayMgr.Instance.RestartGame();
        GameplayMgr.Instance.OnClickReplayButton();
    }

    void OnClickNextBtn()
    {
        Debug.Log("You Clicked Next Button!");

        //todo:here we should switch to next level
        //...
        GameplayMgr.Instance.OnClickNextLevelButton();
    }
}
