using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLevelWindow : Window
{
    public TMP_Text OrderText = null;
    public List<Image> Stars = new List<Image>();

    public void UpdateView(MapLevelData data)
    {
        OrderText.text = data.Order.ToString();
        for (int i = 0; i < data.Rating; i++) Stars[i].color = Color.white;
        for (int i = data.Rating; i < Stars.Count; i++) Stars[i].color = Color.gray;
    }

    public void Play()
    {
        STAGameManager.Instance.nLevelID = MapManager.Instance.Data.SelectedLevel;
        SceneManager.LoadScene("GameScene");
    }
}
