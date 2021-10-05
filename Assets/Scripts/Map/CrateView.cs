using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrateView : MonoBehaviour
{
    [SerializeField] GameObject m_CrateCropPrefab;

    [SerializeField] Transform m_FruitGroup;
    [SerializeField] TMP_Text m_NumberText;
    [SerializeField] Transform m_Target;
    [SerializeField] Transform m_Crate;
    [SerializeField] GameObject m_BackgroundFront;
    [SerializeField] Transform[] m_CropGroups = new Transform[3];
    [SerializeField] ButtonAnimator m_CollectButton;
    [SerializeField] GameObject m_CrateRocketWindow;

    int m_PickTimes = 10;
    List<CrateCrop> m_CrateCrops = new List<CrateCrop>();
    public int LevelID = 1;

    void Start()
    {
        if (CrateManager.Instance.Data.ResumedRewardTypes.Count == 0)
        {
            List<RewardType> rewardTypes = new List<RewardType>();
            List<int> counts = new List<int>();
            for (int i = 0; i < 15; i++)
            {
                var reward = PickReward();
                rewardTypes.Add(reward.Item1);
                counts.Add(reward.Item2);
            }
            CrateManager.Instance.Data.ResumedRewardTypes = rewardTypes;
            CrateManager.Instance.Data.ResumedCounts = counts;
        }
        m_CollectButton.gameObject.SetActive(false);
        m_CollectButton.OnClick.AddListener(() => Collect());

        for (int i = 0; i < m_CropGroups.Length; i++)
        {
            for (int j = 0; j < i + 4; j++)
            {
                CrateCrop crop = Instantiate(m_CrateCropPrefab, m_CropGroups[i]).GetComponent<CrateCrop>();
                var config = CropManager.Instance.LevelToCropConfig(LevelID);



                m_CrateCrops.Add(crop);

                int index = m_CrateCrops.IndexOf(crop);
                var reward = CrateManager.Instance.Data.ResumedRewardTypes[index];
                var count = CrateManager.Instance.Data.ResumedCounts[index];
                crop.Initialize(config.Name, reward, count);
                crop.SetTarget(m_Target);

                if (CrateManager.Instance.Data.ResumedIndexes.Contains(index))
                {
                    crop.Open();
                }
                else
                {

                }

                CanvasGroup canvasGroup = crop.GetComponent<CanvasGroup>();
                RectTransform rt = crop.GetComponent<RectTransform>();

                canvasGroup.alpha = 0;

                rt.DOAnchorPosY(800, 0.5f).From(true).SetEase(Ease.InSine)
                    .OnStart(() => canvasGroup.alpha = 1)
                    .OnComplete(() => rt.DOShakeAnchorPos(0.25f, 10, 4))
                    .SetDelay(i * 0.5f + j * 0.1f);
            }
        }


    }

    (RewardType, int) PickReward()
    {
        var rewardConfigs = ConfigsAsset.GetConfigList<CrateRewardConfig>();
        var rewardConfig = rewardConfigs[Random.Range(0, rewardConfigs.Count)];
        if ((RewardType)(rewardConfig.RewardType) == RewardType.Coin)
        {
            var coinConfigs = ConfigsAsset.GetConfigList<CrateCoinConfig>();
            var coinConfig = coinConfigs[Random.Range(0, coinConfigs.Count)];
            int step = MapManager.Instance.Data.CompleteLevel / coinConfig.LevelStep;
            int countMin = coinConfig.Base * (coinConfig.MultiplierMin + step * coinConfig.StepMultiplierMin);
            int countMax = coinConfig.Base * (coinConfig.MultiplierMax + step * coinConfig.StepMultiplierMax);
            int count = Random.Range(countMin, countMax) / 500 * 500;
            return (RewardType.Coin, count);
        }
        else
        {
            return ((RewardType)rewardConfig.RewardType, Random.Range(rewardConfig.RewardCountMin, rewardConfig.RewardCountMax));
        }
        return default;
    }

    public void Open(CrateCrop crop)
    {
        if (m_PickTimes > 0 && crop)
        {
            int index = m_CrateCrops.IndexOf(crop);
            if (!CrateManager.Instance.Data.ResumedIndexes.Contains(index))
            {
                CrateManager.Instance.Data.ResumedIndexes.Add(index);
            }
            m_PickTimes--;
            m_NumberText.text = m_PickTimes.ToString();
        }

        if (m_PickTimes == 0)
        {
            m_CrateCrops.ForEach(x => x.GetComponentInChildren<ButtonAnimator>().Interactable = false);

            m_CollectButton.gameObject.SetActive(true);
            m_CollectButton.transform.DOScale(Vector3.zero, 0.5f).From().SetEase(Ease.InSine)
                .OnComplete(() => m_CollectButton.transform.DOShakeScale(0.25f, Vector3.one * 0.25f, 6, 0));
        }
    }

    public void ShakeCrate()
    {
        m_Crate.DOShakeScale(0.2f, 0.2f, 2);
    }

    public void Collect()
    {
        StartCoroutine(ICollect());
    }

    IEnumerator ICollect()
    {
        m_BackgroundFront.SetActive(false);
        m_CollectButton.Interactable = false;
        m_CollectButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);

        foreach (var crop in m_CrateCrops)
        {
            crop.SetTarget(m_CollectButton.transform);
            crop.Collect();
        }
        yield return new WaitForSeconds(2);

        CrateManager.Instance.Collect(LevelID);

        GetComponent<WindowAnimator>().Close();

        Window.CreateWindowPrefab(m_CrateRocketWindow);
        //m_CrateCrops.ForEach(x => x.transform.DOJump(m_CollectButton.transform.position, 1, 1, 1));
    }
}
