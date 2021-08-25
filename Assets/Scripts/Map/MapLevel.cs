using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLevel : MonoBehaviour, IPointerClickHandler
{
    public Sprite[] LevelSprites = new Sprite[2];
    public SpriteRenderer ButtonSpriteRenderer;
    public SpriteRenderer[] StarSpriteRenderers = new SpriteRenderer[3];
    public GameObject Frame;
    public TMP_Text LevelText;
    public MapLevelData Data;
    private void Awake()
    {

    }
    private void Start()
    {
        UpdateView();
    }

    public void UpdateView()
    {
        if (Data == null || CropManager.Instance.LevelToCropConfig(Data.Order) == null)
        {
            Destroy(gameObject);
            return;
        }

        ButtonSpriteRenderer.sprite = CropManager.Instance.LevelToCropConfig(Data.Order).ID % 2 == 1 ? LevelSprites[0] : LevelSprites[1];
        ButtonSpriteRenderer.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        LevelText.text = Data.Order.ToString();
        LevelText.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        Data.Rating = IsOpen ? Random.Range(1, 4) : 0; // Test: temp rating

        foreach (SpriteRenderer star in StarSpriteRenderers) star.color = IsOpen ? new Color(1, 1, 1, 0.5f) : Color.clear;
        for (int i = 0; i < Data.Rating; i++) StarSpriteRenderers[i].color = Color.white;
        Frame.SetActive(Data.Rating >= 3);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsOpen) return;
        MapPlayer.Instance.MoveToLevel(this);
    }

    public bool IsOpen => MapManager.Instance.Data.CompelteLevel >= Data.Order - 1;
}
