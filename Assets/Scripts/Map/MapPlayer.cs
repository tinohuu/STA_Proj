using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPlayer : MonoBehaviour
{
    public List<GameObject> LevelWindowPrefabs;
    public float EdgeOffset = 100;
    public ScrollRect MapScrollRect;
    Vector3 screenPoint = new Vector2();
    public Transform RemoteImage;

    public static MapPlayer Instance = null;

    //bool isRunning = false;
    private void Awake()
    {
        if (!Instance) Instance = this;
        MapScrollRect.onValueChanged.AddListener(UpdaterRemoteView);
        RemoteImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(MapScrollRect.content);

        /*var levels = FindObjectsOfType<MapLevel>();
        var curLevel = System.Array.Find(levels, e => e.Data.ID == MapManager.Instance.Data.CompleteLevel);
        Debug.Log(MapManager.Instance.Data.CompleteLevel);
        MoveToLevel(curLevel, false);

        OnClickRomote(0);*/
        //OnClickRomote(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) OnClickRomote(0); 
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

    public void MoveToLevel(MapLevel mapLevel, bool showPanel = true, bool animate = true)
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


        StartCoroutine(IMoveToLevel(mapLevel, animate && mapLevel.Data.ID != MapManager.Instance.Data.SelectedLevel));
        MapManager.Instance.Data.SelectedLevel = mapLevel.Data.ID;
    }

    IEnumerator IMoveToLevel(MapLevel mapLevel, bool animate)
    {
        //isRunning = true;
        yield return null;
        if  (animate)
        {
            SoundManager.Instance.PlaySFX("uiAvatarLanding");
            Tween tween = transform.DOLocalJump(mapLevel.transform.localPosition, 1, 1, 1.5f);
            yield return tween.WaitForCompletion();
            SoundManager.Instance.PlaySFX("uiAvatarLanding");
        }
        else
        {
            transform.localPosition = mapLevel.transform.localPosition;
        }

        int windowIndex = 0;
        if (MapManager.Instance.Data.CompleteLevel + 1 >= MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == 1021).FunctionParams)
            windowIndex = 2;
        else if (MapManager.Instance.Data.CompleteLevel + 1 >= MapManager.Instance.FunctionConfigs.Find(e => e.FunctionID == 1012).FunctionParams)
            windowIndex = 1;

        MapLevelWindow panel = Window.CreateWindowPrefab(LevelWindowPrefabs[windowIndex]).GetComponent<MapLevelWindow>();
        panel.LevelData = mapLevel.Data;
        
        //isRunning = false;
    }


    public void OnClickRomote(float duration = 1)
    {
        StartCoroutine(IOnClickRemote(duration));
    }

    IEnumerator IOnClickRemote(float duration)
    {
        yield return null;
        MapManager.Instance.MoveMap(transform.position, duration);
    }
}
