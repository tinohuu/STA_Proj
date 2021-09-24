using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public List<GameObject> Views = new List<GameObject>();
    public WindowAnimator CurView;

    public static WindowManager Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    /*public GameObject OpenView(GameObject prefab)
    {
        //var window = obj.GetComponent<WindowAnimator>();
        var obj = Instantiate(prefab, Instance.transform);

        Views.Add(obj);
        UpdateView();

        return obj;
    }
    public void CloseView(GameObject obj)
    {
        Instance.Views.Remove(obj);
        Destroy(obj);

        UpdateView();
    }

    void UpdateView()
    {
        for (int i = 0; i < Views.Count; i++)
        {
            if (Views[i])
            {
                Views[i].SetActive(i == Views.Count - 1);
            }
        }
    }*/
}
