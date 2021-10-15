using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlayUI : MonoBehaviour
{
    Button withdrawBtn;

    Button add5Btn;

    Button wildcardBtn;

    Button endGameBtn;

    //the following is about some ui
    public StreakBonusUI streakBonusUI;

    public StreakBonusTipsUI streakBonusTipsUI;

    public GoldAndScoreUI goldAndScoreUI;

    public SettlementUI settlementUI;

    public LoseGameUI loseGameUI;

    public LockEndGameUI lockEndGameUI;

    public BombEndGameUI bombEndGameUI;

    public CongratulationUI congratulateUI;

    bool bIsHidingAdd5Btn = false;

    //Image imgCongratulation;// = new Image();

    private void Awake()
    {
        float aspectRatio = Screen.width * 1.0f / Screen.height;

        if (aspectRatio > 1.78f)
        {
            //GetComponent<Transform>().localScale = new Vector3(Screen.width / 1920.0f, 1.0f, 1.0f);

            //Screen.SetResolution(Screen.width, 1080, true);
        }
        else
        {
            //GetComponent<Transform>().localScale = new Vector3(Screen.width / 1920.0f, Screen.height / 1080.0f, 1.0f);
            //Screen.SetResolution(Screen.width, Screen.height, true);
        }

        /*Sprite congSprite = Resources.Load<Sprite>("UI/SettleCongratulations");
        if (congSprite == null)
            Debug.Log("GamePlayUI::Start... Init congSprite error!!!");
        imgCongratulation.sprite = congSprite;
        imgCongratulation.enabled = false;*/

    }

    public float GetOffsetX()
    {
        switch(Screen.width)
        {
            case 1920:
                return -0.3f;
            case 2340:
                return -2.2f;
            case 2560:
                return -0.3f;
            default:
                return -0.3f;
        }
    }

    // Start is called before the first frame update
    void Start()    
    {
        withdrawBtn = transform.Find("WithdrawBtn").GetComponent<Button>();
        if (withdrawBtn == null)
            Debug.Log("game ui init error! we can not find Withdraw Button! ");
        //Debug.Log("game ui init ... the pos of Withdraw Button is: " + withdrawBtn.GetComponent<Transform>().position);

        Vector3 newPos = withdrawBtn.GetComponent<Transform>().position;
        newPos.x = Screen.width * 0.75f;
        newPos.y = Screen.height * 0.1f;
        //withdrawBtn.GetComponent<Transform>().position = newPos;

        withdrawBtn.onClick.AddListener(delegate { this.OnClickWithdrawBtn(); });
        //withdrawBtn.enabled = false;
        withdrawBtn.gameObject.SetActive(false);

        add5Btn = transform.Find("Add5Btn").GetComponent<Button>();
        if (add5Btn == null)
            Debug.Log("game ui init error! we can not find Add5Btn Button! ");

        newPos = add5Btn.GetComponent<Transform>().position;
        //newPos.x = Screen.width * 0.4f;
        //newPos.y = Screen.height * -0.05f;
        //add5Btn.GetComponent<Transform>().position = newPos;

        add5Btn.onClick.AddListener(delegate { this.OnClickAdd5Btn(); });
        add5Btn.gameObject.SetActive(false);

        endGameBtn = transform.Find("EndGameBtn").GetComponent<Button>();
        if (endGameBtn == null)
            Debug.Log("game ui init error! we can not find EndGameBtn Button! ");

        newPos.x = Screen.width * 0.2f;
        newPos.y = Screen.height * -0.1f;
        //endGameBtn.GetComponent<Transform>().position = newPos;
        endGameBtn.onClick.AddListener(delegate { this.OnClickEndGameBtn(); });
        endGameBtn.gameObject.SetActive(false);

        wildcardBtn = transform.Find("WildCardBtn").GetComponent<Button>();
        if (wildcardBtn == null)
            Debug.Log("game ui init error! we can not find wildcardBtn Button! ");

        newPos = wildcardBtn.GetComponent<Transform>().position;
        Debug.Log("the wild card btn's new pos is: " + newPos + "  the screen width is: " + Screen.width);

        wildcardBtn.onClick.AddListener(delegate { this.OnClickWildCardBtn(); });

        //init some children UI
        streakBonusUI = Object.FindObjectOfType<StreakBonusUI>();
        if (streakBonusUI == null)
            Debug.Log("GamePlayUI::Start()... streakBonusUI is null....");

        streakBonusTipsUI = Object.FindObjectOfType<StreakBonusTipsUI>();
        if(streakBonusTipsUI == null)
            Debug.Log("GamePlayUI::Start()... streakBonusTipsUI is null....");
        HideStreakBonusTipsUI();

        goldAndScoreUI = Object.FindObjectOfType<GoldAndScoreUI>();
        if (goldAndScoreUI == null)
            Debug.Log("GamePlayUI::Start()... goldAndScoreUI is null....");

        newPos = goldAndScoreUI.GetComponent<RectTransform>().position;
        Debug.Log("the origin pos x is: " + newPos.x);
        newPos.x += GetOffsetX();
        Debug.Log("the new pos x is: " + newPos.x);
        goldAndScoreUI.GetComponent<RectTransform>().position = newPos;

        settlementUI = Object.FindObjectOfType<SettlementUI>();
        if (settlementUI == null)
            Debug.Log("GamePlayUI::Start()... settlementUI is null....");
        //settlementUI.GetComponent<CanvasGroup>().alpha = 0;
        HideSettlementUI();

        loseGameUI = Object.FindObjectOfType<LoseGameUI>();
        if (loseGameUI == null)
            Debug.Log("GamePlayUI::Start()... loseGameUI is null....");
        //loseGameUI.GetComponent<CanvasGroup>().alpha = 0;
        HideLoseGameUI();

        lockEndGameUI = Object.FindObjectOfType<LockEndGameUI>();
        if (lockEndGameUI == null)
            Debug.Log("GamePlayUI::Start()... lockEndGameUI is null....");
        DisableLockEndGameUI();

        bombEndGameUI = Object.FindObjectOfType<BombEndGameUI>();
        if(bombEndGameUI == null)
            Debug.Log("GamePlayUI::Start()... bombEndGameUI is null....");
        HideBombEndGameUI();
        //DisableBombEndGameUI();

        congratulateUI = Object.FindObjectOfType<CongratulationUI>();
        if (congratulateUI == null)
            Debug.Log("GamePlayUI::Start()... congratulateUI is null....");
        HideCongratulationUI();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //used for reset the game ui, when we restart or replay a game.
    public void Reset()
    {
        bIsHidingAdd5Btn = false;

        //EnableAllGameButton();
        ShowAllGameUI();

        DisableWithdrawBtn();
        HideWithDrawBtn();

        //endGameBtn.gameObject.SetActive(false);

        DisableAdd5Btn();

        HideStreakBonusTipsUI();

        HideSettlementUI();

        HideLoseGameUI();
    }

    public void EnableWithdrawBtn()
    {
        withdrawBtn.enabled = true;
        withdrawBtn.gameObject.SetActive(true);
    }

    public void DisableWithdrawBtn()
    {
        withdrawBtn.enabled = false;
    }

    public void ShowWithDrawBtn()
    {
        withdrawBtn.gameObject.SetActive(true);
    }

    public void HideWithDrawBtn()
    {
        withdrawBtn.gameObject.SetActive(false);
    }

    public void ShowAdd5Btn()
    {
        /*Vector3 destPos = new Vector3(add5Btn.transform.position.x, Screen.height * 0.15f, add5Btn.transform.position.z);
        add5Btn.transform.DOMove(destPos, 0.7f);*/
        add5Btn.enabled = true ;
        //Vector3 dest = new Vector3(add5Btn.transform.position.x, Screen.height * 0.15f, add5Btn.transform.position.z);
        Vector3 dest = new Vector3(add5Btn.transform.position.x, -4.0f, add5Btn.transform.position.z);
        Debug.Log("the add5 btn target pos is : " + dest + " origin pos is: " + add5Btn.transform.position);
        StopAllCoroutines();
        StartCoroutine(ShowButton(add5Btn, dest, 0.7f));

        //add5Btn.gameObject.SetActive(true);
    }

    public void HideAdd5Btn()
    {
        /*Vector3 destPos = new Vector3(add5Btn.transform.position.x, 0.0f, add5Btn.transform.position.z);
        add5Btn.transform.DOMove(destPos, 0.7f);*/

        //add5Btn.gameObject.SetActive(false);

        if(bIsHidingAdd5Btn)
        {
            return;
        }

        bIsHidingAdd5Btn = true;
        add5Btn.enabled = false;
        Vector3 dest = new Vector3(add5Btn.transform.position.x, -8.0f, add5Btn.transform.position.z);
        StopAllCoroutines();
        StartCoroutine(HideButton(add5Btn, dest));
    }

    public void DisableAdd5Btn()
    {
        add5Btn.gameObject.SetActive(false);
    }

    IEnumerator  ShowButton(Button  btn, Vector3 destPos, float fWaitTime)
    {
        btn.gameObject.SetActive(true);

        float fBeginTime = 0.0f;
/*
        Vector3 moveSpeed = (destPos - btn.transform.position) / 0.7f;

        while(fBeginTime < fWaitTime)
        {
            fBeginTime += Time.deltaTime;
            yield return null;
        }

        while(fBeginTime < (fWaitTime + 0.7f))
        {
            fBeginTime += Time.deltaTime;
            btn.transform.position += moveSpeed * Time.deltaTime;
            yield return null;
        }*/

        btn.transform.DOMove(destPos, fWaitTime);

        while (fBeginTime < fWaitTime)
        {
            fBeginTime += Time.deltaTime;
            yield return null;
        }

    }

    IEnumerator HideButton(Button btn, Vector3 destPos)
    {
        float fBeginTime = 0.0f; ;

        Vector3 moveSpeed = (destPos - btn.transform.position) / 0.5f; 
        while (fBeginTime < 0.5f)
        {
            fBeginTime += Time.deltaTime;
            btn.transform.position += moveSpeed * Time.deltaTime;
            yield return null;
        }

        btn.gameObject.SetActive(false);

        if(btn.name == "Add5Btn" && bIsHidingAdd5Btn)
        {
            bIsHidingAdd5Btn = false;
        }
    }

    

    public void ShowEndGameBtn()
    {
        //endGameBtn.gameObject.SetActive(true);
        //Vector3 dest = new Vector3(endGameBtn.transform.position.x, Screen.height * 0.15f, endGameBtn.transform.position.z);
        Vector3 dest = new Vector3(endGameBtn.transform.position.x, -4.0f, endGameBtn.transform.position.z);

        //StopAllCoroutines();
        StartCoroutine(ShowButton(endGameBtn, dest, 0.7f));
    }

    public void HideEndGameBtn()
    {
        //endGameBtn.gameObject.SetActive(false);

        //Vector3 dest = new Vector3(endGameBtn.transform.position.x, Screen.height * -0.1f, endGameBtn.transform.position.z);
        Vector3 dest = new Vector3(endGameBtn.transform.position.x, -8.0f, endGameBtn.transform.position.z);
        //StopAllCoroutines();
        StartCoroutine(HideButton(endGameBtn, dest));
    }

    public void EnableAllGameButton()
    {
        ShowStreakBonusUI();

        add5Btn.enabled = true;
        wildcardBtn.enabled = true;
    }

    public void DisableAllGameButton()
    {
        withdrawBtn.enabled = false;
        add5Btn.enabled = false;
        wildcardBtn.enabled = false;
    }

    public void HideAllGameUI()
    {
        HideStreakBonusUI();

        HideStreakBonusTipsUI();

        withdrawBtn.gameObject.SetActive(false);
        add5Btn.gameObject.SetActive(false);
        wildcardBtn.gameObject.SetActive(false);
    }

    public void ShowAllGameUI()
    {
        ShowStreakBonusUI();

        withdrawBtn.gameObject.SetActive(true);
        add5Btn.gameObject.SetActive(true);
        wildcardBtn.gameObject.SetActive(true);
    }

    public void WinGame(int nCollect, int nClear, int nScoreStar)
    {
        HideAllGameUI();

        settlementUI.SetSettlementData(nCollect, nClear, nScoreStar);

        ShowCongratulationUI(nScoreStar);
        //ShowCongratulationsToPlayer();

        //ShowSettlementUI(nCollect, nClear);

        //FadeInSettlementUI(nCollect, nClear);
    }

    public void LoseGame(int nCollect, int nClear)
    {
        HideAllGameUI();

        ShowLoseGameUI(nCollect, nClear);
    }


    /*public void ShowCongratulationsToPlayer()
    {
        StartCoroutine(CongratulateToPlayer());
    }

    IEnumerator CongratulateToPlayer()
    {
        imgCongratulation.enabled = true;
        imgCongratulation.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        //imgCongratulation.do

        yield return null;
    }*/

    public void ShowStreakBonusUI()
    {
        Debug.Log("GamePlayUI... ShowStreakBonusUI...");
        streakBonusUI.GetComponent<CanvasGroup>().alpha = 1;
        streakBonusUI.GetComponent<CanvasGroup>().interactable = true;
        streakBonusUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void HideStreakBonusUI()
    {
        streakBonusUI.GetComponent<CanvasGroup>().alpha = 0;
        streakBonusUI.GetComponent<CanvasGroup>().interactable = false;
        streakBonusUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void ShowStreakBonusTipsUI()
    {
        streakBonusTipsUI.GetComponent<CanvasGroup>().alpha = 1;
        streakBonusTipsUI.GetComponent<CanvasGroup>().interactable = true;
        streakBonusTipsUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void HideStreakBonusTipsUI()
    {
        streakBonusTipsUI.GetComponent<CanvasGroup>().alpha = 0;
        streakBonusTipsUI.GetComponent<CanvasGroup>().interactable = false;
        streakBonusTipsUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void ShowSettlementUI(int nCollect, int nClear)
    {
        Debug.Log("GamePlayUI... ShowSettlementUI...");
        settlementUI.GetComponent<CanvasGroup>().alpha = 1;
        settlementUI.GetComponent<CanvasGroup>().interactable = true;
        settlementUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

        //settlementUI.SetSettlementData(nCollect, nClear);
    }

    public void FadeInSettlementUI(int nCollect, int nClear)
    {
        Debug.Log("GamePlayUI... FadeInSettlementUI...");
        settlementUI.GetComponent<CanvasGroup>().alpha = 1;
        settlementUI.GetComponent<CanvasGroup>().interactable = true;
        settlementUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

        //settlementUI.SetSettlementData(nCollect, nClear);

        settlementUI.FadeIn();
    }

    public void HideSettlementUI()
    {
        settlementUI.GetComponent<CanvasGroup>().alpha = 0;
        settlementUI.GetComponent<CanvasGroup>().interactable = false;
        settlementUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void ShowLoseGameUI(int nCollect, int nClear)
    {
        Debug.Log("GamePlayUI... ShowLoseGameUI...");
        /*float fTime = 0.0f;

        while (fTime < 0.5f)
        {
            fTime += Time.deltaTime;

            loseGameUI.transform.position +=
        }*/

        loseGameUI.GetComponent<CanvasGroup>().alpha = 1;
        loseGameUI.GetComponent<CanvasGroup>().interactable = true;
        loseGameUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

        loseGameUI.SetLoseGameData(nCollect, nClear);
    }

    public void HideLoseGameUI()
    {
        loseGameUI.GetComponent<CanvasGroup>().alpha = 0;
        loseGameUI.GetComponent<CanvasGroup>().interactable = false;
        loseGameUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void ShowLockEndGameUI()
    {
        Debug.Log("GamePlayUI... ShowLockEndGameUI...");
        
        lockEndGameUI.GetComponent<CanvasGroup>().alpha = 1;
        lockEndGameUI.GetComponent<CanvasGroup>().interactable = true;
        lockEndGameUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void HideLockEndGameUI()
    {
        lockEndGameUI.GetComponent<CanvasGroup>().alpha = 0;
        lockEndGameUI.GetComponent<CanvasGroup>().interactable = false;
        lockEndGameUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void DisableLockEndGameUI()
    {
        lockEndGameUI.gameObject.SetActive(false);
    }

    public void ShowBombEndGameUI()
    {
        Debug.Log("GamePlayUI... ShowBombEndGameUI...");

        bombEndGameUI.GetComponent<CanvasGroup>().alpha = 1;
        bombEndGameUI.GetComponent<CanvasGroup>().interactable = true;
        bombEndGameUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void HideBombEndGameUI()
    {
        bombEndGameUI.GetComponent<CanvasGroup>().alpha = 0;
        bombEndGameUI.GetComponent<CanvasGroup>().interactable = false;
        bombEndGameUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void DisableBombEndGameUI()
    {
        bombEndGameUI.gameObject.SetActive(false);
    }

    public void ShowCongratulationUI(int nScoreStar)
    {
        congratulateUI.GetComponent<CanvasGroup>().alpha = 1;
        congratulateUI.GetComponent<CanvasGroup>().interactable = true;
        congratulateUI.GetComponent<CanvasGroup>().blocksRaycasts = true;

        //here we call the congratulateUI's method to show each image and do the scale and fade.
        congratulateUI.FadeIn(nScoreStar);
    }

    public void HideCongratulationUI()
    {
        congratulateUI.GetComponent<CanvasGroup>().alpha = 0;
        congratulateUI.GetComponent<CanvasGroup>().interactable = false;
        congratulateUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void EnableWildCardBtn()
    {
        wildcardBtn.enabled = true;
    }

    public void DisableWildCardBtn()
    {
        wildcardBtn.enabled = false;
    }

    public void InitStreakBonus(GameplayMgr.StreakType streakType, GameplayMgr.StreakType nextStreak)
    {
        //Debug.Log("GamePlayUI::InitStreakBonus... the streakType is: " + streakType);
        streakBonusUI.InitStreakBonus(streakType, nextStreak);
    }

    public void SetStreakBonusStatus(int nCount, int nStreakBonus)
    {
        streakBonusUI.SetStreakBonusStatus(nCount, nStreakBonus);
    }

    public void SetClearAllStreakBonus(List<GameplayMgr.StreakBonusInfo> bonusInfos)
    {
        streakBonusUI.SetClearAllStreakBonus(bonusInfos);
    }

    public void WithDrawClearAllStreakBonus(List<GameplayMgr.StreakBonusInfo> bonusInfos)
    {
        streakBonusUI.WithDrawClearAllStreakBonus(bonusInfos);
    }

    public void AddGold(int nValue)
    {
        goldAndScoreUI.AddGold(nValue);
    }

    public void AddScore(int nValue)
    {
        goldAndScoreUI.AddScore(nValue);
    }

    void OnClickWithdrawBtn()
    {
        Debug.Log("GamePlayUI... You clicked Withdraw Button!");

        HideAdd5Btn();
        HideEndGameBtn();

        //GameplayMgr.Instance.WithdrawOnePoker();
        GameplayMgr.Instance.WithDrawClicked();
    }

    void OnClickAdd5Btn()
    {
        //Debug.Log("GamePlayUI... You clicked Add_5 Button!");

        HideAdd5Btn();
        HideEndGameBtn();

        GameplayMgr.Instance.Add5HandPoker();
    }

    public void OnClickEndGameBtn()
    {
        Debug.Log("GamePlayUI... You clicked OnClickEndGameBtn Button!");

        HideAdd5Btn();
        HideEndGameBtn();
        
        //todo: here we shoud call the end game process method.
        //...
        GameplayMgr.Instance.LoseGame();
    }

    void OnClickWildCardBtn()
    {
        Debug.Log("GamePlayUI...  You Clicked WildCard Button");

        GameplayMgr.Instance.OnClickWildCardBtn();
    }

    public void IncWildCard()
    {
        WildCardButton wildCardScript = wildcardBtn.GetComponent<WildCardButton>();

        wildCardScript.IncWildCardItem();
    }

    public void DecWildCard()
    {
        WildCardButton wildCardScript = wildcardBtn.GetComponent<WildCardButton>();

        wildCardScript.DecWildCardItem();
    }

    public void SetWildCardCount(int nCount)
    {
        WildCardButton wildCardScript = wildcardBtn.GetComponent<WildCardButton>();

        wildCardScript.SetWildItemCount(nCount);
    }

    public void ShowCoinEffect()
    {
        Vector3 newPos = streakBonusUI.transform.position - Vector3.forward * 2.0f;
        //GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCoin, newPos, Quaternion.identity);
        GameObject streakCoin = Instantiate(GameplayMgr.Instance.FXCoin, GetComponent<Transform>());
        
        streakCoin.transform.position = newPos;
        streakCoin.transform.localScale = new Vector3(100.0f, 100.0f, 1.0f);
        //streakCoin.GetComponent<Renderer>().sortingLayerName = "UI";
        //streakCoin.GetComponent<Renderer>().sortingOrder = 1;

        ParticleSystem particleSystem;
        particleSystem = streakCoin.transform.GetComponentInChildren<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        //particleRenderer.sortingLayerName = "Default";
        //particleRenderer.sortingOrder = -1;

        Debug.Log(GetComponent<Canvas>().sortingLayerName);
        Debug.Log(GetComponent<Canvas>().sortingOrder);

        Debug.Log(streakCoin.GetComponent<Renderer>().sortingLayerName);
        Debug.Log(streakCoin.GetComponent<Renderer>().sortingOrder);
    }
}
