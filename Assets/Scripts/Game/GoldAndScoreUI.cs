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
        pos.x += -rectBG.width * 0.25f;
        pos.y += -rectBG.height * 0.2f;
        firstScoreStar.transform.position = pos;
        firstScoreStar.enabled = false;

        secondScoreStars[0] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = secondScoreStars[0].transform.position;
        pos.x += -rectBG.width * 0.15f;
        pos.y += rectBG.height * 0.05f;
        secondScoreStars[0].transform.position = pos;
        secondScoreStars[0].enabled = false;

        secondScoreStars[1] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = secondScoreStars[1].transform.position;
        pos.x += -rectBG.width * 0.25f;
        pos.y += rectBG.height * 0.05f;
        secondScoreStars[1].transform.position = pos;
        secondScoreStars[1].enabled = false;

        thirdScoreStars[0] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[0].transform.position;
        pos.x += -rectBG.width * 0.05f;
        pos.y += rectBG.height * 0.3f;
        thirdScoreStars[0].transform.position = pos;
        thirdScoreStars[0].enabled = false;

        thirdScoreStars[1] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[1].transform.position;
        pos.x += -rectBG.width * 0.15f;
        pos.y += rectBG.height * 0.3f;
        thirdScoreStars[1].transform.position = pos;
        thirdScoreStars[1].enabled = false;

        thirdScoreStars[2] = ((GameObject)Instantiate(scoreStarPrefab, scoreBG.transform)).GetComponent<Image>();
        pos = thirdScoreStars[2].transform.position;
        pos.x += -rectBG.width * 0.25f;
        pos.y += rectBG.height * 0.3f;
        thirdScoreStars[2].transform.position = pos;
        thirdScoreStars[2].enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddGold(int nAddValue)
    {
        nGold += nAddValue;
        goldDisplay.text = nGold.ToString("N0");
    }

    public void SetGold(int nValue)
    {
        nGold = nValue;
        goldDisplay.text = nGold.ToString("N0");
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
}
