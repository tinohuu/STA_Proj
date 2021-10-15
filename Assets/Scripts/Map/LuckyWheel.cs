using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LuckyWheel : MonoBehaviour
{
    [SerializeField] SpriteRenderer IconRenderer;
    [SerializeField] Sprite[] IconSprites = new Sprite[2];
    [SerializeField] Transform m_BigWheel;
    [SerializeField] Transform m_SmallWheel;
    [SerializeField] Collider2D m_Collider2D;
    [SerializeField] ButtonAnimator m_ButtonAnimator;
    public int LevelID = 0;
    public int WheelID = 0;

    private void Start()
    {
        m_ButtonAnimator = m_BigWheel.GetComponent<ButtonAnimator>();
        m_BigWheel.transform.localScale = Vector3.zero;
        m_Collider2D.enabled = false;
        UpdateView();
    }

    public void UpdateView()
    {
        IconRenderer.color = MapManager.Instance.Data.CompleteLevel >= LevelID - 1 ? Color.white : new Color(1, 1, 1, 0.5f);
        IconRenderer.sprite = CropManager.Instance.LevelToCropConfig(LevelID).ID % 2 == 1 ? IconSprites[0] : IconSprites[1];

        var levelButton = MapLevelManager.Instance.GetLevelButton(LevelID);
        levelButton.SetAsIcon(transform, false);


    }

    public void ToBig()
    {
        //if (MapManager.Instance.Data.WheelCollectedLevel < LevelID && MapManager.Instance.Data.CompleteLevel >= LevelID)
        {


        }

        SoundManager.Instance.PlaySFX("wheelAvatar");
        m_SmallWheel.transform.DOScale(Vector3.zero, 0.5f);
        m_BigWheel.transform.DOScale(Vector3.one, 0.75f).SetDelay(0.5f).SetEase(Ease.OutBack);
        LuckyWheelManager.Instance.ParentInGroup(this);
        m_Collider2D.enabled = true;
        //m_ButtonAnimator.OnClick.AddListener(() => LuckyWheelManager.Instance.ShowView(this));
    }
}
