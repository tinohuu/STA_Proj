using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STAGameManager : MonoBehaviour
{
    public static STAGameManager Instance;

    public int nChapterID = 1;
    public int nLevelID = 1;
    public List<RewardType> InUseItems = new List<RewardType>();

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("STAGameManager::Awake()... ");
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("STAGameManager::Start()... ");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
