using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoodyText : MonoBehaviour
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
            .SetDelay(1 + delay)
            .OnComplete(() => text.transform.DOScale(Vector3.one, 0.25f));
        DOTween.To(() => text.color, x => text.color = x, Color.white, 1)
            .SetDelay(1 + delay);
    }

    public void PlayVFX()
    {

    }
}
