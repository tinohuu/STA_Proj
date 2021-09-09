using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AddNEdit : MonoBehaviour
{
    public RectTransform rectTransform;

    int nCountIndex;
    Dropdown countSelect;

    public DragButton dragBtn;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        countSelect = rectTransform.transform.Find("NSelect").GetComponent<Dropdown>();
        countSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectCount(index); });

        nCountIndex = 0;
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

        if(pokerInfo.nItemType != (int)GameDefines.PokerItemType.Add_N_Poker)
        {
            Debug.Log("AddNEdit... init, we got wrong poker info....");
            return;
        }

        if(pokerInfo.strItemInfo.Length == 0)
        {
            countSelect.value = nCountIndex;
        }
        else
        {
            nCountIndex = int.Parse(pokerInfo.strItemInfo) - 1;
            countSelect.value = nCountIndex ;
        }
        
    }

    void OnSelectCount(int index)
    {
        nCountIndex = countSelect.value;

        string strItemInfo = string.Format("{0}", nCountIndex + 1);
        dragBtn.strItemInfo = strItemInfo;

        Debug.Log("AddNEdit...  here we select a suit... value is: " + strItemInfo);

        PokerAreaMgr.Instance.updateSelectPokerItemInfo(dragBtn);
    }
}
