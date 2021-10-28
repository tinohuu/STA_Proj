using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardInfoWindow : MonoBehaviour
{
    List<RewardType> m_RewardTypes
        = new List<RewardType>() { RewardType.RemoveCards, RewardType.ClearPlayables, RewardType.WildDrop };

    private void Start()
    {
        var infos = GetComponentsInChildren<RewardInfo>();
        for (int i = 0; i < 3; i++)
        {
            infos[i].Initialize(m_RewardTypes[i]);
        }
    }

    public void Initialize(List<RewardType> rewardTypes)
    {
        m_RewardTypes = rewardTypes;
    }
}
