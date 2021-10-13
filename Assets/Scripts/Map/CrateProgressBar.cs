using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class CrateProgressBar : MonoBehaviour
{
    [SerializeField] RectTransform ProgressBar;
    [SerializeField] RectTransform ProgressBarFill;
    [SerializeField] Image Box;
    [SerializeField] Sprite[] BoxSprites = new Sprite[4];

    SortingGroup m_SortingGroup;
    Canvas m_Canvas;
    int m_OriSortingOrder;
    Vector2 m_OriFillSize;
    Vector3 m_OriLevelButtonScale;
    TMP_Text m_Text;

    private void Awake()
    {
        m_SortingGroup = GetComponentInParent<SortingGroup>();
        m_Canvas = GetComponent<Canvas>();
        m_Text = GetComponentInChildren<TMP_Text>();

        m_OriLevelButtonScale = m_SortingGroup.transform.localScale;
        m_SortingGroup.transform.DOScale(m_OriLevelButtonScale * 1.25f, 0.25f);

        m_OriSortingOrder = m_SortingGroup.sortingOrder;

        m_SortingGroup.sortingOrder = 3;
        m_Canvas.overrideSorting = true;
        m_Canvas.sortingOrder = 2;

        ProgressBar.sizeDelta = new Vector2(0, ProgressBar.sizeDelta.y);
        ProgressBar.DOSizeDelta(new Vector2(4, ProgressBar.sizeDelta.y), 1);

        m_OriFillSize = ProgressBarFill.sizeDelta;
    }

    public void Set(int rating, int maxRating, Crate.Quality quality, bool animating = true)
    {
        m_Text.text = rating + " / " + maxRating;
        float targetWidth = rating / (float)maxRating * ProgressBar.sizeDelta.x;

        if (animating)
        {
            ProgressBarFill.DOSizeDelta(new Vector2(targetWidth, m_OriFillSize.y), 0.25f);
            Box.rectTransform.DOScale(Vector3.one * 1.2f, 0.25f).SetEase(Ease.Flash, 2);
            Box.sprite = BoxSprites[Mathf.Clamp((int)quality, 0, 3)];
        }
        else
        {
            ProgressBarFill.sizeDelta = new Vector2(targetWidth, m_OriFillSize.y);
            Box.sprite = BoxSprites[Mathf.Clamp((int)quality, 0, 3)];
        }
    }

    public void Close()
    {
        ProgressBar.DOSizeDelta(new Vector2(0, ProgressBar.sizeDelta.y), 0.5f)
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDisable()
    {
        m_SortingGroup.sortingOrder = m_OriSortingOrder;
        //m_SortingGroup.transform.localScale = m_OriLevelButtonScale;
        m_SortingGroup.transform.DOScale(m_OriLevelButtonScale, 0.25f);
    }
}
