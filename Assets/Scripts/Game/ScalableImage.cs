using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScalableImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    bool bIsDragging = false;
    bool bIsScaling = false;

    public RectTransform rectTransform;

    Vector2 lastMousePos;
    Vector2 lastDragPos;

    public int nGroupID = 0;
    InputField inputGroup;

    public string strUnlockPokers = "";

    Button btnComponent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        inputGroup = rectTransform.transform.Find("GroupInput").GetComponent<InputField>();
        inputGroup.onValueChanged.AddListener(delegate (string strInput) { this.OnInputGroup(strInput); });

        btnComponent = GetComponent<Button>();
        btnComponent.onClick.AddListener(delegate () { this.onClicked(); });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(JsonReadWriteTest.LockArea lockArea)
    {
        nGroupID = lockArea.nAreaID;
        inputGroup.text = string.Format("{0}", nGroupID);

        rectTransform.sizeDelta = new Vector2(lockArea.fWidth, lockArea.fHeight);
        rectTransform.localPosition = new Vector3(lockArea.fPosX, lockArea.fPosY, 0.0f);
    }

    public void InitUnlockArea(JsonReadWriteTest.UnlockArea unlockArea)
    {
        nGroupID = unlockArea.nAreaID;
        inputGroup.text = string.Format("{0}", nGroupID);

        strUnlockPokers = unlockArea.strUnlockPokers;

        rectTransform.sizeDelta = new Vector2(unlockArea.fWidth, unlockArea.fHeight);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bIsDragging = true;
            lastDragPos = eventData.position;
            //Debug.Log("here we begin to drag a lock... ...");
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            bIsScaling = true;
            lastMousePos = eventData.position;
            //Debug.Log("here we begin to scale a lock area... ...");
        }

        /*if (eventData.button == PointerEventData.InputButton.Right)
        {
            bIsRotating = true;
        }*/
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (bIsDragging)
        {
            Vector3 position = new Vector3(eventData.position.x, eventData.position.y, rectTransform.position.z);
            Vector2 deltaPos = eventData.position - lastDragPos;
            rectTransform.position = new Vector3(rectTransform.position.x + deltaPos.x, rectTransform.position.y + deltaPos.y, rectTransform.position.z);

            lastDragPos = eventData.position;
        }

        if(bIsScaling)
        {
            Vector3 position = new Vector3(eventData.position.x, eventData.position.y, rectTransform.position.z);
            Vector2 deltaSize = eventData.position - lastMousePos;
            Vector2 newSize = new Vector2(rectTransform.rect.width + deltaSize.x, rectTransform.rect.height + deltaSize.y);

            

            if (newSize.x <= 100f)
            {
                newSize.x = 100f;
            }
            if (newSize.y <= 100f)
            {
                newSize.y = 100f;
            }
            if(newSize.x >= 1440f)
            {
                newSize.x = 1440f;
            }
            if (newSize.y >= 800f)
            {
                newSize.y = 800f;
            }

            //Debug.Log("the new size is: " + newSize + "  delta size is: " + deltaSize);
            rectTransform.sizeDelta = newSize;

            lastMousePos = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bIsDragging = false;
        bIsScaling = false;
    }

    void OnInputGroup(string strInput)
    {
        Debug.Log("here we input a group number for a lock area... the value is: " + strInput);

        if (strInput != "")
        {
            nGroupID = int.Parse(strInput);
        }

    }

    void onClicked()
    {
        if(tag == "lockArea")
        {
            PokerAreaMgr.Instance.setCurrentSelectLockArea(this);
        }

        if(tag ==  "unlockArea")
        {
            PokerAreaMgr.Instance.setCurrentSelectUnlockArea(this);
        }
    }

}
