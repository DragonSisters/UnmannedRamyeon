using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConsumerIngredientHandler : MonoBehaviour
{
    private ConsumerUI consumerUI;
    private float correctIngredientProbability = 9f;
    private float ingredientPickUpTime = 3f;
    public float IngredientPickUpTime => ingredientPickUpTime;

    internal List<IngredientScriptableObject> targetIngredients = new List<IngredientScriptableObject>();
    internal List<IngredientScriptableObject> approvedIngredients = new List<IngredientScriptableObject>();
    internal List<IngredientScriptableObject> untargetedIngredients = new List<IngredientScriptableObject>();

    [SerializeField] internal int maxIngredientNumber = 4;
    internal int pickUpCount = 0;
    private Queue<int> orders = new Queue<int>();

    public bool IsIngredientSelectDone => pickUpCount >= maxIngredientNumber;

    public void Initialize()
    {
        consumerUI = gameObject.GetOrAddComponent<ConsumerUI>();
        ResetAllIngredientLists();
        SetAllIngredientLists();
        ShuffleIngredientOrder();
    }

    public void ChooseIngredient()
    {
        GetRandomIngredient(orders.Dequeue());
    }

    private void GetRandomIngredient(int index)
    {
        // 확률에 따라 가져오는 재료가 다릅니다.
        int probability = Random.Range(0, 10);
        bool isCorrect = false;

        IngredientScriptableObject ingredient = null;
        if (probability < correctIngredientProbability)
        {
            ingredient = targetIngredients[index];
            isCorrect = true;
            Debug.Log($"올바른 재료! : {ingredient.Name}");
        }
        else
        {
            int randomIndex = GetRandomIndex(untargetedIngredients);
            ingredient = untargetedIngredients[randomIndex];
            Debug.Log($"틀린 재료! : {ingredient.Name}");
        }

        // 가지고 있는 재료들 리스트에 새로 가져온 재료를 추가합니다.
        approvedIngredients.Add(ingredient);
        Debug.Log($"가지고 있는 재료: {string.Join(", ", approvedIngredients)}");

        pickUpCount++;

        // UI를 업데이트 합니다.
        consumerUI.DisplayIngredientFeedback(isCorrect, index);
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }

    private void ResetAllIngredientLists()
    {
        targetIngredients.Clear();
        approvedIngredients.Clear();
        untargetedIngredients.Clear();
    }

    private void SetAllIngredientLists()
    {
        // 재료를 고르고 필요한 재료의 리스트와 필요한 재료의 총 갯수를 구합니다.
        targetIngredients = IngredientManager.Instance.GetRandomIngredients(maxIngredientNumber);

        // 필요한 재료들을 머리 위에 아이콘으로 표시합니다.
        consumerUI.UpdateIngredientImages(targetIngredients);

        // 필요하지 않은 재료의 리스트를 구합니다.
        untargetedIngredients = GetIngredientLists(targetIngredients, untargetedIngredients);

    }

    private List<IngredientScriptableObject> GetIngredientLists(List<IngredientScriptableObject> standardList, List<IngredientScriptableObject> separatedList, List<IngredientScriptableObject> initialList = null)
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

    private void ShuffleIngredientOrder()
    {
        // 재료를 가져올 순서를 섞습니다.
        var orderList = new List<int> { 0, 1, 2, 3 };
        orderList = IngredientManager.Instance.ShufflePartOfList(orderList, orderList.Count);
        foreach (var item in orderList)
        {
            orders.Enqueue(item);
        }
    }

}
