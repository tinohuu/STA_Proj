using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoodiesBox : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public Transform Box;
    [SerializeField] Transform rewardGroup;
    [SerializeField] GameObject tick;
    [SerializeField] GameObject rewardImagePrefab;
    [SerializeField] Material betterMat;
    [SerializeField] Material bestMat;
    public int Day = 0;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
        if (Day == 28)
        {
            Box.GetComponentInChildren<MeshRenderer>().material = bestMat;
        }
        else if (Day % 7 == 0)
        {
            Box.GetComponentInChildren<MeshRenderer>().material = betterMat;
        }
    }

    public void UpdateView(int collectedDays)
    {


        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
        text.text = "Day " + Day;

        bool isCollected = Day <= collectedDays;
        tick.transform.DOScale(isCollected ? Vector3.one : Vector3.zero, 0.5f);
        Box.transform.DOScale(!isCollected ? Vector3.one : Vector3.zero, 0.5f);

        if (isCollected)
        {
            var config = DailyGoodiesManager.Instance.GetGoodyConfig(Day);
            var countsByType = Reward.StringToReward(config.ItemReward);
            if (config.CoinReward > 0)
            {
                Instantiate(rewardImagePrefab, rewardGroup);
            }

            int i = 0;
            foreach (var type in countsByType.Keys)
            {
                Image image = Instantiate(rewardImagePrefab, rewardGroup).GetComponent<Image>();
                var icons = Resources.LoadAll<Sprite>("Sprites/IconAtlas");
                Sprite sprite = Array.Find(icons, e => e.name == type.ToString());
                image.sprite = sprite;
                image.transform.SetAsFirstSibling();
                i++;
                image.rectTransform.anchoredPosition = Vector2.left * i * 30 + Vector2.up * i * 30;
            }

        }
        else
        {
            rewardGroup.DestroyChildren();
        }
    }
}
