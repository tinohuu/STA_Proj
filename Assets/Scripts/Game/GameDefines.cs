using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDefines
{
    public enum PokerItemType : int
    {
        None,
        Ascending_Poker,
        Descending_Poker,
        Bomb,
        Ladder,
        Add_N_Poker,
        Double_Number_Poker,
        WildDrop                    //this game poker is a wild drop.
    }

    public enum PowerUPType : int
    {
        None,
        Clear_All,
        Remove_Three,
        Wild_Drop,
        Remove_Ascend_Descend,
        Remove_Bomb,
        Remove_Lock,
        Max
    }

    public enum PowerUPUseType : int
    {
        None,
        Once,              //only use once
        Buff               //a buff/defuff
    }

    public enum PowerUPUseStatus : int
    {
        None,
        Unused,
        Using,
        Used
    }

    public class PowerUPInfo
    {
        public PowerUPType itemType;
        public PowerUPUseStatus useStatus;
        public PowerUPUseType useType;
    }


    public enum PokerStatus : int
    {
        None,
        Clearing,  //this is used to recognize that this poker is cleared by ClearAll
    }

    public enum WildCardSource : int
    {
        None,
        Gold,
        Item,
        StreakBonus,
        WildDrop
    }
}
