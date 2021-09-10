using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOption : MonoBehaviour
{
    public string MenuDataName = "";
    public GameObject OnCheckmark;
    public GameObject OffCheckmark;
    private void OnEnable()
    {
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName);
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = (bool)propertyInfo.GetValue(MenuManager.Instance);
        toggle.onValueChanged.AddListener((bool input) => SetValue(toggle.isOn));
        UpdateView(toggle.isOn);
    }

    void SetValue(bool input)
    {
        UpdateView(input);
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName, typeof(bool));
        propertyInfo.SetValue(MenuManager.Instance, input);
    }

    void UpdateView(bool isOn)
    {
        OnCheckmark.SetActive(isOn);
        OffCheckmark.SetActive(!isOn);
    }
}
