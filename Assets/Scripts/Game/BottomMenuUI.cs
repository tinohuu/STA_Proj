using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//this file is used to process botton UI messages

public class BottomMenuUI : MonoBehaviour
{
    private InputField handCardInput;
    private int handCardCount = 0;

    //right-up menu can display chapter, level, hand card count and desk card count.
    //RightUpMenuUI rightUpMenu;

    // Start is called before the first frame update
    void Start()
    {
        //handCardCount = GetComponent<InputField>();
        handCardInput = transform.Find("HandCardInput").GetComponent<InputField>();
        handCardInput.onEndEdit.AddListener(delegate { this.OnInputHandCardCountEnd(); });

        //here we should init the handcard count according to the level config file.
        int nCurrentLevel = EditorScriptMgr.Instance.nCurrentLevel;
        SetHandCardDisplay(nCurrentLevel);

        /*rightUpMenu = Object.FindObjectOfType<RightUpMenuUI>();
        if (rightUpMenu == null)
            Debug.Log("BottomMenuUI::Start() can not init right-up menu. check your code.");*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHandCardDisplay(int nLevel)
    {
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[nLevel - 1];
        handCardInput.text = string.Format("{0}", data.handPokerCount);
    }

    private void OnInputHandCardCountEnd()
    {
        //Debug.Log("hand card count text is: " + handCardInput.text);
        handCardCount = int.Parse(handCardInput.text);
        //Debug.Log("hand card count is: " + handCardCount);

        //here we get the new handCardCount, so we store it and update the display card count
        PokerAreaMgr.Instance.UpdateHandCardInfos(handCardCount);
    }
}
