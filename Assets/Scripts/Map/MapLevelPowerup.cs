using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLevelPowerup : MonoBehaviour
{
    public RewardType RewardType;
    public GameObject PurchaseImage;
    public GameObject InUseImage;
    public GameObject TextImage;
    public bool InUse = false;
    private void Start()
    {
        ConfigsAsset.GetConfig("PublicConfig");
        Reward.Data[RewardType] = UnityEngine.Random.Range(1, 10);
    }
    // Start is called before the first frame update
    void Update()
    {

        InUseImage.SetActive(InUse);
        PurchaseImage.SetActive(!InUse && Reward.Data[RewardType] == 0);
        TextImage.SetActive(!InUse && Reward.Data[RewardType] != 0);
    }

    public void OnClick()
    {
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
