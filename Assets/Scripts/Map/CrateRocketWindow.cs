using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrateRocketWindow : MonoBehaviour
{
    void Start()
    {
        MapManager.Instance.MoveMap(CrateManager.Instance.CurrentCrate.transform.position);

        CrateManager.Instance.CurrentCrate.Box.SetActive(false);

        CrateManager.Instance.CurrentCrate.Rocket.SetActive(true);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(CropManager.Instance.HarvestButton.position);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        WindowAnimator windowAnimator = GetComponent<WindowAnimator>();
        CrateManager.Instance.CurrentCrate.Rocket.transform.DOJump(worldPos, 5, 1, 1).SetDelay(1)
            .OnComplete(() => { Reward.Data[RewardType.Rocket]++; Reward.Data[RewardType.Clock]++; windowAnimator.Close(); });
    }
}
