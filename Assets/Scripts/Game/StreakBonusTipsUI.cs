using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreakBonusTipsUI : MonoBehaviour
{
    RectTransform rectTrans;

    GamePlayUI gameplayUI;

    Button okBtn;

    // Start is called before the first frame update
    void Start()
    {
        gameplayUI = Object.FindObjectOfType<GamePlayUI>();
        if (gameplayUI == null)
            Debug.Log("StreakBonusUI::Awake()... gameplayUI is null....");

        rectTrans = GetComponent<RectTransform>();

        okBtn = rectTrans.transform.Find("OkBtn").GetComponent<Button>();
        okBtn.onClick.AddListener(delegate { this.OnClickOKBtn(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClickOKBtn()
    {
        Debug.Log("here you clicked OK Button!");

        gameplayUI.HideStreakBonusTipsUI();
    }
}
