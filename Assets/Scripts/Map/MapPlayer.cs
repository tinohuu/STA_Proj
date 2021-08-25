using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPlayer : MonoBehaviour, IPointerClickHandler
{
    public GameObject MapLevelPanelPrefab;
    public UnityEvent OnClick = null;
    public float EdgeOffset = 100;
    public ScrollRect MapScrollRect;
    Vector3 screenPoint = new Vector2();
    public Transform RemoteImage;

    public static MapPlayer Instance = null;

    //bool isRunning = false;
    private void Awake()
    {
        Instance = this;
        MapScrollRect.onValueChanged.AddListener(UpdaterRemoteView);
    }

    void UpdaterRemoteView(Vector2 scrollRect = new Vector2())
    {
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPoint.x < 0 - EdgeOffset || screenPoint.x > Screen.width + EdgeOffset)
        {

            RemoteImage.gameObject.SetActive(true);
            RemoteImage.transform.localScale = screenPoint.x < 0 ? new Vector3(-1, 1, 1) : Vector3.one;
            // TODO: CHANGE IMAGE
            screenPoint.x = screenPoint.x < 0 ? 0: Screen.width;
            screenPoint.y = Screen.height / 2;
            RemoteImage.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }
        else
        {
            RemoteImage.gameObject.SetActive(false);
            //RemoteImage = realPoint;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
        OnClick?.Invoke();
    }

    public void MoveToLevel(MapLevel mapLevel, bool showPanel = true)
    {

        //if (isRunning) return;

        StopAllCoroutines();

        // Move to the screen edge when far from the screen area
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPoint.x < 0 - EdgeOffset || screenPoint.x > Screen.width + EdgeOffset)
        {
            RemoteImage.gameObject.SetActive(false);
            screenPoint.x = screenPoint.x < 0 ? 0 - EdgeOffset : Screen.width + EdgeOffset;
            transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }

        // Animate
        if (!showPanel)
        {
            transform.position = mapLevel.transform.position;
            return;
        }



        StartCoroutine(IMoveToLevel(mapLevel));
    }

    IEnumerator IMoveToLevel(MapLevel mapLevel)
    {
        //isRunning = true;

        if  (mapLevel.Data.Order != MapManager.Instance.Data.SelectedLevel)
        {
            MapManager.Instance.Data.SelectedLevel = mapLevel.Data.Order;
            Tween tween = transform.DOLocalJump(mapLevel.transform.localPosition, 1, 1, 1.5f);
            yield return tween.WaitForCompletion();
        }

        MapLevelWindow panel = Window.CreateWindowPrefab(MapLevelPanelPrefab).GetComponent<MapLevelWindow>();
        panel.UpdateView(mapLevel.Data);
        
        //isRunning = false;
    }
}
