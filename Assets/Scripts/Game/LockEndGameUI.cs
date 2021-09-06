using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockEndGameUI : MonoBehaviour
{
    GamePlayUI gameplayUI;

    Button endGameBtn;

    private void Awake()
    {
        gameplayUI = Object.FindObjectOfType<GamePlayUI>();
        if (gameplayUI == null)
            Debug.Log("LockEndGameUI::Awake()... gameplayUI is null....");

        endGameBtn = transform.Find("EndGameBtn").GetComponent<Button>();
        endGameBtn.onClick.AddListener(delegate { this.OnClickEndGameBtn(); });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClickEndGameBtn()
    {
        gameplayUI.HideLockEndGameUI();

        gameplayUI.OnClickEndGameBtn();
    }
}
