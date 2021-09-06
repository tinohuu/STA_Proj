using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLevelPowerup : MonoBehaviour
{
    public GameObject PromptWindowPrefab;
    public RewardType RewardType;
    public GameObject PurchaseImage;
    public GameObject InUseImage;
    public GameObject TextImage;
    public bool InUse = false;
    public bool Interactable = false;
    ButtonAnimator button;
    CanvasGroup canvasGroup;
    private void Start()
    {
        Interactable = MapManager.Instance.Data.CompleteLevel + 1 >= MapManager.Instance.FunctionConfigsByFuncID[(int)RewardType + 1012 - 8].FunctionParams;
        button = GetComponent<ButtonAnimator>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = Interactable ? 1 : 0.5f;
        //button.Interactable = Interactable;

        Reward.Data[RewardType] = UnityEngine.Random.Range(1, 10);
    }
    // Start is called before the first frame update
    void Update()
    {
        InUseImage.SetActive(Interactable && InUse);
        PurchaseImage.SetActive(Interactable && !InUse && Reward.Data[RewardType] == 0);
        TextImage.SetActive(Interactable && !InUse && Reward.Data[RewardType] != 0);
    }

    public void OnClick()
    {
        if (!Interactable)
        {
            TMP_Text text = Window.CreateWindowPrefab(PromptWindowPrefab).GetComponentInChildren<TMP_Text>();
            string rawString = text.text;
            rawString = string.Format(text.text, RewardType.ToString(), MapManager.Instance.FunctionConfigsByFuncID[(int)RewardType + 1012 - 8].FunctionParams);
            rawString = System.Text.RegularExpressions.Regex.Replace(rawString, "([a-z])_?([A-Z])", "$1 $2");
            text.text = rawString;
            return;
        }

        if (Reward.Data[RewardType] == 0)
        {
            // todo: go to store
        }
        else
        {
            InUse = !InUse;
        }
    }


}
