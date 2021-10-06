using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Crate : MonoBehaviour
{
    public enum Quality { Wood, Silver, Gold, Diamond }

    [Header("Setting")]
    [SerializeField] float m_MaxBarWidth = 4;

    [Header("Ref")]
    public GameObject Box;
    public GameObject Rocket;

    [SerializeField] Sprite[] BoxSprites = new Sprite[4];
    [SerializeField] SpriteRenderer BoxRenderer;
    [SerializeField] GameObject Bar;

    [Header("Debug")]
    public int LevelID = 1;
    public Quality CrateQuality = Quality.Wood;

    TMP_Text m_Text;
    ButtonAnimator m_ButtonAnimator;
    private void Start()
    {
        m_Text = GetComponentInChildren<TMP_Text>(true);
        m_ButtonAnimator = GetComponent<ButtonAnimator>();

        ShowProgressCoroutine();

        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);
        m_ButtonAnimator.Interactable = cropConfig.Level <= MapManager.Instance.Data.CompleteLevel;
        m_ButtonAnimator.OnClick.AddListener(() => CrateManager.Instance.ShowCrateView(this));
    }


    void ShowProgress()
    {
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);

        if (MapManager.Instance.Data.CompleteLevel >= cropConfig.MinLevel)
        {
            int curRatingCount = 0;
            for (int i = cropConfig.MinLevel - 1; i < cropConfig.Level; i++)
            {
                var data = MapDataManager.Instance.Data.MapLevelDatas[i];
                curRatingCount += data.Rating;
                if (data.Rating == 0) break;
            }
            int oldRatingCount = curRatingCount - MapDataManager.Instance.NewRatings;

            // todo: test crate quality solution
            int levelCount = cropConfig.Level - cropConfig.MinLevel + 1;
            int totalRating = levelCount * 3;

            int oldQuality = 0;
            int curQuality = 0;
            int oldQualityPoint = 0;
            for (int i = 0; i < 4; i++)
            {
                //int lastQualityPoint = Mathf.RoundToInt(totalRating * i / 4f);
                int qualityPoint = Mathf.RoundToInt(totalRating * i / 4f);

                if (oldRatingCount >= qualityPoint) oldQuality = i;
                if (curRatingCount >= qualityPoint)
                {
                    oldQualityPoint = qualityPoint;
                    curQuality = i;
                }
            }

            var levelButton = MapLevelManager.Instance.GetLevelButton(MapManager.Instance.Data.CompleteLevel);
            var bar = Instantiate(CrateManager.Instance.m_CrateProgressBarPrefab, levelButton.transform).GetComponent<CrateProgressBar>();

            bar.Set(oldRatingCount, curRatingCount, (Quality)oldQuality, false);
            m_Text.text = oldRatingCount.ToString();
            BoxRenderer.sprite = BoxSprites[oldQuality];

            if (MapDataManager.Instance.NewRatings > 0 || true)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(1);

                for (int i = oldRatingCount; i <= curRatingCount; i++)
                {
                    Quality quality = oldQualityPoint - i >= 0 ? (Quality)curQuality : (Quality)oldQuality;
                    string text = i.ToString();
                    sequence.AppendCallback(() =>
                    {
                        bar.Set(i, curRatingCount, quality);
                        m_Text.text = text;
                        BoxRenderer.sprite = BoxSprites[(int)quality];
                    });
                    sequence.AppendInterval(0.25f);
                }
                sequence.AppendInterval(2);
                sequence.AppendCallback(() => bar.Close());
                sequence.Play();
            }
        }
    }

    [ContextMenu("Test")]
    void ShowProgressCoroutine()
    {
        StartCoroutine(IShowProgress());
    }

    IEnumerator IShowProgress()
    {
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);

        if (MapManager.Instance.Data.CompleteLevel >= cropConfig.MinLevel)
        {

            int curRatingCount = 0;
            for (int i = cropConfig.MinLevel - 1; i < cropConfig.Level; i++)
            {
                var data = MapDataManager.Instance.Data.MapLevelDatas[i];
                curRatingCount += data.Rating;
                Debug.Log(data.ID+":"+data.Rating);
                //if (data.Rating == 0) break;
            }
            int oldRatingCount = curRatingCount - MapDataManager.Instance.NewRatings;

            // todo: test crate quality solution
            int levelCount = cropConfig.Level - cropConfig.MinLevel + 1;
            int totalRating = levelCount * 3;

            int oldQuality = 0;
            int curQuality = 0;
            int oldQualityPoint = 0;
            for (int i = 0; i < 4; i++)
            {
                //int lastQualityPoint = Mathf.RoundToInt(totalRating * i / 4f);
                int qualityPoint = Mathf.RoundToInt(totalRating * i / 4f);

                if (oldRatingCount >= qualityPoint) oldQuality = i;
                if (curRatingCount >= qualityPoint)
                {
                    oldQualityPoint = qualityPoint;
                    curQuality = i;
                }
            }

            m_Text.text = oldRatingCount.ToString();
            BoxRenderer.color = Color.white;
            BoxRenderer.sprite = BoxSprites[oldQuality];
            Bar.gameObject.SetActive(true);
            CrateQuality = (Quality)curQuality;

            if ((MapDataManager.Instance.NewRatings > 0 || CrateManager.Instance.ForceShowLevelProgress) && MapManager.Instance.Data.CompleteLevel <= cropConfig.Level)
            {
                yield return null;
                MapManager.Instance.MoveMap(transform.position);
                //Sequence sequence = DOTween.Sequence();
                //sequence.AppendInterval(1);
                yield return new WaitForSeconds(2);

                var levelButton = MapLevelManager.Instance.GetLevelButton(MapManager.Instance.Data.CompleteLevel);
                var bar = Instantiate(CrateManager.Instance.m_CrateProgressBarPrefab, levelButton.transform).GetComponent<CrateProgressBar>();
                bar.Set(oldRatingCount, curRatingCount, (Quality)oldQuality, false);

                for (int i = oldRatingCount; i <= curRatingCount; i++)
                {
                    Quality quality = oldQualityPoint - i >= 0 ? (Quality)oldQuality : (Quality)curQuality;

                    bar.Set(i, oldQualityPoint, quality);
                    m_Text.text = i.ToString();
                    BoxRenderer.sprite = BoxSprites[(int)quality];

                    yield return new WaitForSeconds(0.25f);
                }
                yield return new WaitForSeconds(2);
                bar.Close();
            }
            else
            {
                m_Text.text = curRatingCount.ToString();
                BoxRenderer.sprite = BoxSprites[curQuality];
                Bar.gameObject.SetActive(true);
            }
        }
        else
        {
            BoxRenderer.color = new Color(1, 1, 1, 0.5f);
        }
    }
}
