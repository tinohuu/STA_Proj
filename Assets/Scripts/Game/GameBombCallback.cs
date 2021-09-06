using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBombCallback : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBombBlast()
    {
        Debug.Log("--------------------------GameBombCallback::OnBombBlast--------------------------------------");

        GameplayMgr.Instance.OnBombEndGame();
    }
}
