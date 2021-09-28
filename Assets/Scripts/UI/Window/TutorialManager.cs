using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{

    public GameObject TutorialPrefab;

    [SerializeField] Tutorial m_CurTutorial;

    public bool EnableTutorial = true;
    public delegate GameObject Getter();

    [SavedData] [SerializeField]TutorialManagerData m_Data = new TutorialManagerData();
    Dictionary<string, TutorialData> m_DatasByCode = new Dictionary<string, TutorialData>();

    public static TutorialManager Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float GetSize(RectTransform rt)
    {
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt);
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    public void Show(string code, int progress, Getter getter, UnityAction onClick = null)
    {
        code = code.ToUpper();

        if (!CheckTutorial(code, progress)) return;

        GameObject target = getter();
        if (!target) return;

        CreateTutorial(code, progress, target, onClick);
    }

    public void Show(string code, int progress, GameObject target, UnityAction onClick = null)
    {
        code = code.ToUpper();

        if (!CheckTutorial(code, progress)) return;
        CreateTutorial(code, progress, target, onClick);
    }

    bool CheckTutorial(string code, int progress)
    {
        if (!EnableTutorial) return false;

        if (m_CurTutorial) return false;

        if (!m_DatasByCode.ContainsKey(code) || m_DatasByCode[code].IsComplete || m_DatasByCode[code].Progress != progress - 1) return false;

        return true;
    }

    void CreateTutorial(string code, int progress, GameObject target, UnityAction onClick)
    {
        var config = ConfigsAsset.GetConfigList<TutorialConfig>().Find(e => e.Code == code && e.Progress == progress);
        Debug.Log(config.Code);
        if (onClick == null)
            onClick = () => ExecuteEvents.Execute(target, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        m_CurTutorial = Instantiate(TutorialPrefab, WindowManager.Instance.transform).GetComponent<Tutorial>();
        m_CurTutorial.SetTutorial(config, target, onClick);
    }

    void UpdateData()
    {

        if (m_Data.TutorialDatas.Count == 0)
        {
            //var configs = ConfigsAsset.GetConfigList<TutorialConfig>();
            //foreach (var c in configs) c.Content = c.Content.Replace("\\n", "\n");
            var configsByCode = ConfigsAsset.GetConfigList<TutorialConfig>().GroupBy(e => e.Code).ToDictionary(e => e.Key, e => e.ToList());

            foreach (var code in configsByCode.Keys)
            {
                TutorialData data = new TutorialData();
                data.Code = code;
                data.MaxProgress = data.MaxProgress = configsByCode[code].Count;
                m_Data.TutorialDatas.Add(data);
            }
        }
        else
        {
            var configsByCode = ConfigsAsset.GetConfigList<TutorialConfig>().GroupBy(e => e.Code).ToDictionary(e => e.Key, e => e.ToList());

            foreach (var code in configsByCode.Keys)
            {
                TutorialData data = m_Data.TutorialDatas.Find(e => e.Code == code);
                data.MaxProgress = data.MaxProgress = configsByCode[code].Count;
            }
        }
        m_DatasByCode = m_Data.TutorialDatas.ToDictionary(e => e.Code);
    }

    public void Finish(TutorialConfig config)
    {
        m_DatasByCode[config.Code].Progress++;
        m_CurTutorial = null;
    }
}

[System.Serializable]
public class TutorialManagerData
{
    public List<TutorialData> TutorialDatas = new List<TutorialData>();
}

[System.Serializable]
public class TutorialData
{
    public string Code = "";
    public int Progress = 0;
    public int MaxProgress = 0;
    public bool IsComplete => Progress >= MaxProgress;
}
