using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOption : MonoBehaviour
{
    public string MenuDataName = "";
    private void OnEnable()
    {
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName);
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = (bool)propertyInfo.GetValue(MenuManager.Instance);
    }
    public void SetBool(bool input)
    {
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName, typeof(bool));
        propertyInfo.SetValue(MenuManager.Instance, input);
    }

    public void SetInt(int input)
    {
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName, typeof(int));
        propertyInfo.SetValue(MenuManager.Instance, input);
    }

    public void SetFloat(float input)
    {
        var propertyInfo = typeof(MenuManager).GetProperty(MenuDataName, typeof(int));
        propertyInfo.SetValue(MenuManager.Instance, input);
    }
}
