using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConsumerIngredientHandler : MonoBehaviour
{
    [SerializeField] private ConsumerUI consumerUI;

    /// <summary>
    /// 키오스크에 입력한 재료 목록
    /// </summary>
    private List<IngredientScriptableObject> targetIngredients = new List<IngredientScriptableObject>();
    /// <summary>
    /// 키오스크에 입력하지 않은 재료 목록 모두
    /// </summary>
    private List<IngredientScriptableObject> untargetedIngredients = new List<IngredientScriptableObject>();
    /// <summary>
    /// 원하는 재료 목록. 가져왔다면 목록에서 삭제됩니다.
    /// </summary>
    private Queue<(IngredientScriptableObject ingredient, int index, bool isCorrect)> neededIngredients = new();
    /// <summary>
    /// 현재 가져온 재료 목록
    /// </summary>
    public List<(IngredientScriptableObject ingredient, int index, bool isCorrect)> OwnedIngredients = new();

    public bool IsIngredientSelectDone => 
        OwnedIngredients.Count >= IngredientManager.MAX_INGREDIENT_NUMBER
        && neededIngredients.Count == 0;

    public void Initialize()
    {
        consumerUI = gameObject.GetOrAddComponent<ConsumerUI>();
        ResetAllIngredientLists();
        SetAllIngredientLists();
        ChooseAllIngredients();
    }

    /// <summary>
    /// 입장과 동시에 가져올 재료를 미리 선정해놓습니다.
    /// </summary>
    private void ChooseAllIngredients()
    {
        // 재료를 가져올 순서를 섞습니다.
        var orderList = new List<int> { 0, 1, 2, 3 };
        orderList = IngredientManager.Instance.ShufflePartOfList(orderList, orderList.Count);

        // 먼저 재료를 모두 골라놓습니다.
        for (int i = 0; i < IngredientManager.MAX_INGREDIENT_NUMBER; i++)
        {
            var currentIndex = orderList[i];
            GetNeededIngredient(currentIndex, out var ingredient, out var isCorrect);
            // 이후에 재료를 얻었을 때를 대비해서 미리 저장해둡니다.
            neededIngredients.Enqueue((ingredient, currentIndex, isCorrect));
        }
    }

    public (IngredientScriptableObject ingredient, int index, bool isCorrect) GetNeededIngredientInfo()
    {
        var ingredientInfo = neededIngredients.Peek(); // 제거하지 않고 반환만 합니다. 언제 issue단계가 올지 모르기 때문에
        return ingredientInfo;
    }

    public void AddOwnIngredient(IngredientScriptableObject ingredient, int index, bool isCorrect)
    {
        // 필요재료 리스트에서 뺍니다
        neededIngredients.Dequeue();
        // 얻은재료 리스트에 새로 가져온 재료를 추가합니다.
        OwnedIngredients.Add((ingredient, index, isCorrect));
        Debug.Log($"가지고 있는 재료: {string.Join(", ", OwnedIngredients)}");

        // UI를 업데이트 합니다.
        consumerUI.ActivateFeedbackUIs(index, isCorrect);
    }

    private void GetNeededIngredient(int index, out IngredientScriptableObject ingredient, out bool isCorrect)
    {
        // 확률에 따라 가져오는 재료가 다릅니다.
        int probability = Random.Range(0, 10);

        if (probability < IngredientManager.CORRECT_INGREDIENT_PROBAILITY)
        {
            ingredient = targetIngredients[index];
            isCorrect = true;
            Debug.Log($"올바른 재료! : {ingredient.Name}");
        }
        else
        {
            int randomIndex = GetRandomIndex(untargetedIngredients);
            ingredient = untargetedIngredients[randomIndex];
            isCorrect = false;
            Debug.Log($"틀린 재료! : {ingredient.Name}");
        }
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }

    private void ResetAllIngredientLists()
    {
        targetIngredients.Clear();
        untargetedIngredients.Clear();
        neededIngredients.Clear();
        OwnedIngredients.Clear();
    }

    private void SetAllIngredientLists()
    {
        // 재료를 고르고 필요한 재료의 리스트와 필요한 재료의 총 갯수를 구합니다.
        targetIngredients = IngredientManager.Instance.GetTargetIngredients(IngredientManager.MAX_INGREDIENT_NUMBER);

        // 필요한 재료들을 머리 위에 아이콘으로 표시합니다.
        consumerUI.InitializeIngredintUI(targetIngredients);

        // 필요하지 않은 재료의 리스트를 구합니다.
        untargetedIngredients = GetFilteredIngredients(targetIngredients, untargetedIngredients);

    }

    private List<IngredientScriptableObject> GetFilteredIngredients(
        List<IngredientScriptableObject> standardList, 
        List<IngredientScriptableObject> separatedList,
        List<IngredientScriptableObject> initialList = null)
    {
        if (initialList == null) initialList = IngredientManager.Instance.IngredientScriptableObject;

        foreach (IngredientScriptableObject ingredient in initialList)
        {
            if (!standardList.Contains(ingredient))
            {
                separatedList.Add(ingredient);
            }
        }
        return separatedList;
    }

}
