using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    //bool m_RunByEvent = false;
    UnityAction m_OnClickCircle;
    GameObject m_Target;

    public Button CircleButton;
    // Start is called before the first frame update
    void Start()
    {
        CircleButton.transform.position = m_Target.transform.position;

        if (m_OnClickCircle == null)
        {
            CircleButton.onClick.AddListener(() => ExecuteEvents.Execute(m_Target, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler));
        }
        else
        {
            CircleButton.onClick.AddListener(() => m_OnClickCircle());
        }
        CircleButton.onClick.AddListener(() => GetComponent<WindowAnimator>().Close());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTutorial(GameObject target, UnityAction onClickCircle = null)
    {
        m_Target = target;
        m_OnClickCircle = onClickCircle;
    }
}
