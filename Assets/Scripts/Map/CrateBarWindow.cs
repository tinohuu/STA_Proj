using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateBarWindow : MonoBehaviour
{
    [SerializeField] GameObject m_CrateProgressBarPrefab;

    WindowAnimator m_WindowAnimator;
    Crate m_Crate;

    public void Initialise(Crate crate)
    {
        m_Crate = crate;
    }

    private void Awake()
    {
        m_WindowAnimator = GetComponent<WindowAnimator>();

        m_WindowAnimator.FadeInDelay = 1;
    }

    private void Start()
    {
        var levelButton = MapLevelManager.Instance.GetLevelButton(MapManager.Instance.Data.SelectedLevel);
        m_WindowAnimator.OnWindowFadeIn.AddListener(() => MapManager.Instance.MoveMap(levelButton.transform.position));
        StartCoroutine(IPlay());
    }

    IEnumerator IPlay()
    {
        // Get old quality
        int oldRatingCount = m_Crate.CurRatingCount - MapDataManager.Instance.NewRatings;
        Crate.Quality oldQuality = 0;
        for (int i = 0; i < 4; i++)
        {
            if (oldRatingCount >= m_Crate.QualityRatings[i])
            {
                oldQuality = (Crate.Quality)i;
            }
        }
        m_Crate.SetView(oldQuality, oldRatingCount);

        var levelButton = MapLevelManager.Instance.GetLevelButton(MapManager.Instance.Data.SelectedLevel);
        var bar = Instantiate(m_CrateProgressBarPrefab, levelButton.transform).GetComponent<CrateProgressBar>();
        bar.Set(oldRatingCount, m_Crate.QualityRatings[(int)oldQuality + 1], (Crate.Quality)((int)oldQuality + 1));

        yield return new WaitForSeconds(0.5f);
        for (int i = oldRatingCount; i <= m_Crate.CurRatingCount; i++)
        {
            Crate.Quality quality = m_Crate.QualityRatings[(int)m_Crate.CrateQuality] > i ? oldQuality : m_Crate.CrateQuality;

            bar.Set(i, m_Crate.QualityRatings[(int)quality + 1], (Crate.Quality)((int)quality + 1));
            m_Crate.SetView(quality, oldRatingCount);
            yield return new WaitForSeconds(0.25f);
        }
        m_Crate.SetView(m_Crate.CrateQuality, m_Crate.CurRatingCount);
        MapDataManager.Instance.NewRatings = 0;
        yield return new WaitForSeconds(2);
        bar.Close();
        m_WindowAnimator.Close();
    }
}
