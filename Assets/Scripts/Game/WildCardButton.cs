using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WildCardButton : MonoBehaviour
{
    Image numberBG;

    int nItemCount = 0;
    TMP_Text itemCount;

    GameObject buyPrice;
    TMP_Text txtPrice;

    private void Awake()
    {
        numberBG = gameObject.transform.Find("NumberBG").GetComponent<Image>() ;
        if (numberBG == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the numberBG is null...");

        itemCount = numberBG.transform.Find("Number").GetComponent<TMP_Text>();
        if (itemCount == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the itemCount is null...");

        buyPrice = gameObject.transform.Find("BuyPrice").gameObject;
        if (buyPrice == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the buyPrice is null...");

        txtPrice = buyPrice.transform.Find("Price").GetComponent<TMP_Text>();
        if (txtPrice == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the txtPrice is null...");

        nItemCount = Reward.Data[RewardType.WildCard];
        //itemCount.text = nItemCount.ToString();

        //UpdateItemCount();

    }
    // Start is called before the first frame update
    void Start()
    {
        //UpdateItemDisplay();

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
        else if (nItemCount >= 10)
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
            ShowWildCount();
            HideWildPrice();
        }
        else
        {
            HideWildCount();
            ShowWildPrice();
        }
    }

    public void IncWildCardItem()
    {
        nItemCount++;

        UpdateItemCount();

        UpdateItemDisplay();

        Debug.Log("the IncWildCardItem nItemCount is: " + nItemCount);
    }

    public void DecWildCardItem()
    {
        nItemCount--;

        nItemCount = nItemCount < 0 ? 0 : nItemCount;

        UpdateItemCount();

        UpdateItemDisplay();

        Debug.Log("the DecWildCardItem nItemCount is: " + nItemCount);
    }

    public void SetWildItemCount(int nCount)
    {
        nItemCount = nCount;

        UpdateItemCount();

        UpdateItemDisplay();
    }

    void HideWildCount()
    {
        numberBG.enabled = false;
        itemCount.enabled = false;
    }

    void ShowWildCount()
    {
        numberBG.enabled = true;
        itemCount.enabled = true;
    }

    void ShowWildPrice()
    {
        int nPrice = GameplayMgr.Instance.GetWildCardCost();
        if (nPrice <= 1000)
            txtPrice.text = string.Format("{0}", nPrice);
        else
            txtPrice.text = string.Format("{0}K", (float)nPrice/1000.0f);

        buyPrice.SetActive(true);
    }

    void HideWildPrice()
    {
        buyPrice.SetActive(false);
    }
}

