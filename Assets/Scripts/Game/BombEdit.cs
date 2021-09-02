using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombEdit : MonoBehaviour
{
    public RectTransform rectTransform;

    public int stepCount = 1;
    InputField stepInput;

    public DragButton dragBtn;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        stepInput = rectTransform.transform.Find("StepInput").GetComponent<InputField>();
        stepInput.onValueChanged.AddListener(delegate (string strValue) { this.OnInputValue(strValue); });
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

        if (pokerInfo.nItemType != (int)GameDefines.PokerItemType.Bomb)
        {
            Debug.Log("BombEdit... init, we got wrong poker info....");
            return;
        }

        //Debug.Log("we init the poker info strItemInfo." + pokerInfo.strItemInfo);
        if(pokerInfo.strItemInfo.Length > 0)
        {
            stepCount = int.Parse(pokerInfo.strItemInfo);

            CheckCorrectStepCount();
        }
        
    }

    void OnInputValue(string strInput)
    {
        Debug.Log("here we input a value is: " + strInput);

        stepCount = int.Parse(strInput);

        CheckCorrectStepCount();

        string strInfo = string.Format("{0}", stepCount);
        dragBtn.strItemInfo = strInfo;
    }

    void CheckCorrectStepCount()
    {
        stepCount = stepCount < 1 ? 1 : stepCount;
        stepCount = stepCount > 50 ? 50 : stepCount;
    }
}
