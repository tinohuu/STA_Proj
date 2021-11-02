using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWindow : MonoBehaviour
{
    [SerializeField] GameObject m_QuestViewPrefab;
    [SerializeField] RectTransform m_QuestViewGroup;

    // Start is called before the first frame update
    void Start()
    {
        UpdateView();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateView()
    {
        m_QuestViewGroup.DestroyChildren();
        foreach (var data in QuestManager.Instance.Data.QuestDatas)
        {
            var view = Instantiate(m_QuestViewPrefab, m_QuestViewGroup).GetComponent<QuestView>();
            view.Data = data;
        }
    }
}
