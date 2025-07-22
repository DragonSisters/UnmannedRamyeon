using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

using Random = UnityEngine.Random;

public class IngredientManager : Singleton<IngredientManager>
{
    [SerializeField] internal List<IngredientScriptableObject> ingredientScriptableObject = new();
    [SerializeField] private float correctIngredientProbability = 9f;
    [SerializeField] private float IngredientPickUpTime = 3f;

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

    public IEnumerator ChooseIngredientRoutine(List<IngredientScriptableObject> targetIngredients, List<IngredientScriptableObject> untargetIngredients, List<IngredientScriptableObject> ownedIngredients, List<int> orders)
    {
        IngredientScriptableObject ingredient = null;

        // 재료를 가져올 순서를 섞습니다.
        orders = ShufflePartOfList(orders, orders.Count);

        // 순서대로 가져옵니다.
        for(int i = 0; i < orders.Count; i++)
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
                int randomIndex = GetRandomIndex(untargetIngredients);
                ingredient = untargetIngredients[randomIndex];
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



    public void ResetIngredientLists(List<IngredientScriptableObject> ingredientsList)
    {
        ingredientsList.Clear();
    }
}
