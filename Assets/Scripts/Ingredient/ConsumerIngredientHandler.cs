using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumerIngredientHandler : MonoBehaviour
{
    [SerializeField] private float correctIngredientProbability = 9f;
    [SerializeField] private float IngredientPickUpTime = 3f;

    [Header("재료 관련 변수")]
    [SerializeField] internal List<IngredientScriptableObject> targetIngredients = new List<IngredientScriptableObject>();
    [SerializeField] internal List<IngredientScriptableObject> ownedIngredients = new List<IngredientScriptableObject>();
    [SerializeField] internal List<IngredientScriptableObject> untargetedIngredients = new List<IngredientScriptableObject>();
    [SerializeField] internal int maxIngredientNumber;
    internal List<int> orders = new List<int> { 1, 2, 3, 4 };

    public List<IngredientScriptableObject> GetIngredientLists(List<IngredientScriptableObject> standardList, List<IngredientScriptableObject> separatedList, List<IngredientScriptableObject> initialList = null)
    {
        if (initialList == null) initialList = IngredientManager.Instance.ingredientScriptableObject;

        foreach (IngredientScriptableObject ingredient in initialList)
        {
            if (!standardList.Contains(ingredient))
            {
                separatedList.Add(ingredient);
            }
        }
        return separatedList;
    }

    public IEnumerator ChooseIngredientRoutine()
    {
        IngredientScriptableObject ingredient = null;

        // 재료를 가져올 순서를 섞습니다.
        orders = IngredientManager.Instance.ShufflePartOfList(orders, orders.Count);

        // 순서대로 가져옵니다.
        for (int i = 0; i < orders.Count; i++)
        {
            int order = orders[i];

            // 확률에 따라 가져오는 재료가 다릅니다.
            int probability = Random.Range(0, 10);

            if (probability < correctIngredientProbability)
            {
                ingredient = targetIngredients[i];
                // @anditsoon TODO: UI 업데이트
                Debug.Log($"올바른 재료! : {ingredient.Name}");
            }
            else
            {
                int randomIndex = GetRandomIndex(untargetedIngredients);
                ingredient = untargetedIngredients[randomIndex];
                Debug.Log($"틀린 재료! : {ingredient.Name}");
                // @anditsoon TODO: UI 업데이트
            }

            // 가지고 있는 재료들 리스트에 새로 가져온 재료를 추가합니다.
            ownedIngredients.Add(ingredient);
            Debug.Log($"가지고 있는 재료: {string.Join(", ", ownedIngredients)}");

            // @anditsoon TODO: 추후 알맞게 시간 변경할 것
            yield return new WaitForSeconds(IngredientPickUpTime);
        }
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }



    public void ResetAllIngredientLists()
    {
        targetIngredients.Clear();
        ownedIngredients.Clear();
        untargetedIngredients.Clear();
    }
}
