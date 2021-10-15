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

    Image streakBonusBG;
    Image currentStreak;
    Image nextStreak;

    Button tipsButton;

    RectTransform rectTrans;

    public GameplayMgr.StreakType currentStreakType;
    public GameplayMgr.StreakType nextStreakType;

    GamePlayUI gameplayUI;

    List<GameplayMgr.StreakBonusInfo> clearAllStreakBonus = new List<GameplayMgr.StreakBonusInfo>();
    List<GameplayMgr.StreakBonusInfo> withdrawClearAllStreakBonus = new List<GameplayMgr.StreakBonusInfo>();

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

        streakBonusBG = rectTrans.transform.Find("BonusBG").GetComponent<Image>();
        currentStreak = rectTrans.transform.Find("BonusImage").GetComponent<Image>();
        nextStreak = rectTrans.transform.Find("NextBonusImage").GetComponent<Image>();

        //test code
        streakBonusBG.rectTransform.sizeDelta = new Vector2(608.0f, 156.0f);
        Rect rectSize = rectTrans.rect;
        for (int i = 0; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i] = new GameObject();
            streakSprites[i].transform.SetParent(gameObject.transform);
            streakSprites[i].AddComponent<Image>().sprite = whiteDotSprite;
            streakSprites[i].GetComponent<RectTransform>().sizeDelta  = new Vector2(0.4f, 0.4f);

            //Vector3 posOffset = new Vector3(rectSize.width * (i * 0.06f - 0.3f), rectSize.height * 0.05f, 0.0f);
            Vector3 posOffset = new Vector3(- rectSize.width/200 + 0.5f + (i * 0.4f), rectSize.height * 0.01f - 1.6f, 0.0f);
            //streakSprites[i].transform.position = rectTrans.transform.position + posOffset;
            //Vector3 posOffset = new Vector3(0.0f, 0.0f, 0.0f);
            //streakSprites[i].transform.position = posOffset;

            //Debug.Log("the streak sprite pos is: " + streakSprites[i].transform.position + "  posoffset is: " + posOffset);

            streakSprites[i].SetActive(false);
        }

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

        InitDotSprites(nCount);

        UpdateStreakImage();
    }

    void InitDotSprites(int nCount)
    {
        streakBonusBG.rectTransform.sizeDelta = new Vector2(358.0f + 50.0f * nCount, 156.0f);

        int nOffset = 0;
        if (nCount == 5)
            nOffset = 0;
        else if (nCount == 4)
            nOffset = 1;
        else if (nCount == 6)
            nOffset = -1;

        Rect rectSize = GetComponent<RectTransform>().rect;
        
        for (int i = 0; i < nCount; ++i)
        {
            streakSprites[i].GetComponent<Image>().sprite = whiteDotSprite;
            streakSprites[i].SetActive(true);

            Vector3 posOffset = new Vector3(-rectSize.width / 200 + 1.2f + (i + nOffset) * 0.4f, rectSize.height * 0.01f - 1.5f, 0.0f);
            streakSprites[i].transform.position = rectTrans.transform.position + posOffset;
        }

        for (int i = nCount; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i].SetActive(false);
        }

    }

    public void SetStreakBonusStatus(int nCount, int nStreakBonus)
    {
        GameplayMgr.PokerColor[] streakBonus = new GameplayMgr.PokerColor[GameplayMgr.Max_StreakBonus_Count];
        GameplayMgr.Instance.StreakBonusDecode(nStreakBonus, ref streakBonus);

        string strDebug = "";
        for (int i = 0; i < 6; ++i)
            strDebug += ("  " + streakBonus[i]);
        //Debug.Log("StreakBonusUI::SetStreakBonusStatus the nStreakBonus is: " + nStreakBonus + "  the array is: " + strDebug);

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

        //int nTotalCount = GameplayMgr.Instance.GetStreakFinishCount(currentStreakType);

        //InitDotSprites(nTotalCount);

        /*for (int i = nCount; i < nTotalCount; ++i)
        {
            streakSprites[i].SetActive(true);
            streakSprites[i].GetComponent<Image>().sprite = whiteDotSprite;
        }

        for (int i = nTotalCount; i < GameplayMgr.Max_StreakBonus_Count; ++i)
        {
            streakSprites[i].SetActive(false);
        }*/
    }

    public void ShowCoinEffect()
    {
        Vector3 newPos = gameplayUI.streakBonusUI.transform.position + Vector3.forward * 2.0f;
        GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCoin, gameplayUI.streakBonusUI.transform);
        streakCoin.transform.localScale = new Vector3(100.0f, 100.0f, 1.0f);

        /*ParticleSystem particleSystem;
        particleSystem = streakCoin.transform.GetComponentInChildren<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        
        Debug.Log(GetComponent<Canvas>().sortingLayerName);
        Debug.Log(GetComponent<Canvas>().sortingOrder);

        Debug.Log(streakCoin.GetComponent<Renderer>().sortingLayerName);
        Debug.Log(streakCoin.GetComponent<Renderer>().sortingOrder);
        */
    }

    public void SetClearAllStreakBonus(List<GameplayMgr.StreakBonusInfo> bonusInfos)
    {
        clearAllStreakBonus.Clear();

        clearAllStreakBonus = bonusInfos;
    }

    public void WithDrawClearAllStreakBonus(List<GameplayMgr.StreakBonusInfo> bonusInfos)
    {
        withdrawClearAllStreakBonus.Clear();

        withdrawClearAllStreakBonus = bonusInfos;

        for(int i = bonusInfos.Count - 1; i >= 0; --i)
        {
            GameplayMgr.StreakBonusInfo info = bonusInfos[i];
            int nCount = GameplayMgr.Instance.GetStreakFinishCount((GameplayMgr.StreakType)info.StreakType);

            InitStreakBonus(GameplayMgr.Instance.GetPreviousStreakType(), currentStreakType);

            //这里如果是最后一个，将其解码，然后减去实际的个数，设置到显示的数组中，或者直接每个都解码，然后设置数组
            if(i == 0)
            {; }
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
