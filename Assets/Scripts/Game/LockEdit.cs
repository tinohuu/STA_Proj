using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LockEdit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    bool bIsDragging = false;
    //bool bIsRotating = false;

    public RectTransform rectTransform;

    public GameplayMgr.PokerSuit suitSelection;
    Dropdown suitSelect;

    public GameplayMgr.PokerColor colorSelection;
    Dropdown colorSelect;

    public int numberSelection;//1-13
    Dropdown numberSelect;

    public int nGroupID;
    InputField inputGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(delegate () { this.OnClickLock(); });

        suitSelect = rectTransform.transform.Find("Suit_Dropdown").GetComponent<Dropdown>();
        suitSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectSuit(index); });

        colorSelect = rectTransform.transform.Find("Color_Dropdown").GetComponent<Dropdown>();
        colorSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectColor(index); });

        numberSelect = rectTransform.transform.Find("Number_Dropdown").GetComponent<Dropdown>();
        numberSelect.onValueChanged.AddListener(delegate (int index) { this.OnSelectNumber(index); });

        inputGroup = rectTransform.transform.Find("InputGroup").GetComponent<InputField>();
        inputGroup.onValueChanged.AddListener(delegate (string strInput) { this.OnInputGroup(strInput); });

        /*Debug.Log("LockEdit::Awake ...  entering...... ......");
        if (inputGroup == null)
            Debug.Log("Awake ...  the inputGroup is null");*/
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(JsonReadWriteTest.LockInfo lockInfo)
    {
        suitSelection   = (GameplayMgr.PokerSuit)lockInfo.nSuit;
        colorSelection  = (GameplayMgr.PokerColor)lockInfo.nColor;
        numberSelection = lockInfo.nNumber;
        nGroupID        = lockInfo.nGroupID;

        Debug.Log("the lock group id is: " + nGroupID);

        if (inputGroup == null)
            Debug.Log("the input group is null!!!");

        inputGroup.text = string.Format("{0}", nGroupID);

        suitSelect.value   = lockInfo.nSuit;
        colorSelect.value  = lockInfo.nColor;
        numberSelect.value = lockInfo.nNumber;
    }

    private void OnClickLock()
    {
        Debug.Log("here we click a lock... ...");

        PokerAreaMgr.Instance.setCurrentSelectLock(this);
    }

    void OnSelectSuit(int index)
    {
        Debug.Log("here we select a suit... value is: " + index);
        suitSelection = (GameplayMgr.PokerSuit)index;

    }

    void OnSelectColor(int index)
    {
        Debug.Log("here we select a color... value is: " + index);
        colorSelection = (GameplayMgr.PokerColor)index;
    }

    void OnSelectNumber(int index)
    {
        Debug.Log("here we select a number... value is: " + index);
        numberSelection = index;
    }

    void OnInputGroup(string strInput)
    {
        Debug.Log("here we input a group number... value is: " + strInput);

        if(strInput != "")
        {
            nGroupID = int.Parse(strInput);
        }
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bIsDragging = true;
            Debug.Log("here we begin to drag a lock... ...");
        }

        /*if (eventData.button == PointerEventData.InputButton.Right)
        {
            bIsRotating = true;
        }*/
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(bIsDragging)
        {
            Vector3 position = new Vector3(eventData.position.x, eventData.position.y, rectTransform.position.z);

            rectTransform.position = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bIsDragging = false;
        //bIsRotating = false;


    }
}
