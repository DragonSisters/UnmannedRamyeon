using UnityEngine;

public class FinanceManager : Singleton<FinanceManager>
{
    /// <summary>
    /// 목표 매출액
    /// </summary>
    [SerializeField] private int goalMoney = 500000;

    /// <summary>
    /// 현재 매출액
    /// </summary>
    public int CurrentMoney => currentMoney;
    private int currentMoney;

    public void IncreaseCurrentMoney(int money)
    {
        currentMoney += money;
    }

    public void DecreaseCurrentMoney(int money)
    {
        currentMoney -= money;
    }
}
