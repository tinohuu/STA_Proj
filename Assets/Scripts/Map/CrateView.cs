using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrateView : MonoBehaviour
{
    [SerializeField] Transform m_FruitGroup;
    [SerializeField] TMP_Text m_NumberText;
    public Transform CrateTop;
    public Transform Crate;
    int m_PickTimes = 10;

    // Start is called before the first frame update
    void Start()
    {
        var crops = m_FruitGroup.GetComponentsInChildren<CrateCrop>();
        for (int i = 0; i < crops.Length; i++)
        {
            var crop = crops[i];

            CanvasGroup canvasGroup = crop.GetComponent<CanvasGroup>();
            RectTransform rt = crop.GetComponent<RectTransform>();

            canvasGroup.alpha = 0;

            rt.DOAnchorPosY(800, 0.5f).From(true).SetEase(Ease.InSine)
                .OnStart(() => canvasGroup.alpha = 1)
                .OnComplete(() => rt.DOShakeAnchorPos(0.25f, 10, 4))
                .SetDelay(i * 0.1f);
        }
    }

    public void Open()
    {
        if (m_PickTimes > 0)
        m_PickTimes--;
        m_NumberText.text = m_PickTimes.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
