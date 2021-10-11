using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheelRewardSlot : MonoBehaviour
{
    [SerializeField] Image m_RewardImage;
    [SerializeField] TMP_Text[] Texts = new TMP_Text[3];
    public string RewardText { get; set; }

    public void UpdateView(RewardType type, int count)
    {
        //Debug.Log(count);

        m_RewardImage.sprite = type.ToSprite();
        TMP_Text text;

        if (type == RewardType.Coin) text = Texts[0];
        else if (type == RewardType.Rocket) text = Texts[2];
        else text = Texts[1];

        text.gameObject.SetActive(true);

        if (type == RewardType.Rocket) text.text = string.Format(text.text, count.ToString());
        else text.text = string.Format(text.text, count.ToString("N0"));

        RewardText = text.text;
        RewardText += "\n";
        RewardText += type.ToString().ToIcon() + " ";
        RewardText += System.Text.RegularExpressions.Regex.Replace(type.ToString(), "([a-z])_?([A-Z])", "$1 $2");
    }
}
