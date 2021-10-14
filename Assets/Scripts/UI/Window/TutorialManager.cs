using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    [Header("Setting")]
    public bool EnableTutorial = true;

    [Header("Ref")]
    public GameObject TutorialPrefab;

    [Header("Debug")]
    [SerializeField] Tutorial m_CurTutorial;
    [SavedData] public TutorialManagerData Data = new TutorialManagerData();
    Dictionary<string, TutorialData> m_DatasByCode = new Dictionary<string, TutorialData>();

    public static TutorialManager Instance;

    public delegate GameObject Getter();
    public delegate bool Checker();

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        UpdateData();
    }

    float GetSize(RectTransform rt)
    {
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt);
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    public bool Show(string code, int progress, Getter getter, float scale = 1, float delay = 0, UnityAction onClick = null, UnityAction onStart = null, UnityAction onExit = null)
    {
        code = code.ToUpper();

        if (!CheckTutorial(code, progress)) return false;

        GameObject target = getter();
        if (!target) return false;

        StartCoroutine(ICreateTutorial(code, progress, target, scale, delay, onClick, onStart, onExit));

        return true;
    }

    public bool Show(string code, int progress, GameObject target, float scale = 1, float delay = 0, UnityAction onClick = null, UnityAction onStart = null, UnityAction onExit = null)
    {
        code = code.ToUpper();

        if (!CheckTutorial(code, progress)) return false;
        StartCoroutine(ICreateTutorial(code, progress, target, scale, delay, onClick, onStart, onExit));

        return true;
    }

    bool CheckTutorial(string code, int progress)
    {
        if (!EnableTutorial) return false;

        if (m_CurTutorial) return false;

        if (!m_DatasByCode.ContainsKey(code) || m_DatasByCode[code].IsComplete || m_DatasByCode[code].Progress != progress - 1) return false;

        return true;
    }

    /*void CreateTutorial(string code, int progress, GameObject target = null, float scale = 1, float delay = 0, UnityAction onClick = null, UnityAction onStart = null, UnityAction onExit = null)
    {
        var config = ConfigsAsset.GetConfigList<TutorialConfig>().Find(e => e.Code == code && e.Progress == progress);
        Debug.Log(config.Code);
        if (onClick == null)
            onClick = () => ExecuteEvents.Execute(target, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        m_CurTutorial = Instantiate(TutorialPrefab, WindowManager.Instance.transform).GetComponent<Tutorial>();
        m_CurTutorial.SetTutorial(config, target, scale, onClick, onStart, onExit);
    }*/

    IEnumerator ICreateTutorial(string code, int progress, GameObject target = null, float scale = 1, float delay = 0, UnityAction onClick = null, UnityAction onStart = null, UnityAction onExit = null)
    {
        var config = ConfigsAsset.GetConfigList<TutorialConfig>().Find(e => e.Code == code && e.Progress == progress);
        Debug.Log(config.Code);
        if (onClick == null)
            onClick = () => ExecuteEvents.Execute(target, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        yield return new WaitForSeconds(delay);

        m_CurTutorial = Instantiate(TutorialPrefab, WindowManager.Instance.transform).GetComponent<Tutorial>();
        m_CurTutorial.SetTutorial(config, target, scale, onClick, onStart, onExit);
    }

    void UpdateData()
    {
        if (Data.TutorialDatas.Count == 0)
        {
            var configsByCode = ConfigsAsset.GetConfigList<TutorialConfig>().GroupBy(e => e.Code).ToDictionary(e => e.Key, e => e.ToList());

            foreach (var code in configsByCode.Keys)
            {
                TutorialData data = new TutorialData();
                data.Code = code;
                data.MaxProgress = data.MaxProgress = configsByCode[code].Count;
                Data.TutorialDatas.Add(data);
            }
        }
        else
        {
            var configsByCode = ConfigsAsset.GetConfigList<TutorialConfig>().GroupBy(e => e.Code).ToDictionary(e => e.Key, e => e.ToList());

            foreach (var code in configsByCode.Keys)
            {
                TutorialData data = Data.TutorialDatas.Find(e => e.Code == code);
                data.MaxProgress = data.MaxProgress = configsByCode[code].Count;
            }
        }
        m_DatasByCode = Data.TutorialDatas.ToDictionary(e => e.Code);
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
