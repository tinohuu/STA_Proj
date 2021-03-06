using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoldAndScoreUI : MonoBehaviour
{
    RectTransform rectTrans;

    Rect rectBG;

    public GameObject scoreStarPrefab;

    public int nGold { get; set; } = 0;
    public int nScore { get; set; } = 0;

    TMP_Text goldDisplay;

    Image scoreBG;

    Image firstStarBar;
    Text firstStarText;    //this is for debug only

    Image secondStarBar;
    Image thirdStarBar;

    Image firstScoreStar;
    Image[] secondScoreStars = new Image[2];
    Image[] thirdScoreStars = new Image[3];

    GamePlayUI gameplayUI;

    private void Awake()
    {
        gameplayUI = Object.FindObjectOfType<GamePlayUI>();
        if (gameplayUI == null)
            Debug.Log("GoldAndScoreUI::Awake()... gameplayUI is null....");

        scoreStarPrefab = (GameObject)Resources.Load("UI/ScoreStar");
        if (scoreStarPrefab == null)
            Debug.Log("GoldAndScoreUI::Awake()... scoreStarPrefab is null....");
    }

    // Start is called before the first frame update
    void Start()
    {
        /*nGold = GameplayMgr.Instance.nGold;
        nScore = GameplayMgr.Instance.nScore;*/

        rectTrans = GetComponent<RectTransform>();

        goldDisplay = rectTrans.transform.Find("GoldDisplay").GetComponent<TMP_Text>();
        goldDisplay.text = nGold.ToString("N0");

        scoreBG = rectTrans.transform.Find("ScoreBG").GetComponent<Image>();
        firstStarBar = scoreBG.transform.Find("FirstStarBar").GetComponent<Image>();
        firstStarText = scoreBG.transform.Find("FirstStarScore").GetComponent<Text>();

        secondStarBar = scoreBG.transform.Find("SecondStarBar").GetComponent<Image>();
        thirdStarBar = scoreBG.transform.Find("ThirdStarBar").GetComponent<Image>();

        firstStarText.text = 0.ToString();

        firstStarBar.GetComponent<RectTransform>().localScale = new Vector3(0.0f, 1.0f, 1.0f);
        secondStarBar.GetComponent<RectTransform>().localScale = new Vector3(0.0f, 1.0f, 1.0f);
        thirdStarBar.GetComponent<RectTransform>().localScale = new Vector3(0.0f, 1.0f, 1.0f);

        rectBG = scoreBG.rectTransform.rect;

        firstScoreStar = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        Vector3 pos = firstScoreStar.transform.position;
        pos.x += -rectBG.width * 0.0025f;
        pos.y += -rectBG.height * 0.002f;
        firstScoreStar.transform.position = pos;
        firstScoreStar.enabled = false;

        //Debug.Log("the score star pos is: " + pos);

        secondScoreStars[0] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = secondScoreStars[0].transform.position;
        pos.x += -rectBG.width * 0.0015f;
        pos.y += rectBG.height * 0.0005f;
        secondScoreStars[0].transform.position = pos;
        secondScoreStars[0].enabled = false;

        secondScoreStars[1] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = secondScoreStars[1].transform.position;
        pos.x += -rectBG.width * 0.0025f;
        pos.y += rectBG.height * 0.0005f;
        secondScoreStars[1].transform.position = pos;
        secondScoreStars[1].enabled = false;

        thirdScoreStars[0] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[0].transform.position;
        pos.x += -rectBG.width * 0.0005f;
        pos.y += rectBG.height * 0.003f;
        thirdScoreStars[0].transform.position = pos;
        thirdScoreStars[0].enabled = false;

        thirdScoreStars[1] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[1].transform.position;
        pos.x += -rectBG.width * 0.0015f;
        pos.y += rectBG.height * 0.003f;
        thirdScoreStars[1].transform.position = pos;
        thirdScoreStars[1].enabled = false;

        thirdScoreStars[2] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[2].transform.position;
        pos.x += -rectBG.width * 0.0025f;
        pos.y += rectBG.height * 0.003f;
        thirdScoreStars[2].transform.position = pos;
        thirdScoreStars[2].enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddGold(int nAddValue)
    {
        //nGold += nAddValue;
        //goldDisplay.text = nGold.ToString("N0");

        StartCoroutine(AddGoldEffect(nAddValue));
    }

    IEnumerator AddGoldEffect(int nAddValue)
    {
        int nStepCount = 20;
        int nStep = nAddValue / nStepCount;

        if(nAddValue > 0)
        {
            nStep = nStep >= 1 ? nStep : 1;
            nStepCount = nStep >= 1 ? nStepCount : nAddValue;
        }
        else
        {
            nStep = Mathf.Abs(nStep) >= 1 ? nStep : -1;
            nStepCount = Mathf.Abs(nStep) >= 1 ? nStepCount : Mathf.Abs(nAddValue);
        }

        for(int i = 0; i < nStepCount; ++i)
        {
            nGold += nStep;
            goldDisplay.text = nGold.ToString("N0");

            yield return new WaitForSeconds(0.05f);
        }

        StopCoroutine(AddGoldEffect(nAddValue));
    }

    public void SetGold(int nValue)
    {
        nGold = nValue;
        goldDisplay.text = nGold.ToString("N0");
    }

    public void SetScore(int nValue)
    {
        nScore = nValue;
        //goldDisplay.text = nGold.ToString("N0");
    }

    public void AddScore(int nAddValue)
    {
        nScore += nAddValue;

        firstStarText.text = nScore.ToString();

        float[] fillAmounts = new float[3];

        GameplayMgr.Instance.Test_ComputeStars(nScore, ref fillAmounts);

        firstStarBar.GetComponent<RectTransform>().localScale = new Vector3(fillAmounts[0], 1.0f, 1.0f);
        secondStarBar.GetComponent<RectTransform>().localScale = new Vector3(fillAmounts[1], 1.0f, 1.0f);
        thirdStarBar.GetComponent<RectTransform>().localScale = new Vector3(fillAmounts[2], 1.0f, 1.0f);

        firstScoreStar.enabled = fillAmounts[0] >= 1.0f ? true : false;

        secondScoreStars[0].enabled = fillAmounts[1] >= 1.0f ? true : false;
        secondScoreStars[1].enabled = fillAmounts[1] >= 1.0f ? true : false;

        thirdScoreStars[0].enabled = fillAmounts[2] >= 1.0f ? true : false;
        thirdScoreStars[1].enabled = fillAmounts[2] >= 1.0f ? true : false;
        thirdScoreStars[2].enabled = fillAmounts[2] >= 1.0f ? true : false;

        //star
        
    }

    public void ShowCoinEffectTest()
    {
        Vector3 newPos = gameplayUI.goldAndScoreUI.transform.position - Vector3.forward * 2.0f;

        //GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCoin, newPos, Quaternion.identity);


        //GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCoin, streakBonusBG.transform);
        GameObject streakCoin = Instantiate(GameplayMgr.Instance.ParticleTest, scoreBG.transform);


        //Vector3 newPos = gameplayUI.streakBonusUI.transform.position - Vector3.forward * 2.0f;
        //streakCoin.transform.SetParent(streakBonusBG.transform);
        streakCoin.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        streakCoin.transform.localScale = new Vector3(100.0f, 100.0f, 1.0f);
    }
}
