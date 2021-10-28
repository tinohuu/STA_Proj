using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropHarvestWindow : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] bool enableAnimationTest = true;
    [SerializeField] bool hasCropEffect = false;
    [SerializeField] bool isJointEffect = true;

    [Header("Crop Ref")]
    [SerializeField] GameObject m_CellPrefab;
    [SerializeField] RectTransform m_TopLayoutGroup;
    [SerializeField] RectTransform m_BottomLayoutGroup;

    [Header("Ref")]
    [SerializeField] TMP_Text m_CropsText;
    [SerializeField] TMP_Text m_CoinText;
    [SerializeField] TMP_Text m_PlusText;
    [SerializeField] TMP_Text m_AdInfoText;
    [SerializeField] ButtonAnimator m_CollectButton;

    [Header("Debug")]
    [SerializeField] List<GameObject> cells = new List<GameObject>();

    float lastGyroTime = 0;
    static Vector2 oriGravity = new Vector2();
    private void OnEnable()
    {
        if (oriGravity == new Vector2()) oriGravity = Physics2D.gravity;
        Input.gyro.enabled = true;
    }
    private void OnDisable()
    {
        m_CoinText.rectTransform.DOKill();
        m_TopLayoutGroup.GetComponent<CanvasGroup>().DOKill();
        m_BottomLayoutGroup.GetComponent<CanvasGroup>().DOKill();
        Physics2D.gravity = oriGravity;
        Input.gyro.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ShowCrop();
        StartCoroutine(AnimateCrops());
        m_CoinText.Format(CropHarvest.Instance.GetHarvestCoin().ToString("N0"));
        m_AdInfoText.Format((CropHarvest.Instance.GetHarvestCoin() * 2).ToString("N0"));
        //CoinText.rectTransform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);

        TutorialManager.Instance.Show("Harvest", 2, m_CollectButton.gameObject, 0.5f);
    }

    private void FixedUpdate()
    {
        if (cells.Count == 0) return;
        if (!hasCropEffect && Input.gyro.userAcceleration.magnitude > 2)
        {
            hasCropEffect = true;
            //isJointEffect = Random.Range(0, 2) == 0;
            foreach (GameObject cell in cells)
            {
                cell.GetComponentInChildren<Rigidbody2D>().isKinematic = false;
                cell.GetComponentInChildren<CircleCollider2D>().enabled = true;
                if (isJointEffect)
                {
                    cell.GetComponentInChildren<CircleCollider2D>().radius = 10;
                    cell.GetComponentInChildren<TargetJoint2D>().target = transform.position;
                    cell.GetComponentInChildren<TargetJoint2D>().enabled = true;
                }
                else
                {
                    cell.GetComponentInChildren<Rigidbody2D>().mass = 1000;
                }
            }

        }
        if (hasCropEffect && Input.gyro.userAcceleration.magnitude > 0.1f)
        {
            lastGyroTime = Time.time;
            if (isJointEffect)
            {
                foreach (GameObject cell in cells)
                {
                    Vector3 force = Quaternion.AngleAxis(Random.Range(-10, 10), Vector3.forward) * Input.gyro.userAcceleration * Random.Range(10000, 20000);
                    cell.GetComponentInChildren<Rigidbody2D>().AddForce(force);
                }
            }
            else
            {
                Physics2D.gravity = Input.gyro.gravity * 1000;
                foreach (GameObject cell in cells)
                {
                    Vector3 force = Input.gyro.userAcceleration * 8000000;
                    cell.GetComponentInChildren<Rigidbody2D>().AddForce(force);
                }
            }

        }
    }

    void ShowCrop()
    {
        int firstLevelOfMap = 0;// MapManager.MapMakerConfig.LevelToStarting(MapManager.Instance.Data.CompleteLevel);
        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        cells.Clear();

        List<string> cropNames = new List<string>();
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
            {
                cropNames.Add(cropConfig.Name);
            }
            else break;
        }

        string cropNamesText = "";
        for (int i = 0; i < cropNames.Count; i++)
        {
            GameObject cell = Instantiate(m_CellPrefab, i % 2 == 0 && cropNames.Count > 3 ? m_TopLayoutGroup : m_BottomLayoutGroup);

            if (cropNames.Count <= 3) cell.transform.Rotate(- Vector3.forward * 45);

            cell.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Sprites/Crops/Crop_Fruit_" + cropNames[i]);
            //cell.GetComponentInChildren<Image>().SetNativeSize();
            //cell.GetComponentInChildren<Image>().rectTransform.sizeDelta /= 2;
            cells.Add(cell);

            cropNamesText += i == 0 ? "" : " & ";
            cropNamesText += cropNames[i];
        }


        m_CropsText.text = cropNames.Count > 3 ? "" : cropNamesText;

        m_TopLayoutGroup.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = cropNames.Count <= 3;
        m_BottomLayoutGroup.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = cropNames.Count <= 3;

        LayoutRebuilder.ForceRebuildLayoutImmediate(m_TopLayoutGroup);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_BottomLayoutGroup);
    }

    IEnumerator AnimateCrops()
    {
        CanvasGroup topGroup = m_TopLayoutGroup.GetComponent<CanvasGroup>();
        CanvasGroup btmGroup = m_BottomLayoutGroup.GetComponent<CanvasGroup>();

        if (MapManager.Instance.MapID == 1)
        {
            ShowCrop();
            topGroup.alpha = 1;
            btmGroup.alpha = 1;
            yield break;
        }

        for (int i = 0; i < 3; i++)
        {
            ShowCrop();
            topGroup.DOFade(1, 0.5f);
            btmGroup.DOFade(1, 0.5f);
            m_CropsText.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1.5f);

            do yield return null;
            while (Time.time - lastGyroTime < 2);

            topGroup.DOFade(0, 0.5f);
            btmGroup.DOFade(0, 0.5f);
            m_CropsText.DOFade(0, 0.5f);
            m_PlusText.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1);
            hasCropEffect = false;
            cells.Clear();
            m_TopLayoutGroup.DestroyChildren();
            m_BottomLayoutGroup.DestroyChildren();

            m_PlusText.DOFade(1, 0.5f);
            yield return new WaitForSeconds(0.5f);
            m_PlusText.DOFade(0, 0.5f);
            //yield return new WaitForSeconds(0.5f);
            i = 0;
        }
    }

    public void Harvest()
    {
        FindObjectOfType<CropHarvest>().Harvest();
        SoundManager.Instance.PlaySFX("coin", true);
        SoundManager.Instance.PlaySFX("harvestStart");
    }

    void OnDestroy()
    {
        MapLevelManager.Instance.ShowTutorial();
    }
}
