using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestMapBarWindow : MonoBehaviour
{
    [SerializeField] GameObject m_BarPrefab;
    // Start is called before the first frame update
    Transform m_BarGroup;
    QuestMapButton m_MapButton;
    void Start()
    {
        var questDatasToShow = QuestManager.Instance.GetDatasToShow();
        if (questDatasToShow.Count > 0)
        {
            m_MapButton = FindObjectOfType<QuestMapButton>();
            m_BarGroup = m_MapButton.GetComponentInChildren<HorizontalLayoutGroup>().transform;
            for (int i = 0; i < questDatasToShow.Count; i++)
            {
                var bar = Instantiate(m_BarPrefab, m_BarGroup).GetComponent<QuestMapBar>();
                bar.Initialize(questDatasToShow[i], () => CheckToClose(), i * 0.25f);
            }
        }
    }

    void CheckToClose()
    {
        if (QuestManager.Instance.GetDatasToShow().Count == 0)
        {
            m_BarGroup.DestroyChildren();
            m_MapButton.CheckCollectable();
            GetComponent<WindowAnimator>().Close();

        }
    }
}
