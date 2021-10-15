using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MapLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Sprite[] LevelSprites = new Sprite[2];
    [SerializeField] SpriteRenderer ButtonSpriteRenderer;
    [SerializeField] SpriteRenderer[] StarSpriteRenderers = new SpriteRenderer[3];
    [SerializeField] GameObject Frame;
    [SerializeField] TMP_Text LevelText;
    [SerializeField] Transform m_TopIconGroup;
    [SerializeField] Transform m_BottomIconGroup;
    public MapLevelData Data;
    private void Start()
    {
        UpdateView();
    }

    #region Icon
    public  void SetAsIcon(Transform icon, bool top = true)
    {
        icon.SetParent(top ? m_TopIconGroup : m_BottomIconGroup);
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localScale = Vector3.one;
        AnimateIcons();
    }

    public  void UnsetAsIcon(Transform icon, Transform parent)
    {
        icon.SetParent(parent);
        //icon.transform.localPosition = Vector3.zero;
        //icon.transform.localScale = Vector3.one;
        AnimateIcons();
    }

    void AnimateIcons()
    {
        bool hasMultipleTopIcons = m_TopIconGroup.childCount > 1;
        bool hasMultipleBottomIcons = m_BottomIconGroup.childCount > 1;

        StopAllCoroutines();

        for (int i = 0; i < m_TopIconGroup.childCount; i++)
        {
            Transform child = m_TopIconGroup.GetChild(i);
            if (!child) continue;
            child.localScale = hasMultipleTopIcons ? Vector3.zero : Vector3.one;
        }
        if (hasMultipleTopIcons) StartCoroutine(IAnimateIcons(m_TopIconGroup));

        for (int i = 0; i < m_BottomIconGroup.childCount; i++)
        {
            Transform child = m_BottomIconGroup.GetChild(i);
            if (!child) continue;
            child.localScale = hasMultipleBottomIcons ? Vector3.zero : Vector3.one;
        }
        if (hasMultipleBottomIcons) StartCoroutine(IAnimateIcons(m_BottomIconGroup));
    }

    IEnumerator IAnimateIcons(Transform group)
    {
        for (int i = 0; i < group.childCount; i = (i + 1) % group.childCount)
        {
            Transform child = group.GetChild(i);
            if (!child) continue;
            child.DOScale(Vector3.one, 0.25f);
            yield return new WaitForSeconds(2);
            child.DOScale(Vector3.zero, 0.25f);
        }
    }
    #endregion

    public void UpdateView()
    {
        if (Data == null)
        {
            Destroy(gameObject);
            return;
        }

        ButtonSpriteRenderer.sprite = CropManager.Instance.LevelToCropConfig(Data.ID).ID % 2 == 1 ? LevelSprites[0] : LevelSprites[1];
        ButtonSpriteRenderer.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        LevelText.text = Data.ID.ToString();
        LevelText.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        //Data.Rating = IsOpen && Data.ID != MapManager.Instance.Data.CompleteLevel + 1 ? Random.Range(3, 4) : 0; // Test: temp rating

        foreach (SpriteRenderer star in StarSpriteRenderers) star.color = IsOpen ? new Color(1, 1, 1, 0.5f) : Color.clear;

        if (MapDataManager.Instance.NewRatingLevel == Data.ID && MapDataManager.Instance.NewRating > 0)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            int oldRating = Data.Rating - MapDataManager.Instance.NewRating;
            for (int i = 0; i < Mathf.Clamp(Data.Rating, 0, 3); i++)
            {
                if (i + 1 > oldRating)
                {
                    string sfxName = "uiLevelButtonStar" + (i + 1).ToString();
                    sequence.AppendCallback(() => SoundManager.Instance.PlaySFX(sfxName));
                    sequence.Append(StarSpriteRenderers[i].transform.DOScale(Vector3.one * 2, 0.4f).From(true));
                    StarSpriteRenderers[i].color = Color.white;
                    sequence.Join(StarSpriteRenderers[i].DOFade(0, 0.4f).From());
                }
                else
                StarSpriteRenderers[i].color = Color.white;
            }
            if (Data.Rating >= 3)
                sequence.AppendCallback(() => { Frame.SetActive(true); Frame.transform.DOScale(Vector3.one * 0.5f, 0.1f).From(); });
            sequence.PlayForward();
            StartCoroutine(ResetNewRating());
        }
        else
        {
            for (int i = 0; i < Mathf.Clamp(Data.Rating, 0, 3); i++) StarSpriteRenderers[i].color = Color.white;
            Frame.SetActive(Data.Rating >= 3);
        }




    }

    IEnumerator ResetNewRating()
    {
        yield return new WaitForSeconds(1);
        MapDataManager.Instance.NewRating = 0;
    }

    internal object Find()
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsOpen) return;
        MapPlayer.Instance.MoveToLevel(this);
    }

    public bool IsOpen => MapManager.Instance.Data.CompleteLevel >= Data.ID - 1; 
}
