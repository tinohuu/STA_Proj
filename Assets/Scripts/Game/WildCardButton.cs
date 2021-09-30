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

    private void Awake()
    {
        numberBG = gameObject.transform.Find("NumberBG").GetComponent<Image>() ;
        if (numberBG == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the numberBG is null...");

        itemCount = numberBG.transform.Find("Number").GetComponent<TMP_Text>();
        if (itemCount == null)
            Debug.Log("_______________WildCardButton::Awake ... ... the itemCount is null...");

        nItemCount = Reward.Data[RewardType.WildCard]; 
        itemCount.text = nItemCount.ToString();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncWildCardItem()
    {
        nItemCount++;

        itemCount.text = nItemCount.ToString();

        Debug.Log("the IncWildCardItem nItemCount is: " + nItemCount);
    }

    public void DecWildCardItem()
    {
        nItemCount--;

        nItemCount = nItemCount < 0 ? 0 : nItemCount;

        itemCount.text = nItemCount.ToString();

        Debug.Log("the DecWildCardItem nItemCount is: " + nItemCount);
    }

    public void SetWildItemCount(int nCount)
    {
        nItemCount = nCount;

        itemCount.text = nItemCount.ToString();
    }
}
