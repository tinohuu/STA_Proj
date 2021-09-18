using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageConfig
{
    public int ID;
    public int LevelID;
    public int ChapterNum;
    public int Cost;
    public int DoubleCost;
    public int QuadrupleCost;
    public int Deck;
    public int HandCards;
    public int WildCost;
    public int DoubleWildCost;
    public int QuadrupleWildCost;
    public string CancelCost;
    public string DoubleCancelCost;
    public string QuadrupleCancelCost;
    public string BuyCard;
    public string DoubleBuyCard;
    public string QuadrupleBuyCard;
    public string CardScore;
    public int EndCardScore;
    public string Star;
    public int CardCoin;
    public int DoubleCardCoin;
    public int QuadrupleCardCoin;
    public int RestCardCoin;
    public int DoubleRestCardCoin;
    public int QuadrupleRestCardCoin;
    public string StreakBonus;
    public string DoubleStreakBonus;
    public string QuadrupleStreakBonus;
    public string StreakBonusScore;
    public string StreakType;
    public int PowerUpScore;
    public string PowerUpsOrder;
    public int StageIcon;
}
