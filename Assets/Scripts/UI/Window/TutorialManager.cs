using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{

    public GameObject TutorialPrefab;
    public bool EnableTutorial = true;
    public delegate GameObject Getter();

    public static TutorialManager Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

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

    public void Show(string name, GameObject target = null, UnityAction onClick = null)
    {
        if (!EnableTutorial) return;

        Tutorial t = Instantiate(TutorialPrefab, WindowManager.Instance.transform).GetComponent<Tutorial>();
        t.SetTutorial(target, onClick);
    }

    public void Show(string name, Getter getter = null, UnityAction onClick = null)
    {
        if (!EnableTutorial) return;

        // if tutorial not shown yet
        var result = getter();
        if (result)
        {
            // then show tip with name and callback 
            // make button onclick as callback
        }

        if (onClick == null)
        {
            ExecuteEvents.Execute(result, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }
        else
        {
            onClick.Invoke();
        }
    }
}

[System.Serializable]
public class TutorialData
{
    public List<bool> Tutorials = new List<bool>();
}
