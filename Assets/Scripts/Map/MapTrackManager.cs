using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using STA.Mapmaker;
using System;
using System.Linq;

public class MapTrackManager : MonoBehaviour, IMapmakerModule
{
    [Header("Ref")]
    public GameObject TrackPointPrefab;
    public Transform TrackPointGroup;

    [Header("Config")]
    public float MapScale = 1.5f;
    public bool ShowPath = true;

    List<MapTrackPoint> trackPoints = new List<MapTrackPoint>();
    int curIndex = 0;
    float moveRangeH = 1080;
    float moveRangeEdge = 0;
    float moveMaxH = 1080;

    public static MapTrackManager Instance = null;
    RectTransform rectTransform;
    CanvasScaler canvasScaler;

    public Type Mapmaker_ItemType => typeof(MapTrackPoint);
    public string[] Mapmaker_InputInfos => new string[0];

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Mapmaker_CreateItems(Mapmaker.GetConfig(this));

        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();

        transform.localScale = Vector3.one * MapScale;

        // Get moveable range data
        moveMaxH = canvasScaler.referenceResolution.y;
        moveRangeH = moveMaxH / MapScale;
        moveRangeEdge = (moveMaxH - moveRangeH) / MapScale;
    }

    public void UpdatePoints()
    {
        trackPoints = TrackPointGroup.GetComponentsInChildren<MapTrackPoint>().ToList();
        trackPoints.Sort((x, y) => x.transform.position.x.CompareTo(x.transform.position.x));
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        if (trackPoints.Count > 1)
        {
            try
            {

                {
                    //Debug.Log("Doing");
                    // Switch waypoints
                    if (curIndex < trackPoints.Count - 1 && Camera.main.transform.position.x > trackPoints[curIndex + 1].transform.position.x) curIndex++;
                    if (curIndex > 0 && Camera.main.transform.position.x < trackPoints[curIndex].transform.position.x) curIndex--;

                    // Get current and next waypoints data
                    float curX = trackPoints[curIndex].transform.position.x;
                    float curY = trackPoints[curIndex].transform.localPosition.y;
                    float nextX = curIndex + 1 == trackPoints.Count ? curX : trackPoints[curIndex + 1].transform.position.x;
                    float nextY = curIndex + 1 == trackPoints.Count ? curY : trackPoints[curIndex + 1].transform.localPosition.y;

                    // Calculate content pos y data
                    //Debug.Log(moveRangeEdge);
                    curY = Mathf.Clamp(Mathf.Abs(curY) - moveRangeEdge, 0, moveRangeH) / moveRangeH * moveMaxH * (MapScale - 1);
                    nextY = Mathf.Clamp(Mathf.Abs(nextY) - moveRangeEdge, 0, moveRangeH) / moveRangeH * moveMaxH * (MapScale - 1);


                    float posY = Mathf.SmoothStep(curY, nextY,
                        (Camera.main.transform.position.x - curX) / (nextX - curX));

                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posY);
                }
            }
            catch
            {
                UpdatePoints();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowPath && TrackPointGroup.childCount > 1)
        {
            for (int i = 1; i < TrackPointGroup.childCount; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(TrackPointGroup.GetChild(i - 1).position, TrackPointGroup.GetChild(i).position);
            }
        }
    }

    public void Mapmaker_CreateItems(string json)
    {
        if (json == null) return;

        TrackPointGroup.DestroyChildren();

        var configs = JsonExtensions.JsonToList<Mapmaker_BaseConfig>(json);

        if (configs.Count == 0) return;

        foreach (var config in configs)
        {
            var point = Instantiate(TrackPointPrefab, TrackPointGroup).GetComponent<MapTrackPoint>();
            point.transform.localPosition = config.LocPos;
        }

        UpdatePoints();
    }

    public Transform Mapmaker_AddItem()
    {
        Vector3 initLocPos = TrackPointGroup.InverseTransformPoint(Vector3.zero);

        var points = TrackPointGroup.GetComponentsInChildren<MapTrackPoint>().ToList();

        if (points.Find(e => Mathf.Abs(e.transform.localPosition.x - initLocPos.x) < 100))
        {
            Mapmaker.Log("Track points too close.");
            return null;
        }

        GameObject obj = Instantiate(TrackPointPrefab, TrackPointGroup);
        obj.transform.localPosition = initLocPos;
        UpdatePoints();

        return obj.transform;
    }

    public string[] Mapmaker_UpdateInputs(Transform target) => new string[0];

    public void Mapmaker_ApplyInputs(Transform target, string[] inputDatas) { UpdatePoints();}

    public string Mapmaker_ToConfig()
    {
        var points = TrackPointGroup.GetComponentsInChildren<MapTrackPoint>();
        var configs = new List<Mapmaker_BaseConfig>();
        foreach (var point in points)
        {
            var config = new Mapmaker_BaseConfig();
            config.LocPos = point.transform.localPosition;
            configs.Add(config);
        }
        return JsonExtensions.ListToJson(configs);
    }

    public void Mapmaker_DeleteItem(GameObject target)
    {
        DestroyImmediate(gameObject);
        UpdatePoints();
    }
}
