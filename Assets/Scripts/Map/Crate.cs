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
    public GameObject Reward;

    [SerializeField] Sprite[] BoxSprites = new Sprite[4];
    [SerializeField] SpriteRenderer BoxRenderer;
    [SerializeField] GameObject Bar;
    [SerializeField] GameObject m_CrateProgressWindow;

    [Header("Debug")]
    public int LevelID = 1;
    public Quality CrateQuality = Quality.Wood;

    TMP_Text m_Text;
    ButtonAnimator m_ButtonAnimator;

    public int RatingCount { get; private set; }
    private void Start()
    {
        m_Text = GetComponentInChildren<TMP_Text>(true);
        m_ButtonAnimator = GetComponent<ButtonAnimator>();

        UpdateView();
    }


    public void UpdateView()
    {
        StartCoroutine(IUpdateView());
    }

    IEnumerator IUpdateView()
    {
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);
        m_ButtonAnimator.Interactable = LevelID > CrateManager.Instance.Data.CollectedCrateLevel;
        m_ButtonAnimator.OnClick.AddListener(() => OnClickCrate());

        if (LevelID > CrateManager.Instance.Data.CollectedCrateLevel && MapManager.Instance.Data.CompleteLevel >= cropConfig.MinLevel)
        {

            int curRatingCount = 0;
            for (int i = cropConfig.MinLevel - 1; i < cropConfig.Level; i++)
            {
                var data = MapDataManager.Instance.Data.MapLevelDatas[i];
                curRatingCount += data.Rating;
                Debug.Log(data.ID+":"+data.Rating);
                //if (data.Rating == 0) break;
            }
            RatingCount = curRatingCount;

            int oldRatingCount = curRatingCount - MapDataManager.Instance.NewRatings;

            // todo: test crate quality solution
            int levelCount = cropConfig.Level - cropConfig.MinLevel + 1;
            int totalRating = levelCount * 3;

            int oldQuality = 0;
            int curQuality = 0;
            int oldQualityPoint = 0;

            var publicConfig = ConfigsAsset.GetConfigList<CratePublicConfig>()[0];
            float[] qualitySteps = new float[] { publicConfig.StepWood, publicConfig.StepSliver, publicConfig.StepGold, publicConfig.PicksDiamond };
            System.Array.ForEach(qualitySteps, e => e /= publicConfig.StepsTotal);

            for (int i = 0; i < 4; i++)
            {
                //int lastQualityPoint = Mathf.RoundToInt(totalRating * i / 4f);
                int qualityPoint = Mathf.RoundToInt(totalRating * qualitySteps[i]);
                if (qualityPoint == oldQualityPoint) qualityPoint++;

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
            Bar.gameObject.SetActive(false);
        }
    }

    void OnClickCrate()
    {
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);
        if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
        {
            CrateManager.Instance.ShowCrateView(this);
        }
        else if (cropConfig.MinLevel <= MapManager.Instance.Data.CompleteLevel)
        {
            var window = Window.CreateWindowPrefab(m_CrateProgressWindow).GetComponent<CrateProgressWindow>();
            window.Initialize(this);
        }
    }
}
