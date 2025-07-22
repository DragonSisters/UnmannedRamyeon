using System;
using System.Collections.Generic;
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
        var shuffledList = new List<IngredientScriptableObject>(ingredientScriptableObject);

        // Fisher-Yates 셔플 (일부만)
        for (int i = 0; i < count; i++)
        {
            var randomIndex = Random.Range(i, shuffledList.Count);
            var temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // 앞에서 n개만 반환
        var selectedList = shuffledList.GetRange(0, count);
        Debug.Log($"손님이 고른 재료들: {string.Join(", ", selectedList)}");
        return selectedList;
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

    public void PickIngredient(List<IngredientScriptableObject> targetedIngredients, List<IngredientScriptableObject> untargetedIngredients, List<IngredientScriptableObject> ownedIngredients)
    {
        int probability = Random.Range(0, 10);
        int index = 0;

        if (probability < correctIngredientProbability)
        {
            index = GetRandomIndex(targetedIngredients);
            UpdateIngredientsLists(targetedIngredients, index, ownedIngredients);
            Debug.Log($"필요한 재료들 업데이트: {string.Join(", ", targetedIngredients)}");
        }
        else
        {
            index = GetRandomIndex(untargetedIngredients);
            UpdateIngredientsLists(untargetedIngredients, index, ownedIngredients);
            Debug.Log($"필요 없는 재료들 업데이트: {string.Join(", ", untargetedIngredients)}");
        }
    }

    public void UpdateIngredientsLists(List<IngredientScriptableObject> ingredientList, int index, List<IngredientScriptableObject> ownedIngredients)
    {
        IngredientScriptableObject ingredient = ingredientList[index];
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
