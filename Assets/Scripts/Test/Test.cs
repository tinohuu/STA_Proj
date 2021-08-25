using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AnimationCurve Curve;
    // Start is called before the first frame update
    void Start()
    {
        Save oriSave = new Save();
        oriSave.Set(new TestSaveA(10));
        oriSave.Set(new TestSaveA(12));
        //oriSave.Set(new TestSaveB());

        SaveSystem.Save(oriSave);
        Save save = SaveSystem.Load();
        Debug.Log("Health: " + save.Get<TestSaveA>().Health);
        //Debug.Log("Speed: " + save.Get<TestSaveB>().Speed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class TestSaveA
{
    public int Health = 10;

    public TestSaveA(int health)
    {
        Health = health;
    }
}

[System.Serializable]
public class TestSaveB
{
    public float Speed = 13.5f;
}

