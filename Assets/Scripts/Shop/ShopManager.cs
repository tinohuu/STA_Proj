using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] GameObject m_ShopViewPrefab;
    [SavedData] public ShopData Data = new ShopData();
    public static ShopManager Instance;

    ShopView m_ShopView;
    private void Awake()
    {
        if (!Instance) Instance = this;
        UpdateData();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateData()
    {
        var configs = ConfigsAsset.GetConfigList<ShopConfig>();
        int countDiff = configs.Count - Data.PurchaseTimes.Count;
        if (Data.PurchaseTimes.Count < configs.Count)
            for (int i = 0; i < countDiff; i++) Data.PurchaseTimes.Add(0);
    }

    public void Open()
    {
        if (!m_ShopView) m_ShopView = Instantiate(m_ShopViewPrefab, WindowManager.Instance.transform).GetComponent<ShopView>();
    }
}

[System.Serializable]
public class ShopData
{
    
    public List<int> PurchaseTimes = new List<int>();
}
