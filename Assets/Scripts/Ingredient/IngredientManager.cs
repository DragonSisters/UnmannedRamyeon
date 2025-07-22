using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

using Random = UnityEngine.Random;

public class IngredientManager : Singleton<IngredientManager>
{
    [SerializeField] internal List<IngredientScriptableObject> ingredientScriptableObject = new();
    [SerializeField] private float correctIngredientProbability = 9f;

    void Start()
    {
        if(ingredientScriptableObject.Count <= 0)
        {
            throw new System.Exception($"재료매니저에 등록된 재료가 하나도 없습니다. {this.name}를 확인해주세요.");
        }

        foreach (var item in ingredientScriptableObject)
        {
            var isValidate = IsValidate(item);
            if(!isValidate)
            {
                throw new System.Exception($"재료({item.Name})에 문제가 있습니다 scriptable object를 확인해주세요.");
            }
        }
        
    }

    private bool IsValidate(IngredientScriptableObject ingredient)
    {
        if(string.IsNullOrEmpty(ingredient.Name))
        {
            return false;
        }
        if(ingredient.Icon == null)
        {
            return false;
        }
        if(ingredient.Price == 0)
        {
            return false;
        }
        return true;
    }

    public List<IngredientScriptableObject> GetRandomIngredients(int count)
    {
        if (count > ingredientScriptableObject.Count)
        {
            Debug.LogWarning("요청한 개수가 리스트 크기보다 큽니다.");
            count = ingredientScriptableObject.Count;
        }

        // 원본 리스트 복사
        var shuffledList = ShufflePartOfList(ingredientScriptableObject, count);

        // 앞에서 n개만 반환
        var selectedList = shuffledList.GetRange(0, count);
        Debug.Log($"손님이 고른 재료들: {string.Join(", ", selectedList)}");
        return selectedList;
    }

    public List<T> ShufflePartOfList<T>(List<T> originalList, int count)
    {
        List<T> shuffledList = new List<T>(originalList);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledList.Count);
            T temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        return shuffledList;
    }

    public List<IngredientScriptableObject> GetIngredientLists(List<IngredientScriptableObject> standardList, List<IngredientScriptableObject> separatedList, List<IngredientScriptableObject> initialList = null)
    {
        if (initialList == null) initialList = ingredientScriptableObject;

        foreach (IngredientScriptableObject ingredient in initialList)
        {
            if (!standardList.Contains(ingredient))
            {
                separatedList.Add(ingredient);
            }
        }

        return separatedList;
    }

    public IngredientScriptableObject PickIngredientAndResortLists(List<int> orders, List<IngredientScriptableObject> targetedIngredients, List<IngredientScriptableObject> untargetedIngredients, List<IngredientScriptableObject> ownedIngredients)
    {
        IngredientScriptableObject ingredient = null;

        // 재료를 가져올 순서를 섞습니다.
        orders = ShufflePartOfList(orders, orders.Count);

        // 옳은 재료 혹은 틀린 재료를 가져옵니다.
        int probability = Random.Range(0, 10);
        int index = 0;

        if (probability < correctIngredientProbability)
        {
            index = GetRandomIndex(targetedIngredients);
            ingredient = targetedIngredients[index];
            UpdateIngredientsLists(targetedIngredients, ingredient, ownedIngredients);
            Debug.Log($"필요한 재료들 업데이트: {string.Join(", ", targetedIngredients)}");
        }
        else
        {
            index = GetRandomIndex(untargetedIngredients);
            ingredient = untargetedIngredients[index];
            UpdateIngredientsLists(untargetedIngredients, ingredient, ownedIngredients);
            Debug.Log($"필요 없는 재료들 업데이트: {string.Join(", ", untargetedIngredients)}");
        }

        return ingredient;
    }

    public void UpdateIngredientsLists(List<IngredientScriptableObject> ingredientList, IngredientScriptableObject ingredient, List<IngredientScriptableObject> ownedIngredients)
    {
        Debug.Log($"선택된 재료: {ingredient}");

        ingredientList.Remove(ingredient);

        ownedIngredients.Add(ingredient);
        Debug.Log($"가지고 있는 재료들 업데이트: {string.Join(", ", ownedIngredients)}");
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }

    public void ResetIngredientLists(List<IngredientScriptableObject> ingredientsList)
    {
        ingredientsList.Clear();
    }
}
