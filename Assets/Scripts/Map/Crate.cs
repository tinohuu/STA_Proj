using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Linq;

public class Crate : MonoBehaviour
{
    public enum Quality { None, Wood, Silver, Gold, Diamond }

    [Header("Setting")]
    [SerializeField] float m_MaxBarWidth = 4;

    [Header("Ref")]
    public GameObject Box;
    [SerializeField] GameObject m_Rating;
    [SerializeField] GameObject m_Tick;

    [SerializeField] Sprite[] BoxSprites = new Sprite[5];
    [SerializeField] SpriteRenderer BoxRenderer;
    [SerializeField] GameObject m_CrateBarWindow;
    [SerializeField] GameObject m_CrateProgressWindow;

    [Header("Debug")]
    public int LevelID = 1;
    public int CurRatingCount { get; private set; }
    public int TotalRatingCount { get; private set; }
    public int[] QualityRatings { get; private set; }
    public Quality CrateQuality { get; private set; }

    TMP_Text m_Text;
    ButtonAnimator m_ButtonAnimator;

    bool m_CanInteract => !CrateManager.Instance.Data.CollectedCrateLevels.Contains(LevelID) && CropManager.Instance.LevelToCropConfig(LevelID).MinLevel <= MapManager.Instance.Data.CompleteLevel;

    private void Start()
    {
        m_Text = GetComponentInChildren<TMP_Text>(true);
        m_ButtonAnimator = GetComponent<ButtonAnimator>();

        UpdateData();
        UpdateView();

    }

    void UpdateData()
    {
        if (LevelID == 0 ||MapManager.Instance.HasMapmaker) return;
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);

        // Get current rating count
        int _curRatingCount = 0;
        for (int i = cropConfig.MinLevel - 1; i < cropConfig.Level; i++)
        {
            var data = MapDataManager.Instance.Data.MapLevelDatas[i];
            _curRatingCount += data.Rating;
            //Debug.Log(data.ID + ":" + data.Rating);
        }
        CurRatingCount = _curRatingCount;

        // Get total rating count
        int levelCount = cropConfig.Level - cropConfig.MinLevel + 1;
        TotalRatingCount = levelCount * 3;

        // Get quality points
        var publicConfig = ConfigsAsset.GetConfigList<CratePublicConfig>()[0];
        float[] qualitySteps = new float[] { 0, publicConfig.StepWood, publicConfig.StepSliver, publicConfig.StepGold, publicConfig.StepDiamond };
        for (int i = 0; i < qualitySteps.Length; i++) qualitySteps[i] /= publicConfig.StepsTotal;

        int[] _qualityRatings = new int[5];
        int lastQualityPoint = 0;

        _qualityRatings[0] = 0;
        for (int i = 1; i < 5; i++)
        {
            //int lastQualityPoint = Mathf.RoundToInt(totalRating * i / 4f);
            int qualityPoint = Mathf.RoundToInt(TotalRatingCount * qualitySteps[i]);
            if (qualityPoint == lastQualityPoint) qualityPoint++;
            lastQualityPoint = qualityPoint;
            _qualityRatings[i] = qualityPoint;
            //if (CurRatingCount > lastQualityPoint && CurRatingCount <= qualityPoint) CrateQuality = (Quality)i;
        }

        QualityRatings = _qualityRatings;

        CrateQuality = GetQuality(CurRatingCount);
    }

    public void UpdateView()
    {
        if (LevelID < 1 || MapManager.Instance.HasMapmaker) return;
        m_Tick.gameObject.SetActive(CrateManager.Instance.Data.CollectedCrateLevels.Contains(LevelID));

        BoxRenderer.color = m_CanInteract ? Color.white : new Color(1, 1, 1, 0.5f);
        m_ButtonAnimator.Interactable = m_CanInteract;

        m_ButtonAnimator.OnClick.AddListener(() => OnClickCrate());

        m_Rating.gameObject.SetActive(m_CanInteract);

        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);
        if ((MapDataManager.Instance.NewRating > 0 || CrateManager.Instance.ForceShowLevelProgress) && MapDataManager.Instance.NewRatingLevel <= cropConfig.Level && MapDataManager.Instance.NewRatingLevel >= cropConfig.MinLevel)
        {
            var barWindow = Window.CreateWindowPrefab(m_CrateBarWindow).GetComponent<CrateBarWindow>();
            barWindow.Initialise(this);
        }
        else
        {
            SetView(CrateQuality, CurRatingCount);
        }

    }

    public void SetView(Quality quality, int rating)
    {
        BoxRenderer.sprite = BoxSprites[(int)quality];
        m_Text.text = rating.ToString();
    }

    void OnClickCrate()
    {
        var cropConfig = CropManager.Instance.LevelToCropConfig(LevelID);

        if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
        {
            CrateManager.Instance.ShowCrateView(this);
        }
        else
        {
            var window = Window.CreateWindowPrefab(m_CrateProgressWindow).GetComponent<CrateProgressWindow>();
            window.Initialize(this);
        }
    }

    public Quality GetQuality(int rating)
    {
        int quality = 0;
        for (int i = 0; i < 5; i++)
        {
            if (rating >= QualityRatings[i])
                quality = i;
            else
                break;
        }
        return (Quality)quality;
    }
}
