using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        withdrawBtn.GetComponent<Transform>().position = newPos;

        withdrawBtn.onClick.AddListener(delegate { this.OnClickWithdrawBtn(); });
        withdrawBtn.enabled = false;

        add5Btn = transform.Find("Add5Btn").GetComponent<Button>();
        if (add5Btn == null)
            Debug.Log("game ui init error! we can not find Add5Btn Button! ");

        newPos = add5Btn.GetComponent<Transform>().position;
        newPos.x = Screen.width * 0.4f;
        newPos.y = Screen.height * 0.15f;
        add5Btn.GetComponent<Transform>().position = newPos;

        add5Btn.onClick.AddListener(delegate { this.OnClickAdd5Btn(); });
        add5Btn.gameObject.SetActive(false);

        endGameBtn = transform.Find("EndGameBtn").GetComponent<Button>();
        if (endGameBtn == null)
            Debug.Log("game ui init error! we can not find EndGameBtn Button! ");

        newPos.x = Screen.width * 0.2f;
        newPos.y = Screen.height * 0.15f;
        endGameBtn.GetComponent<Transform>().position = newPos;
        endGameBtn.onClick.AddListener(delegate { this.OnClickEndGameBtn(); });
        endGameBtn.gameObject.SetActive(false);

        wildcardBtn = transform.Find("WildCardBtn").GetComponent<Button>();
        if (wildcardBtn == null)
            Debug.Log("game ui init error! we can not find wildcardBtn Button! ");

        newPos = wildcardBtn.GetComponent<Transform>().position;
        newPos.x = Screen.width * 0.95f;
        newPos.y = Screen.height * 0.1f;
        wildcardBtn.GetComponent<Transform>().position = newPos;

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

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //used for reset the game ui, when we restart or replay a game.
    public void Reset()
    {
        //EnableAllGameButton();
        ShowAllGameUI();

        DisableWithdrawBtn();

        HideAdd5Btn();

        HideStreakBonusTipsUI();

        HideSettlementUI();

        HideLoseGameUI();
    }

    public void EnableWithdrawBtn()
    {
        withdrawBtn.enabled = true;
    }

    public void DisableWithdrawBtn()
    {
        withdrawBtn.enabled = false;
    }

    public void ShowAdd5Btn()
    {
        add5Btn.gameObject.SetActive(true);
    }

    public void HideAdd5Btn()
    {
        add5Btn.gameObject.SetActive(false);
    }

    public void ShowEndGameBtn()
    {
        endGameBtn.gameObject.SetActive(true);
    }

    public void HideEndGameBtn()
    {
        endGameBtn.gameObject.SetActive(false);
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

    public void WinGame(int nCollect, int nClear)
    {
        HideAllGameUI();

        ShowCongratulationUI();

        ShowSettlementUI(nCollect, nClear);
    }

    public void LoseGame(int nCollect, int nClear)
    {
        HideAllGameUI();

        ShowLoseGameUI(nCollect, nClear);
    }

    public void ShowCongratulationUI()
    {
    }

    public void ShowStreakBonusUI()
    {
        Debug.Log("GamePlayUI... ShowSettlementUI...");
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

        settlementUI.SetSettlementData(nCollect, nClear);
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

        GameplayMgr.Instance.WithdrawOnePoker();
    }

    void OnClickAdd5Btn()
    {
        //Debug.Log("GamePlayUI... You clicked Add_5 Button!");

        HideAdd5Btn();
        HideEndGameBtn();

        GameplayMgr.Instance.Add5HandPoker();
    }

    void OnClickEndGameBtn()
    {
        HideEndGameBtn();

        HideAdd5Btn();

        //todo: here we shoud call the end game process method.
        //...
        GameplayMgr.Instance.LoseGame();
    }

    void OnClickWildCardBtn()
    {
        Debug.Log("GamePlayUI...  You Clicked WildCard Button");

        GameplayMgr.Instance.OnClickWildCardBtn();
    }
}
