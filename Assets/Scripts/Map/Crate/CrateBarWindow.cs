using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateBarWindow : MonoBehaviour
{
    [SerializeField] GameObject m_CrateProgressBarPrefab;
    [SerializeField] GameObject m_CrateUnlockWindowPrefab;

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
        var levelButton = MapLevelManager.Instance.GetLevelButton(MapDataManager.Instance.NewRatingLevel);
        m_WindowAnimator.OnWindowFadeIn.AddListener(() => MapManager.Instance.MoveMap(levelButton.transform.position));
        StartCoroutine(IPlay());
    }

    IEnumerator IPlay()
    {

        // Get old quality
        int oldRatingCount = m_Crate.CurRatingCount - MapDataManager.Instance.NewRating;
        oldRatingCount = Mathf.Clamp(oldRatingCount, 0, 99);
        int oldQuality = (int)m_Crate.GetQuality(oldRatingCount);

        m_Crate.SetView((Crate.Quality)oldQuality, oldRatingCount);

        var levelButton = MapLevelManager.Instance.GetLevelButton(MapDataManager.Instance.NewRatingLevel);
        var bar = Instantiate(m_CrateProgressBarPrefab, levelButton.transform).GetComponent<CrateProgressBar>();

        //quality = Mathf.Clamp(quality, 0, 3);

        int nextQuality = Mathf.Clamp(oldQuality + 1, 0, 4);
        bar.Set(oldRatingCount, m_Crate.QualityRatings[nextQuality], (Crate.Quality)nextQuality);

        yield return new WaitForSeconds(0.5f);

        int curQuality = 0;
        for (int i = oldRatingCount; i <= m_Crate.CurRatingCount; i++)
        {
            curQuality = (int)m_Crate.GetQuality(i);

            //quality = i >= m_Crate.QualityRatings[(int)oldQuality] ? oldQuality : oldQuality + 1;
            //quality = Mathf.Clamp(quality, 0, 3);

            nextQuality = Mathf.Clamp(curQuality + 1, 0, 4);
            bar.Set(i, m_Crate.QualityRatings[nextQuality], (Crate.Quality)nextQuality);
            m_Crate.SetView((Crate.Quality)curQuality, i);

            yield return new WaitForSeconds(0.25f);
        }

        m_Crate.SetView((Crate.Quality)curQuality, m_Crate.CurRatingCount);
        MapDataManager.Instance.NewRating = 0;
        yield return new WaitForSeconds(2);

        bar.Close();
        m_WindowAnimator.Close();
        if (oldQuality != curQuality)
        {
            var window = Window.CreateWindowPrefab(m_CrateUnlockWindowPrefab).GetComponent<CrateUnlockWindow>();
            window.Initialise((Crate.Quality)curQuality, m_Crate.CurRatingCount);
        }
    }
}
