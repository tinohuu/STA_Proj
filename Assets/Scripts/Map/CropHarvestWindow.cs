using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropHarvestWindow : MonoBehaviour
{
    [SerializeField] GameObject CellPrefab;
    [SerializeField] RectTransform TopLayoutGroup;
    [SerializeField] RectTransform BottomLayoutGroup;
    [SerializeField] TMP_Text CropsText;
    [SerializeField] TMP_Text CoinText;
    [SerializeField] TMP_Text PlusText;
    [SerializeField] List<GameObject> cells = new List<GameObject>();
    static Vector2 oriGravity = new Vector2();
    [SerializeField] bool enableAnimationTest = true;
    [SerializeField] bool hasCropEffect = false;
    [SerializeField] bool isJointEffect = true;
    float lastGyroTime = 0;
    private void OnEnable()
    {
        if (oriGravity == new Vector2()) oriGravity = Physics2D.gravity;
        Input.gyro.enabled = true;
    }
    private void OnDisable()
    {
        CoinText.rectTransform.DOKill();
        TopLayoutGroup.GetComponent<CanvasGroup>().DOKill();
        BottomLayoutGroup.GetComponent<CanvasGroup>().DOKill();
        Physics2D.gravity = oriGravity;
        Input.gyro.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ShowCrop();
        StartCoroutine(AnimateCrops());
        CoinText.rectTransform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
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
        int firstLevelOfMap = MapManager.MapMakerConfig.LevelToStarting(MapManager.Instance.Data.CompleteLevel);
        CropConfig cropConfig = CropManager.Instance.LevelToCropConfig(firstLevelOfMap);
        int configIndex = CropManager.Instance.CropConfigs.IndexOf(cropConfig);
        cells.Clear();
        for (int i = configIndex; i < CropManager.Instance.CropConfigs.Count; i++)
        {
            cropConfig = CropManager.Instance.CropConfigs[i];
            if (cropConfig.Level <= MapManager.Instance.Data.CompleteLevel)
            {
                GameObject cell = Instantiate(CellPrefab, i % 2 == 0 ? TopLayoutGroup : BottomLayoutGroup);
                cell.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Sprites/Crops/Crop_Fruit_" + cropConfig.Name);
                //cell.GetComponentInChildren<Image>().SetNativeSize();
                //cell.GetComponentInChildren<Image>().rectTransform.sizeDelta /= 2;
                cells.Add(cell);
            }
            else break;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(TopLayoutGroup);
        LayoutRebuilder.ForceRebuildLayoutImmediate(BottomLayoutGroup);
    }

    IEnumerator AnimateCrops()
    {
        CanvasGroup topGroup = TopLayoutGroup.GetComponent<CanvasGroup>();
        CanvasGroup btmGroup = BottomLayoutGroup.GetComponent<CanvasGroup>();
        for (int i = 0; i < 3; i++)
        {
            ShowCrop();
            topGroup.DOFade(1, 0.5f);
            btmGroup.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1.5f);

            do yield return null;
            while (Time.time - lastGyroTime < 2);

            topGroup.DOFade(0, 0.5f);
            btmGroup.DOFade(0, 0.5f);
            PlusText.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1);
            hasCropEffect = false;
            cells.Clear();
            TopLayoutGroup.DestroyChildren();
            BottomLayoutGroup.DestroyChildren();

            PlusText.DOFade(1, 0.5f);
            yield return new WaitForSeconds(0.5f);
            PlusText.DOFade(0, 0.5f);
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
}
