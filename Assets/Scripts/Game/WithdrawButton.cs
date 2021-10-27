using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WithdrawButton : MonoBehaviour
{
    Image numberBG;

    int nItemCount = 0;
    TMP_Text itemCount;

    GameObject buyPrice;
    TMP_Text txtPrice;

    private void Awake()
    {
        numberBG = gameObject.transform.Find("NumberBG").GetComponent<Image>();
        if (numberBG == null)
            Debug.Log("_______________WithdrawButton::Awake ... ... the numberBG is null...");

        itemCount = numberBG.transform.Find("Number").GetComponent<TMP_Text>();
        if (itemCount == null)
            Debug.Log("_______________WithdrawButton::Awake ... ... the itemCount is null...");

        buyPrice = gameObject.transform.Find("BuyPrice").gameObject;
        if (buyPrice == null)
            Debug.Log("_______________WithdrawButton::Awake ... ... the buyPrice is null...");

        txtPrice = buyPrice.transform.Find("Price").GetComponent<TMP_Text>();
        if (txtPrice == null)
            Debug.Log("_______________WithdrawButton::Awake ... ... the txtPrice is null...");


        nItemCount = Reward.Data[RewardType.Undo];

        //UpdateItemCount();

        //UpdateItemDisplay();
        /*if (nItemCount > 99)
            itemCount.text = "99+";
        else
            itemCount.text = nItemCount.ToString();*/

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateItemCount()
    {
        if (nItemCount > 99)
        {
            itemCount.text = "99+";
            numberBG.GetComponent<RectTransform>().sizeDelta = new Vector2(124.0f, 84.0f); 
        }
        else if(nItemCount >= 10)
        {
            itemCount.text = nItemCount.ToString();
            numberBG.GetComponent<RectTransform>().sizeDelta = new Vector2(104.0f, 84.0f);
        }
        else
        {
            itemCount.text = nItemCount.ToString();
            numberBG.GetComponent<RectTransform>().sizeDelta = new Vector2(84.0f, 84.0f);
        }
            
    }

    void UpdateItemDisplay()
    {
        if (nItemCount > 0)
        {
            ShowWithdrawCount();
            HideWithdrawPrice();
        }
        else
        {
            HideWithdrawCount();
            ShowWithdrawPrice();
        }
    }

    public void IncWildCardItem()
    {
        nItemCount++;

        UpdateItemCount();

        UpdateItemDisplay();
    }

    public void DecWildCardItem()
    {
        nItemCount--;

        nItemCount = nItemCount < 0 ? 0 : nItemCount;

        UpdateItemCount();

        UpdateItemDisplay();
    }

    public void SetWithdrawItemCount(int nCount)
    {
        nItemCount = nCount;

        UpdateItemCount();

        UpdateItemDisplay();
    }

    void ShowWithdrawCount()
    {
        numberBG.enabled = true;
        itemCount.enabled = true;
    }

    void HideWithdrawCount()
    {
        numberBG.enabled = false;
        itemCount.enabled = false;
    }

    void ShowWithdrawPrice()
    {
        int nPrice = GameplayMgr.Instance.GetWithdrawCost();
        if (nPrice <= 1000)
            txtPrice.text = string.Format("{0}", nPrice);
        else
            txtPrice.text = string.Format("{0}K", (float)nPrice / 1000.0f);

        buyPrice.SetActive(true);
    }

    void HideWithdrawPrice()
    {
        buyPrice.SetActive(false);
    }
}
