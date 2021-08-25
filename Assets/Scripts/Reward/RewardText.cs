using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardText : MonoBehaviour
{
    public string RewardDataName = "";
    TMP_Text text;
    private void OnEnable()
    {
        var propertyInfo = typeof(MenuManager).GetProperty(RewardDataName);
        text.text = ((float)propertyInfo.GetValue(MenuManager.Instance)).ToString();
    }
    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
