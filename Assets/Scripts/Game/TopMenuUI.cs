//2021.7.15 created

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

//here we process the top menu UI message
public class TopMenuUI : MonoBehaviour
{
    private Button addCardBtn;

    private Button addLockBtn;

    private Button addLockAreaBtn;

    private Toggle displayLockArea;

    private Button addAscendingBtn;

    private Button addDescendingBtn;

    private Button addBombBtn;

    private Button saveLevelBtn;

    
    // Start is called before the first frame update
    void Start()
    {
        addCardBtn = transform.Find("AddCard").GetComponent<Button>();
        addCardBtn.onClick.AddListener(delegate() { this.OnClickAddCard();});

        addLockBtn = transform.Find("AddLock").GetComponent<Button>();
        addLockBtn.onClick.AddListener(delegate () { this.OnClickAddLock(); });

        addLockAreaBtn = transform.Find("AddLockArea").GetComponent<Button>();
        addLockAreaBtn.onClick.AddListener(delegate () { this.OnClickAddLockArea(); });

        displayLockArea = transform.Find("LockToggle").GetComponent<Toggle>();
        displayLockArea.onValueChanged.AddListener(delegate (bool bValue) { this.onDisplayLockAreaChanged(bValue); });

        addAscendingBtn = transform.Find("AddAscendingCard").GetComponent<Button>();
        addAscendingBtn.onClick.AddListener(delegate () { this.OnClickAddAscendingCard(); });

        addDescendingBtn = transform.Find("AddDescendingCard").GetComponent<Button>();
        addDescendingBtn.onClick.AddListener(delegate () { this.OnClickAddDescendingCard(); });

        addBombBtn = transform.Find("AddBombCard").GetComponent<Button>();
        addBombBtn.onClick.AddListener(delegate () { this.OnClickAddBombCard(); });
        

        saveLevelBtn = transform.Find("SaveLevel").GetComponent<Button>();
        saveLevelBtn.onClick.AddListener(delegate () { this.OnClickSaveLevel(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickAddCard()
    {
        PokerAreaMgr.Instance.AddOnePoker();
    }

    private void OnClickAddLock()
    {
        PokerAreaMgr.Instance.AddOneLock();
    }

    private void OnClickAddLockArea()
    {
        PokerAreaMgr.Instance.AddLockArea();
    }

    private void onDisplayLockAreaChanged(bool bValue)
    {
        Debug.Log("here we clicked the toggle..." + bValue);

        PokerAreaMgr.Instance.onDisplayLockAreaChanged(bValue);
    }

    private void OnClickAddAscendingCard()
    {
        Debug.Log("here we clicked the add ascending poker button...");

        PokerAreaMgr.Instance.AddAscendingCard();
    }

    private void OnClickAddDescendingCard()
    {
        Debug.Log("here we clicked the add descending poker button...");

        PokerAreaMgr.Instance.AddDescendingCard();
    }

    private void OnClickAddBombCard()
    {
        Debug.Log("here we clicked the add bomb poker button...");

        PokerAreaMgr.Instance.AddBombCard();
    }

    private void OnClickSaveLevel()
    {
        //we first sort the lock area here
        PokerAreaMgr.Instance.PrepareLockAreaDataForSave();
        PokerAreaMgr.Instance.PrepareUnlockAreaDataForSave();

        PokerAreaMgr.Instance.PrepareLockDataForSave();

        //TODO: here we store the current level information
        EditorScriptMgr.Instance.SaveCurrentLevel();

        Messagebox.MessageBox(IntPtr.Zero, "保存成功！", "提示", 0);

    }

    public void OnGUI()
    {
        //GUI.Button(new Rect(1200.0f, 0.0f, 100.0f, 50.0f), "保存成功！");
    }

    public class Messagebox
    {
        [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr handle, String message, String title, int type);
    }

}
