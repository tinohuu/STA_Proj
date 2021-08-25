using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public new BoxCollider2D collider;
    private void Awake()
    {
        /*RaycastHit2D[] results = new RaycastHit2D[10];
        Debug.Log(collider.Cast(Vector2.right, results));
        foreach (RaycastHit2D hit in results)
        {
            if (hit)
                Debug.Log("Done");
        }*/
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(collider.bounds.center, transform.localScale, transform.rotation.eulerAngles.z, Vector2.zero);
        Debug.Log(hits.Length);
    }
}
