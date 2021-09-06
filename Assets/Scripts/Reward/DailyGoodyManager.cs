using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyGoodyManager : MonoBehaviour
{
    public DailyGoodyData Data = new DailyGoodyData();
    public static DailyGoodyManager Instance = null;
    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class DailyGoodyData
{
    public DateTime LastGoodyTime = new DateTime();
    public int StreakDays = 0;
    public int Version = 1;
}