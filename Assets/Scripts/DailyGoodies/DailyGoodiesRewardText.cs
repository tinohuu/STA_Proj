using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoodiesRewardText : MonoBehaviour
{
    public RewardType RewardType = RewardType.None;
    public int Count = 0;

    Image image;
    TMP_Text text;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void UpdateView(float delay)
    {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TMP_Text>();

        var icons = Resources.LoadAll<Sprite>("Sprites/IconAtlas");
        Sprite sprite = Array.Find(icons, e => e.name == RewardType.ToString());
        image.sprite = sprite;
        image.color = new Color(1, 1, 1, 0);
        text.color = new Color(1, 1, 1, 0);
        image.transform.localScale = Vector3.zero;
        text.transform.localScale = Vector3.zero;

        text.text = Count.ToString("N0");

        image.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .OnComplete(() => image.transform.DOScale(Vector3.one, 0.25f))
            .SetDelay(delay);
        DOTween.To(() => image.color, x => image.color = x, Color.white, 1)
            .SetDelay(delay);

        text.transform.DOScale(Vector3.one * 1.1f, 0.25f)
            .SetDelay(0.25f + delay)
            .OnComplete(() => text.transform.DOScale(Vector3.one, 0.25f));
        DOTween.To(() => text.color, x => text.color = x, Color.white, 1)
            .SetDelay(0.25f + delay);
    }

    public void OnCollect(Vector3 target)
    {
        DOTween.To(() => text.color, x => text.color = x, Color.clear, 0.5f);
        if (RewardType == RewardType.Coin)
        {
            ParticleManager.Instance.CreateCoin(image.transform.position);
            DOTween.To(() => image.color, x => image.color = x, Color.clear, 0.5f);
        }
        else
        {
            image.transform.DOMove(target, 0.5f).OnComplete(() => DestroyImmediate(gameObject));
        }

    }
}
