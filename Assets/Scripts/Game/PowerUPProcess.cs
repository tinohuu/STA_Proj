using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUPProcess// : MonoBehaviour
{
    public List<GameDefines.PowerUPInfo> powerUpInfos = new List<GameDefines.PowerUPInfo>();

    bool bHasClearAll     = false;
    bool bUsingClearAll   = false;

    bool bHasClearThree   = false;
    bool bUsingClearThree = false;

    bool bHasWildDrop     = false;
    bool bUsingWildDrop   = false;

    public void Init()
    {
        Debug.Log("here we init the power up process, to get information from the STAGameManager... ... ");

        int nRemoveCards = Reward.Data[RewardType.RemoveCards];
        if(nRemoveCards > 0)
        {
            GameDefines.PowerUPInfo info = new GameDefines.PowerUPInfo();
            info.itemType = GameDefines.PowerUPType.Remove_Three;
            info.useStatus = GameDefines.PowerUPUseStatus.Unused;
            info.useType = GameDefines.PowerUPUseType.Once;
            powerUpInfos.Add(info);

            bHasClearThree = true;
            bUsingClearThree = true;
        }

        int nClearPlayable = Reward.Data[RewardType.ClearPlayables];
        if(nClearPlayable > 0)
        {
            GameDefines.PowerUPInfo info = new GameDefines.PowerUPInfo();
            info.itemType = GameDefines.PowerUPType.Clear_All;
            info.useStatus = GameDefines.PowerUPUseStatus.Unused;
            info.useType = GameDefines.PowerUPUseType.Once;
            powerUpInfos.Add(info);

            bHasClearAll = true;
            bUsingClearAll = false;
        }

        powerUpInfos.Sort(delegate(GameDefines.PowerUPInfo info1, GameDefines.PowerUPInfo info2)
        {
            int nItem1 = (int)info1.itemType;
            int nItem2 = (int)info2.itemType;
            return nItem1.CompareTo(nItem2);
        });
    }

    public void Reset()
    {
        powerUpInfos.Clear();

        Init();
    }

    //use the once powerups, we should use it at the beginning of a frame
    public void BeginUsePowerUPs()
    {
        if(powerUpInfos.Count == 0)
        {
            GameplayMgr.Instance.FinishUsingOncePowerUPs();
            return;
        }

        for (int i = 0; i < powerUpInfos.Count; ++i)
        {
            if(powerUpInfos[i].useType == GameDefines.PowerUPUseType.Once
                && powerUpInfos[i].useStatus != GameDefines.PowerUPUseStatus.Used)
            {
                if(UseOncePowerUP(powerUpInfos[i]))
                    powerUpInfos[i].useStatus = GameDefines.PowerUPUseStatus.Used;
            }
        }
    }

    public bool HasFinishedOncePowerUPUsage()
    {
        for (int i = 0; i < powerUpInfos.Count; ++i)
        {
            if (powerUpInfos[i].useType == GameDefines.PowerUPUseType.Once
                && powerUpInfos[i].useStatus != GameDefines.PowerUPUseStatus.Used)
            {
                return false;
            }
        }

        return true;
    }

    public void FinishUsingClearAll()
    {
        bUsingClearAll = false;
    }

    public void FinishUsingClearThree()
    {
        bUsingClearThree = false;
    }

    public void FinishUsingWildDrop()
    {
        bUsingWildDrop = false;
    }

    public bool FinishUsingAllOncePowerUP()
    {
        bool bRet = true;

        bRet &= ((bHasClearAll && !bUsingClearAll) || (!bHasClearAll));

        bRet &= ((bHasClearThree && !bUsingClearThree) || (!bHasClearThree));

        bRet &= ((bHasWildDrop && !bUsingWildDrop) || (!bHasWildDrop));

        return bRet;
    }

    //here we check the buff powerups,
    public void Update()
    {
    }

    bool UseOncePowerUP(GameDefines.PowerUPInfo powerUPInfo)
    {
        bool bRet = false;

        switch (powerUPInfo.itemType)
        {
            case GameDefines.PowerUPType.Clear_All:
                UsePowerUP_ClearAll(powerUPInfo);
                bRet = true;
                break;

            case GameDefines.PowerUPType.Remove_Three:
                bRet = UsePowerUP_RemoveThree(powerUPInfo);
                break;

            case GameDefines.PowerUPType.Wild_Drop:
                bRet = UsePowerUP_WildDrop(powerUPInfo);
                break;

            default:break;
        }

        return bRet;
    }

    void UsePowerUP_ClearAll(GameDefines.PowerUPInfo powerUPInfo)
    {
        Debug.Log("here we used powerup clear_all");

        bUsingClearAll = true;

        GameplayMgr.Instance.UsePowerUP_ClearAll(powerUPInfo);
    }

    bool UsePowerUP_RemoveThree(GameDefines.PowerUPInfo powerUPInfo)
    {
        if (bHasClearAll && bUsingClearAll)
            return false ;

        Debug.Log("here we used powerup clear_three");

        bUsingClearThree = true;

        return GameplayMgr.Instance.UsePowerUP_RemoveThree(powerUPInfo);
    }

    bool UsePowerUP_WildDrop(GameDefines.PowerUPInfo powerUPInfo)
    {
        if ((bHasClearAll && bUsingClearAll) || (bHasClearThree && bUsingClearThree))
            return false;

        Debug.Log("here we used powerup wilddrop");

        bUsingWildDrop = true;

        return true;
    }
}
