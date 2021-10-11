using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrateProgressWindow : MonoBehaviour
{
    [SerializeField] TMP_Text m_RatingText;
    [SerializeField] RectTransform m_Player;
    [SerializeField] Transform PlayerPointGroup;
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
        m_RatingText.text = crate.RatingCount.ToString();
        m_Player.transform.position = PlayerPointGroup.GetChild((int)crate.CrateQuality).position;
    }
}
