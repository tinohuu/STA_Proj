using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardNumber : MonoBehaviour
{
    public int current = 0;
    private void Awake()
    {
        Reward.Coin += 10;
        DOTween.To(() => current, x => current = x, Reward.Coin, 2);
    }
}
