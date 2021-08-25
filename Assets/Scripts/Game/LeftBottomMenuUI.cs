using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftBottomMenuUI : MonoBehaviour
{
    public LevelInfoInit levelInfoInit;

    private Button AddLevelBtn;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("LeftBottomMenuUI::Start... begin...");

        levelInfoInit = Object.FindObjectOfType<LevelInfoInit>();
        if (levelInfoInit == null)
            Debug.Log("LeftBottomMenuUI::Start... can not init levelinfoinit menu. check your code.");

        AddLevelBtn = transform.Find("AddLevel").GetComponent<Button>();
        AddLevelBtn.onClick.AddListener(delegate () { this.OnClickAddLevel(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickAddLevel()
    {
        int nNewLevel = EditorScriptMgr.Instance.AddNewLevel();

        Debug.Log("here we add a new leve, ID is: " + nNewLevel);

        levelInfoInit.SwitchToNewLevel(nNewLevel);
    }
}
