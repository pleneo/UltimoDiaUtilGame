using UnityEngine;

[CreateAssetMenu(menuName = "Ultimo Dia Util/Day/Economy Config", fileName = "Economy_")]
public class EconomyConfig : ScriptableObject
{
    public int initialMoney = 0;
    public int initialDebt = 1000;
    public int payPerCorrectDecision = 10;
    public int penaltyPerMistake = 5;
    public int dailyExpenses = 0;
    public int warningLimit = 3;
    public bool autoPayDebtFromMoney = true;
}
