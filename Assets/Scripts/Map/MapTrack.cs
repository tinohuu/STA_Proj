using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MapTrack : MonoBehaviour
{
    [Header("Ref")]
    public Transform TrackPointsGroup;
    [Header("Config")]
    public float MapScale = 1.5f;
    public bool ShowPath = true;

    List<Transform> points;
    int curIndex = 0;
    float moveRangeH = 1080;
    float moveRangeEdge = 0;
    float moveMaxH = 1080;

    public static MapTrack Instance = null;
    RectTransform rectTransform;
    CanvasScaler canvasScaler;
    
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();

        transform.localScale = Vector3.one * MapScale;

        points = new List<Transform>(TrackPointsGroup.GetComponentsInChildren<Transform>());

        points.RemoveAt(0);
        for (int i = 0; i < points.Count; i++)
        {
            //points[i].transform.localPosition = new Vector2(MapManager.MapMakerConfig.CurMapData.TrackDatas[i].PosX, MapManager.MapMakerConfig.CurMapData.TrackDatas[i].PosY);
        }

        // Get moveable range data
        moveMaxH = canvasScaler.referenceResolution.y;
        moveRangeH = moveMaxH / MapScale;
        moveRangeEdge = (moveMaxH - moveRangeH) / MapScale;
    }

    public void UpdateWayPoints()
    {
        points.Sort((x, y) => x.position.x.CompareTo(y.position.x));
        points = new List<Transform>(TrackPointsGroup.GetComponentsInChildren<Transform>());
        points.RemoveAt(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (points.Count > 1)
        {
            // Switch waypoints
            if (Camera.main.transform.position.x > points[curIndex + 1].transform.position.x && curIndex < points.Count - 1) curIndex++;
            if (Camera.main.transform.position.x < points[curIndex].transform.position.x && curIndex > 0) curIndex--;

            // Get current and next waypoints data
            float curX = points[curIndex].transform.position.x;
            float curY = points[curIndex].transform.localPosition.y;
            float nextX = curIndex + 1 == points.Count ? curX : points[curIndex + 1].transform.position.x;
            float nextY = curIndex + 1 == points.Count ? curY : points[curIndex + 1].transform.localPosition.y;

            // Calculate content pos y data
            curY = Mathf.Clamp(Mathf.Abs(curY) - moveRangeEdge, 0, moveRangeH) / moveRangeH * moveMaxH * (MapScale - 1);
            nextY = Mathf.Clamp(Mathf.Abs(nextY) - moveRangeEdge, 0, moveRangeH) / moveRangeH * moveMaxH * (MapScale - 1);
            float posY = Mathf.SmoothStep(curY, nextY,
                (Camera.main.transform.position.x - curX) / (nextX - curX));
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posY);
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowPath && TrackPointsGroup.childCount > 1)
        {
            for (int i = 1; i < TrackPointsGroup.childCount; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(TrackPointsGroup.GetChild(i - 1).position, TrackPointsGroup.GetChild(i).position);
            }
        }
    }
}
