using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreakBonusUI : MonoBehaviour
{
    Sprite redDotSprite;
    Sprite blackDotSprite;
    Sprite whiteDotSprite;

    Sprite addGoldSprite;
    Sprite addPokerSprite;
    Sprite addWildcardSprite;

    int nSpriteCount = 0;
    GameObject[] streakSprites = new GameObject[6];

    Image currentStreak;
    Image nextStreak;

    Button tipsButton;

    RectTransform rectTrans;

    public GameplayMgr.StreakType currentStreakType;
    public GameplayMgr.StreakType nextStreakType;

    GamePlayUI gameplayUI;

    private void Awake()
    {
        gameplayUI = Object.FindObjectOfType<GamePlayUI>();
        if (gameplayUI == null)
            Debug.Log("StreakBonusUI::Awake()... gameplayUI is null....");
    }

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();

        tipsButton = GetComponent<Button>();
        tipsButton.onClick.AddListener( delegate { this.OnClickTipsButton(); });

        redDotSprite = Resources.Load<Sprite>("StreakBonus/BonusDotRed");
        if (redDotSprite == null)
            Debug.Log("StreakBonusUI::Start... Init redDotSprite error!!!");

        blackDotSprite = Resources.Load<Sprite>("StreakBonus/BonusDotBlack");
        if (blackDotSprite == null)
            Debug.Log("StreakBonusUI::Start... Init blackDotSprite error!!!");

        whiteDotSprite = Resources.Load<Sprite>("StreakBonus/BonusDotNone");
        if (whiteDotSprite == null)
            Debug.Log("StreakBonusUI::Start... Init whiteDotSprite error!!!");

        addGoldSprite = Resources.Load<Sprite>("StreakBonus/BonusCoin");
        if (addGoldSprite == null)
            Debug.Log("StreakBonusUI::Start... Init addGoldSprite error!!!");

        addPokerSprite = Resources.Load<Sprite>("StreakBonus/BonusPlus1");
        if (addPokerSprite == null)
            Debug.Log("StreakBonusUI::Start... Init addPokerSprite error!!!");

        addWildcardSprite = Resources.Load<Sprite>("StreakBonus/BonusWild");
        if (addWildcardSprite == null)
            Debug.Log("StreakBonusUI::Start... Init addWildcardSprite error!!!");

        //test code
        Rect rectSize = rectTrans.rect;
        for (int i = 0; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i] = new GameObject();
            streakSprites[i].transform.SetParent(gameObject.transform);
            streakSprites[i].AddComponent<Image>().sprite = whiteDotSprite;
            streakSprites[i].GetComponent<RectTransform>().sizeDelta  = new Vector2(40.0f, 40.0f);

            Vector3 posOffset = new Vector3(rectSize.width * (i * 0.06f - 0.3f), rectSize.height * 0.05f, 0.0f);
            streakSprites[i].transform.position = rectTrans.transform.position + posOffset;

            streakSprites[i].SetActive(false);
        }

        currentStreak = rectTrans.transform.Find("BonusImage").GetComponent<Image>();
        nextStreak = rectTrans.transform.Find("NextBonusImage").GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitStreakBonus(GameplayMgr.StreakType streak, GameplayMgr.StreakType next)
    {
        currentStreakType = streak;
        nextStreakType = next;
        
        int nCount = GameplayMgr.Instance.GetStreakFinishCount(streak);

        for(int i = 0; i < nCount; ++i)
        {
            streakSprites[i].GetComponent<Image>().sprite = whiteDotSprite;
            streakSprites[i].SetActive(true);
        }

        for(int i = nCount; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i].SetActive(false);
        }

        UpdateStreakImage();
    }

    public void SetStreakBonusStatus(int nCount, int nStreakBonus)
    {
        GameplayMgr.PokerColor[] streakBonus = new GameplayMgr.PokerColor[GameplayMgr.Max_StreakBonus_Count];
        GameplayMgr.Instance.StreakBonusDecode(nStreakBonus, ref streakBonus);

        string strDebug = "";
        for (int i = 0; i < 6; ++i)
            strDebug += ("  " + streakBonus[i]);
        Debug.Log("StreakBonusUI::SetStreakBonusStatus the nStreakBonus is: " + nStreakBonus + "  the array is: " + strDebug);

        for(int i = 0; i < nCount; ++i)
        {
            switch (streakBonus[i])
            {
                case GameplayMgr.PokerColor.Red:
                    streakSprites[i].GetComponent<Image>().sprite = redDotSprite;
                    //Debug.Log("StreakBonusUI::SetStreakBonusStatus the color is red i is: " + i);
                    break;
                case GameplayMgr.PokerColor.Black:
                    streakSprites[i].GetComponent<Image>().sprite = blackDotSprite;
                    //Debug.Log("StreakBonusUI::SetStreakBonusStatus the color is black i is: " + i);
                    break;
                default:
                    streakSprites[i].GetComponent<Image>().sprite = whiteDotSprite;
                    //Debug.Log("StreakBonusUI::SetStreakBonusStatus the color is white i is: " + i);
                    break;
            }
                
            streakSprites[i].SetActive(true);
        }

        int nTotalCount = GameplayMgr.Instance.GetStreakFinishCount(currentStreakType);
        for (int i = nCount; i < nTotalCount; ++i)
        {
            streakSprites[i].SetActive(true);
            streakSprites[i].GetComponent<Image>().sprite = whiteDotSprite;
        }

        for (int i = nTotalCount; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i].SetActive(false);
        }
    }

    void UpdateStreakImage()
    {
        SetStreakImage(currentStreak, currentStreakType);
        SetStreakImage(nextStreak, nextStreakType);
    }

    void SetStreakImage(Image streakImage, GameplayMgr.StreakType streak)
    {
        switch (streak)
        {
            case GameplayMgr.StreakType.Add_Gold:
                streakImage.sprite = addGoldSprite;
                break;
            case GameplayMgr.StreakType.Add_Poker:
                streakImage.sprite = addPokerSprite;
                break;
            case GameplayMgr.StreakType.Add_Wildcard:
                streakImage.sprite = addWildcardSprite;
                break;
            default: break;
        }
    }

    void OnClickTipsButton()
    {
        Debug.Log("you clicked the streak bonus tips button!");

        //todo:here we show the streak bonus tips button
        gameplayUI.ShowStreakBonusTipsUI();
    }
}
