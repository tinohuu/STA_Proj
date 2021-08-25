using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoseGameUI : MonoBehaviour
{
    int nCollectGold = 0;
    int nClearGold = 0;

    RectTransform rectTrans;

    TMP_Text collectGold;
    TMP_Text clearGold;
    TMP_Text totalGold;

    Button gotoMapBtn;
    Button retryBtn;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();

        collectGold = rectTrans.transform.Find("CreditCollectText").GetComponent<TMP_Text>();
        collectGold.text = nCollectGold.ToString("N0");

        clearGold = rectTrans.transform.Find("LevelClearText").GetComponent<TMP_Text>();
        clearGold.text = nClearGold.ToString("N0");

        totalGold = rectTrans.transform.Find("TotalText").GetComponent<TMP_Text>();
        totalGold.text = (nCollectGold + nClearGold).ToString("N0");

        gotoMapBtn = rectTrans.transform.Find("ToMapBtn").GetComponent<Button>();
        gotoMapBtn.onClick.AddListener(delegate { this.OnClickGotoMapBtn(); });

        retryBtn = rectTrans.transform.Find("RetryBtn").GetComponent<Button>();
        retryBtn.onClick.AddListener(delegate { this.OnClickRetryBtn(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLoseGameData(int nCollect, int nClear)
    {
        nCollectGold = nCollect;
        nClearGold = nClear;

        collectGold.text = nCollectGold.ToString("N0");
        clearGold.text = nClearGold.ToString("N0");
        totalGold.text = (nCollectGold + nClearGold).ToString("N0");
    }

    void OnClickGotoMapBtn()
    {
        Debug.Log("you clicked goto map Button!");
    }

    void OnClickRetryBtn()
    {
        Debug.Log("you clicked Retry Button!");

        GameplayMgr.Instance.RestartGame();
    }
}
