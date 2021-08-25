using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public delegate void Handler();
    public Handler OnWin = null;
    public Handler OnCollectDeck = null;
    public Handler OnCollectStarts = null;
    public Handler OnCompleteStreaks = null;

    public static QuestManager Instance = null;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

    }
}

public class Quest
{
    public QuestData Data = null;
    public Sprite QuestSprite = null;
    public string QuestText = "";

    public void Progress()
    {
        Debug.Log("Progress +");
    }

    public void Subscribe(bool unsubscribe = false)
    {
        switch (Data.QuestType)
        {
            case QuestData.Type.win:
                if (!unsubscribe) QuestManager.Instance.OnWin += new QuestManager.Handler(Progress);
                else QuestManager.Instance.OnWin -= new QuestManager.Handler(Progress);
                break;
        }
    }
}

[System.Serializable]
public class QuestData
{
    public enum Type { none, win, winFirst, winInRow, winHighest, collectDeck, collectStars, CompleteStreaks}
    public Type QuestType = Type.none;
    public int LevelMin = 0;
    public int Progress = 0;
    public int MaxProgress = 0;
    public List<int> Rewards = new List<int>();
}
