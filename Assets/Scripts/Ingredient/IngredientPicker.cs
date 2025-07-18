using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngredientPicker : MonoBehaviour
{
    [Header("재료들 리스트")]
    [SerializeField] private List<IngredientScriptableObject> targetIngredients = new();
    [SerializeField] private List<IngredientScriptableObject> ownedIngredients = new();
    [SerializeField] private List<IngredientScriptableObject> requiredIngredients = new();
    [SerializeField] private List<IngredientScriptableObject> unrequiredIngredients = new();

    [Header("재료를 고를 확률")]
    [SerializeField] private float correctIngredientProbability = 0.9f; 

    [Header("기타")]
    [SerializeField] private Consumer consumer;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            PickIngredient();
        }
    }

    // 1. 타겟 재료들 리스트를 구한다
    public List<IngredientScriptableObject> GetTargetIngredients()
    {
        targetIngredients = gameObject.GetComponent<Consumer>().ingredients;

        return targetIngredients;
    }

    // 2. 현재 필요한 재료의 리스트를 구한다
    public void GetRequiredIngredients()
    {
        foreach(IngredientScriptableObject ingredient in targetIngredients)
        {
            if(!ownedIngredients.Contains(ingredient))
            {
                requiredIngredients.Add(ingredient);
            }
        }
    }

    // 3. 필요하지 않은 재료의 리스트를 구한다
    public void GetUnrequiredIngredients()
    {
        foreach(IngredientScriptableObject ingredient in IngredientManager.Instance.ingredientScriptableObject)
        {
            if(!targetIngredients.Contains(ingredient))
            {
                unrequiredIngredients.Add(ingredient);
            }
        }
    }

    // 4. 확률적으로 재료를 선택한다.
    public void PickIngredient()
    {
        GetTargetIngredients();
        GetRequiredIngredients();

        if (targetIngredients.Count <= 0 || ownedIngredients.Count <= targetIngredients.Count) return; 
        
        IngredientScriptableObject selectedIngredient = SelectRandomIngredient();
        UpdateOwnedIngredients(selectedIngredient);
        UpdateRequiredIngredients(selectedIngredient);
    } 

    private IngredientScriptableObject SelectRandomIngredient()
    {
        int probability = Random.Range(0, 10);
        int index = 0;

        if(probability < correctIngredientProbability)
        {
            index = GetRandomIndex(requiredIngredients);
        }
        else
        {
            index = GetRandomIndex(unrequiredIngredients);
        }

        Debug.Log($"선택된 재료: {requiredIngredients[index]}");

        return requiredIngredients[index];
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }

    private void UpdateRequiredIngredients(IngredientScriptableObject ingredient)
    {
        requiredIngredients.Remove(ingredient);
        Debug.Log($"필요한 재료들 업데이트: {string.Join(", ", requiredIngredients)}");
    }

    private void UpdateOwnedIngredients(IngredientScriptableObject ingredient)
    {
        ownedIngredients.Add(ingredient);
        Debug.Log($"가지고 있는 재료들 업데이트: {string.Join(", ", ownedIngredients)}");
    }
}
