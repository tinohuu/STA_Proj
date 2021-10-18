using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CropEndpoint : MonoBehaviour
{
    [SerializeField] GameObject m_CropGrowthPrefab;
    [SerializeField] Transform m_RewardIcon;
    [SerializeField] Transform m_RewardBig;

    static CropGrowthWindow m_GrowthWindow;
    int m_LevelID;
    string m_CropName;

    public void Initialise(int levelID, string cropName)
    {
        m_LevelID = levelID;
        m_CropName = cropName;
    }

    void Start()
    {
        m_RewardBig.localScale = Vector3.zero;
        m_RewardIcon.GetComponent<SpriteRenderer>().color = MapManager.Instance.Data.CompleteLevel + 1 < m_LevelID ? new Color(1, 1, 1, 0.5f) : Color.white;

        if (CropManager.Instance.EnableEndpoint && MapManager.Instance.Data.CompleteLevel >= m_LevelID && !m_GrowthWindow)
        {
            SoundManager.Instance.PlaySFX("cropNewCropUnlocked");
            m_GrowthWindow = Window.CreateWindowPrefab(m_CropGrowthPrefab).GetComponent<CropGrowthWindow>();
            m_GrowthWindow.Initialize(m_CropName, this);
        }
    }

    public void AnimateReward()
    {
        StartCoroutine(IAnimateReward());

    }
    IEnumerator IAnimateReward()
    {
        var levelButton = GetComponentInParent<MapLevel>();
        if (!levelButton) yield break;
        levelButton.UnsetAsIcon(transform, CropManager.Instance.transform);
        m_RewardIcon.DOScale(Vector3.zero, 0.25f);
        yield return new WaitForSeconds(0.25f);
        SoundManager.Instance.PlaySFX("itemClockGet");
        transform.position = levelButton.transform.position;
        m_RewardBig.DOScale(Vector3.one, 0.5f);
        m_RewardBig.DOJump(CropHarvest.Instance.transform.position, 3, 1, 0.75f).SetDelay(1.5f).OnComplete(() => Finish());
    }

    void Finish()
    {
        Reward.Data[RewardType.Clock]++;
        CropManager.Instance.Data.CollectedClockLevelID = m_LevelID;
        Destroy(gameObject);
        TutorialManager.Instance.Show("Harvest", 1, CropHarvest.Instance.gameObject);
    }
}