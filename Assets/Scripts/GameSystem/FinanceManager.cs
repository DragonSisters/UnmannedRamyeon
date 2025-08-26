using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FinanceManager : Singleton<FinanceManager>
{
    public bool IsSuccess => currentMoney >= goalMoney;
    /// <summary>
    /// 목표 매출액
    /// </summary>
    [Header("목표 매출액")]
    [SerializeField] private int easyModeGoalMoney = 20000;
    [SerializeField] private int hardModeGoalMoney = 50000;
    private int goalMoney
    {
        get { return GameManager.Instance.IsHardMode ? hardModeGoalMoney : easyModeGoalMoney; }
    }

    [Header("UI관련")]
    [SerializeField] private TMP_Text goalMoneyUi;
    [SerializeField] private TMP_Text totalMoneyUi;
    [SerializeField] private GameObject priceUiPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private int poolSize = 10;

    private ObjectPool<PriceUI> priceUiPool;
    public const float LOSING_MONEY_DURATION = 0.5f;
    private const float PRICE_UI_SHOW_DURATION = 2f;

    /// <summary>
    /// 현재 매출액
    /// </summary>
    public int CurrentMoney => currentMoney;
    private int currentMoney;

    public void OnGameEnter()
    {
        // 모든 오브젝트 정리
        if (priceUiPool != null)
        {
            priceUiPool.Clear();
        }
        // 가격 UI 풀 생성
        priceUiPool = new ObjectPool<PriceUI>(priceUiPrefab, poolSize, parent);
        // 목표 매출액 UI 업데이트
        goalMoneyUi.text = string.Format("목표금액: {0}원", goalMoney);
        // currentMoney 초기화
        currentMoney = 0;
        UpdatePrices();
    }

    public void OnGameEnd()
    {
        DespawnAllPriceUI();
    }

    public void IncreaseCurrentMoney(int money)
    {
        currentMoney += money;
        UpdatePrices();
        StartCoroutine(ActivateCoinUI(money));
    }

    public void DecreaseCurrentMoney(int money)
    {
        if(money < 0)
        {
            Debug.LogError($"금액을 감소시킬 때 음수를 넣지 말아주세요. 현재 입력금액: {money}");
            return;
        }
        else if(money == 0)
        {
            // 0이면 변동 없으므로 그냥 pass
            return;
        }

        currentMoney -= money;
        UpdatePrices();
        StartCoroutine(ActivateCoinUI(-money));
    }

    private void UpdatePrices()
    {
        // 현재 매출액 UI 업데이트
        totalMoneyUi.text = string.Format("{0}원", CurrentMoney);
    }

    private PriceUI SpawnPriceUI()
    {
        return priceUiPool.GetOrCreate();
    }

    private void DespawnPriceUI(PriceUI priceUI)
    {
        if (priceUI != null)
        {
            priceUiPool.Return(priceUI);
        }
    }

    private void DespawnAllPriceUI()
    {
        var activeObjects = priceUiPool.GetActiveObjects();

        // 역순으로 순회하여 안전한 제거
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = activeObjects[i];
            priceUiPool.Return(obj);
        }
    }

    private IEnumerator ActivateCoinUI(int money)
    {
        var coinUI = SpawnPriceUI();
        coinUI.SetValues(money);
        coinUI.PlayAnimation();

        yield return new WaitForSeconds(PRICE_UI_SHOW_DURATION);
        
        DespawnPriceUI(coinUI);
    }
}
