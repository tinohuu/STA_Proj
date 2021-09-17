using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Sprite[] LevelSprites = new Sprite[2];
    [SerializeField] SpriteRenderer ButtonSpriteRenderer;
    [SerializeField] SpriteRenderer[] StarSpriteRenderers = new SpriteRenderer[3];
    [SerializeField] GameObject Frame;
    [SerializeField] TMP_Text LevelText;
    public MapLevelData Data;

    private void Start()
    {
        UpdateView();
    }

    public void UpdateView()
    {
        if (Data == null)
        {
            Destroy(gameObject);
            return;
        }

        //ButtonSpriteRenderer.sprite = CropManager.Instance.LevelToCropConfig(Data.ID).ID % 2 == 1 ? LevelSprites[0] : LevelSprites[1];
        ButtonSpriteRenderer.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        LevelText.text = Data.ID.ToString();
        LevelText.color = IsOpen ? Color.white : new Color(1, 1, 1, 0.5f);

        Data.Rating = IsOpen && Data.ID != MapManager.Instance.Data.CompleteLevel + 1 ? Random.Range(1, 4) : 0; // Test: temp rating

        foreach (SpriteRenderer star in StarSpriteRenderers) star.color = IsOpen ? new Color(1, 1, 1, 0.5f) : Color.clear;
        for (int i = 0; i < Data.Rating; i++) StarSpriteRenderers[i].color = Color.white;
        Frame.SetActive(Data.Rating >= 3);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsOpen) return;
        MapPlayer.Instance.MoveToLevel(this);
    }

    public bool IsOpen => MapManager.Instance.Data.CompleteLevel >= Data.ID - 1;
}
