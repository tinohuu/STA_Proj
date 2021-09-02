using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AscDesEdit : MonoBehaviour
{
    public RectTransform rectTransform;

    public GameplayMgr.PokerSuit suitSelection;
    Dropdown suitSelect;

    public int numberSelection;//1-13
    Dropdown numberSelect;

    Text textDisplay;

    public DragButton dragBtn;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        suitSelect = rectTransform.transform.Find("Suit").GetComponent<Dropdown>();
        suitSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectSuit(index); });

        numberSelect = rectTransform.transform.Find("Number").GetComponent<Dropdown>();
        numberSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectNumber(index); });

        textDisplay = rectTransform.transform.Find("Text").GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(DragButton button, JsonReadWriteTest.PokerInfo pokerInfo)
    {
        dragBtn = button;

        if (pokerInfo.nItemType != (int)GameDefines.PokerItemType.Ascending_Poker
            && pokerInfo.nItemType != (int)GameDefines.PokerItemType.Descending_Poker)
        {
            Debug.Log("AscDesEdit... init, we got wrong poker info....");
            return;
        }

        if(pokerInfo.nItemType == (int)GameDefines.PokerItemType.Descending_Poker)
        {
            textDisplay.text = "ºı≈∆";
        }

        if (pokerInfo.strItemInfo == "")
        {
            suitSelection = GameplayMgr.PokerSuit.Suit_None;
            suitSelect.value = (int)suitSelection;
            numberSelection = 0;
            numberSelect.value = numberSelection;
        }
        else
        {
            string[] strParams = pokerInfo.strItemInfo.Split('_');
            suitSelection = (GameplayMgr.PokerSuit)int.Parse(strParams[0]);
            numberSelection = int.Parse(strParams[1]);

            suitSelect.value = (int)suitSelection;
            numberSelect.value = numberSelection;
        }
    }

    void OnSelectSuit(int index)
    {
        
        suitSelection = (GameplayMgr.PokerSuit)index;

        string strInfo = string.Format("{0}_{1}", (int)suitSelection, numberSelection);
        dragBtn.strItemInfo = strInfo;

        Debug.Log("AscDesEdit...  here we select a suit... value is: " + strInfo);

        PokerAreaMgr.Instance.updateSelectPokerItemInfo(dragBtn);
    }

    void OnSelectNumber(int index)
    {
        
        numberSelection = index;

        string strInfo = string.Format("{0}_{1}", (int)suitSelection, numberSelection);
        dragBtn.strItemInfo = strInfo;
        Debug.Log("AscDesEdit...  here we select a number... value is: " + strInfo);

        PokerAreaMgr.Instance.updateSelectPokerItemInfo(dragBtn);
    }
    
    /*string FormatItemInfo()
    {
        if(suitSelection == GameplayMgr.PokerSuit.Suit_None 
            || numberSelection == 0)
        {
            return "";
        }

        string strParams = string.Format("{0}_{1}", (int))
    }*/
}
