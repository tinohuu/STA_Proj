using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropGrowthWindow : MonoBehaviour
{
    [SerializeField] Image CropImage;
    [SerializeField] TMP_Text CropText;
    [SerializeField] ButtonAnimator m_ButtonAnimator;
    CropEndpoint m_CropEndpoint;

    public void Initialize(string cropName, CropEndpoint cropEndpoint)
    {
        CropImage.sprite = Resources.Load<Sprite>("Sprites/Crops/Crop_Fruit_" + cropName);
        CropText.text = cropName;
        m_CropEndpoint = cropEndpoint;
    }

    void Start()
    {
        m_ButtonAnimator.OnClick.AddListener(() => StartCoroutine(IClick()));
    }

    IEnumerator IClick()
    {
        GetComponent<WindowAnimator>().FadeOut(false, false);
        MapManager.Instance.MoveMap(m_CropEndpoint.transform.position);
        yield return new WaitForSeconds(1.5f);
        m_CropEndpoint.AnimateReward();
        yield return new WaitForSeconds(2.5f);
        GetComponent<WindowAnimator>().Close();
    }
}
