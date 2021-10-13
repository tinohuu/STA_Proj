using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrateProgressWindow : MonoBehaviour
{
    [SerializeField] TMP_Text m_RatingText;
    [SerializeField] RectTransform m_Player;
    [SerializeField] Transform m_QualityRatingGroup;
    [SerializeField] Transform m_PlayerPointGroup;
    [SerializeField] Transform m_PickNumberGroup;
    Crate m_Crate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Crate crate)
    {
        m_RatingText.text = crate.CurRatingCount.ToString();
        m_Player.transform.position = m_PlayerPointGroup.GetChild((int)crate.CrateQuality).position;
        for (int i = 0; i < m_QualityRatingGroup.childCount; i++)
        {
            m_QualityRatingGroup.GetChild(i).GetComponent<TMP_Text>().text = crate.QualityRatings[i].ToString();
        }

        var publicConfig = ConfigsAsset.GetConfigList<CratePublicConfig>()[0];
        m_PickNumberGroup.GetChild(0).GetComponent<TMP_Text>().text = publicConfig.PicksWood.ToString();
        m_PickNumberGroup.GetChild(1).GetComponent<TMP_Text>().text = publicConfig.PicksSliver.ToString();
        m_PickNumberGroup.GetChild(2).GetComponent<TMP_Text>().text = publicConfig.PicksGold.ToString();
        m_PickNumberGroup.GetChild(3).GetComponent<TMP_Text>().text = publicConfig.PicksDiamond.ToString();
    }
}
