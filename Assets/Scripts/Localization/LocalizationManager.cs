using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    Dictionary<string, string> TextsByCode = new Dictionary<string, string>();

    public static LocalizationManager Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void OnEnable()
    {
        TextsByCode = ConfigsAsset.GetConfigList<TextConfig>().ToDictionary(e => e.Code.ToUpper(), e => e.Content);
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(Localize);
    }

    public void Localize(Object obj)
    {
        TMP_Text text = obj as TMP_Text;
        Localize(text);
    }

    public void Localize(TMP_Text text)
    {
        if (!text.richText || text.gameObject.scene.name == null || text.gameObject.scene.rootCount == 0) return;
        //Debug.LogWarning("Localize " + text.text);
        string code = text.text.ToUpper();
        if (text && TextsByCode.ContainsKey(code))
        {
            text.SetText(TextsByCode[code].Replace("\\n", "\n"));
            text.ForceMeshUpdate();
        }
    }

    public string LocalizeString(string raw)
    {
        raw = raw.ToUpper();
        if (TextsByCode.ContainsKey(raw))
        {
            return TextsByCode[raw].Replace("\\n", "\n");
        }
        return raw;
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(Localize);
    }
}

public static class LocalizationExtensions
{
    public static void Format(this TMP_Text text, object arg0 = null, object arg1 = null, object arg2 = null)
    {
        LocalizationManager.Instance.Localize(text);
        text.text = string.Format(text.text, arg0, arg1, arg2);
    }

    public static void Format(this TMP_Text text, object[] args = null)
    {
        LocalizationManager.Instance.Localize(text);
        text.text = string.Format(text.text, args);
    }

    public static string ToLocalized(this string raw)
    {
        return LocalizationManager.Instance.LocalizeString(raw);
    }
}
